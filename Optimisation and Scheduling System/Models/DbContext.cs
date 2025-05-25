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

        // Existing DbSets
        public DbSet<Line> Lines { get; set; }
        public DbSet<LineShift> LineShifts { get; set; }
        public DbSet<DriverModel> DriverModels { get; set; }

        // Add DbSet for DriverScheduleAssignment
        public DbSet<DriverScheduleAssignment> DriverScheduleAssignments { get; set; }

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

            // Configure DriverScheduleAssignment entity mappings
            modelBuilder.Entity<DriverScheduleAssignment>()
                .Property(x => x.Id).HasColumnName("id");
            modelBuilder.Entity<DriverScheduleAssignment>()
                .Property(x => x.DriverId).HasColumnName("driverid");
            modelBuilder.Entity<DriverScheduleAssignment>()
                .Property(x => x.Day).HasColumnName("day");
            modelBuilder.Entity<DriverScheduleAssignment>()
                .Property(x => x.Route).HasColumnName("route");
            modelBuilder.Entity<DriverScheduleAssignment>()
                .Property(x => x.Shift).HasColumnName("shift");
            modelBuilder.Entity<DriverScheduleAssignment>()
                .Property(x => x.IsBackup).HasColumnName("isbackup");

            // Map tables to their schemas
            modelBuilder.Entity<Line>().ToTable("line", "public");
            modelBuilder.Entity<LineShift>().ToTable("lineshift", "public");
            modelBuilder.Entity<DriverModel>().ToTable("drivermodel", "public");
            modelBuilder.Entity<DriverScheduleAssignment>().ToTable("driverscheduleassignments", "public");
        }
    }
}
