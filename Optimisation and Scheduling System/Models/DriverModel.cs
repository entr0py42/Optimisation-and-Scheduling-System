using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Optimisation_and_Scheduling_System.Models
{
    [Table("drivermodel")]
    public class DriverModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Gender { get; set; }
        public DateTime WorkerSince { get; set; }

        [NotMapped]
        public int Performance { get; set; } = 85; // Default performance score

        [NotMapped]
        public int ExperienceYears => (int)((DateTime.Now - WorkerSince).TotalDays / 365);
    }
}