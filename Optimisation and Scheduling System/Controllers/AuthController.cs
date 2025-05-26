using Optimisation_and_Scheduling_System.DataDb;
using Optimisation_and_Scheduling_System.Models.Common;
using Optimisation_and_Scheduling_System.Services;
using Optimisation_and_Scheduling_System.Services.Interfaces;
using Optimisation_and_Scheduling_System.Constants;
using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Optimisation_and_Scheduling_System.Models;

namespace Optimisation_and_Scheduling_System.Controllers
{
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;

        public AuthController() : this(new AuthService()) { }

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost]
        public ActionResult Register(RegisterModel model)
        {
            if (!ModelState.IsValid) return View(model);

            // Register the user first
            AuthResult result = _authService.Register(model.Name, model.Password);
            if (!result.Success)
            {
                TempData["ErrorMessage"] = result.Message;
                return View(model);
            }

            // Create the DriverModel record
            var driver = new DriverModel
            {
                Name = model.Name,
                Gender = model.Gender,
                WorkerSince = DateTime.Now

            };

            // Save the DriverModel
            using (var context = new AppDbContext())
            {
                context.DriverModels.Add(driver);
                context.SaveChanges();
            }

            TempData["SuccessMessage"] = "Registration successful! Please login.";
            return RedirectToAction("Login");
        }


        [HttpPost]
        public ActionResult Login(LoginModel model)
        {
            if (!ModelState.IsValid) return View(model);

            AuthResult result = _authService.Login(model.Name, model.Password);
            if (!result.Success)
            {
                TempData["ErrorMessage"] = result.Message;
                return View(model);
            }

            // Create the authentication ticket with the user's role
            FormsAuthenticationTicket ticket = new FormsAuthenticationTicket(
                1,
                model.Name,
                DateTime.Now,
                DateTime.Now.AddMinutes(30),
                false,
                result.UserRole,
                FormsAuthentication.FormsCookiePath);

            // Encrypt the ticket
            string encryptedTicket = FormsAuthentication.Encrypt(ticket);

            // Create the cookie
            HttpCookie cookie = new HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket);
            Response.Cookies.Add(cookie);

            TempData["SuccessMessage"] = "Login successful!";
            
            // Redirect based on user role
            switch (result.UserRole)
            {
                case UserRoles.Manager:
                    return RedirectToAction("Index", "Manager");
                case UserRoles.Driver:
                    return RedirectToAction("Index", "Driver");
                default:
                    return RedirectToAction("Index", "Home");
            }
        }

        [HttpGet]
        public ActionResult Register() => View();

        [HttpGet]
        public ActionResult Login() => View();



        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();

            return RedirectToAction("Login", "Auth");
        }


    }
}
