using AM.Lib;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Services.Description;

namespace AM.Services
{
    public class ServiceBase : Disposable
    {
        public static ConcurrentDictionary<string, int> connections;
        public static ConcurrentDictionary<string, int> maxConnections;

        private string serviceType;

        protected DbConnection db = null;

        static ServiceBase()
        {
            Handlers.Register();

            connections = new ConcurrentDictionary<string, int>(Environment.ProcessorCount * 2, 16);
            maxConnections = new ConcurrentDictionary<string, int>(Environment.ProcessorCount * 2, 16);
            //connections.TryAdd(typeof(CommonService).FullName, 0);
            //maxConnections.TryAdd(typeof(CommonService).FullName, 0);
        }

        protected ServiceBase(string connectionStringName)
        {
            db = DbFactory.Create(connectionStringName);
        }

        protected ServiceBase()
        {
            db = DbFactory.Create("AuthoMail");

            serviceType = this.GetType().FullName;

            connections.AddOrUpdate(serviceType, 1, (_, val) => val + 1);
            maxConnections.AddOrUpdate(serviceType, 1, (key, val) => Math.Max(connections[key], maxConnections[key]));
        }

        /// <summary>
        /// Dispose managed resources.
        /// </summary>
        protected override void DisposeManagedObjects()
        {
            if (db != null)
            {
                // Closing a closed connection doesn't make a different so calling this method as a backup
                // in case the connection was kept open.
                db.Close();
                db.Dispose();
                db = null;

                connections.AddOrUpdate(serviceType, 0, (_, val) => val - 1);
            }
        }

        /// <summary>
        /// Free your own state (unmanaged objects).
        /// Set large fields to null.
        /// </summary>
        protected override void DisposeUnManagedObjects()
        {

            // There are no unmanaged resources to free
            return;
        }
    }
}
