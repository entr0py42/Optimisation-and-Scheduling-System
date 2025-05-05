using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Optimisation_and_Scheduling_System.Models
{
    [Table("line")]
    public class Line
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Garage { get; set; }

        public ICollection<LineShift> LineShifts { get; set; }
    }
}