namespace Optimisation_and_Scheduling_System.Models.Common
{
	public class AuthResult
	{
        public bool Success { get; set; }
        public string Message { get; set; }
        public string UserRole { get; set; }

        public static AuthResult Ok() => new AuthResult { Success = true };
        public static AuthResult Fail(string message) => new AuthResult { Success = false, Message = message };
    }
}