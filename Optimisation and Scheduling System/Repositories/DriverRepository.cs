using Npgsql;
using Optimisation_and_Scheduling_System.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace Optimisation_and_Scheduling_System.Repositories
{
	public class DriverRepository : IDriverRepository
    {
        private readonly string _connectionString = ConfigurationManager.ConnectionStrings["PostgresConnection"].ConnectionString;
        public void CreateDriver(string name, DateTime workerSince)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                using (var cmd = new NpgsqlCommand(@"
                    INSERT INTO DriverModel (Name, WorkerSince, DayTimeHours, NighttimeHours, WeekendHours, WeekendNightHours, PaidOffDays, UnpaidOffDays)
                    VALUES (@name, @since, 0, 0, 0, 0, 0, 0)", connection))
                {
                    cmd.Parameters.AddWithValue("name", name);
                    cmd.Parameters.AddWithValue("since", workerSince);
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}