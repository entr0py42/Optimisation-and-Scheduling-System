using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using Npgsql;



namespace Optimisation_and_Scheduling_System.App_Start
{
    public class PostgreSqlConfiguration : DbConfiguration
    {
        public PostgreSqlConfiguration()
        {
            SetProviderFactory("Npgsql", NpgsqlFactory.Instance);
        }
    }
}