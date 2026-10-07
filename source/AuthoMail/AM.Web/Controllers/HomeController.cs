using AM.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using System.Security.Claims;
using AM.Entities;
using AM.Web.USPSIMService;
using System.Globalization;
using AM.Lib;
using AM.Web.Models;
using AutoMapper;
using AM.Web.AMHub;
using System.Threading.Tasks;
using System.IO;

namespace AM.Web.Controllers
{
    [Authorize(Roles = AMRoles.Admin + "," + AMRoles.BA + "," + AMRoles.Client + "," + AMRoles.ClientUserManager + "," + AMRoles.CoreLogicUserManager + "," + AMRoles.PnSOperator + "," + AMRoles.Delfus + "," + AMRoles.BAOffShore)]
    [RoutePrefix("AuthoMailHealth")]
    public class HomeController : BaseController
    {
        IHomeService homeSvc;
        IPullBackService pullBackSvc;
        IChartDataService chartDataSvc;
        ISearchService serSvc;
        public HomeController(IHomeService homeSvc, IPullBackService pullBackSvc, IChartDataService chartDataSvc, ISearchService serSvc)
        {
            this.homeSvc = homeSvc;
            this.pullBackSvc = pullBackSvc;
            this.chartDataSvc = chartDataSvc;
            this.serSvc = serSvc;
        }
        public ActionResult Index()
        {
            ViewBag.BusinessId = homeSvc.GetBusinessFriendlyIdNameByclientId(User.Identity.GetClientId().Value);
            return View();
        }

