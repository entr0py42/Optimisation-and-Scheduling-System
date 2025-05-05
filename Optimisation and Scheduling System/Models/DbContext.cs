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



        // DbSets for the Line and LineShift models
        public DbSet<Line> Lines { get; set; }
        public DbSet<LineShift> LineShifts { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);



            // Configure column names to be lowercase
            modelBuilder.Entity<Line>()
                .Property(x => x.Id).HasColumnName("id");
            modelBuilder.Entity<Line>()
                .Property(x => x.Name).HasColumnName("name");
            modelBuilder.Entity<Line>()
                .Property(x => x.Garage).HasColumnName("garage");

            modelBuilder.Entity<LineShift>()
                .Property(x => x.Id).HasColumnName("id");
            modelBuilder.Entity<LineShift>()
                .Property(x => x.LineId).HasColumnName("lineid");
            modelBuilder.Entity<LineShift>()
                .Property(x => x.ShiftTimeStart).HasColumnName("shifttimestart");
            modelBuilder.Entity<LineShift>()
                .Property(x => x.ShiftTimeEnd).HasColumnName("shifttimeend");

            // Ensure the tables are also lowercase
            modelBuilder.Entity<Line>().ToTable("line", "public");
            modelBuilder.Entity<LineShift>().ToTable("lineshift", "public");
        }



    }
}