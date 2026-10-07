using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;

namespace AM.Web.AMHub
{
    public class AMErrorNotificationHub : Hub
    {
        ErrorNotificationSignalR _eNoti;
        public AMErrorNotificationHub()
            : this(ErrorNotificationSignalR.Instance)
        {
        }
        public AMErrorNotificationHub(ErrorNotificationSignalR _eNoti)
        {
            this._eNoti = _eNoti;
        }
        public void InitialConnection(string conId)//Get Connection id
        {
            _eNoti.UpdateErrorNotification(conId);
        }
        public override System.Threading.Tasks.Task OnReconnected()
        {
            return base.OnReconnected();
        }
        public override System.Threading.Tasks.Task OnDisconnected(bool stopCalled)
        {
            return base.OnDisconnected(stopCalled);
        }
        public override System.Threading.Tasks.Task OnConnected()
        {
            return base.OnConnected();
        }
    }

}