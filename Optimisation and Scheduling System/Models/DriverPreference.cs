using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Optimisation_and_Scheduling_System.Models
{
    public class DriverPreference
    {
        public int DriverId { get; set; }  // The ID of the driver
        public List<int> ShiftPreferences { get; set; }  // List of ShiftIds in the order of preference (most to least)

        public DriverPreference()
        {
            ShiftPreferences = new List<int>();  // Initialize the preferences list
        }
    }
}
