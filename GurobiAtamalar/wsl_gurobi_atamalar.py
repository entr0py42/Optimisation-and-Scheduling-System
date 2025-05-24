from gurobipy import Model, GRB, quicksum
import read_db
import numpy as np
import json
import os

# Ensure license environment variables are set
os.environ["WLSACCESSID"] = "daa3d856-65ae-46fc-8fe2-b161380c7f91"
os.environ["WLSSECRET"] = "b9f8f64f-d54d-497a-942d-d50c7a374bcb"
os.environ["LICENSEID"] = "2661618"

# --- 1. DATA ---

drivers = read_db.get_drivers()
routes = read_db.get_routes_by_day()
days = range(1, 8)  # 1-week planning
user_preferences = read_db.get_preferences()

# Create a mapping from (day, route, shift_index) to actual shift_id
index_to_shift_id = {}
for r in routes:
    for day in days:
        shift_ids = routes[r]['shifts_by_day'][day]
        for idx, shift_id in enumerate(shift_ids):
            index_to_shift_id[(day, r, idx)] = shift_id

# After loading all data but before optimization, add:
print("\n--- Input Data Analysis ---")
print(f"Number of drivers: {len(drivers)}")
print(f"Number of routes: {len(routes)}")

total_shifts = 0
for r in routes:
    for day in days:
        total_shifts += len(routes[r]['shifts_by_day'][day])
print(f"Total shifts across all days: {total_shifts}")
print(f"Total shifts per day: {total_shifts/7:.1f}")
print(f"Required drivers per day (including backups): {(total_shifts/7)*2:.1f}")
print("-------------------------\n")

# --- 2. MODEL ---

model = Model("DriverScheduling")

# --- 3. VARIABLES ---

x, b, y = {}, {}, {}
for d in drivers:
    for day in days:
        y[d, day] = model.addVar(vtype=GRB.BINARY, name=f"Backup_{d}_{day}")
        for r in routes:
            for s in range(len(routes[r]['shifts_by_day'][day])):
                x[d, day, r, s] = model.addVar(vtype=GRB.BINARY, name=f"x_{d}{day}{r}_{s}")
                b[d, day, r, s] = model.addVar(vtype=GRB.BINARY, name=f"b_{d}{day}{r}_{s}")
model.update()

# --- 4. CONSTRAINTS ---

for day in days:
    for r in routes:
        for s in range(len(routes[r]['shifts_by_day'][day])):
            model.addConstr(quicksum(x[d, day, r, s] for d in drivers) == 1)

for d in drivers:
    for day in days:
        model.addConstr(
            quicksum(x[d, day, r, s] for r in routes for s in range(len(routes[r]['shifts_by_day'][day]))) + y[d, day] == 1
        )

for d in drivers:
    for day in days:
        for r in routes:
            for s in range(len(routes[r]['shifts_by_day'][day])):
                model.addConstr(b[d, day, r, s] <= y[d, day])

for d in drivers:
    model.addConstr(
        quicksum(x[d, day, r, s] * 8 for day in days for r in routes for s in range(len(routes[r]['shifts_by_day'][day]))) <= 48
    )

for day in days:
    for r in routes:
        for s in range(len(routes[r]['shifts_by_day'][day])):
            model.addConstr(quicksum(b[d, day, r, s] for d in drivers) == 1)

for d in drivers:
    for day in days:
        model.addConstr(
            quicksum(b[d, day, r, s] for r in routes for s in range(len(routes[r]['shifts_by_day'][day]))) <= 1
        )

max_shifts = model.addVar(vtype=GRB.INTEGER)
min_shifts = model.addVar(vtype=GRB.INTEGER)
for d in drivers:
    total_shifts = quicksum(x[d, day, r, s] for day in days for r in routes for s in range(len(routes[r]['shifts_by_day'][day])))
    model.addConstr(max_shifts >= total_shifts)
    model.addConstr(min_shifts <= total_shifts)

# --- 5. OBJECTIVE FUNCTION ---

def get_preference(d, shift_id):
    return user_preferences.get(d, {}).get(shift_id, 7)

