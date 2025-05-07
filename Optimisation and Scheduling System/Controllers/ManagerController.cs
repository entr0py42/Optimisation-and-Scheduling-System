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


        // POST: Manager/AddLine
        [HttpPost]
        public ActionResult AddLine(Line line)
        {
            if (ModelState.IsValid)
            {
                // Ensure that the Id is not being manually set
                line.Id = 0;  // Reset the Id to 0 to ensure it is not set manually

                _lineRepository.AddLine(line);  // Add line to database
                return RedirectToAction("Lines");  // Redirect to the lines page
            }

            // If the model is invalid, return to the same page with errors
            var lines = _lineRepository.GetAllLines();
            return View("Lines", lines);
        }

        // POST: Manager/DeleteLine
        [HttpPost]
        public ActionResult DeleteLine(int id)
        {
            var line = _lineRepository.GetLineById(id);
            if (line != null)
            {
                _lineRepository.DeleteLine(line);  // Delete line from database
            }

            return RedirectToAction("Lines");  // Redirect to the lines page
        }



        // View all shifts for a line
        public ActionResult LineShifts(int lineId)
        {
            var line = _lineRepository.GetLineById(lineId);
            if (line == null) return HttpNotFound();

            ViewBag.LineName = line.Name;
            var shifts = _lineRepository.GetLineShifts(lineId);
            return View("LineShifts", shifts);
        }


        // Add a shift
        [HttpPost]
        public ActionResult AddLineShift(LineShift shift)
        {
            if (ModelState.IsValid)
            {
                _lineRepository.AddLineShift(shift);
                return RedirectToAction("LineShifts", new { lineId = shift.LineId });
            }

            ViewBag.LineId = shift.LineId;
            var shifts = _lineRepository.GetLineShifts(shift.LineId);
            return View("LineShifts", shifts);
        }

        // Delete a shift
        [HttpPost]
        public ActionResult DeleteLineShift(int id, int lineId)
        {
            _lineRepository.DeleteLineShift(id);
            return RedirectToAction("LineShifts", new { lineId });
        }


    }
}
