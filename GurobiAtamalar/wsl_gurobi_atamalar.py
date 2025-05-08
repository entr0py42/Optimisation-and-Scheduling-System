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

# Structure the preferences based on the correct format
structured_preferences = {
    d: {
        day: {
            r: {
                shift_id: user_preferences.get(d, {}).get(shift_id)  # Use None if preference is missing
                for shift_id in routes[r]['shifts_by_day'][day]  # Correct key is 'shifts_by_day'
            }
            for r in routes
        }
        for day in days
    }
    for d in drivers
}

print("Creating optimization model...")

# --- 2. MODEL ---

model = Model("DriverScheduling")

# --- 3. VARIABLES ---

x = {}  # Primary assignment: driver d to route r, shift s, day t
b = {}  # Backup assignment
y = {}  # Whether driver is backup on day

# Initialize variables
for d in drivers:
    for day in days:
        y[d, day] = model.addVar(vtype=GRB.BINARY, name=f"Backup_{d}_{day}")
        for r in routes:
            for s in range(len(routes[r]['shifts_by_day'][day])):  # Corrected to use shifts_by_day
                x[d, day, r, s] = model.addVar(vtype=GRB.BINARY, name=f"x_{d}{day}{r}_{s}")
                b[d, day, r, s] = model.addVar(vtype=GRB.BINARY, name=f"b_{d}{day}{r}_{s}")

model.update()

# --- 4. CONSTRAINTS ---

# 4.1 Each shift must have exactly 1 primary driver
for day in days:
    for r in routes:
        for s in range(len(routes[r]['shifts_by_day'][day])):  # Corrected to use shifts_by_day
            model.addConstr(quicksum(x[d, day, r, s] for d in drivers) == 1, name=f"OneDriver_{day}{r}{s}")

# 4.2 Each driver works either a shift or is backup (not both)
for d in drivers:
    for day in days:
        model.addConstr(quicksum(x[d, day, r, s] for r in routes for s in range(len(routes[r]['shifts_by_day'][day]))) + y[d, day] == 1,
                        name=f"WorkOrBackup_{d}_{day}")

# 4.4 Backup shifts only for backup drivers
for d in drivers:
    for day in days:
        for r in routes:
            for s in range(len(routes[r]['shifts_by_day'][day])):  # Corrected to use shifts_by_day
                model.addConstr(b[d, day, r, s] <= y[d, day], name=f"BackupValid_{d}{day}{r}_{s}")

# 4.5 Weekly hours cap (48 hours, 8h per shift)
for d in drivers:
    model.addConstr(
        quicksum(x[d, day, r, s] * 8 for day in days for r in routes for s in range(len(routes[r]['shifts_by_day'][day]))) <= 48,
        name=f"MaxWeeklyHours_{d}"
    )

# 4.6 For every shift one backup
for day in days:
    for r in routes:
        for s in range(len(routes[r]['shifts_by_day'][day])):  # Corrected to use shifts_by_day
            model.addConstr(
                quicksum(b[d, day, r, s] for d in drivers) == 1,
                name=f"OneBackup_{day}{r}{s}"
            )
for d in drivers:
    for day in days:
        model.addConstr(
            quicksum(b[d, day, r, s] for r in routes for s in range(len(routes[r]['shifts_by_day'][day]))) <= 1,
            name=f"Max1BackupPerDay_{d}_{day}"
        )

# Max and min shifts constraints
max_shifts = model.addVar(vtype=GRB.INTEGER, name="MaxShifts")
min_shifts = model.addVar(vtype=GRB.INTEGER, name="MinShifts")

for d in drivers:
    total_shifts = quicksum(x[d, day, r, s] for day in days for r in routes for s in range(len(routes[r]['shifts_by_day'][day])))
    model.addConstr(max_shifts >= total_shifts, name=f"MaxShiftDriver_{d}")
    model.addConstr(min_shifts <= total_shifts, name=f"MinShiftDriver_{d}")

# --- 5. OBJECTIVE FUNCTION ---

# Primary driver contribution
primary_objective = quicksum(
    x[d, day, r, s] * (
        (7 - structured_preferences[d][day].get(r, {}).get(s, 7)) * 1000 +
        drivers[d]['performance'] * 500 +
        drivers[d]['experience_years'] * 100 +
        (1 if drivers[d]['gender'] == 'F' else 0) * 50
    )
    for d in drivers for day in days for r in routes for s in range(len(routes[r]['shifts_by_day'][day]))
)

