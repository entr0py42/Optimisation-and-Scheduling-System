using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Optimisation_and_Scheduling_System.Models
{
	public class UserModel
	{
        public string UserName { get; set; }
        public string HashedPassword { get; set; }
        public string UserRole { get; set; }

    }
}