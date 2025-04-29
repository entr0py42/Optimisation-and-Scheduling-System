using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Optimisation_and_Scheduling_System.Models
{
    [Table("offdaysrequests", Schema = "public")]
    public class OffdayRequest
    {
        [Key]
        [Column("reqid")]
        public int ReqId { get; set; }

        [Column("driverid")]
        public int? DriverId { get; set; }

        [Required]
        [Column("startdate")]
        public DateTime StartDate { get; set; }

        [Required]
        [Column("enddate")]
        public DateTime EndDate { get; set; }

        [Required]
        [Column("reason")]
        public string Reason { get; set; }

        [Column("status")]
        public int Status { get; set; }
    }
}