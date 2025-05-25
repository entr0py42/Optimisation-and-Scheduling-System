import psycopg2
import json

def get_db_connection():
    # Update connection to use individual parameters
    connection = psycopg2.connect(
        host="localhost",  # Hostname
        port=5432,         # Port number
        user="postgres",   # Username
        password="5656",   # Password
        database="optimisation_db"  # Database name
    )
    return connection

def save_schedule_to_db(assignments, backups):
    connection = None
    try:
        connection = get_db_connection()
        cursor = connection.cursor()

        cursor.execute("TRUNCATE TABLE driverscheduleassignments RESTART IDENTITY;")
        #print("🧹 Existing schedule cleared.")


        insert_query = """
            INSERT INTO driverscheduleassignments
            (DriverId, Day, Route, Shift, IsBackup)
            VALUES (%s, %s, %s, %s, %s)
        """

        # Insert assignments (IsBackup = False)
        for day_key, routes in assignments.items():
            # day_key: "Day_1" -> store as "1" or "Day_1" if you want literal
            day = day_key.replace("Day_", "")

            for route_shift_key, drivers in routes.items():
                # route_shift_key example: "Route_1Shift1"
                route_part, shift_part = route_shift_key.split("Shift")
                route = int(route_part.replace("Route_", ""))
                shift = int(shift_part)

                for driver_info in drivers:
                    driver_id = driver_info["driver"]
                    # preference is available but your table does not store it
                    cursor.execute(insert_query, (driver_id, day, route, shift, False))

        # Insert backup assignments (IsBackup = True)
        for day_key, drivers_backups in backups.items():
            day = day_key.replace("Day_", "")

            for driver_key, backup_list in drivers_backups.items():
                # driver_key is like "Driver_101"
                driver_id = int(driver_key.replace("Driver_", ""))

                for backup in backup_list:
                    route = backup["route"]
                    shift = backup["shift"]
                    cursor.execute(insert_query, (driver_id, day, route, shift, True))

        connection.commit()
        print("✅ Schedule and backups saved successfully")

    except Exception as e:
        if connection:
            connection.rollback()
        print(f"❌ Error saving schedule: {e}")

    finally:
        if connection:
            cursor.close()
            connection.close()


def save():
    with open('clean2_driver_schedule.json') as f:
        data = json.load(f)
        save_schedule_to_db(data['assignments'], data['backups'])
