using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using Microsoft.AspNet.Identity;
using AM.Services;

namespace AM.Web.AMHub
{
    public class ErrorNotificationSignalR
    {
        private readonly static Lazy<ErrorNotificationSignalR> _instance =
           new Lazy<ErrorNotificationSignalR>(() => new ErrorNotificationSignalR());
        // We're going to broadcast to all clients a maximum of N minutes
        private readonly TimeSpan interval =
            TimeSpan.FromMilliseconds(1000 * 60 * 12);
        private readonly IHubContext _hubContext;
        private Timer _broadcastLoop;
        //private bool _modelUpdated;
        private object _model;
        private string connectionId;
        private int? LastId;
        private Dictionary<string, float> stockAlertDict;
        public ErrorNotificationSignalR()
        {
            // Save our hub context so we can easily use it 
            // to send to its connected clients
            _hubContext = GlobalHost.ConnectionManager.GetHubContext<AMErrorNotificationHub>();

            // Start the broadcast loop
            _broadcastLoop = new Timer(
                AMErrors,
                null,
                interval,
                interval);
        }
        private void AMErrors(object state)
        {
            _model = GetErrorData();
            _hubContext.Clients.All.getAMErrors(_model);
        }
        public static ErrorNotificationSignalR Instance
        {
            get
            {
                return _instance.Value;
            }
        }
        public void UpdateErrorNotification(string connectionId)
        {
            this.connectionId = connectionId;
            if (_model == null) _model = GetErrorData();
            _hubContext.Clients.Client(connectionId).getAMErrors(_model);
        }
        private object GetErrorData()
        {
            using (IHomeService homeSvc = new HomeService())
            using (IInventoryService invSvc = new InventoryService())
            {
                var errorsData = homeSvc.GetErrorNotificationList(LastId);
                int maxId = errorsData != null && errorsData.Count > 0 ? errorsData.Max(e => e.Id) : 0;
                if (maxId != 0 && maxId != LastId)
                    LastId = maxId;
                var result = invSvc.InventoryStockAlertLevel(stockAlertDict);
                stockAlertDict = result.Value;
                return new
                {
                    errorsData = errorsData,
                    stocklevel = result.Key
                };
            }
        }
    }


}