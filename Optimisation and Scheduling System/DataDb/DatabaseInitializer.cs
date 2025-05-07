using Npgsql;
using System.Configuration;

namespace Optimisation_and_Scheduling_System.DataDb
{
	public class DatabaseInitializer
	{
        public static void InitializeDatabase()
        {
            string connStr = ConfigurationManager.ConnectionStrings["PostgresConnection"].ConnectionString;

            using (var connection = new NpgsqlConnection(connStr))
            {
                connection.Open();

                // Create Driver table
                string createDriver = @"
            CREATE TABLE IF NOT EXISTS DriverModel (
                Id SERIAL PRIMARY KEY,
                Name VARCHAR(255),
                Gender VARCHAR(1),
                DayTimeHours INTEGER,
                NighttimeHours INTEGER,
                WeekendHours INTEGER,
                WeekendNightHours INTEGER,
                WorkerSince TIMESTAMP
            );";

                // Create User table
                string createUser = @"
            CREATE TABLE IF NOT EXISTS UserModel (
                UserName VARCHAR(255) PRIMARY KEY,
                HashedPassword TEXT NOT NULL,
                UserRole VARCHAR(50) NOT NULL
            );";



                // Create the Line table
                var createLineTable = @"
            CREATE TABLE IF NOT EXISTS Line (
                Id SERIAL PRIMARY KEY,
                Name VARCHAR(255) NOT NULL,
                Garage VARCHAR(255)
            );";

                // Create the LineShift table
                var createLineShiftTable = @"
            CREATE TABLE IF NOT EXISTS LineShift (
                Id SERIAL PRIMARY KEY,
                LineId INT NOT NULL,
                ShiftTimeStart TIME NOT NULL, 
                ShiftTimeEnd TIME NOT NULL,
                Day INT NOT NULL,
                IsDayShift BOOLEAN NOT NULL,
                FOREIGN KEY (LineId) REFERENCES Line(Id) ON DELETE CASCADE
            );";


                // Create DriverPreference table
                var createDriverPreferenceTable = @"
            CREATE TABLE IF NOT EXISTS DriverPreferences (
                DriverId INT NOT NULL,
                ShiftId INT NOT NULL,
                PreferenceOrder INT NOT NULL,
                PRIMARY KEY (DriverId, ShiftId),
                FOREIGN KEY (DriverId) REFERENCES DriverModel(Id) ON DELETE CASCADE,
                FOREIGN KEY (ShiftId) REFERENCES LineShift(Id) ON DELETE CASCADE
            );";





                using (var cmd = new NpgsqlCommand(createDriver, connection))
                    cmd.ExecuteNonQuery();

                using (var cmd = new NpgsqlCommand(createUser, connection))
                    cmd.ExecuteNonQuery();


                using (var cmd = new NpgsqlCommand(createLineTable, connection))
                    cmd.ExecuteNonQuery();
                

                using (var cmd = new NpgsqlCommand(createLineShiftTable, connection))
                    cmd.ExecuteNonQuery();

                using (var cmd = new NpgsqlCommand(createDriverPreferenceTable, connection))
                    cmd.ExecuteNonQuery();



            }
        }
    }
}