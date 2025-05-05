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
        private readonly string _pythonExecutablePath;
        private readonly string _gurobiFolderPath;
        private readonly string _guroiScriptPath;
        private readonly string _outputJsonPath;

        public OptimizationService()
        {
            // Default paths - adjust these to your environment
            _pythonExecutablePath = "python"; // Assuming python is in PATH
            _gurobiFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "GurobiAtamalar");
            _guroiScriptPath = Path.Combine(_gurobiFolderPath, "GurobiAtamalar2.py");
            _outputJsonPath = Path.Combine(_gurobiFolderPath, "clean_driver_schedule.json");
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

                // Check if the optimization script exists
                if (!File.Exists(_guroiScriptPath))
                {
                    throw new Exception($"Optimization script not found at: {_guroiScriptPath}. " +
                        $"Please ensure GurobiAtamalar2.py is in the correct directory.");
                }

                // Ensure the output directory exists
                Directory.CreateDirectory(Path.GetDirectoryName(_outputJsonPath));

                // Create process start info
                var processStartInfo = new ProcessStartInfo
                {
                    FileName = _pythonExecutablePath,
                    Arguments = $"\"{_guroiScriptPath}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
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
                        errorBuilder.AppendLine($"Python optimization script failed with exit code {process.ExitCode}");
                        
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