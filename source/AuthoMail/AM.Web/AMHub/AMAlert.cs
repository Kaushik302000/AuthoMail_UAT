using AM.Services;
using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;

namespace AM.Web.AMHub
{
    public class AMAlert
    {
        private readonly static Lazy<AMAlert> _instance =
            new Lazy<AMAlert>(() => new AMAlert());
        // We're going to broadcast to all clients a maximum of N minutes
        private readonly TimeSpan interval =
            TimeSpan.FromMilliseconds(1000 * 60 * Config.Web_API_AlertElapsedMinute);

        private readonly IHubContext _hubContext;
        private Timer _broadcastLoop;
        private AMGlobalStatus _model;
        //private bool _modelUpdated;
        public AMAlert()
        {
            // Save our hub context so we can easily use it 
            // to send to its connected clients
            _hubContext = GlobalHost.ConnectionManager.GetHubContext<AMAlertHub>();

            // Start the broadcast loop
            _broadcastLoop = new Timer(
                GetAMGlobalStatus,
                null,
                interval,
                interval);
        }
        private void GetAMGlobalStatus(object state)
        {
            _model = GetData();
            _hubContext.Clients.All.getAMDetailStatus(_model);
        }
        internal void RefreshAMAlert()
        {
            _model = GetData();
            _hubContext.Clients.All.getAMDetailStatus(_model);
        }
        public void UpdateAMAlert(string conId)
        {
            if (_model == null) _model = GetData();
            _hubContext.Clients.Client(conId).getAMDetailStatus(_model);
        }
        public static AMAlert Instance
        {
            get
            {
                return _instance.Value;
            }
        }

        public AMHealth GetAMHealth
        {
            get
            {
                if (_model == null) _model = GetData();
                return new AMHealth { AlertLevel = _model.AlertLevel, IsAMDelay = _model.IsAMDelay, LastUpdatedOn = _model.LastUpdatedOn };
            }
        }
        public AMGlobalStatus GetGlobalStatus
        {
            get
            {
                if (_model == null) _model = GetData();
                return _model;
            }
        }

        private AMGlobalStatus GetData()
        {
            using (IMonitorBatchesService alertSvc = new MonitorBatchesService())
            {
                var data = alertSvc.GetPendingBatchesToSubmit(30);
                return new AMGlobalStatus { DelayedProcessBatches = data.DelayedBatches.Count(), DelayedProcessMailPieces = data.DelayedMailPieces.Count(), DelayedAMReportBackBatches = data.DelayedAMReportBackBatches.Count(), DelayedAMReportBackMailPieces = data.DelayedAMReportBackMailPieces.Count(), DelayedPrinterBatches = data.DelayedPrinterBatches.Count(), DelayedPrinterMailPieces = data.DelayedPrinterMailPieces.Count(), MinHour = data.MinHour, MaxHour = data.MaxHour, LastBatchReceivedOn = data.LastBatchReceivedOn, LastUpdatedOn = data.LastUpdatedOn, IsReportBackDelay = data.IsReportBackDelay, IsPrinterDelay = data.IsPrinterDelay, IsReceivedDelay = data.IsReceivedDelay, IsProcessDelay = data.IsProcessDelay, AlertLevel = data.AlertLevel, IsAMDelay = data.IsAMDelay, DelayedPickUpMailPieces = data.DelayedPickUpMailPieces.Count() };
            }
        }
    }
}