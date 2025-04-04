using Optimisation_and_Scheduling_System.DataDb;
using Optimisation_and_Scheduling_System.Models.Common;
using Optimisation_and_Scheduling_System.Services;
using Optimisation_and_Scheduling_System.Services.Interfaces;
using System.Web.Mvc;
using System.Web.Security;

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

            AuthResult result = _authService.Register(model.Name, model.Password);
            if (!result.Success)
            {
                TempData["ErrorMessage"] = result.Message;
                return View(model);
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

            
            FormsAuthentication.SetAuthCookie(model.Name, false); 

            TempData["SuccessMessage"] = "Login successful!";
            return RedirectToAction("Index", "Home");
        }


        [HttpGet]
        public ActionResult Register() => View();

        [HttpGet]
        public ActionResult Login() => View();
    }
}
