using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace Optimisation_and_Scheduling_System.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {

            if (User.Identity.IsAuthenticated)
            {
                var role = ((FormsIdentity)User.Identity).Ticket.UserData;

                
                if (role == "Manager")
                {
                    return RedirectToAction("Index", "Manager");
                }
                if (role == "Driver")
                {   
                    return RedirectToAction("Index", "Driver");
                }
                else
                    return View();
            }
            else
            {
                return View();
            }
        }

        public ActionResult About()
        {
            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}