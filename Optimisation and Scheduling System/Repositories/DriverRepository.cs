using Npgsql;
using Optimisation_and_Scheduling_System.Models;
using Optimisation_and_Scheduling_System.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Configuration;

namespace Optimisation_and_Scheduling_System.Repositories
{
    public class DriverRepository : IDriverRepository
    {
        private readonly string _connectionString = ConfigurationManager.ConnectionStrings["PostgresConnection"].ConnectionString;

        // Create a new driver
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

        // Get all drivers from the database
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

        // Get the preferences of a specific driver
        public DriverPreference GetDriverPreferences(int driverId)
        {
            DriverPreference preferences = null;

            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                using (var cmd = new NpgsqlCommand(@"
                    SELECT ShiftId, PreferenceOrder
                    FROM DriverPreferences
                    WHERE DriverId = @driverId
                    ORDER BY PreferenceOrder", connection))
                {
                    cmd.Parameters.AddWithValue("driverId", driverId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        preferences = new DriverPreference { DriverId = driverId, ShiftPreferences = new List<int>() };
                        while (reader.Read())
                        {
                            preferences.ShiftPreferences.Add(reader.GetInt32(0)); // Add shift ID to the preferences list
                        }
                    }
                }
            }

            return preferences;
        }

        // Save or update the preferences for a specific driver
        public void SaveDriverPreferences(DriverPreference preference)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();

                // Delete existing preferences
                using (var cmd = new NpgsqlCommand("DELETE FROM DriverPreferences WHERE DriverId = @driverId", connection))
                {
                    cmd.Parameters.AddWithValue("driverId", preference.DriverId);
                    cmd.ExecuteNonQuery();
                }

                // Insert new preferences
                foreach (var shiftId in preference.ShiftPreferences)
                {
                    using (var cmd = new NpgsqlCommand(@"
                        INSERT INTO DriverPreferences (DriverId, ShiftId, PreferenceOrder)
                        VALUES (@driverId, @shiftId, @preferenceOrder)", connection))
                    {
                        cmd.Parameters.AddWithValue("driverId", preference.DriverId);
                        cmd.Parameters.AddWithValue("shiftId", shiftId);
                        cmd.Parameters.AddWithValue("preferenceOrder", preference.ShiftPreferences.IndexOf(shiftId) + 1); // Assign preference order
                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }

        // Get available line shifts (this assumes there is a LineShifts table)
        public List<LineShift> GetAvailableLineShifts()
        {
            var shifts = new List<LineShift>();

            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                using (var cmd = new NpgsqlCommand(@"
            SELECT ls.Id, ls.Day, ls.ShiftTimeStart, ls.ShiftTimeEnd, ls.IsDayShift, ls.LineId, l.Name
            FROM LineShift ls
            INNER JOIN Line l ON ls.LineId = l.Id", connection))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var shift = new LineShift
                            {
                                Id = reader.GetInt32(0),
                                Day = reader.GetInt32(1),
                                ShiftTimeStart = reader.GetTimeSpan(2),
                                ShiftTimeEnd = reader.GetTimeSpan(3),
                                IsDayShift = reader.GetBoolean(4),
                                LineId = reader.GetInt32(5),
                                Line = new Line
                                {
                                    Id = reader.GetInt32(5), // same as LineId
                                    Name = reader.GetString(6)
                                }
                            };
                            shifts.Add(shift);
                        }
                    }
                }
            }

            return shifts;
        }

    }
}
