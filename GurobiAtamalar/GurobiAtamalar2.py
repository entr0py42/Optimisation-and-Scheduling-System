from gurobipy import Model, GRB, quicksum
import numpy as np

# **1. Drivers Data**
# **1. Drivers Data**
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
    16: {'performance': 82, 'experience_years': 4, 'gender': 'M'},
    17: {'performance': 79, 'experience_years': 10, 'gender': 'F'},
    18: {'performance': 88, 'experience_years': 9, 'gender': 'M'},
    19: {'performance': 85, 'experience_years': 6, 'gender': 'F'},
    20: {'performance': 93, 'experience_years': 15, 'gender': 'M'}
}
# **2. Routes Data (Includes Shift Times)**
routes = {
    1: {'garage': 'İkitelli',  'shift_times': ['06:00-14:00', '14:00-22:00']},
    2: {'garage': 'İkitelli', 'shift_times': ['05:30-13:30', '13:30-21:30']},
    3: {'garage': 'İkitelli',  'shift_times': ['07:00-15:00', '15:00-23:00']},
    4: {'garage': 'İkitelli', 'shift_times': ['06:30-14:30', '14:30-22:30']},
    5: {'garage': 'İkitelli', 'shift_times': ['07:30-15:30', '15:30-23:30']},
    6: {'garage': 'İkitelli', 'shift_times': ['06:00-14:00', '14:00-22:00']},
}

weeks = range(1, 5)  # 1 aylık planlama
shifts_per_day = 2

# **3. Driver Preferences (Updated for 20 Drivers with Shift Info)**
pref_matrix = {
    d: {
        w: {
            r: {0: np.random.randint(1, 7), 1: np.random.randint(1, 7)} for r in routes
        } for w in weeks
    } for d in drivers.keys()
}
# **4. Model Definition**
m = Model("Driver Scheduling")

# **5. Decision Variables**
x = {}  # Assignments
y = {}  # Backup Assignments
b = {}  # Backup route and shift assignment
for d in drivers:
    for w in weeks:
        for r in routes:
            for s in range(shifts_per_day):
                x[d, w, r, s] = m.addVar(vtype=GRB.BINARY, name=f"x_{d}_{w}_{r}_{s}")
                b[d, w, r, s] = m.addVar(vtype=GRB.BINARY, name=f"b_{d}_{w}_{r}_{s}")
        y[d, w] = m.addVar(vtype=GRB.BINARY, name=f"y_{d}_{w}")

m.update()

#kısıtlar

for w in weeks:
    for r in routes:
        for s in range(shifts_per_day):
            sorted_drivers = sorted(drivers.keys(), key=lambda d: (-drivers[d]['performance'], -drivers[d]['experience_years'], drivers[d]['gender'] == 'F'))
            
            for i in range(len(sorted_drivers) - 1):
                d1 = sorted_drivers[i]
                d2 = sorted_drivers[i + 1]
                
                # Enforce that if a higher-ranked driver is available, they must be assigned first
                m.addConstr(
                    x[d2, w, r, s] <= (1 - x[d1, w, r, s]),
                    name=f"higher_priority_first_{d1}_{d2}_{w}_{r}_{s}"
                )


# **6. Constraints**
# Ensure primary assignments strictly prioritize high-performance drivers first
for w in weeks:
    for r in routes:
        for s in range(shifts_per_day):
            sorted_drivers = sorted(drivers.keys(), key=lambda d: (-drivers[d]['performance'], -drivers[d]['experience_years'], drivers[d]['gender'] == 'F'))
            for i, d in enumerate(sorted_drivers):
                m.addConstr(
                    quicksum(x[d, w, r, s] for r in routes for s in range(shifts_per_day)) >= 1 - y[d, w],
                    name=f"ensure_high_perf_primary_{d}_{w}"
                )

# 6.1. Assign primary shifts strictly based on performance, preferences, seniority, and gender
for w in weeks:
    for r in routes:
        for s in range(shifts_per_day):
            sorted_drivers = sorted(drivers.keys(), key=lambda d: (-drivers[d]['performance'], -drivers[d]['experience_years'], drivers[d]['gender'] == 'F'))
            m.addConstr(
                quicksum(x[d, w, r, s] for d in sorted_drivers) == 1,  # Exactly one driver per shift
                name=f"ensure_one_driver_{w}_{r}_{s}"
            )
            for i in range(len(sorted_drivers) - 1):
                d1 = sorted_drivers[i]
                d2 = sorted_drivers[i + 1]
                m.addConstr(
                    x[d2, w, r, s] <= (1 - x[d1, w, r, s]),  # Ensures higher-ranked drivers get assigned first
                    name=f"higher_priority_first_{d1}_{d2}_{w}_{r}_{s}"
                )

