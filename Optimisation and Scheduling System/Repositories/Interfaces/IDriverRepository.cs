using System;
using System.Collections.Generic;
using Optimisation_and_Scheduling_System.Models;

namespace Optimisation_and_Scheduling_System.Repositories.Interfaces
{
    public interface IDriverRepository
    {
        List<DriverModel> GetAllDrivers();
        List<DriverPreference> GetDriverPreferences(int driverId);
        void SaveDriverPreferences(List<DriverPreference> preferences);
        List<LineShift> GetAvailableLineShifts();
    }
}
