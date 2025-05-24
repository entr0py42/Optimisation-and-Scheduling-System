using System;
using System.Collections.Generic;

namespace Optimisation_and_Scheduling_System.Models
{
    public class DriverPreferencesViewModel
    {
        public int DriverId { get; set; }  // The ID of the driver
        public List<int> ShiftPreferences { get; set; } = new List<int>();  // List of ShiftIds in order of preference
        public List<LineShift> AvailableShifts { get; set; } = new List<LineShift>();
    }
}
