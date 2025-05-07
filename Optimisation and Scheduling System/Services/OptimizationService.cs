using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Optimisation_and_Scheduling_System.Models;
using Optimisation_and_Scheduling_System.Services.Interfaces;
using System.Text;

namespace Optimisation_and_Scheduling_System.Services
{
    public class OptimizationService : IOptimizationService
    {
        private readonly string _gurobiFolderPath;
        private readonly string _wslBatchPath;
        private readonly string _outputJsonPath;

        public OptimizationService()
        {
            // Default paths - adjust these to your environment
            _gurobiFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "GurobiAtamalar");
            _wslBatchPath = Path.Combine(_gurobiFolderPath, "run_gurobi_wsl.bat");
            _outputJsonPath = Path.Combine(_gurobiFolderPath, "wsl_driver_schedule.json");
        }

        public async Task<OptimizationResultModel> RunDriverSchedulingOptimizationAsync()
        {
            try
            {
                // Check if the GurobiAtamalar directory exists
                if (!Directory.Exists(_gurobiFolderPath))
                {
                    throw new Exception($"Gurobi directory not found at: {_gurobiFolderPath}. " +
                        $"Please place the GurobiAtamalar folder with optimization scripts in the parent directory of the application.");
                }

                // Check if the batch script exists
                if (!File.Exists(_wslBatchPath))
                {
                    throw new Exception($"WSL batch script not found at: {_wslBatchPath}. " +
                        $"Please ensure run_gurobi_wsl.bat is in the GurobiAtamalar directory.");
                }

                // Create process start info for the batch file
                var processStartInfo = new ProcessStartInfo
                {
                    FileName = _wslBatchPath,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = false,  // Show window so user can see progress
                    WorkingDirectory = _gurobiFolderPath
                };

                // Start the process
                using (var process = new Process())
                {
                    process.StartInfo = processStartInfo;
                    process.Start();

                    // Read output and error asynchronously
                    var output = await process.StandardOutput.ReadToEndAsync();
                    var error = await process.StandardError.ReadToEndAsync();
                    
                    // Wait for the process to exit
                    await Task.Run(() => process.WaitForExit());

                    // Check if the process completed successfully
                    if (process.ExitCode != 0)
                    {
                        var errorBuilder = new StringBuilder();
                        errorBuilder.AppendLine($"WSL optimization script failed with exit code {process.ExitCode}");
                        
                        if (!string.IsNullOrEmpty(error))
                        {
                            errorBuilder.AppendLine("Error details:");
                            errorBuilder.AppendLine(error);
                        }

                        throw new Exception(errorBuilder.ToString());
                    }

                    // Parse JSON output if the process completed successfully
                    if (File.Exists(_outputJsonPath))
                    {
                        // Using File.ReadAllText for .NET Framework compatibility
                        string json = await Task.Run(() => File.ReadAllText(_outputJsonPath));
                        var result = JsonConvert.DeserializeObject<OptimizationResultModel>(json);
                        result.Status = "Completed";
                        return result;
                    }
                    else
                    {
                        throw new FileNotFoundException(
                            "Output JSON file not found after optimization process completed. " +
                            $"Expected location: {_outputJsonPath}");
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle errors and return incomplete result
                var errorResult = new OptimizationResultModel
                {
                    Status = $"Error: {ex.Message}"
                };
                return errorResult;
            }
        }

        public async Task<string> GetOptimizationResultAsJsonAsync()
        {
            if (File.Exists(_outputJsonPath))
            {
                return await Task.Run(() => File.ReadAllText(_outputJsonPath));
            }
            
            return JsonConvert.SerializeObject(new { error = "No optimization results available." });
        }
    }
} 