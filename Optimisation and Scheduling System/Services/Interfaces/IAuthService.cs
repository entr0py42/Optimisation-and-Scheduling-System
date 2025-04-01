using Optimisation_and_Scheduling_System.Models.Common;

namespace Optimisation_and_Scheduling_System.Services.Interfaces
{
    public interface IAuthService
    {
        AuthResult Register(string name, string password);
        AuthResult Login(string name, string password);
    }
}
