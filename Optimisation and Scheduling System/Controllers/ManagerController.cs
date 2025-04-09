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

        public ManagerController() : this(new DriverRepository()) { }

        public ManagerController(IDriverRepository driverRepository)
        {
            _driverRepository = driverRepository;
        }

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult GetAllDrivers()
        {
            List<DriverModel> drivers = _driverRepository.GetAllDrivers();
            return View("Drivers", drivers);
        }

        [HttpGet]
        public JsonResult GetDriversJson()
        {
            List<DriverModel> drivers = _driverRepository.GetAllDrivers();
            return Json(drivers, JsonRequestBehavior.AllowGet);
        }
    }
} 