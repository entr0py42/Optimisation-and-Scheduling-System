using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Optimisation_and_Scheduling_System.Services.Interfaces
{
	public interface IPasswordValidator
	{
        bool IsValid(string password, out string message);
    }
}