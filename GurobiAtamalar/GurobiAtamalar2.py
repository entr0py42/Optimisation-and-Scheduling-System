from gurobipy import Model, GRB, quicksum
import numpy as np
import json

# --- 1. DATA ---

drivers = {
    1: {'performance': 92, 'experience_years': 10, 'gender': 'M'},
    2: {'performance': 92, 'experience_years': 5, 'gender': 'F'},
    3: {'performance': 78, 'experience_years': 8, 'gender': 'M'},
    4: {'performance': 78, 'experience_years': 15, 'gender': 'F'},
    5: {'performance': 95, 'experience_years': 20, 'gender': 'M'},
    6: {'performance': 85, 'experience_years': 9, 'gender': 'M'},
    7: {'performance': 80, 'experience_years': 7, 'gender': 'F'},
    8: {'performance': 89, 'experience_years': 11, 'gender': 'M'},
    9: {'performance': 90, 'experience_years': 12, 'gender': 'F'},
    10: {'performance': 88, 'experience_years': 6, 'gender': 'M'},
    11: {'performance': 86, 'experience_years': 14, 'gender': 'F'},
    12: {'performance': 84, 'experience_years': 3, 'gender': 'M'},
    13: {'performance': 83, 'experience_years': 7, 'gender': 'F'},
    14: {'performance': 91, 'experience_years': 8, 'gender': 'M'},
    15: {'performance': 87, 'experience_years': 5, 'gender': 'F'},
    16: {'performance': 82, 'experience_years': 4, 'gender': 'F'},
    17: {'performance': 79, 'experience_years': 10, 'gender': 'F'},
    18: {'performance': 88, 'experience_years': 9, 'gender': 'M'},
    19: {'performance': 85, 'experience_years': 6, 'gender': 'F'},
    20: {'performance': 93, 'experience_years': 15, 'gender': 'M'}
}

routes = {
    1: {'garage': 'İkitelli', 'shift_times': ['06:00-14:00', '14:00-22:00']},
    2: {'garage': 'İkitelli', 'shift_times': ['05:30-13:30', '13:30-21:30']},
    3: {'garage': 'İkitelli', 'shift_times': ['07:00-15:00', '15:00-23:00']},
    4: {'garage': 'İkitelli', 'shift_times': ['06:30-14:30', '14:30-22:30']},
    5: {'garage': 'İkitelli', 'shift_times': ['07:30-15:30', '15:30-23:30']},
    6: {'garage': 'İkitelli', 'shift_times': ['06:00-14:00', '14:00-22:00']},
}

days = range(1, 8)  # 1-week planning

# Generate driver preferences randomly (1=most preferred, 6=least)
preferences = {
    d: {
        day: {
            r: {s: np.random.randint(1, 7) for s in range(len(routes[r]['shift_times']))} for r in routes
        } for day in days
    } for d in drivers
}

# --- 2. MODEL ---

model = Model("DriverScheduling")

# --- 3. VARIABLES ---

x = {}  # Primary assignment: driver d to route r, shift s, day t
b = {}  # Backup assignment
y = {}  # Whether driver is backup on day

for d in drivers:
    for day in days:
        y[d, day] = model.addVar(vtype=GRB.BINARY, name=f"Backup_{d}_{day}")
        for r in routes:
            for s in range(len(routes[r]['shift_times'])):
                x[d, day, r, s] = model.addVar(vtype=GRB.BINARY, name=f"x_{d}_{day}_{r}_{s}")
                b[d, day, r, s] = model.addVar(vtype=GRB.BINARY, name=f"b_{d}_{day}_{r}_{s}")

model.update()

# --- 4. CONSTRAINTS ---

# 4.1 Each shift must have exactly 1 primary driver
for day in days:
    for r in routes:
        for s in range(len(routes[r]['shift_times'])):
            model.addConstr(quicksum(x[d, day, r, s] for d in drivers) == 1, name=f"OneDriver_{day}_{r}_{s}")

# 4.2 Each driver works either a shift or is backup (not both)
for d in drivers:
    for day in days:
        model.addConstr(quicksum(x[d, day, r, s] for r in routes for s in range(len(routes[r]['shift_times']))) + y[d, day] == 1,
                        name=f"WorkOrBackup_{d}_{day}")

# 4.3 Backup drivers
for d in drivers:
    for day in days:
        model.addConstr(quicksum(b[d, day, r, s] for r in routes for s in range(len(routes[r]['shift_times']))) <= 2 * y[d, day],
                        name=f"BackupTwoShifts_{d}_{day}")

# 4.4 Backup shifts only for backup drivers
for d in drivers:
    for day in days:
        for r in routes:
            for s in range(len(routes[r]['shift_times'])):
                model.addConstr(b[d, day, r, s] <= y[d, day], name=f"BackupValid_{d}_{day}_{r}_{s}")

# 4.5 Weekly hours cap (48 hours, 8h per shift)
for d in drivers:
    model.addConstr(
        quicksum(x[d, day, r, s] * 8 for day in days for r in routes for s in range(len(routes[r]['shift_times']))) <= 48,
        name=f"MaxWeeklyHours_{d}"
    )

# --- 5. OBJECTIVE FUNCTION ---

model.setObjective(
    quicksum(
        x[d, day, r, s] * (
            -preferences[d][day][r][s] * 1000 +   # 1st priority: Preference (lower is better, so higher preference should give a higher score)
            drivers[d]['performance'] * 100 +       # 2nd priority: Performance (higher is better)
            drivers[d]['experience_years'] * 10 +   # 3rd priority: Experience (higher is better)
            (1 if drivers[d]['gender'] == 'F' else 0) * 50  # 4th priority: Gender (bonus for female drivers)
        )
        for d in drivers for day in days for r in routes for s in range(len(routes[r]['shift_times']))
    ),
    GRB.MAXIMIZE  # Maximizing the score (higher is better)
)



# --- 6. SOLVE ---

model.optimize()

# --- 7. RESULTS ---

results = {"assignments": {}, "backups": {}}

for day in days:
    results["assignments"][f"Day_{day}"] = {}
    for r in routes:
        for s in range(len(routes[r]['shift_times'])):
            assigned = [d for d in drivers if x[d, day, r, s].X > 0.5]
            results["assignments"][f"Day_{day}"][f"Route_{r}_Shift_{s}"] = assigned

    results["backups"][f"Day_{day}"] = {}
    for d in drivers:
        if y[d, day].X > 0.5:
            assigned_backups = [(r, s) for r in routes for s in range(len(routes[r]['shift_times'])) if b[d, day, r, s].X > 0.5]
            results["backups"][f"Day_{day}"][f"Driver_{d}"] = assigned_backups

# --- 8. SAVE TO FILE ---

with open("clean_driver_schedule.json", "w") as f:
    json.dump(results, f, indent=4)

print("✅ Schedule created and saved as 'clean_driver_schedule.json'.")
