using Npgsql;
using Optimisation_and_Scheduling_System.Models;
using Optimisation_and_Scheduling_System.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace Optimisation_and_Scheduling_System.Repositories
{
    public class DriverRepository : IDriverRepository
    {
        private readonly string _connectionString = ConfigurationManager.ConnectionStrings["PostgresConnection"].ConnectionString;

        // Get all drivers from the database
        public List<DriverModel> GetAllDrivers()
        {
            List<DriverModel> drivers = new List<DriverModel>();

            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                using (var cmd = new NpgsqlCommand(@"
                    SELECT Id, Name, Gender, WorkerSince
                    FROM drivermodel
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
                                Gender = reader.GetString(2),
                                WorkerSince = reader.GetDateTime(3)
                            };
                            drivers.Add(driver);
                        }
                    }
                }
            }

            return drivers;
        }

        // Get the preferences of a specific driver
        public List<DriverPreference> GetDriverPreferences(int driverId)
        {
            var preferences = new List<DriverPreference>();

            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                using (var cmd = new NpgsqlCommand(@"
                    SELECT ShiftId, PreferenceOrder
                    FROM driverpreferences
                    WHERE DriverId = @driverId
                    ORDER BY PreferenceOrder", connection))
                {
                    cmd.Parameters.AddWithValue("driverId", driverId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            preferences.Add(new DriverPreference
                            {
                                DriverId = driverId,
                                ShiftId = reader.GetInt32(0),
                                PreferenceOrder = reader.GetInt32(1)
                            });
                        }
                    }
                }
            }

            return preferences;
        }

        // Save or update the preferences for a specific driver
        public void SaveDriverPreferences(List<DriverPreference> preferences)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Delete existing preferences
                        using (var cmd = new NpgsqlCommand("DELETE FROM driverpreferences WHERE DriverId = @driverId", connection))
                        {
                            cmd.Parameters.AddWithValue("driverId", preferences.First().DriverId);
                            cmd.ExecuteNonQuery();
                        }

                        // Insert new preferences
                        foreach (var pref in preferences)
                        {
                            using (var cmd = new NpgsqlCommand(@"
                                INSERT INTO driverpreferences (DriverId, ShiftId, PreferenceOrder)
                                VALUES (@driverId, @shiftId, @preferenceOrder)", connection))
                            {
                                cmd.Parameters.AddWithValue("driverId", pref.DriverId);
                                cmd.Parameters.AddWithValue("shiftId", pref.ShiftId);
                                cmd.Parameters.AddWithValue("preferenceOrder", pref.PreferenceOrder);
                                cmd.ExecuteNonQuery();
                            }
                        }

                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
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
                    FROM lineshift ls
                    INNER JOIN line l ON ls.LineId = l.Id", connection))
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
                                    Id = reader.GetInt32(5),
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

        public List<DriverScheduleAssignment> GetDriverScheduleAssignments(int driverId)
        {
            var assignments = new List<DriverScheduleAssignment>();

            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                using (var cmd = new NpgsqlCommand(@"
            SELECT a.Id, a.DriverId, a.Day, r.Name AS RouteName, a.Shift, a.IsBackup
            FROM driverscheduleassignments a
            INNER JOIN line r ON a.Route = r.Id
            WHERE a.DriverId = @driverId
            ORDER BY a.Day, a.Shift", connection))
                {
                    cmd.Parameters.AddWithValue("driverId", driverId);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var assignment = new DriverScheduleAssignment
                            {
                                Id = reader.GetInt32(0),
                                DriverId = reader.GetInt32(1),
                                Day = reader.GetString(2),
                                RouteName = reader.GetString(3), // New field
                                Shift = reader.GetInt32(4),
                                IsBackup = reader.GetBoolean(5)
                            };
                            assignments.Add(assignment);
                        }
                    }
                }
            }

            return assignments;
        }


    }
}
