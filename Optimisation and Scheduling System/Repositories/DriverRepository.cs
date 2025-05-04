using Npgsql;
using Optimisation_and_Scheduling_System.Models;
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
                    INSERT INTO DriverModel (Name, WorkerSince, DayTimeHours, NighttimeHours, WeekendHours, WeekendNightHours)
                    VALUES (@name, @since, 0, 0, 0, 0)", connection))
                {
                    cmd.Parameters.AddWithValue("name", name);
                    cmd.Parameters.AddWithValue("since", workerSince);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public List<DriverModel> GetAllDrivers()
        {
            List<DriverModel> drivers = new List<DriverModel>();
            
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                using (var cmd = new NpgsqlCommand(@"
                    SELECT Id, Name, DayTimeHours, NighttimeHours, WeekendHours, 
                           WeekendNightHours, WorkerSince 
                    FROM DriverModel
                    ORDER BY Name", connection))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var driver = new DriverModel
                            {
                                Id = reader.GetInt32(0),
                                Name = reader.GetString(1),
                                DayTimeHours = reader.GetInt32(2),
                                NighttimeHours = reader.GetInt32(3),
                                WeekendHours = reader.GetInt32(4),
                                WeekendNightHours = reader.GetInt32(5),
                                WorkerSince = reader.GetDateTime(6)
                            };
                            drivers.Add(driver);
                        }
                    }
                }
            }
            
            return drivers;
        }
    }
}