# Backup driver contribution
backup_objective = 2 * quicksum(
    b[d, day, r, s] * (
        (7 - structured_preferences[d][day].get(r, {}).get(s, 7)) * 1000 +
        drivers[d]['performance'] * 500 +
        drivers[d]['experience_years'] * 100 +
        (1 if drivers[d]['gender'] == 'F' else 0) * 50
    )
    for d in drivers for day in days for r in routes for s in range(len(routes[r]['shifts_by_day'][day]))
)

# Fairness penalty
fairness_penalty = 400 * (max_shifts - min_shifts)

# Set the total objective
model.setObjective(primary_objective + backup_objective - fairness_penalty, GRB.MAXIMIZE)

# --- 6. SOLVE ---

try:
    model.optimize()
    if model.status == GRB.Status.OPTIMAL:
        print("\n✅ Optimal solution found.\n")
    elif model.status == GRB.Status.INFEASIBLE:
        print("\n Infeasible. Writing IIS to 'model.ilp'...")
        model.computeIIS()
        model.write("model.ilp")
    else:
        print(f"\n Status: {model.status}")
except Exception as e:
    print(f"\n Error during optimization: {e}")



# Print assignments
print("\n=== Primary Driver Assignments ===\n")
for day in days:
    print(f"\nDay {day}")
    for r in routes:
        for s in range(len(routes[r]['shifts_by_day'][day])):
            for d in drivers:
                if x[d, day, r, s].X > 0.5:
                    print(f"  Route {r}, Shift {s} → Driver {d} "
                          f"(Performance: {drivers[d]['performance']}, "
                          f"Experience: {drivers[d]['experience_years']}, "
                          f"Gender: {drivers[d]['gender']})")

print("\n=== Backup Driver Assignments ===\n")
for day in days:
    print(f"\nDay {day}")
    for r in routes:
        for s in range(len(routes[r]['shifts_by_day'][day])):
            for d in drivers:
                if b[d, day, r, s].X > 0.5:
                    print(f"  Route {r}, Shift {s} → Backup Driver {d} "
                          f"(Performance: {drivers[d]['performance']}, "
                          f"Experience: {drivers[d]['experience_years']}, "
                          f"Gender: {drivers[d]['gender']})")

# --- 7. BUILD RESULTS ---

results = {
    "assignments": {},
    "backups": {},
    "preferences_matrix": {}
}

for day in days:
    results["assignments"][f"Day_{day}"] = {}
    for r in routes:
        for s in range(len(routes[r]['shifts_by_day'][day])):
            assigned_drivers = [d for d in drivers if x[d, day, r, s].X > 0.5]
            results["assignments"][f"Day_{day}"][f"Route_{r}Shift{s}"] = []
            for d in assigned_drivers:
                results["assignments"][f"Day_{day}"][f"Route_{r}Shift{s}"].append({
                    "driver": d,
                    "preference": structured_preferences[d][day].get(r, {}).get(s, 7)
                })

    results["backups"][f"Day_{day}"] = {}
    for d in drivers:
        if y[d, day].X > 0.5:
            assigned_backups = []
            for r in routes:
                for s in range(len(routes[r]['shifts_by_day'][day])):
                    if b[d, day, r, s].X > 0.5:
                        assigned_backups.append({
                            "route": r,
                            "shift": s,
                            "preference": structured_preferences[d][day].get(r, {}).get(s, 7)
                        })
            if assigned_backups:
                results["backups"][f"Day_{day}"][f"Driver_{d}"] = assigned_backups

# --- 7.5 PREFERENCES MATRIX ---

for d in drivers:
    results["preferences_matrix"][f"Driver_{d}"] = {}
    for day in days:
        results["preferences_matrix"][f"Driver_{d}"][f"Day_{day}"] = {}
        for r in routes:
            results["preferences_matrix"][f"Driver_{d}"][f"Day_{day}"][f"Route_{r}"] = {}
            for s in range(len(routes[r]['shifts_by_day'][day])):
                results["preferences_matrix"][f"Driver_{d}"][f"Day_{day}"][f"Route_{r}"][f"Shift_{s}"] = structured_preferences[d][day].get(r, {}).get(s, 7)

# --- 8. SAVE TO FILE ---

with open("clean2_driver_schedule.json", "w") as f:
    json.dump(results, f, indent=4)

print("✅ Schedule created and saved as 'clean2_driver_schedule.json'.")
