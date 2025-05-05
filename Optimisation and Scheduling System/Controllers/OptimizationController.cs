using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using Optimisation_and_Scheduling_System.Models;
using Optimisation_and_Scheduling_System.Services;
using Optimisation_and_Scheduling_System.Services.Interfaces;
using Newtonsoft.Json;
using System.Diagnostics;
using System.IO;

namespace Optimisation_and_Scheduling_System.Controllers
{
    [Authorize(Roles = "Manager")]
    [RoutePrefix("Optimization")]
    public class OptimizationController : Controller
    {
        private readonly IOptimizationService _optimizationService;

        public OptimizationController()
        {
            _optimizationService = new OptimizationService();
        }

        // Constructor for dependency injection during testing
        public OptimizationController(IOptimizationService optimizationService)
        {
            _optimizationService = optimizationService;
        }

        // GET: Optimization
        [Route("")]
        [Route("Index")]
        public ActionResult Index()
        {
            return View();
        }

        // GET: Optimization/RunDriverScheduling
        [Route("RunDriverScheduling")]
        public ActionResult RunDriverScheduling()
        {
            return View();
        }

        // POST: Optimization/RunDriverScheduling
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("RunDriverScheduling")]
        public async Task<ActionResult> RunDriverScheduling(FormCollection collection)
        {
            try
            {
                // Run the optimization process
                var result = await _optimizationService.RunDriverSchedulingOptimizationAsync();
                
                // Handle the result
                if (result.Status.StartsWith("Error"))
                {
                    ModelState.AddModelError("", result.Status);
                    return View();
                }

                // Store the result in TempData for the results page
                TempData["OptimizationResult"] = JsonConvert.SerializeObject(result);
                
                return RedirectToAction("SchedulingResults");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error running optimization: {ex.Message}");
                return View();
            }
        }

        // GET: Optimization/SchedulingResults
        [Route("SchedulingResults")]
        public ActionResult SchedulingResults()
        {
            // Retrieve the result from TempData
            if (TempData["OptimizationResult"] != null)
            {
                var resultJson = TempData["OptimizationResult"].ToString();
                var result = JsonConvert.DeserializeObject<OptimizationResultModel>(resultJson);
                return View(result);
            }

            // If no result is available, redirect to the run optimization page
            return RedirectToAction("RunDriverScheduling");
        }

        // GET: Optimization/GetResultsJson
        [HttpGet]
        [Route("GetResultsJson")]
        public async Task<ActionResult> GetResultsJson()
        {
            try
            {
                var json = await _optimizationService.GetOptimizationResultAsJsonAsync();
                return Content(json, "application/json");
            }
            catch (Exception ex)
            {
                return Json(new { error = $"Error retrieving optimization results: {ex.Message}" }, JsonRequestBehavior.AllowGet);
            }
        }
    }
} 