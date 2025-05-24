import psycopg2
import json
import datetime
from configparser import ConfigParser
from collections import defaultdict


# Fetch connection string from the configuration file
def get_db_connection():
    # Update connection to use individual parameters
    connection = psycopg2.connect(
        host="localhost",  # Hostname
        port=5432,         # Port number
        user="postgres",   # Username
        password="19391945",   # Password
        database="optimisation_db"  # Database name
    )
    return connection


# Fetch drivers from the database
def get_drivers():
    # Establish connection to DB
    connection = get_db_connection()
    cursor = connection.cursor()

    # Query for drivers from the database
    cursor.execute("SELECT Id, Name, Gender, WorkerSince, Performance, ExperienceYears FROM DriverModel;")
    rows = cursor.fetchall()

    # Initialize drivers dictionary
    drivers = {}

    for row in rows:
        driver_id = row[0]  
        name = row[1]  
        gender = row[2]  
        worker_since = row[3]
        performance = row[4] if row[4] is not None else 85  # Default performance if NULL
        experience_years = row[5] if row[5] is not None else 0  # Default experience if NULL

        # If gender is None, default to 'M'
        if gender is None:
            gender = 'M'

        drivers[driver_id] = {
            'name': name,
            'gender': gender,
            'worker_since': worker_since,
            'performance': performance,
            'experience_years': experience_years
        }

    cursor.close()
    connection.close()

    return drivers


# Fetch routes from the database
def get_routes():
    connection = get_db_connection()
    cursor = connection.cursor()

    cursor.execute("SELECT Id, Name, Garage FROM Line;")
    rows = cursor.fetchall()

    routes = {}
    for row in rows:
        route_id, name, garage = row
        routes[route_id] = {
            'name': name,
            'garage': garage
        }

    cursor.close()
    connection.close()

    return routes


# Fetch shift times for routes from the database
def get_shifts():
    connection = get_db_connection()
    cursor = connection.cursor()

    cursor.execute("SELECT Id, LineId, ShiftTimeStart, ShiftTimeEnd, Day, IsDayShift FROM LineShift;")
    rows = cursor.fetchall()

    shifts = {}
    for row in rows:
        shift_id, line_id, shift_start, shift_end, day, is_day_shift = row
        if line_id not in shifts:
            shifts[line_id] = []
        shifts[line_id].append({
            'shift_id': shift_id,
            'start': shift_start,
            'end': shift_end,
            'day': day,
            'is_day_shift': is_day_shift
        })

    cursor.close()
    connection.close()

    return shifts


# Fetch driver preferences from the database
def get_preferences():
    connection = get_db_connection()
    cursor = connection.cursor()

    cursor.execute("SELECT DriverId, ShiftId, PreferenceOrder FROM DriverPreferences;")
    rows = cursor.fetchall()

    preferences = {}
    for row in rows:
        driver_id, shift_id, preference_order = row
        if driver_id not in preferences:
            preferences[driver_id] = {}
        preferences[driver_id][shift_id] = preference_order

    cursor.close()
    connection.close()

    return preferences


# Step 1: Load shifts from DB
def load_route_shifts():
    connection = get_db_connection()
    cursor = connection.cursor()

    cursor.execute("SELECT id, lineid, day FROM lineshift;")
    shifts = cursor.fetchall()

    routes = {}  # structure: {route_id: {day: [shift_id, shift_id, ...]}}
    for shift_id, line_id, day in shifts:
        if line_id not in routes:
            routes[line_id] = {}
        if day not in routes[line_id]:
            routes[line_id][day] = []
        routes[line_id][day].append(shift_id)

    cursor.close()
    connection.close()
    return routes

def get_routes_by_day():
    connection = get_db_connection()
    cursor = connection.cursor()

    query = """
        SELECT 
            l.id AS line_id,
            l.garage,
            s.id AS shift_id,
            s.day
        FROM line l
        JOIN lineshift s ON l.id = s.lineid;
    """

    cursor.execute(query)
    rows = cursor.fetchall()

    # routes[line_id] = {'garage': ..., 'shifts_by_day': {day: [shift_ids...]}}
    routes = {}

    for line_id, garage, shift_id, day in rows:
        if line_id not in routes:
            routes[line_id] = {
                'garage': garage,
                'shifts_by_day': defaultdict(list)
            }
        routes[line_id]['shifts_by_day'][day].append(shift_id)

    # Convert defaultdicts to regular dicts
    for r in routes:
        routes[r]['shifts_by_day'] = dict(routes[r]['shifts_by_day'])

    cursor.close()
    connection.close()

    return routes


# Add test section at the end of the file
if __name__ == "__main__":
    try:
        print("Testing database connections and data retrieval...")
        
        print("\nTesting drivers retrieval...")
        drivers = get_drivers()
        print(f"Retrieved {len(drivers)} drivers")
        
        print("\nTesting routes retrieval...")
        routes = get_routes()
        print(f"Retrieved {len(routes)} routes")
        
        print("\nTesting shifts retrieval...")
        shifts = get_shifts()
        print(f"Retrieved {len(shifts)} routes with shifts")
        
        print("\nTesting preferences retrieval...")
        preferences = get_preferences()
        print(f"Retrieved preferences for {len(preferences)} drivers")
        
        print("\nTesting routes by day retrieval...")
        routes_by_day = get_routes_by_day()
        print(f"Retrieved {len(routes_by_day)} routes with day-wise shifts")
        
        print("\n✅ All database operations completed successfully!")
        
    except Exception as e:
        print(f"\n❌ Error during database operations: {str(e)}")

