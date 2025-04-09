using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Optimisation_and_Scheduling_System.Models;

namespace Optimisation_and_Scheduling_System.Repositories.Interfaces
{
	public interface IUserRepository
	{
        bool UserExists(string name);
        bool ValidateUser(string name, string hashedPassword);
        void CreateUser(string name, string hashedPassword, string role);
        UserModel GetUser(string name);
    }
}