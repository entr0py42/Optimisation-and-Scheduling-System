using Optimisation_and_Scheduling_System.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Optimisation_and_Scheduling_System.Controllers
{
    public class OffdaysController : Controller
    {
        private AppDbContext db = new AppDbContext();

        // GET: Offdays
        public ActionResult Index()
        {
            var requests = db.OffdayRequests.ToList();

            return View("~/Views/Manager/Offdays.cshtml", requests);

        }
    }

}