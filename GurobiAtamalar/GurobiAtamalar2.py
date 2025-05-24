from gurobipy import Model, GRB, quicksum
import read_db
import json
import os

# Ensure license environment variables are set
os.environ["WLSACCESSID"] = "daa3d856-65ae-46fc-8fe2-b161380c7f91"
os.environ["WLSSECRET"] = "b9f8f64f-d54d-497a-942d-d50c7a374bcb"
os.environ["LICENSEID"] = "2661618"

# --- 1. DATA ---
print("\nLoading data from database...")

# Get data from database
drivers_raw = read_db.get_drivers()
routes_raw = read_db.get_routes_by_day()
preferences_raw = read_db.get_preferences()

# Process drivers data
drivers = {}
for d_id, d_data in drivers_raw.items():
    drivers[d_id] = {
        'performance': d_data['performance'],
        'experience_years': d_data['experience_years'],
        'gender': d_data['gender'],
        'name': d_data['name']
    }

# Process routes data
routes = {}
for r_id, r_data in routes_raw.items():
    routes[r_id] = {
        'garage': r_data['garage'],
        'shifts_by_day': r_data['shifts_by_day']
    }

days = range(1, 8)  # 1-week planning

print(f"Loaded {len(drivers)} drivers and {len(routes)} routes")

# --- 2. MODEL ---
print("\nCreating optimization model...")
model = Model("DriverScheduling")

# --- 3. VARIABLES ---
x = {}  # Primary assignment: driver d to route r, shift s, day t
b = {}  # Backup assignment
y = {}  # Whether driver is backup on day

for d in drivers:
    for day in days:
        y[d, day] = model.addVar(vtype=GRB.BINARY, name=f"Backup_{d}_{day}")
        for r in routes:
            for s in range(len(routes[r]['shifts_by_day'][day])):
                x[d, day, r, s] = model.addVar(vtype=GRB.BINARY, name=f"x_{d}_{day}_{r}_{s}")
                b[d, day, r, s] = model.addVar(vtype=GRB.BINARY, name=f"b_{d}_{day}_{r}_{s}")

model.update()

# --- 4. CONSTRAINTS ---
print("Adding constraints...")

# 4.1 Each shift must have exactly 1 primary driver
for day in days:
    for r in routes:
        for s in range(len(routes[r]['shifts_by_day'][day])):
            model.addConstr(quicksum(x[d, day, r, s] for d in drivers) == 1)

# 4.2 Each driver works either a shift or is backup (not both)
for d in drivers:
    for day in days:
        model.addConstr(
            quicksum(x[d, day, r, s] for r in routes for s in range(len(routes[r]['shifts_by_day'][day]))) + y[d, day] == 1
        )

# 4.3 Each shift must have exactly one backup driver
for day in days:
    for r in routes:
        for s in range(len(routes[r]['shifts_by_day'][day])):
            model.addConstr(quicksum(b[d, day, r, s] for d in drivers) == 1)

# 4.4 Backup shifts only for backup drivers and limit to max 2 backup assignments per day
for d in drivers:
    for day in days:
        # Backup shifts only for backup drivers
        for r in routes:
            for s in range(len(routes[r]['shifts_by_day'][day])):
                model.addConstr(b[d, day, r, s] <= y[d, day])
        
        # Limit backup assignments to 2 per day
        model.addConstr(
            quicksum(b[d, day, r, s] for r in routes for s in range(len(routes[r]['shifts_by_day'][day]))) <= 2 * y[d, day]
        )

# 4.5 Weekly hours cap (48 hours, 8h per shift)
for d in drivers:
    model.addConstr(
        quicksum(x[d, day, r, s] * 8 for day in days for r in routes for s in range(len(routes[r]['shifts_by_day'][day]))) <= 48
    )

# --- 5. OBJECTIVE FUNCTION ---
print("Setting objective function...")

def get_preference(d, shift_id):
    return preferences_raw.get(d, {}).get(shift_id, 7)  # Default to least preferred if not found

