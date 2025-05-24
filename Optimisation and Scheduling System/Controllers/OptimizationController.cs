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
                
                // If there's an error, show it on the same page
                if (result.Status.StartsWith("Error"))
                {
                    TempData["ErrorMessage"] = result.Status;
                    return View();
                }

                // Store the result in TempData for the results page
                TempData["OptimizationResult"] = JsonConvert.SerializeObject(result);
                TempData["SuccessMessage"] = "Optimization completed successfully!";
                
                return RedirectToAction("SchedulingResults");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error running optimization: {ex.Message}";
                return View();
            }
        }

        // GET: Optimization/SchedulingResults
        [Route("SchedulingResults")]
        public async Task<ActionResult> SchedulingResults()
        {
            OptimizationResultModel result = null;
            
            // First try to get results from TempData (if coming from RunDriverScheduling)
            if (TempData["OptimizationResult"] != null)
            {
                try
                {
                    var resultJson = TempData["OptimizationResult"].ToString();
                    result = JsonConvert.DeserializeObject<OptimizationResultModel>(resultJson);
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = $"Error parsing optimization results: {ex.Message}";
                }
            }
            
            // If no result in TempData, try to load the latest results from file
            if (result == null)
            {
                try
                {
                    var json = await _optimizationService.GetOptimizationResultAsJsonAsync();
                    if (!json.Contains("error"))
                    {
                        result = JsonConvert.DeserializeObject<OptimizationResultModel>(json);
                        result.Status = "Completed";
                        result.CreatedAt = DateTime.Now;
                    }
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = $"Error loading optimization results: {ex.Message}";
                }
            }
            
            // If we have results, show them
            if (result != null)
            {
                // Pass any success message from the optimization process
                if (TempData["SuccessMessage"] != null)
                {
                    ViewBag.SuccessMessage = TempData["SuccessMessage"].ToString();
                }
                return View(result);
            }
            
            // If no result is available anywhere, redirect to the run optimization page
            TempData["ErrorMessage"] = "No optimization results available. Please run the optimization first.";
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