        [HttpPost]
        public ActionResult GetMailPieceIdByLoanNumber(string BusinessFriendlyIdName, string BusinessFriendlyIdValue)
        {
            try
            {
                var estToday = DateTime.UtcNow.UtcToEst().Date;
                // EST Start End
                var startDate = (estToday + new TimeSpan(0, 0, 0));
                var endDate = (estToday + new TimeSpan(23, 59, 59));
                var model = homeSvc.GetMailPieceIdFromBusinessFriendlyValue(BusinessFriendlyIdName, BusinessFriendlyIdValue, User.Identity.GetClientId().Value);

                if (model.Count > 1)
                {
                    return Json(JsonResponseFactory.SuccessResponse("PullBack"), JsonRequestBehavior.AllowGet);
                }
                else if (model.Count == 1)
                {
                    var data = pullBackSvc.GetPullBackRequestList(BusinessFriendlyIdName, BusinessFriendlyIdValue, null, startDate, endDate, null, User.Identity.GetClientId().Value, null);
                    //var data = Mapper.Map<List<MailPieceViewModel>>(pullbackRequestData);
                    return Json(JsonResponseFactory.SuccessResponse(new { model = data.OrderBy(s => s.ReceivedOn) }), JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(JsonResponseFactory.SuccessResponse("AdvancePullBack"), JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception e)
            {
                return Json(JsonResponseFactory.ErrorResponse(e.ToString()), JsonRequestBehavior.AllowGet);
            }

        }

        public ActionResult Search()
        {
            ViewBag.BusinessId = homeSvc.GetBusinessFriendlyIdNameByclientId(User.Identity.GetClientId().Value);
            ViewBag.DocType = new SelectList(homeSvc.GetDocTypeList(User.Identity.GetClientId().Value), "Id", "DocTypeName");
            return View();
        }
        [HttpGet]
        public ActionResult GetMailPieceByUniqueId(string mailPieceId)
        {
            var data = serSvc.GetMailPieceByUniqueId(new Guid(mailPieceId));
            var model = Mapper.Map<MailPieceDetailsViewModel>(data);
            return Json(JsonResponseFactory.SuccessResponse(new { model = model }), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult _GetSearchData(string BusinessFriendlyIdName, string BusinessFriendlyIdValue, string mailPieceUniqueId, string receivedOnStart, string receivedOnEnd, string status, string channel, List<int> docType, string trackingNumber, string clientBatchId, string batchId, string mailPieceId, List<string> errorCode)
        {
            try
            {
                int? mailBatchId = batchId.IsNullOrWhiteSpace() ? (int?)null : Convert.ToInt32(batchId);
                int? batchMailPieceId = mailPieceId.IsNullOrWhiteSpace() ? (int?)null : Convert.ToInt32(mailPieceId);
                var exactStartDate = receivedOnStart != string.Empty ? DateTime.ParseExact(receivedOnStart, "MM/dd/yy", CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces) : (DateTime?)null;
                var exactEndDate = receivedOnEnd != string.Empty ? DateTime.ParseExact(receivedOnEnd, "MM/dd/yy", CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces) : (DateTime?)null;
                var data = serSvc.GetMailPieceByParam(User.Identity.GetClientId().Value, BusinessFriendlyIdName, BusinessFriendlyIdValue, mailPieceUniqueId, exactStartDate, exactEndDate, docType, status, channel, clientBatchId, mailBatchId, trackingNumber, errorCode, false, batchMailPieceId);
                //ViewBag.ShippingCarrier = data.Value.
                

                if (data.Key != 0 && data.Value != null)
                {
                    data.Value.ForEach(a =>
                    {
                        a.IsWithinRetentionPeriod = CheckWithinRetentionPeriod(a.RetentionPeriod, Common.Now, a.ReceivedOn.Value, a.ClientId);
                    });
                }
                //var model = Mapper.Map<List<SearchViewModel>>(data);

                return Json(JsonResponseFactory.SuccessResponse(new { model = data }), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(JsonResponseFactory.ErrorResponse(e.ToString()), JsonRequestBehavior.AllowGet);
            }
        }

        private bool CheckWithinRetentionPeriod(int retentionDays, DateTime now, DateTime receivedOn, int clientId)
        {
            int days = retentionDays;
            DateTime startDate = now;
            do
            {
                startDate = startDate.AddDays(-1);
                days -= IsBusinessDay(startDate, Config.GetHolidaysByClientId(clientId), false, false) ? 1 : 0;
            } while (0 != days);

            return startDate < receivedOn;
        }
        private static bool IsBusinessDay(DateTime date, List<Holiday> holidays, bool isSatWorkDay, bool isSunWorkDay)
        {
            if (holidays.Select(c => c.HolidayDate).Contains(date.Date))
                return false;

            if (date.DayOfWeek == DayOfWeek.Saturday)
                return isSatWorkDay;

            if (date.DayOfWeek == DayOfWeek.Sunday)
                return isSunWorkDay;

            return true;
        }

        [HttpPost]
        public ActionResult _DownloadSearchData(string BusinessFriendlyIdName, string BusinessFriendlyIdValue, string mailPieceUniqueId, string receivedOnStart, string receivedOnEnd, string status, string channel, List<int> docType, string trackingNumber, string clientBatchId, string batchId, string mailPieceId, List<string> errorCode, int timezoneOffset, string clientName)
        {
            try
            {
                int? mailBatchId = batchId.IsNullOrWhiteSpace() ? (int?)null : Convert.ToInt32(batchId);
                int? batchMailPieceId = mailPieceId.IsNullOrWhiteSpace() ? (int?)null : Convert.ToInt32(mailPieceId);
                var exactStartDate = receivedOnStart != string.Empty ? DateTime.ParseExact(receivedOnStart, "MM/dd/yy", CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces) : (DateTime?)null;
                var exactEndDate = receivedOnEnd != string.Empty ? DateTime.ParseExact(receivedOnEnd, "MM/dd/yy", CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces) : (DateTime?)null;
                var model = serSvc.GetMailPieceByParam(User.Identity.GetClientId().Value, BusinessFriendlyIdName, BusinessFriendlyIdValue, mailPieceUniqueId, exactStartDate, exactEndDate, docType, status, channel, clientBatchId, mailBatchId, trackingNumber, errorCode, true, batchMailPieceId);
                if (model.Value == null)
                {
                    return Json(JsonResponseFactory.ErrorResponse(model.Key.ToString()), JsonRequestBehavior.AllowGet);
                }
                string fileGuid = Guid.NewGuid().ToString();
                //var model = Mapper.Map<List<SearchViewModel>>(data.Value);

                model.Value.ForEach(m =>
                {
                    m.ReceivedOn = m.ReceivedOn.HasValue ? m.ReceivedOn.Value.AddMinutes(-timezoneOffset) : m.ReceivedOn;
                    m.EsignedOn = m.Status == "eSigned" ? m.ShippedOn.Value.AddMinutes(-timezoneOffset) : m.EsignedOn;
                    m.ShippedOn = m.Status != "eSigned" && m.ShippedOn.HasValue ? m.ShippedOn.Value.AddMinutes(-timezoneOffset) : (DateTime?)null;
                    m.PickedUpOn = m.PickedUpOn.HasValue ? m.PickedUpOn.Value.AddMinutes(-timezoneOffset) : m.PickedUpOn;
                    m.DueDate = m.DueDate.HasValue ? m.DueDate.Value.AddMinutes(-timezoneOffset) : m.DueDate;
                    m.ShipmentStatusUpdatedOn = m.ShipmentStatusUpdatedOn.HasValue ? m.ShipmentStatusUpdatedOn.Value.AddMinutes(-timezoneOffset) : m.ShipmentStatusUpdatedOn;
                    m.IsESignRequested = m.ExpiryDate.HasValue ? true : false;
                    m.ImpressionCount = m.Channel.ToLower().Contains("credco") ? (m.TotalSheets * 2).ToString() : "NA";
                });
                var dataTable = ExcelHelper.ToDataTable<SearchViewModel>(model.Value);


                List<string> edeliveryColumnsToDelete = new List<string>() { "IsEDeliveryEnabled", "ExpiryDate" };
                foreach (var column in edeliveryColumnsToDelete)
                    dataTable.Columns.Remove(column);


                if (User.IsInRole(AM.Entities.AMRoles.BAOffShore))
                {
                    List<string> columnsToDelete = new List<string>() { "FirstName", "LastName", "City", "ZIP", "State", "RequestedPdf" };
                    foreach (var column in columnsToDelete)
                        dataTable.Columns.Remove(column);
                }

                if (dataTable.Columns.Contains("LoanNumber") && !BusinessFriendlyIdName.IsNullOrWhiteSpace())
                {
                    dataTable.Columns["LoanNumber"].ColumnName = BusinessFriendlyIdName;
                }

                var memoryStream = ExcelHelper.ExportDataSet(dataTable);
                memoryStream.Seek(0, SeekOrigin.Begin);
                TempData[fileGuid] = memoryStream.ToArray();
                string fileName = string.Empty;
                if (!receivedOnStart.IsNullOrWhiteSpace())
                {
                    string[] fileNameArray = { "Search", exactStartDate.Value.ToString("MMddyyyy"), "To", exactEndDate.Value.ToString("MMddyyyy"), clientName };
                    fileName = string.Join("_", fileNameArray);
                }
                else
                {
                    string[] fileNameArray = { "Search", clientName };
                    fileName = string.Join("_", fileNameArray);
                }
                fileName = string.Format("{0}.xlsx", fileName);
                return Json(JsonResponseFactory.SuccessResponse(new { model = new { FileGuid = fileGuid, FileName = fileName } }), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(JsonResponseFactory.ErrorResponse(e.ToString()), JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult DownloadSearch(string fileGuid, string fileName)
        {
            if (TempData[fileGuid] != null)
            {
                byte[] data = TempData[fileGuid] as byte[];
                return File(data, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            else
            {
                return new EmptyResult();
            }

        }
        [HttpPost]
        public ActionResult TodayChartDetails(string status, int batchId)
        {
            var data = chartDataSvc.GetMailPieceByParam(batchId, status, User.Identity.GetClientId().Value);

            var model = Mapper.Map<List<MailPieceViewModel>>(data);
            return Json(JsonResponseFactory.SuccessResponse(new { model = model.OrderByDescending(s => s.ReceivedOn) }), JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetMailPieceByBatchId(int batchId)
        {
            var data = chartDataSvc.GetMailPieceByBatchId(batchId, User.Identity.GetClientId().Value);
            var model = Mapper.Map<List<MailPieceViewModel>>(data);
            return Json(JsonResponseFactory.SuccessResponse(new { model = model.OrderByDescending(s => s.ReceivedOn) }), JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult WeeklyChartDetails(string status, string date)
        {
            var exactDate = DateTime.ParseExact(date, "MM/dd/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces);
            var model = chartDataSvc.GetBatchByParam(exactDate, status, User.Identity.GetClientId().Value);
            return Json(JsonResponseFactory.SuccessResponse(new { model = model.OrderByDescending(s => s.ReceivedOn) }), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult GetMailPieceDataByStatusAndBatchId(string status, int batchId)
        {
            var model = chartDataSvc.GetMailPieceByParam(batchId, status, User.Identity.GetClientId().Value);
            return Json(model.OrderByDescending(s => s.MailBatches.Id).ToList(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult DailySummaryChartDetails(string status, string docTypeCode)
        {
            var estToday = DateTime.UtcNow.UtcToEst().Date;
            var data = chartDataSvc.GetMailPiecesByDocTypeIdAndStatus(estToday, status, User.Identity.GetClientId().Value, docTypeCode);
            var model = Mapper.Map<List<MailPieceViewModel>>(data);
            return Json(JsonResponseFactory.SuccessResponse(new { model = model.OrderByDescending(s => s.ReceivedOn) }), JsonRequestBehavior.AllowGet);
        }


        [AllowAnonymous, Route("37f9ac3e-40b3-4ae8-ac9a-d84634638065")]
        public ActionResult AMHealth()
        {
            return View();
        }
        //[AllowAnonymous]
        //[HttpGet, Route("getamstatusdetail")]
        //public JsonResult AMStatusDetail()
        //{
        //    try
        //    {
        //        return Json(AMAlert.Instance.GetGlobalStatus, JsonRequestBehavior.AllowGet);
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //        //return Json(GetData(), JsonRequestBehavior.AllowGet);
        //    }
        //}
        //[HttpGet, Route("getamstatus")]
        //public JsonResult AMStatus()
        //{
        //    try
        //    {
        //        return Json(AMAlert.Instance.GetAMHealth, JsonRequestBehavior.AllowGet);
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //        //var _model = GetData();
        //        //return Json(new AMHealth { AlertLevel = _model.AlertLevel, IsAMDelay = _model.IsAMDelay, LastUpdatedOn = _model.LastUpdatedOn }, JsonRequestBehavior.AllowGet);
        //    }
        //}
        //private AMGlobalStatus GetData()
        //{
        //    using (IMonitorBatchesService alertSvc = new MonitorBatchesService())
        //    {
        //        var data = alertSvc.GetPendingBatchesToSubmit(30);
        //        return new AMGlobalStatus { DelayedProcessBatches = data.DelayedBatches.Count(), DelayedProcessMailPieces = data.DelayedMailPieces.Count(), DelayedAMReportBackBatches = data.DelayedAMReportBackBatches.Count(), DelayedAMReportBackMailPieces = data.DelayedAMReportBackMailPieces.Count(), DelayedPrinterBatches = data.DelayedPrinterBatches.Count(), DelayedPickUpMailPieces = data.DelayedPrinterMailPieces.Count(), MinHour = data.MinHour, MaxHour = data.MaxHour, LastBatchReceivedOn = data.LastBatchReceivedOn, LastUpdatedOn = data.LastUpdatedOn, IsReportBackDelay = data.IsReportBackDelay, IsPrinterDelay = data.IsPrinterDelay, IsReceivedDelay = data.IsReceivedDelay, IsProcessDelay = data.IsProcessDelay, AlertLevel = data.AlertLevel, IsAMDelay = data.IsAMDelay };
        //    }
        //}
    }
}