import json
import psycopg2

def get_db_connection():
    connection = psycopg2.connect(
        host="localhost",
        port=5432,
        user="postgres",
        password="5656",
        database="optimisation_db"
    )
    return connection

# Map Day_X to weekday names
day_mapping = {
    "Day_1": "Monday",
    "Day_2": "Tuesday",
    "Day_3": "Wednesday",
    "Day_4": "Thursday",
    "Day_5": "Friday",
    "Day_6": "Saturday",
    "Day_7": "Sunday"
}

def save():
    json_file = "optimized_schedule.json"
    with open(json_file, 'r') as f:
        data = json.load(f)

    assignments = data.get("assignments", {})
    backups = data.get("backups", {})

    conn = get_db_connection()
    cur = conn.cursor()

    insert_query = """
        INSERT INTO public.driverscheduleassignments (driverid, day, route, shift, isbackup)
        VALUES (%s, %s, %s, %s, %s)
    """

    try:
        cur.execute("DELETE FROM public.driverscheduleassignments")
        
        # Insert regular assignments (isbackup = False)
        for day_key, routes in assignments.items():
            day = day_mapping.get(day_key, day_key)  # Default to original if no mapping
            for route_shift_key, details in routes.items():
                driver_id = details.get("driver_id")
                parts = route_shift_key.split('_')
                route = int(parts[1])
                shift = int(parts[3])
                cur.execute(insert_query, (driver_id, day, route, shift, False))

        # Insert backup assignments (isbackup = True)
        for day_key, drivers in backups.items():
            day = day_mapping.get(day_key, day_key)
            for driver_key, backup_info in drivers.items():
                driver_id = int(driver_key.split('_')[1])
                backup_assignments = backup_info.get("backup_assignments", [])

                for backup in backup_assignments:
                    route = backup.get("route")
                    shift = backup.get("shift_id")
                    cur.execute(insert_query, (driver_id, day, route, shift, True))

        conn.commit()
        print("Data inserted successfully.")

    except Exception as e:
        conn.rollback()
        print("Error inserting data:", e)

    finally:
        cur.close()
        conn.close()
