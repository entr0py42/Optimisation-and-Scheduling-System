using System.Collections.Generic;
using System.Web.Mvc;
using Optimisation_and_Scheduling_System.Models;
using Optimisation_and_Scheduling_System.Repositories;
using Optimisation_and_Scheduling_System.Repositories.Interfaces;

namespace Optimisation_and_Scheduling_System.Controllers
{
    [Authorize(Roles = "Manager")]
    public class ManagerController : Controller
    {
        private readonly IDriverRepository _driverRepository;
        private readonly ILineRepository _lineRepository;

        // Constructor that accepts both repositories
        public ManagerController() : this(new DriverRepository(), new LineRepository(new AppDbContext())) { }

        // Dependency Injection constructor
        public ManagerController(IDriverRepository driverRepository, ILineRepository lineRepository)
        {
            _driverRepository = driverRepository;
            _lineRepository = lineRepository;
        }

        // The default action that returns the manager dashboard view
        public ActionResult Index()
        {
            return View();
        }

        // Action to retrieve all drivers
        public ActionResult GetAllDrivers()
        {
            List<DriverModel> drivers = _driverRepository.GetAllDrivers();
            return View("Drivers", drivers);  // You need a view named "Drivers" to display the list
        }

        // JSON action to return all drivers as JSON
        [HttpGet]
        public JsonResult GetDriversJson()
        {
            List<DriverModel> drivers = _driverRepository.GetAllDrivers();
            return Json(drivers, JsonRequestBehavior.AllowGet);
        }

        // Action to retrieve all lines
        public ActionResult Lines()
        {
            var lines = _lineRepository.GetAllLines();
            return View("Lines", lines);  // You need a view named "Lines" to display the list
        }

        // Action to retrieve a specific line by ID
        public ActionResult GetLineById(int id)
        {
            var line = _lineRepository.GetLineById(id);
            return View("LineDetails", line);  // You need a view named "LineDetails" to display the line details
        }

        // Action to retrieve shifts for a specific line
        public ActionResult GetLineShifts(int lineId)
        {
            var lineShifts = _lineRepository.GetLineShifts(lineId);
            return View("LineShifts", lineShifts);  // You need a view named "LineShifts" to display the shifts
        }
    }
}
