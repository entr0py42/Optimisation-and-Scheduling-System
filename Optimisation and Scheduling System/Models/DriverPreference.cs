using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations.Schema;

namespace Optimisation_and_Scheduling_System.Models
{
    [Table("driverpreferences")]
    public class DriverPreference
    {
        public int DriverId { get; set; }  // The ID of the driver
        public int ShiftId { get; set; }
        public int PreferenceOrder { get; set; }

        public virtual DriverModel Driver { get; set; }
        public virtual LineShift Shift { get; set; }

        public DriverPreference()
        {
            // Initialize the preferences list
        }
    }
}
