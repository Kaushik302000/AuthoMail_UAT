using AM.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Cors;
using AM.Services;
using AM.Lib;
using AM.Web.AMHub;

namespace AM.Web.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    [Authorize(Roles = AMRoles.Admin + "," + AMRoles.BA + "," + AMRoles.Client + "," + AMRoles.ClientUserManager + "," + AMRoles.CoreLogicUserManager + "," + AMRoles.PnSOperator + "," + AMRoles.API + "," + AMRoles.BAOffShore)]
    [RoutePrefix("api/alert")]
    public class AlertApiController : ApiController
    {
        IMonitorBatchesService alertSvc;
        public AlertApiController(IMonitorBatchesService alertSvc)
        {
            this.alertSvc = alertSvc;
        }
        [Route("GetDelayedBatches/{*cutOff}")]
        [HttpGet]
        public MonitorReport GetDelayedBatches(string cutOff)
        {
            try
            {
                int defaultCutOff = 30;
                int effectiveCutOff;
                if (!int.TryParse(cutOff, out effectiveCutOff))
                    effectiveCutOff = defaultCutOff;
                return alertSvc.GetPendingBatchesToSubmit(effectiveCutOff);
            }
            catch (Exception ex)
            {
                alertSvc.SendUnexpectedExceptionEmail(ex.ToString());
                throw ex;
            }
        }

        [AllowAnonymous]
        [HttpGet, Route("getamstatusdetail")]
        public AMGlobalStatus AMStatusDetail()
        {
            return AMAlert.Instance.GetGlobalStatus;
        }
    }
}
