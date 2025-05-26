using Npgsql;
using System;
using System.Configuration;

namespace Optimisation_and_Scheduling_System.DataDb
{
    public class DatabaseSeeder
    {
        private readonly string _connectionString = ConfigurationManager.ConnectionStrings["PostgresConnection"].ConnectionString;

        public void SeedData()
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Seed Drivers with performance and experience data
                        var seedDrivers = @"
                            INSERT INTO drivermodel (Id, Name, Gender, WorkerSince, Performance, ExperienceYears)
                            VALUES 
                                (101, 'John Doe', 'M', '2020-01-01', 92, 10),
                                (102, 'Jane Smith', 'F', '2019-06-15', 92, 5),
                                (103, 'Bob Johnson', 'M', '2021-03-10', 78, 8),
                                (104, 'Alice Brown', 'F', '2018-09-20', 78, 15),
                                (105, 'Charlie Wilson', 'M', '2017-12-01', 95, 20),
                                (106, 'Eva Martinez', 'F', '2019-03-15', 85, 9),
                                (107, 'David Lee', 'M', '2020-07-01', 80, 7),
                                (108, 'Sarah Johnson', 'F', '2018-11-30', 89, 11),
                                (109, 'Michael Chen', 'M', '2017-05-20', 90, 12),
                                (110, 'Lisa Anderson', 'F', '2021-01-10', 88, 6),
                                (111, 'James Wilson', 'M', '2019-08-15', 87, 8),
                                (112, 'Emily Davis', 'F', '2018-03-20', 91, 13),
                                (113, 'Robert Taylor', 'M', '2020-11-05', 83, 5),
                                (114, 'Maria Garcia', 'F', '2017-09-30', 94, 16),
                                (115, 'Thomas Moore', 'M', '2019-12-10', 86, 7)
                            ON CONFLICT (Id) DO NOTHING;";

                        // Seed Lines (Routes)
                        var seedLines = @"
                            INSERT INTO line (Id, Name, Garage)
                            VALUES 
                                (1, 'Route 1', 'İkitelli'),
                                (2, 'Route 2', 'İkitelli'),
                                (3, 'Route 3', 'İkitelli'),
                                (4, 'Route 4', 'İkitelli')
                            ON CONFLICT (Id) DO NOTHING;";

                        // Seed LineShifts for all 7 days
                        var seedLineShifts = @"
                            INSERT INTO lineshift (Id, LineId, ShiftTimeStart, ShiftTimeEnd, Day, IsDayShift)
                            SELECT 
                                ((LineId - 1) * 14) + ((Day - 1) * 2) + ShiftNum AS Id,
                                LineId,
                                CASE ShiftNum 
                                    WHEN 1 THEN 
                                        CASE LineId
                                            WHEN 1 THEN '06:00'::TIME
                                            WHEN 2 THEN '05:30'::TIME
                                            WHEN 3 THEN '07:00'::TIME
                                            WHEN 4 THEN '06:30'::TIME
                                        END
                                    WHEN 2 THEN 
                                        CASE LineId
                                            WHEN 1 THEN '14:00'::TIME
                                            WHEN 2 THEN '13:30'::TIME
                                            WHEN 3 THEN '15:00'::TIME
                                            WHEN 4 THEN '14:30'::TIME
                                        END
                                END AS ShiftTimeStart,
                                CASE ShiftNum 
                                    WHEN 1 THEN 
                                        CASE LineId
                                            WHEN 1 THEN '14:00'::TIME
                                            WHEN 2 THEN '13:30'::TIME
                                            WHEN 3 THEN '15:00'::TIME
                                            WHEN 4 THEN '14:30'::TIME
                                        END
                                    WHEN 2 THEN 
                                        CASE LineId
                                            WHEN 1 THEN '22:00'::TIME
                                            WHEN 2 THEN '21:30'::TIME
                                            WHEN 3 THEN '23:00'::TIME
                                            WHEN 4 THEN '22:30'::TIME
                                        END
                                END AS ShiftTimeEnd,
                                Day,
                                CASE ShiftNum WHEN 1 THEN true ELSE false END AS IsDayShift
                            FROM 
                                (SELECT * FROM generate_series(1, 4) AS LineId) l
                                CROSS JOIN (SELECT * FROM generate_series(1, 7) AS Day) d
                                CROSS JOIN (SELECT * FROM generate_series(1, 2) AS ShiftNum) s
                            ON CONFLICT (Id) DO NOTHING;";

                        // Seed Driver Preferences for all shifts and days (1 is most preferred, 7 is least preferred)
                        var seedPreferences = @"
                            WITH shift_series AS (
                                SELECT Id as ShiftId 
                                FROM lineshift
                            ),
                            driver_series AS (
                                SELECT Id as DriverId 
                                FROM drivermodel
                            )
                            INSERT INTO driverpreferences (DriverId, ShiftId, PreferenceOrder)
                            SELECT 
                                d.DriverId,
                                s.ShiftId,
                                1 + (random() * 6)::integer as PreferenceOrder
                            FROM 
                                driver_series d
                                CROSS JOIN shift_series s
                            ON CONFLICT (DriverId, ShiftId) DO NOTHING;";

                        // Seed default admin user
                        var seedUsers = @"
                            INSERT INTO usermodel (UserName, HashedPassword, UserRole)
                            VALUES ('admin', 'AQAAAAEAACcQAAAAEBpXbkP0WpKKJFO+fj5+tt8cX4qfVkZNDHzZxFo8RZnZCYU0WByhxXh4LUOtUYz0Yw==', 'Admin')
                            ON CONFLICT (UserName) DO NOTHING;";

                        using (var cmd = new NpgsqlCommand(seedDrivers, connection))
                            cmd.ExecuteNonQuery();

                        //using (var cmd = new NpgsqlCommand(seedLines, connection))
                        //    cmd.ExecuteNonQuery();

                        //using (var cmd = new NpgsqlCommand(seedLineShifts, connection))
                        //    cmd.ExecuteNonQuery();

                        //using (var cmd = new NpgsqlCommand(seedPreferences, connection))
                        //    cmd.ExecuteNonQuery();

                        using (var cmd = new NpgsqlCommand(seedUsers, connection))
                            cmd.ExecuteNonQuery();

                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        throw new Exception($"Error seeding database: {ex.Message}", ex);
                    }
                }
            }
        }
    }
} 