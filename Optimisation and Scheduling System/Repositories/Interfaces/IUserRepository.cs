using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Optimisation_and_Scheduling_System.Repositories.Interfaces
{
	public interface IUserRepository
	{
        bool UserExists(string username);
        void CreateUser(string username, string hashedPassword, string role);
        bool ValidateUser(string username, string hashedPassword);
    }
}