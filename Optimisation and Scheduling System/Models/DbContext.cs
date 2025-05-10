using System;
using System.Data.Entity;
using Optimisation_and_Scheduling_System.Models;

namespace Optimisation_and_Scheduling_System.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext() : base("PostgresConnection")
        {
        }

        // DbSets for the Line, LineShift, and DriverModel models
        public DbSet<Line> Lines { get; set; }
        public DbSet<LineShift> LineShifts { get; set; }
        public DbSet<DriverModel> DriverModels { get; set; }  // Added this line

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Line entity mappings
            modelBuilder.Entity<Line>()
                .Property(x => x.Id).HasColumnName("id");
            modelBuilder.Entity<Line>()
                .Property(x => x.Name).HasColumnName("name");
            modelBuilder.Entity<Line>()
                .Property(x => x.Garage).HasColumnName("garage");

            // Configure LineShift entity mappings
            modelBuilder.Entity<LineShift>()
                .Property(x => x.Id).HasColumnName("id");
            modelBuilder.Entity<LineShift>()
                .Property(x => x.LineId).HasColumnName("lineid");
            modelBuilder.Entity<LineShift>()
                .Property(x => x.ShiftTimeStart).HasColumnName("shifttimestart");
            modelBuilder.Entity<LineShift>()
                .Property(x => x.ShiftTimeEnd).HasColumnName("shifttimeend");
            modelBuilder.Entity<LineShift>()
                .Property(x => x.Day).HasColumnName("day");
                        modelBuilder.Entity<LineShift>()
                .Property(x => x.IsDayShift).HasColumnName("isdayshift");

            // Configure DriverModel entity mappings
            modelBuilder.Entity<DriverModel>()
                .Property(x => x.Id).HasColumnName("id");
            modelBuilder.Entity<DriverModel>()
                .Property(x => x.Name).HasColumnName("name");
            modelBuilder.Entity<DriverModel>()
                .Property(x => x.Gender).HasColumnName("gender");
            modelBuilder.Entity<DriverModel>()
                .Property(x => x.WorkerSince).HasColumnName("workersince");

            // Map entities to tables
            modelBuilder.Entity<Line>().ToTable("line", "public");
            modelBuilder.Entity<LineShift>().ToTable("lineshift", "public");
            modelBuilder.Entity<DriverModel>().ToTable("drivermodel", "public"); // Ensure this line is present
        }
    }
}