# 6.2. Backup assignments should follow the same priority order but only include drivers who were NOT assigned
for w in weeks:
    for d in drivers:
        m.addConstr(
            quicksum(x[d, w, r, s] for r in routes for s in range(shifts_per_day)) + y[d, w] == 1,
            name=f"assign_or_backup_{d}_{w}"
        )



# 6.1. Each shift must be assigned exactly one driver
for w in weeks:
    for r in routes:
        for s in range(shifts_per_day):
            m.addConstr(quicksum(x[d, w, r, s] for d in drivers) == 1, name=f"shift_filled_{w}_{r}_{s}")


# 6.2. Ensure each backup driver is assigned to exactly two shifts based on their preferences
for w in weeks:
    for d in drivers:
        m.addConstr(
            quicksum(b[d, w, r, s] for r in routes for s in range(shifts_per_day)) == 2 * y[d, w],
            name=f"backup_two_shifts_{d}_{w}"
        )

# 6.2. Each driver works max 48 hours per week (assuming 8-hour shifts)
for d in drivers:
    for w in weeks:
        m.addConstr(quicksum(x[d, w, r, s] * 8 for r in routes for s in range(shifts_per_day)) <= 48, name=f"max_hours_{d}_{w}")



# 6.4. Assignments prioritize high performance, seniority, and female drivers
priority_scores = {
    d: (drivers[d]['performance'] * 10000 + drivers[d]['experience_years'] * 100 + (1 if drivers[d]['gender'] == 'F' else 0))
    for d in drivers
}



# 6.3. Ensure backup drivers are assigned dynamically and assigned backup routes and shifts
for w in weeks:
    for d in drivers:
        m.addConstr(
            quicksum(x[d, w, r, s] for r in routes for s in range(shifts_per_day)) + y[d, w] == 1,
            name=f"assign_or_backup_{d}_{w}"
        )
        for r in routes:
            for s in range(shifts_per_day):
                m.addConstr(b[d, w, r, s] <= y[d, w], name=f"backup_assignment_{d}_{w}_{r}_{s}")


# amaç fonksiyonu
m.setObjective(
    quicksum(x[d, w, r, s] * (drivers[d]['performance'] * 100000 + drivers[d]['experience_years'] * 1000 + (1 if drivers[d]['gender'] == 'F' else 0) * 500) for d in drivers for w in weeks for r in routes for s in range(shifts_per_day)) -
    quicksum(y[d, w] * (drivers[d]['performance'] * 50000) for d in drivers for w in weeks),  # Penalize backup for high-performance drivers
    GRB.MAXIMIZE
)




# **8. Solve Model**
m.optimize()


# **9. Print Results**
print("\n=== Aylık Atama Sonuçları ===")
for w in weeks:
    print(f"\nHafta {w}")
    for r in routes:
        for s in range(shifts_per_day):
            assigned_drivers = [d for d in drivers if x[d, w, r, s].x > 0.5]
            print(f" Hat {r}, Shift {s}: {', '.join(f'Sürücü {d}' for d in assigned_drivers)}")
    backup_drivers = [d for d in drivers if y[d, w].x > 0.5]
    if backup_drivers:
        print("\nYEDEK Sürücüler:")
        for d in backup_drivers:
            backup_assignments = [(r, s) for r in routes for s in range(shifts_per_day) if b[d, w, r, s].x > 0.5]
            if backup_assignments:
                for r, s in backup_assignments:
                    print(f" Sürücü {d} → YEDEK (Hat {r}, Shift {s})")
            else:
                print(f" Sürücü {d} → YEDEK (Atanacak hat bulunamadı)")





import json

# Create a dictionary to store results
results = {
    "assignments": {},
    "backup_drivers": {}
}

# Store shift assignments
for w in weeks:
    results["assignments"][f"Week_{w}"] = {}
    for r in routes:
        results["assignments"][f"Week_{w}"][f"Route_{r}"] = {}
        for s in range(shifts_per_day):
            assigned_drivers = [d for d in drivers if x[d, w, r, s].x > 0.5]
            results["assignments"][f"Week_{w}"][f"Route_{r}"][f"Shift_{s}"] = assigned_drivers

# Store backup assignments
for w in weeks:
    backup_drivers = [d for d in drivers if y[d, w].x > 0.5]
    results["backup_drivers"][f"Week_{w}"] = {}
    for d in backup_drivers:
        backup_assignments = [(r, s) for r in routes for s in range(shifts_per_day) if b[d, w, r, s].x > 0.5]
        results["backup_drivers"][f"Week_{w}"][f"Driver_{d}"] = backup_assignments

# Define the file path
json_file_path = "driver_schedule_results.json"

# Save to JSON file
with open(json_file_path, "w") as json_file:
    json.dump(results, json_file, indent=4)

print(f"JSON file '{json_file_path}' has been created successfully!")



