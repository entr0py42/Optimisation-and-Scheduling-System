using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Optimisation_and_Scheduling_System.Repositories.Interfaces
{
	public interface IDriverRepository
	{
        void CreateDriver(string name, DateTime workerSince);
    }
}