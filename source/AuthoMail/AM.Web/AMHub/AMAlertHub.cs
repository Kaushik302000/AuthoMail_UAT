using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AM.Services;
using Microsoft.AspNet.SignalR.Hubs;

namespace AM.Web.AMHub
{
    public class AMAlertHub : Hub
    {
        private AMAlert _amAlert;
        public AMAlertHub()
            : this(AMAlert.Instance)
        {
        }
        public AMAlertHub(AMAlert amAlert)
        {
            _amAlert = amAlert;
        }
        public void RefreshAMAlert()
        {
            _amAlert.RefreshAMAlert();
        }
        public void GetConnection(string conId)//Get Connection id
        {
            _amAlert.UpdateAMAlert(conId);
        }
        public override System.Threading.Tasks.Task OnReconnected()
        {
            _amAlert.UpdateAMAlert(Context.ConnectionId);
            return base.OnReconnected();
        }
    }
    public class SOHubPipelineModule : HubPipelineModule
    {
        protected override void OnIncomingError(ExceptionContext exceptionContext,
                                                IHubIncomingInvokerContext invokerContext)
        {
            dynamic caller = invokerContext.Hub.Clients.Caller;
            caller.ExceptionHandler(exceptionContext.Error.Message);
        }
    }
    public class AMHealth
    {
        public string AlertLevel { get; set; }
        public bool IsAMDelay { get; set; }
        public DateTime LastUpdatedOn { get; set; }
    }
}