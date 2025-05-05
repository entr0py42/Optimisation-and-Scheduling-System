using System.Threading.Tasks;
using Optimisation_and_Scheduling_System.Models;

namespace Optimisation_and_Scheduling_System.Services.Interfaces
{
    public interface IOptimizationService
    {
        Task<OptimizationResultModel> RunDriverSchedulingOptimizationAsync();
        Task<string> GetOptimizationResultAsJsonAsync();
    }
} 