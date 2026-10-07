using AM.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using AM.Entities;
using AM.Web.Models;
using AM.Lib;
namespace AM.Web.Controllers
{
    [Authorize(Roles = AMRoles.Admin + "," + AMRoles.BA + "," + AMRoles.Client + "," + AMRoles.ClientUserManager + "," + AMRoles.CoreLogicUserManager + "," + AMRoles.PnSOperator + "," + AMRoles.Delfus + "," + AMRoles.BAOffShore)]//Todo: For Role "API"
    [RoutePrefix("api/chart")]
    public class ChartApiController : ApiController
    {
        IChartDataService chartDataSvc;
        public ChartApiController(IChartDataService chartDataSvc)
        {
            this.chartDataSvc = chartDataSvc;
        }

        [Route("GetWeeklyChartData")]
        [HttpGet]
        public object GetWeeklyChartData()
        {
            var model = chartDataSvc.GetWeeklyChartData(User.Identity.GetClientId().Value);
            return model.OrderByDescending(s => s.Date).ToList();
        }

        [Route("GetTodayChartData")]
        [HttpGet]
        public object GetTodayChartData()
        {
            var estToday = DateTime.UtcNow.UtcToEst().Date;
            var model = chartDataSvc.GetTodayMailPieceChartData(estToday, User.Identity.GetClientId().Value);
            return model.OrderByDescending(s => s.MailBatchId).ToList();
        }
        [HttpGet, Route("DailySummaryChart")]
        public object GetDailySummaryChart()
        {
            int clientId = User.Identity.GetClientId().Value;
            var estToday = DateTime.UtcNow.UtcToEst().Date;
            var model = chartDataSvc.GetDailySummaryChartByDocType(estToday, clientId);
            return model;
        }
        [HttpGet, Route("GetAllClientsWeeklyChartData")]
        public object GetAllClientsWeeklyChartData()
        {
            return chartDataSvc.GetWeeklyChartData();
        }
        [HttpGet, Route("GetAllClientsTodayChartData")]
        public object GetAllClientsTodayChartData()
        {
            return chartDataSvc.GetTodayMailPieceChartData(DateTime.UtcNow.UtcToEst());
        }
    }
}
