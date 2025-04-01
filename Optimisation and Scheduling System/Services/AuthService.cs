using Optimisation_and_Scheduling_System.Constants;
using Optimisation_and_Scheduling_System.Models.Common;
using Optimisation_and_Scheduling_System.Repositories;
using Optimisation_and_Scheduling_System.Repositories.Interfaces;
using Optimisation_and_Scheduling_System.Services.Interfaces;
using Optimisation_and_Scheduling_System.Services.Validators;
using System;
using System.Security.Cryptography;
using System.Text;

namespace Optimisation_and_Scheduling_System.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepo;
        private readonly IDriverRepository _driverRepo;
        private readonly IPasswordValidator _passwordValidator;

        public AuthService()
            : this(new UserRepository(), new DriverRepository(), new PasswordValidator()) { }

        public AuthService(
            IUserRepository userRepo,
            IDriverRepository driverRepo,
            IPasswordValidator passwordValidator)
        {
            _userRepo = userRepo;
            _driverRepo = driverRepo;
            _passwordValidator = passwordValidator;
        }

        public AuthResult Register(string name, string password)
        {
            try
            {
                if (_userRepo.UserExists(name))
                    return AuthResult.Fail("Username is already taken.");

                if (!_passwordValidator.IsValid(password, out string validationMessage))
                    return AuthResult.Fail(validationMessage);

                string hashed = HashPassword(password);

                _driverRepo.CreateDriver(name, DateTime.UtcNow);
                _userRepo.CreateUser(name, hashed, UserRoles.Driver);

                return AuthResult.Ok();
            }
            catch (Exception)
            {
                return AuthResult.Fail("Registration failed. Please try again later.");
            }
        }

        public AuthResult Login(string name, string password)
        {
            try
            {
                if (!_userRepo.UserExists(name))
                    return AuthResult.Fail("User does not exist.");

                string hashed = HashPassword(password);
                bool valid = _userRepo.ValidateUser(name, hashed);

                return valid
                    ? AuthResult.Ok()
                    : AuthResult.Fail("Invalid password.");
            }
            catch (Exception)
            {
                return AuthResult.Fail("Login failed. Please try again later.");
            }
        }

        private string HashPassword(string password)
        {
            using (var sha = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(password);
                var hash = sha.ComputeHash(bytes);
                return BitConverter.ToString(hash).Replace("-", "").ToLower();
            }
        }
    }
}
