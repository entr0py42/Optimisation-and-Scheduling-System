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
        password="5656",   # Password
        database="optimisation_db"  # Database name
    )
    return connection


# Fetch drivers from the database
def get_drivers():
    # Establish connection to DB
    connection = get_db_connection()
    cursor = connection.cursor()

    # Query for drivers from the database
    cursor.execute("SELECT * FROM DriverModel;")
    rows = cursor.fetchall()

    # Initialize drivers dictionary
    drivers = {}

    for row in rows:
        driver_id = row[0]  # Assuming the driver ID is the first column
        name = row[1]  # Assuming name is the second column
        gender = row[2]  # Assuming gender is the third column
        daytime_hours = row[3]  # Adjust if needed
        nighttime_hours = row[4]
        weekend_hours = row[5]
        weekend_night_hours = row[6]
        worker_since = row[7]  # Assuming the 'worker_since' field is a timestamp

        # If gender is None, default to 'M'
        if gender is None:
            gender = 'M'

        # Calculate experience in years
        current_date = datetime.datetime.now()
        experience_years = (current_date - worker_since).days // 365

        # You can also add logic to fetch performance dynamically, for now assuming it's part of the DB
        performance = 85  # Or another logic to fetch performance dynamically from a table or calculation

        drivers[driver_id] = {
            'name': name,
            'gender': gender,
            'daytime_hours': daytime_hours,
            'nighttime_hours': nighttime_hours,
            'weekend_hours': weekend_hours,
            'weekend_night_hours': weekend_night_hours,
            'worker_since': worker_since,
            'performance': performance,  # Adjust to fetch dynamically
            'experience_years': experience_years
        }

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

