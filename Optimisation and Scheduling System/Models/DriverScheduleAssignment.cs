using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Optimisation_and_Scheduling_System.Models
{
    [Table("driverscheduleassignments")]
    public class DriverScheduleAssignment
    {
        public int Id { get; set; }  // Primary key

        public int DriverId { get; set; }  // Foreign key

        public string Day { get; set; }  // Not null, max length not specified, so default string

        public int Route { get; set; }

        public int Shift { get; set; }

        public bool IsBackup { get; set; } = false;

        public string RouteName { get; set; }

        public TimeSpan ShiftTimeStart { get; set; }
        public TimeSpan ShiftTimeEnd { get; set; }



    }
}