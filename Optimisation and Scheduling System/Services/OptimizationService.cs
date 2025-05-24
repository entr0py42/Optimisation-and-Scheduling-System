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
        private readonly string _pythonScriptPath;
        private readonly string _outputJsonPath;
        private readonly string _pythonPath;

        public OptimizationService()
        {
            // Get the solution directory path
            var currentDir = AppDomain.CurrentDomain.BaseDirectory;
            var projectDir = Directory.GetParent(currentDir).FullName;
            var solutionDir = Directory.GetParent(projectDir).FullName;
            
            // Set paths relative to the solution directory
            _gurobiFolderPath = Path.Combine(solutionDir, "GurobiAtamalar");
            _pythonScriptPath = Path.Combine(_gurobiFolderPath, "GurobiAtamalar2.py");
            _outputJsonPath = Path.Combine(_gurobiFolderPath, "optimized_schedule.json");

            // Set Python path from virtual environment
            _pythonPath = Path.Combine(solutionDir, ".venv", "Scripts", "python.exe");
            if (!File.Exists(_pythonPath))
            {
                // Fallback to venv directory if .venv doesn't exist
                _pythonPath = Path.Combine(solutionDir, "venv", "Scripts", "python.exe");
            }

            // Log the paths for debugging
            System.Diagnostics.Debug.WriteLine($"Current Directory: {currentDir}");
            System.Diagnostics.Debug.WriteLine($"Project Directory: {projectDir}");
            System.Diagnostics.Debug.WriteLine($"Solution Directory: {solutionDir}");
            System.Diagnostics.Debug.WriteLine($"Gurobi Folder Path: {_gurobiFolderPath}");
            System.Diagnostics.Debug.WriteLine($"Python Script Path: {_pythonScriptPath}");
            System.Diagnostics.Debug.WriteLine($"Output JSON Path: {_outputJsonPath}");
            System.Diagnostics.Debug.WriteLine($"Python Path: {_pythonPath}");
        }

        public async Task<OptimizationResultModel> RunDriverSchedulingOptimizationAsync()
        {
            try
            {
                // Check if the GurobiAtamalar directory exists
                if (!Directory.Exists(_gurobiFolderPath))
                {
                    throw new Exception($"Gurobi directory not found at: {_gurobiFolderPath}");
                }

                // Check if the Python script exists
                if (!File.Exists(_pythonScriptPath))
                {
                    throw new Exception($"Python script not found at: {_pythonScriptPath}");
                }

                // Check if Python exists
                if (!File.Exists(_pythonPath))
                {
                    throw new Exception($"Python not found at: {_pythonPath}. Please ensure Python is installed in the virtual environment.");
                }

                // Delete existing output file if it exists
                if (File.Exists(_outputJsonPath))
                {
                    File.Delete(_outputJsonPath);
                }

                // Create process start info for the Python script
                var processStartInfo = new ProcessStartInfo
                {
                    FileName = _pythonPath,
                    Arguments = $"\"{_pythonScriptPath}\"", // Quote the path to handle spaces
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WorkingDirectory = _gurobiFolderPath
                };

                var result = new OptimizationResultModel();
                result.Status = "Running optimization...";

                // Start the process
                using (var process = new Process())
                {
                    process.StartInfo = processStartInfo;
                    var outputBuilder = new StringBuilder();
                    var errorBuilder = new StringBuilder();

                    // Set up output handling
                    process.OutputDataReceived += (sender, e) =>
                    {
                        if (!string.IsNullOrEmpty(e.Data))
                        {
                            outputBuilder.AppendLine(e.Data);
                            System.Diagnostics.Debug.WriteLine($"Python Output: {e.Data}");
                        }
                    };

                    process.ErrorDataReceived += (sender, e) =>
                    {
                        if (!string.IsNullOrEmpty(e.Data))
                        {
                            errorBuilder.AppendLine(e.Data);
                            System.Diagnostics.Debug.WriteLine($"Python Error: {e.Data}");
                        }
                    };

                    // Start the process and begin reading output
                    process.Start();
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();

                    // Wait for the process to exit with a timeout
                    bool processExited = await Task.Run(() => process.WaitForExit(300000)); // 5-minute timeout

                    if (!processExited)
                    {
                        try { process.Kill(); } catch { }
                        throw new Exception("Optimization process timed out after 5 minutes.");
                    }

                    // Check if the process completed successfully
                    if (process.ExitCode != 0)
                    {
                        var error = errorBuilder.ToString();
                        var output = outputBuilder.ToString();
                        
                        var message = new StringBuilder();
                        message.AppendLine($"Optimization script failed with exit code {process.ExitCode}");
                        
                        if (!string.IsNullOrEmpty(error))
                        {
                            message.AppendLine("Error output:");
                            message.AppendLine(error);
                        }
                        
                        if (!string.IsNullOrEmpty(output))
                        {
                            message.AppendLine("Standard output:");
                            message.AppendLine(output);
                        }

                        throw new Exception(message.ToString());
                    }

                    // Check if output file exists and parse it
                    if (File.Exists(_outputJsonPath))
                    {
                        // Wait briefly to ensure file is fully written
                        await Task.Delay(100);
                        
                        string json = await Task.Run(() => File.ReadAllText(_outputJsonPath));
                        result = JsonConvert.DeserializeObject<OptimizationResultModel>(json);
                        result.Status = "Completed";
                        result.CreatedAt = DateTime.Now;
                        return result;
                    }
                    else
                    {
                        var output = outputBuilder.ToString();
                        var error = errorBuilder.ToString();
                        
                        var message = new StringBuilder();
                        message.AppendLine($"Output JSON file not found at: {_outputJsonPath}");
                        
                        if (!string.IsNullOrEmpty(output))
                        {
                            message.AppendLine("Process output:");
                            message.AppendLine(output);
                        }
                        
                        if (!string.IsNullOrEmpty(error))
                        {
                            message.AppendLine("Process errors:");
                            message.AppendLine(error);
                        }

                        throw new Exception(message.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                return new OptimizationResultModel
                {
                    Status = $"Error: {ex.Message}"
                };
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