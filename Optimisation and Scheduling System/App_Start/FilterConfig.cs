using System.Web;
using System.Web.Mvc;

namespace Optimisation_and_Scheduling_System
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}
