using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Optimisation_and_Scheduling_System.Models
{
    public class OptimizationResultModel
    {
        [JsonProperty("assignments")]
        public Dictionary<string, Dictionary<string, AssignmentDetails>> Assignments { get; set; }
        
        [JsonProperty("backups")]
        public Dictionary<string, Dictionary<string, BackupDetails>> Backups { get; set; }

        [JsonProperty("schedule_info")]
        public ScheduleInfo ScheduleInfo { get; set; }
        
        [JsonIgnore]
        public DateTime CreatedAt { get; set; }
        
        [JsonIgnore]
        public string Status { get; set; }

        public OptimizationResultModel()
        {
            Assignments = new Dictionary<string, Dictionary<string, AssignmentDetails>>();
            Backups = new Dictionary<string, Dictionary<string, BackupDetails>>();
            ScheduleInfo = new ScheduleInfo();
            CreatedAt = DateTime.Now;
            Status = "Pending";
        }
    }

    public class AssignmentDetails
    {
        [JsonProperty("driver_id")]
        public int DriverId { get; set; }

        [JsonProperty("driver_name")]
        public string DriverName { get; set; }

        [JsonProperty("preference")]
        public int Preference { get; set; }

        [JsonProperty("performance")]
        public int Performance { get; set; }

        [JsonProperty("experience")]
        public int Experience { get; set; }
    }

    public class BackupDetails
    {
        [JsonProperty("driver_name")]
        public string DriverName { get; set; }

        [JsonProperty("backup_assignments")]
        public List<BackupAssignment> BackupAssignments { get; set; }
    }

    public class BackupAssignment
    {
        [JsonProperty("route")]
        public int Route { get; set; }

        [JsonProperty("shift_id")]
        public int ShiftId { get; set; }

        [JsonProperty("preference")]
        public int Preference { get; set; }
    }

    public class ScheduleInfo
    {
        [JsonProperty("total_drivers")]
        public int TotalDrivers { get; set; }

        [JsonProperty("total_routes")]
        public int TotalRoutes { get; set; }

        [JsonProperty("days_planned")]
        public int DaysPlanned { get; set; }
    }
} 