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

                using (var cmd = new NpgsqlCommand(createDriver, connection))
                    cmd.ExecuteNonQuery();

                using (var cmd = new NpgsqlCommand(createUser, connection))
                    cmd.ExecuteNonQuery();
            }
        }
    }
}