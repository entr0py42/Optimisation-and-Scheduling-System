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
                DayTimeHours INTEGER,
                NighttimeHours INTEGER,
                WeekendHours INTEGER,
                WeekendNightHours INTEGER,
                WorkerSince TIMESTAMP,
                PaidOffDays INTEGER,
                UnpaidOffDays INTEGER
            );";

                // Create User table
                string createUser = @"
            CREATE TABLE IF NOT EXISTS UserModel (
                UserName VARCHAR(255) PRIMARY KEY,
                HashedPassword TEXT NOT NULL,
                UserRole VARCHAR(50) NOT NULL
            );";


                // Off Days Requests tablosu
                string createOffDaysRequests = @"
                CREATE TABLE IF NOT EXISTS OffDaysRequests (
                    ReqId SERIAL PRIMARY KEY,
                    DriverId INTEGER,
                    StartDate DATE NOT NULL,
                    EndDate DATE NOT NULL,
                    Reason TEXT NOT NULL,
                    Status INTEGER DEFAULT -1,
                    CONSTRAINT fk_driver FOREIGN KEY (DriverId)
                        REFERENCES DriverModel (Id)
                        ON DELETE CASCADE,
                    CONSTRAINT status_check CHECK (Status IN (0, 1, -1))
                );";




                using (var cmd = new NpgsqlCommand(createDriver, connection))
                    cmd.ExecuteNonQuery();

                using (var cmd = new NpgsqlCommand(createUser, connection))
                    cmd.ExecuteNonQuery();

                using (var cmd = new NpgsqlCommand(createOffDaysRequests, connection))
                    cmd.ExecuteNonQuery();
            }
        }
    }
}