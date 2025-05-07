using System;
using System.Collections.Generic;

namespace Optimisation_and_Scheduling_System.Models
{
    public class DriverPreferencesViewModel
    {
        public int DriverId { get; set; }  // The ID of the driver
        public List<int> ShiftPreferences { get; set; }  // List of ShiftIds in the order of preference
        public List<LineShift> AvailableShifts { get; set; }  // List of available shifts
    }
}
