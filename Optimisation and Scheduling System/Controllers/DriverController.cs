using System.Web.Mvc;

namespace Optimisation_and_Scheduling_System.Controllers
{
    [Authorize(Roles = "Driver")]
    public class DriverController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }
    }
} 