# Primary assignment objective
primary_objective = quicksum(
    x[d, day, r, s] * (
        (7 - get_preference(d, routes[r]['shifts_by_day'][day][s])) * 100000 +  # Preference
        drivers[d]['performance'] * 500 +  # Performance
        drivers[d]['experience_years'] * 100 +  # Experience
        (1 if drivers[d]['gender'] == 'F' else 0) * 50  # Gender diversity
    )
    for d in drivers for day in days for r in routes for s in range(len(routes[r]['shifts_by_day'][day]))
)

# Backup assignment objective (weighted less than primary assignments)
backup_objective = 0.5 * quicksum(
    b[d, day, r, s] * (
        (7 - get_preference(d, routes[r]['shifts_by_day'][day][s])) * 100000 +  # Preference
        drivers[d]['performance'] * 500 +  # Performance
        drivers[d]['experience_years'] * 100 +  # Experience
        (1 if drivers[d]['gender'] == 'F' else 0) * 50  # Gender diversity
    )
    for d in drivers for day in days for r in routes for s in range(len(routes[r]['shifts_by_day'][day]))
)

# Combine objectives
model.setObjective(primary_objective + backup_objective, GRB.MAXIMIZE)

# --- 6. SOLVE ---
print("\nSolving model...")
try:
    model.optimize()
    
    if model.status == GRB.Status.OPTIMAL:
        print("\n[SUCCESS] Optimal solution found.")
        
        # --- 7. BUILD RESULTS ---
        print("\nBuilding results...")
        results = {"assignments": {}, "backups": {}, "schedule_info": {}}
        
        # Add basic info
        results["schedule_info"] = {
            "total_drivers": len(drivers),
            "total_routes": len(routes),
            "days_planned": len(list(days))
        }

        for day in days:
            results["assignments"][f"Day_{day}"] = {}
            for r in routes:
                for s in range(len(routes[r]['shifts_by_day'][day])):
                    shift_id = routes[r]['shifts_by_day'][day][s]
                    assigned_drivers = [d for d in drivers if x[d, day, r, s].X > 0.5]
                    if assigned_drivers:
                        d = assigned_drivers[0]  # There should be exactly one
                        results["assignments"][f"Day_{day}"][f"Route_{r}_Shift_{shift_id}"] = {
                            "driver_id": d,
                            "driver_name": drivers[d]['name'],
                            "preference": get_preference(d, shift_id),
                            "performance": drivers[d]['performance'],
                            "experience": drivers[d]['experience_years']
                        }

            results["backups"][f"Day_{day}"] = {}
            for d in drivers:
                if y[d, day].X > 0.5:
                    backup_assignments = []
                    for r in routes:
                        for s in range(len(routes[r]['shifts_by_day'][day])):
                            if b[d, day, r, s].X > 0.5:
                                shift_id = routes[r]['shifts_by_day'][day][s]
                                backup_assignments.append({
                                    "route": r,
                                    "shift_id": shift_id,
                                    "preference": get_preference(d, shift_id)
                                })
                    if backup_assignments:
                        results["backups"][f"Day_{day}"][f"Driver_{d}"] = {
                            "driver_name": drivers[d]['name'],
                            "backup_assignments": backup_assignments
                        }

        # Save results
        with open("optimized_schedule.json", "w") as f:
            json.dump(results, f, indent=4)

        print("[SUCCESS] Schedule created and saved as 'optimized_schedule.json'")
        
    elif model.status == GRB.Status.INFEASIBLE:
        print("\n[ERROR] Model is infeasible. Computing IIS...")
        model.computeIIS()
        print("\nConstraints causing infeasibility:")
        for c in model.getConstrs():
            if c.IISConstr:
                print(f"- {c.ConstrName}")
    else:
        print(f"\n[WARNING] Optimization ended with status: {model.status}")
        
except Exception as e:
    print(f"\n[ERROR] Error during optimization: {str(e)}")
