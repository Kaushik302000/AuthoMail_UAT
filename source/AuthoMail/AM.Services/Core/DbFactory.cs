using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AM.Services
{
    static class DbFactory
    {
        /// <summary>
        /// Uses connection string with name 'DefaultConnection' to create connection.
        /// </summary>
        /// <returns></returns>
        internal static DbConnection Create()
        {
            return Create("DefaultConnection");
        }

        /// <summary>
        /// Uses connection string with specified name to create connection.
        /// </summary>
        /// <param name="connectionStringName">Connection string name</param>
        /// <returns></returns>
        internal static DbConnection Create(string connectionStringName)
        {
            var cs = ConfigurationManager.ConnectionStrings[connectionStringName];

            if (cs == null)
            {
                throw new ArgumentException("Connection string with name '{0}' does not exist.", connectionStringName);
            }

            var factory = DbProviderFactories.GetFactory(cs.ProviderName);
            var connection = factory.CreateConnection();
            connection.ConnectionString = cs.ConnectionString;
            return connection;
        }
    }
}
