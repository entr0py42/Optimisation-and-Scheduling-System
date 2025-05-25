using Npgsql;
using System;
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
                CREATE TABLE IF NOT EXISTS drivermodel (
                    Id INTEGER PRIMARY KEY,
                    Name VARCHAR(255),
                    Gender VARCHAR(1),
                    WorkerSince TIMESTAMP,
                    Performance INTEGER,
                    ExperienceYears INTEGER
                );

                CREATE SEQUENCE IF NOT EXISTS drivermodel_id_seq START WITH 1000;
                ALTER TABLE drivermodel ALTER COLUMN Id SET DEFAULT nextval('drivermodel_id_seq');";

                // Create User table
                string createUser = @"
                CREATE TABLE IF NOT EXISTS usermodel (
                    UserName VARCHAR(255) PRIMARY KEY,
                    HashedPassword TEXT NOT NULL,
                    UserRole VARCHAR(50) NOT NULL
                );";

                // Create the Line table
                var createLineTable = @"
                CREATE TABLE IF NOT EXISTS line (
                    Id SERIAL PRIMARY KEY,
                    Name VARCHAR(255) NOT NULL,
                    Garage VARCHAR(255)
                );";

                // Create the LineShift table
                var createLineShiftTable = @"
                CREATE TABLE IF NOT EXISTS lineshift (
                    Id SERIAL PRIMARY KEY,
                    LineId INT NOT NULL,
                    ShiftTimeStart TIME NOT NULL, 
                    ShiftTimeEnd TIME NOT NULL,
                    Day INT NOT NULL,
                    IsDayShift BOOLEAN NOT NULL,
                    FOREIGN KEY (LineId) REFERENCES line(Id) ON DELETE CASCADE
                );";

                // Create DriverPreference table
                var createDriverPreferenceTable = @"
                CREATE TABLE IF NOT EXISTS driverpreferences (
                    DriverId INT NOT NULL,
                    ShiftId INT NOT NULL,
                    PreferenceOrder INT NOT NULL,
                    PRIMARY KEY (DriverId, ShiftId),
                    FOREIGN KEY (DriverId) REFERENCES drivermodel(Id) ON DELETE CASCADE,
                    FOREIGN KEY (ShiftId) REFERENCES lineshift(Id) ON DELETE CASCADE
                );";


                var createDriverScheduleAssignments = @"
                CREATE TABLE IF NOT EXISTS driverscheduleassignments (
                    Id SERIAL PRIMARY KEY,
                    DriverId INT NOT NULL,
                    Day VARCHAR(20) NOT NULL,
                    Route INT NOT NULL,
                    Shift INT NOT NULL,
                    IsBackup BOOLEAN NOT NULL DEFAULT FALSE,
                    FOREIGN KEY (DriverId) REFERENCES drivermodel(Id) ON DELETE CASCADE
                );";






                try
                {
                    using (var transaction = connection.BeginTransaction())
                    {
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

                        using (var cmd = new NpgsqlCommand(createDriverScheduleAssignments, connection))
                            cmd.ExecuteNonQuery();


                        transaction.Commit();

                        // After creating tables, seed the data
                        var seeder = new DatabaseSeeder();
                        seeder.SeedData();
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception($"Error initializing database: {ex.Message}", ex);
                }
            }
        }
    }
}