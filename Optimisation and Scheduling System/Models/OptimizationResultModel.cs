using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Optimisation_and_Scheduling_System.Models
{
    public class OptimizationResultModel
    {
        [JsonProperty("assignments")]
        public Dictionary<string, Dictionary<string, List<int>>> Assignments { get; set; }
        
        [JsonProperty("backups")]
        public Dictionary<string, Dictionary<string, List<List<int>>>> Backups { get; set; }
        
        [JsonIgnore]
        public DateTime CreatedAt { get; set; }
        
        [JsonIgnore]
        public string Status { get; set; }

        public OptimizationResultModel()
        {
            Assignments = new Dictionary<string, Dictionary<string, List<int>>>();
            Backups = new Dictionary<string, Dictionary<string, List<List<int>>>>();
            CreatedAt = DateTime.Now;
            Status = "Pending";
        }
    }
} 