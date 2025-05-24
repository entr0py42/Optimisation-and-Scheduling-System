using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Optimisation_and_Scheduling_System.Models;
using Optimisation_and_Scheduling_System.Repositories;
using Optimisation_and_Scheduling_System.Repositories.Interfaces;

namespace Optimisation_and_Scheduling_System.Controllers
{
    [Authorize(Roles = "Driver")]
    public class DriverController : Controller
    {
        private readonly IDriverRepository _driverRepository;

        // Constructor with Dependency Injection for IDriverRepository
        public DriverController(IDriverRepository driverRepository)
        {
            _driverRepository = driverRepository;
        }

        // Parameterless constructor for cases where DI might not work
        public DriverController()
        {
            // This constructor can create a mock or default repository if necessary
            _driverRepository = new DriverRepository(); // Replace with your default repo or mock
        }

        // Action to display the Driver Preferences form
        public ActionResult Preferences()
        {
            // Get the driver's ID from the logged-in user
            var driverId = GetDriverIdFromUser();

            // Get the driver's preferences (if any)
            var preferences = _driverRepository.GetDriverPreferences(driverId);

            // Get the available line shifts for the form (for preferences selection)
            var availableShifts = _driverRepository.GetAvailableLineShifts();

            // Create the ViewModel to pass to the view
            var model = new DriverPreferencesViewModel
            {
                DriverId = driverId,
                ShiftPreferences = preferences.Select(p => p.ShiftId).ToList(),
                AvailableShifts = availableShifts
            };

            // Return the view with the model containing preferences and available shifts
            return View(model);
        }

        // This method fetches the driver's ID based on the logged-in user's identity
        private int GetDriverIdFromUser()
        {
            // Assuming the username (User.Identity.Name) corresponds to the driver's name in the database
            string username = User.Identity.Name;

            // Retrieve the driver from the database using the username
            using (var dbContext = new AppDbContext())
            {
                var driver = dbContext.DriverModels
                                      .FirstOrDefault(d => d.Name == username); // Assuming 'Name' is the column holding the driver's name

                if (driver != null)
                {
                    return driver.Id; // Return the driver's ID
                }

                // Handle the case when no driver is found, maybe throw an exception or return a default value
                throw new InvalidOperationException("Driver not found in the database.");
            }
        }

        // Action to save the preferences to the database
        [HttpPost]
        public ActionResult SavePreferences(DriverPreferencesViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var preferences = viewModel.ShiftPreferences.Select((shiftId, index) => new DriverPreference
                {
                    DriverId = viewModel.DriverId,
                    ShiftId = shiftId,
                    PreferenceOrder = index + 1
                }).ToList();

                _driverRepository.SaveDriverPreferences(preferences);

                TempData["Success"] = "Preferences saved successfully!";
                return RedirectToAction("Index");
            }

            // Reload available shifts since they're not persisted in the posted model
            viewModel.AvailableShifts = _driverRepository.GetAvailableLineShifts();
            return View("Preferences", viewModel);
        }

        // Index action for DriverController (you can customize this action as needed)
        public ActionResult Index()
        {
            // You can implement logic for the Driver's Dashboard or similar here
            return View();
        }
    }
}