primary_objective = quicksum(
    x[d, day, r, s] * (
        (7 - get_preference(d, index_to_shift_id[(day, r, s)])) * 100000 +
        drivers[d]['performance'] * 500 +
        drivers[d]['experience_years'] * 100 +
        (1 if drivers[d]['gender'] == 'F' else 0) * 50
    )
    for d in drivers for day in days for r in routes for s in range(len(routes[r]['shifts_by_day'][day]))
)

backup_objective = 2 * quicksum(
    b[d, day, r, s] * (
        (7 - get_preference(d, index_to_shift_id[(day, r, s)])) * 100000 +
        drivers[d]['performance'] * 500 +
        drivers[d]['experience_years'] * 100 +
        (1 if drivers[d]['gender'] == 'F' else 0) * 50
    )
    for d in drivers for day in days for r in routes for s in range(len(routes[r]['shifts_by_day'][day]))
)

fairness_penalty = 1000000 * (max_shifts - min_shifts)

model.setObjective(primary_objective + backup_objective - fairness_penalty, GRB.MAXIMIZE)

# --- 6. SOLVE ---

try:
    model.optimize()
    
    if model.status == GRB.Status.OPTIMAL:
        print("\n✅ Optimal solution found.\n")
        
        results = {"assignments": {}, "backups": {}, "preferences_matrix": {}}

        for day in days:
            results["assignments"][f"Day_{day}"] = {}
            for r in routes:
                for s in range(len(routes[r]['shifts_by_day'][day])):
                    shift_id = index_to_shift_id[(day, r, s)]
                    assigned_drivers = [d for d in drivers if x[d, day, r, s].X > 0.5]
                    results["assignments"][f"Day_{day}"][f"Route_{r}Shift{shift_id}"] = [
                        {"driver": d, "preference": get_preference(d, shift_id)} for d in assigned_drivers
                    ]

            results["backups"][f"Day_{day}"] = {}
            for d in drivers:
                if y[d, day].X > 0.5:
                    assigned_backups = []
                    for r in routes:
                        for s in range(len(routes[r]['shifts_by_day'][day])):
                            if b[d, day, r, s].X > 0.5:
                                shift_id = index_to_shift_id[(day, r, s)]
                                assigned_backups.append({"route": r, "shift": shift_id, "preference": get_preference(d, shift_id)})
                    if assigned_backups:
                        results["backups"][f"Day_{day}"][f"Driver_{d}"] = assigned_backups

        for d in drivers:
            results["preferences_matrix"][f"Driver_{d}"] = {}
            for day in days:
                results["preferences_matrix"][f"Driver_{d}"][f"Day_{day}"] = {}
                for r in routes:
                    results["preferences_matrix"][f"Driver_{d}"][f"Day_{day}"][f"Route_{r}"] = {}
                    for s in range(len(routes[r]['shifts_by_day'][day])):
                        shift_id = index_to_shift_id[(day, r, s)]
                        results["preferences_matrix"][f"Driver_{d}"][f"Day_{day}"][f"Route_{r}"][f"Shift_{s}"] = get_preference(d, shift_id)

        with open("clean2_driver_schedule.json", "w") as f:
            json.dump(results, f, indent=4)

        print("✅ Schedule created and saved as 'clean2_driver_schedule.json'.")
        
    elif model.status == GRB.Status.INFEASIBLE:
        print("\n❌ Model is infeasible. Computing and displaying IIS (Irreducible Inconsistent Subsystem)...")
        model.computeIIS()
        print("\nConstraints in the IIS:")
        for c in model.getConstrs():
            if c.IISConstr:
                print(f"Constraint {c.ConstrName}: {c.Sense} {c.RHS}")
        
        print("\nVariables in the IIS:")
        for v in model.getVars():
            if v.IISLB:
                print(f'Lower bound: {v.VarName} >= {v.LB}')
            if v.IISUB:
                print(f'Upper bound: {v.VarName} <= {v.UB}')
                
        model.write("infeasible_model.ilp")
        print("\nDetailed model information written to 'infeasible_model.ilp'")
    else:
        print(f"\n⚠️ Optimization ended with status: {model.status}")
        
except Exception as e:
    print(f"\n🚫 Error during optimization: {str(e)}")
    raise
