using Optimisation_and_Scheduling_System.Services.Interfaces;
using System.Linq;

namespace Optimisation_and_Scheduling_System.Services.Validators
{
    public class PasswordValidator : IPasswordValidator
    {
        public bool IsValid(string password, out string message)
        {
            if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
            {
                message = "Password must be at least 8 characters long.";
                return false;
            }

            if (!password.Any(char.IsDigit))
            {
                message = "Password must contain at least one number.";
                return false;
            }

            if (!password.Any(char.IsLower))
            {
                message = "Password must contain at least one lowercase letter.";
                return false;
            }

            if (!password.Any(char.IsUpper))
            {
                message = "Password must contain at least one uppercase letter.";
                return false;
            }

            if (!password.Any(c => "!@#$%^&*()_+[]{}|;:,.<>?".Contains(c)))
            {
                message = "Password must contain at least one special character.";
                return false;
            }

            message = null;
            return true;
        }
    }
}
