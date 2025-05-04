using System;

namespace Optimisation_and_Scheduling_System.Models
{
    public class DriverModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int DayTimeHours { get; set; }
        public int NighttimeHours { get; set; }
        public int WeekendHours { get; set; }
        public int WeekendNightHours { get; set; }
        public DateTime WorkerSince { get; set; }
    }
}