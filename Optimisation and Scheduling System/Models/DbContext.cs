using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using Npgsql;
using Optimisation_and_Scheduling_System.Models;


namespace Optimisation_and_Scheduling_System.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext() : base("PostgresConnection")
        {
        }

        public DbSet<OffdayRequest> OffdayRequests { get; set; }

    }
}