using AM.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AM.Entities;
using AM.Web.Models;
using System.Globalization;
using AM.Lib;
using AutoMapper;
using System.IO;
using log4net;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.Identity;
using Microsoft.Owin;
using AM.Web;
namespace AM.Web.Controllers
{
    [Authorize(Roles = AMRoles.Admin + "," + AMRoles.BA + "," + AMRoles.Client + "," + AMRoles.ClientUserManager + "," + AMRoles.CoreLogicUserManager + "," + AMRoles.PnSOperator + "," + AMRoles.Delfus + "," + AMRoles.BAOffShore)]
    public class ReportController : BaseController
    {
        IHomeService homeSvc;
        IReportService repoSvc;
        IInventoryService invSvc;
        private readonly AMUserManager userManager;

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        public ReportController(IHomeService homeSvc, IReportService repoSvc, IInventoryService invSvc, AMUserManager userManager)
        {
            this.homeSvc = homeSvc;
            this.repoSvc = repoSvc;
            this.invSvc = invSvc;
            this.userManager = userManager;
        }
        public ActionResult SummaryControlReport()
        {
            return View();
        }
        [HttpPost]
        public ActionResult _GetSummaryControlReport(string startDate, string endDate, string channel)
        {
            var exactStartDate = DateTime.ParseExact(startDate, "MM/dd/yy", CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces);
            var exactEndDate = DateTime.ParseExact(endDate, "MM/dd/yy", CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces);
            var data = repoSvc.GetSummaryControlReport(User.Identity.GetClientId().Value, exactStartDate, exactEndDate, channel);

            var reportViewModel = GetBatchNameAndTime(data);
            var model = Mapper.Map<List<SummaryControlViewModel>>(data);

            return Json(JsonResponseFactory.SuccessResponse(new { BatchList = reportViewModel, model = model.OrderByDescending(s => s.ReceivedOn) }), JsonRequestBehavior.AllowGet);
        }

        private static List<ReportViewModel> GetBatchNameAndTime(List<MailPieces> model)
        {
            List<ReportViewModel> reportViewModel = new List<ReportViewModel>();
            foreach (var item in model)
            {
                var reportModel = (new ReportViewModel
                {
                    Key = item.MailBatches.Id,
                    Value = string.Format("{0} - {1}", item.MailBatches.ReceivedOn.Value.ToString("H:mm"), item.MailBatches.BatchName)
                });
                if (!reportViewModel.Any(s => s.Value == reportModel.Value))
                {
                    reportViewModel.Add(reportModel);
                }
            }
            return reportViewModel;
        }

        public ActionResult MonthlyReconReport()
        {
            ViewBag.dateList = repoSvc.GetMonthlyReconDate(User.Identity.GetClientId().Value);
            ViewBag.BusinessId = homeSvc.GetBusinessFriendlyIdNameByclientId(User.Identity.GetClientId().Value).FirstOrDefault();
            return View();
        }
        [HttpPost]
        public ActionResult _MonthReconReport(string loanNumber, string docType, string startDate, string endDate, string channel)
        {
            var exactStartDate = DateTime.ParseExact(startDate, "MM/dd/yy", CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces);
            var exactEndDate = DateTime.ParseExact(endDate, "MM/dd/yy", CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces);

            var data = repoSvc.GetMonthlyReconReport(User.Identity.GetClientId().Value, exactStartDate, exactEndDate, channel);
            //var model=Mapper.Map<List<MonthlyReconReportViewModel>>(data);
            return Json(JsonResponseFactory.SuccessResponse(new { model = data.OrderByDescending(s => s.ReceivedOn) }), JsonRequestBehavior.AllowGet);

        }

        public ActionResult DownloadMonthlyReconReport(string startDate, string endDate, string channel, string fileName, int timezoneOffset, string buId)
        {
            var exactStartDate = DateTime.ParseExact(startDate, "MM/dd/yy", CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces);
            var exactEndDate = DateTime.ParseExact(endDate, "MM/dd/yy", CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces);
            var data = repoSvc.GetMonthlyReconReport(User.Identity.GetClientId().Value, exactStartDate, exactEndDate, channel);
            var model = Mapper.Map<List<MonthlyReconReportViewModel>>(data);
            model.ForEach(m =>
            {
                m.ReceivedOn = m.ReceivedOn.HasValue ? m.ReceivedOn.Value.AddMinutes(-timezoneOffset) : m.ReceivedOn;
                m.ShippedOn = m.ShippedOn.HasValue ? m.ShippedOn.Value.AddMinutes(-timezoneOffset) : m.ShippedOn;
                m.SubmittedOn = m.SubmittedOn.HasValue ? m.SubmittedOn.Value.AddMinutes(-timezoneOffset) : m.SubmittedOn;
                m.PulledBackOn = m.PulledBackOn.HasValue ? m.PulledBackOn.Value.AddMinutes(-timezoneOffset) : m.PulledBackOn;
                m.DeliveryDate = m.DeliveryDate.HasValue ? m.DeliveryDate.Value.AddMinutes(-timezoneOffset) : m.DeliveryDate;
            });
            var dataTable = ExcelHelper.ToDataTable<MonthlyReconReportViewModel>(model);

            dataTable.Columns.Remove("BlankPageNumbersList");
            dataTable.Columns.Remove("LetterPageNumbers");
            dataTable.Columns.Remove("LegalPageNumbers");
            dataTable.Columns.Remove("BlankLetterPages");
            dataTable.Columns.Remove("BlankLegalPages");
            dataTable.AcceptChanges();


            if (User.IsInRole(AM.Entities.AMRoles.BAOffShore))
            {
                List<string> columnsToDelete = new List<string>() { "FirstName", "LastName", "Address", "City", "ZIP", "State" };
                foreach (var column in columnsToDelete)
                    dataTable.Columns.Remove(column);
            }

            if (!buId.IsNullOrWhiteSpace())
            {
                dataTable.Columns["LoanNumber"].ColumnName = buId;
                dataTable.AcceptChanges();
            }


            var memoryStream = ExcelHelper.ExportDataSet(dataTable);
            memoryStream.Seek(0, SeekOrigin.Begin);

            var fileNameArry = new string[] { fileName, exactStartDate.ToString("MMddyyy"), "TO", exactEndDate.ToString("MMddyyy"), channel };
            fileName = string.Join("_", fileNameArry.Where(s => s != "").ToArray());
            fileName = string.Format("{0}.xlsx", fileName);
            return File(memoryStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        public ActionResult ErrorsReport()
        {
            return View();
        }
        [HttpPost]
        public ActionResult _GetErrorsReport(string startDate, string endDate, string channel)
        {
            try
            {
                var exactStartDate = DateTime.ParseExact(startDate, "MM/dd/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces);
                var exactEndDate = DateTime.ParseExact(endDate, "MM/dd/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces);
                var data = repoSvc.GetErrorReportByDateOrBatch(User.Identity.GetClientId().Value, exactStartDate, exactEndDate, null, channel);
                var reportViewModel = GetBatchNameAndTime(data);
                var model = Mapper.Map<List<ErrorReportViewModel>>(data);
                return Json(JsonResponseFactory.SuccessResponse(new { BatchList = reportViewModel, model = model.OrderByDescending(s => s.ReceivedOn) }), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(JsonResponseFactory.ErrorResponse(e.ToString()), JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult ErrorSLAReport()
        {
            ViewBag.DocType = new SelectList(homeSvc.GetDocTypeList(User.Identity.GetClientId().Value), "Id", "DocTypeName");
            return View();
        }
        [HttpPost]
        public ActionResult _GetErrorsSLAReport(string startDate, string endDate, string filterBy, int? docType)
        {
            var exactStartDate = DateTime.ParseExact(startDate, "MM/dd/yy", CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces);
            var exactEndDate = DateTime.ParseExact(endDate, "MM/dd/yy", CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces);
            //Update startDate and endDate
            var data = repoSvc.GetSLAMissErrorReportByDateOrBatch(User.Identity.GetClientId().Value, exactStartDate, exactEndDate, null, docType, filterBy);
            var reportViewModel = GetBatchNameAndTime(data);
            var model = Mapper.Map<List<ErrorSLAReportViewModel>>(data);
            return Json(JsonResponseFactory.SuccessResponse(new { BatchList = reportViewModel, model = model.OrderByDescending(s => s.ReceivedOn) }), JsonRequestBehavior.AllowGet);
        }
        public ActionResult DailyDetailOperationsReport()
        {
            return View();
        }
        [HttpPost]
        public ActionResult _GetDailyDetailOperationsReport(string date, string channel)
        {
            try
            {
                var exactDate = DateTime.ParseExact(date, "MM/dd/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces);
                var data = repoSvc.GetDailyDetailOperationsByDateOrBatch(User.Identity.GetClientId().Value, exactDate, null, channel, ShippingStatus.Shipped.ToString());
                var reportViewModel = GetBatchNameAndTime(data);
                var model = Mapper.Map<List<DailyDetailReportViewModel>>(data);
                return Json(JsonResponseFactory.SuccessResponse(new { BatchList = reportViewModel, model = model.OrderByDescending(s => s.ReceivedOn) }), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(JsonResponseFactory.ErrorResponse(e.ToString()), JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult PullbackRequestReport()
        {
            var estToday = DateTime.UtcNow.UtcToEst().Date;
            // EST Week
            ViewBag.startDate = (estToday.AddDays(-7)).ToString("MM/dd/yy HH:mm:ss");
            ViewBag.endDate = (estToday + new TimeSpan(23, 59, 59)).ToString("MM/dd/yy HH:mm:ss");
            return View();
        }
        [HttpPost]
        public ActionResult _GetPullbackRequestReport(string startDate, string endDate)
        {
            var exactStartDate = DateTime.ParseExact(startDate, "MM/dd/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces);
            var exactEndDate = DateTime.ParseExact(endDate, "MM/dd/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces);
            var model = repoSvc.GetPullbackRequestReport(User.Identity.GetClientId().Value, exactStartDate, exactEndDate);
            return Json(JsonResponseFactory.SuccessResponse(new { model = model.OrderByDescending(s => s.RequestedOn) }), JsonRequestBehavior.AllowGet);
        }
        [Authorize(Roles = AMRoles.Admin + "," + AMRoles.BA + "," + AMRoles.BAOffShore)]
        public ActionResult SupplyInventoryReport()
        {
            ViewBag.Envelopes = new SelectList(invSvc.GetEnvelopes(), "Id", "EnvelopeName");
            return View();
        }
        [HttpPost, Authorize(Roles = AMRoles.Admin + "," + AMRoles.BA + "," + AMRoles.BAOffShore)]
        public ActionResult _SupplyInventoryReport(List<string> envelopes)
        {
            var model = repoSvc.SupplyInventoryReport(envelopes);
            return Json(JsonResponseFactory.SuccessResponse(new { model = model }), JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public ActionResult ReportWizard()
        {
            ViewBag.BusinessId = homeSvc.GetBusinessFriendlyIdNameByclientId(User.Identity.GetClientId().Value);
            ViewBag.DocType = new SelectList(homeSvc.GetDocTypeList(User.Identity.GetClientId().Value), "Id", "DocTypeName");
            if (User.IsInRole(AMRoles.Admin) || User.IsInRole(AMRoles.BA) || User.IsInRole(AMRoles.BAOffShore))
            {
                var list = new List<SelectListItem>();
                var clientGroup = homeSvc.GetAllActiveClient().GroupBy(s => s.GroupAlias);
                foreach (var group in clientGroup)
                {
                    var groupName = new SelectListGroup { Name = group.Key };
                    foreach (var value in group)
                    {
                        SelectListItem item = new SelectListItem { Text = value.Name, Value = value.Id.ToString(), Group = groupName };
                        list.Add(item);
                    }
                }
                //ViewBag.ClientId = new SelectList(homeSvc.GetAllActiveClient(), "Id", "Name");
                ViewBag.ClientId = list;
            }
            else
            {
                ViewBag.ClientId = User.Identity.GetClientId();// new SelectList(homeSvc.GetAllActiveClient().Where(s => s.Id == User.Identity.GetClientId()), "Id", "Name", );
            }
            return View();
        }
        [HttpPost]
        public JsonResult _ReportWizard(List<int> clientId, string businessFriendlyIdName, string businessFriendlyIdValue, DateTime? startDate, DateTime? endDate, List<int> docType, string status, string channel, List<string> shippingMethod, string trackingNo)
        {

            try
            {
                var result = repoSvc.GetReportWizard(clientId, businessFriendlyIdName, businessFriendlyIdValue, startDate, endDate, docType, status, channel, shippingMethod, trackingNo);
                return Json(JsonResponseFactory.SuccessResponse(new { model = result }), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(JsonResponseFactory.ErrorResponse(ex.Message), JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost]
        public async Task<ActionResult> _DownloadReportWizard(List<int> clientId, string businessFriendlyIdName, string businessFriendlyIdValue, string startDate, string endDate, List<int> docType, string status, string channel, List<string> shippingMethod, string trackingNo, int timezoneOffset)
        {
            try
            {
                var exactStartDate = startDate != string.Empty ? DateTime.ParseExact(startDate, "MM/dd/yy", CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces) : (DateTime?)null;
                var exactEndDate = endDate != string.Empty ? DateTime.ParseExact(endDate, "MM/dd/yy", CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces) : (DateTime?)null;

                var result = repoSvc.GetReportWizard(clientId, businessFriendlyIdName, businessFriendlyIdValue, exactStartDate, exactEndDate, docType, status, channel, shippingMethod, trackingNo);

                result.ForEach(m =>
                {
                    m.ReceivedOn = m.ReceivedOn.HasValue ? m.ReceivedOn.Value.AddMinutes(-timezoneOffset) : m.ReceivedOn;
                    m.PrintedOn = m.PrintedOn.HasValue ? m.PrintedOn.Value.AddMinutes(-timezoneOffset) : m.PrintedOn;
                    m.MailedOn = m.MailedOn.HasValue ? m.MailedOn.Value.AddMinutes(-timezoneOffset) : m.MailedOn;
                    m.DueDate = m.DueDate.HasValue ? m.DueDate.Value.AddMinutes(-timezoneOffset) : m.DueDate;
                    m.DeliveryDate = m.DeliveryDate.HasValue ? m.DeliveryDate.Value.AddMinutes(-timezoneOffset) : m.DeliveryDate;
                    m.ReturnDeliveryDate = m.ReturnDeliveryDate.HasValue ? m.ReturnDeliveryDate.Value.AddMinutes(-timezoneOffset) : m.ReturnDeliveryDate;
                    m.ImpressionCount = m.Channel.ToLower().Contains("credco") && (m.PagesShipped % 2 == 0) 
                                        ? m.PagesShipped.ToString()
                                       : m.Channel.ToLower().Contains("credco") ? (m.PagesShipped + 1).ToString() 
                                       : "NA";
                });
                var dataTable = ExcelHelper.ToDataTable<ReportWizard>(result);

                if (User.IsInRole(AM.Entities.AMRoles.BAOffShore))
                {
                    List<string> columnsToDelete = new List<string>() { "FirstName", "LastName", "RcptAddressStreet", "City", "ZIP", "State" };
                    foreach (var column in columnsToDelete)
                        dataTable.Columns.Remove(column);
                }

                List<Client> floodClients;
                List<Client> gosClients;
                using (IClientService clientSvc = new ClientService())
                {
                    floodClients = clientSvc.GetClientsByAlias("Flood Integration");
                    gosClients = clientSvc.GetClientsByAlias("GOS-Print Ready");
                }
                if (!floodClients.Select(x => x.Id).Intersect(clientId).Any())
                {
                    dataTable.Columns.Remove("RequestorName");
                    dataTable.Columns.Remove("PropertyStreetAddress");
                    dataTable.Columns.Remove("PropertyCity");
                    dataTable.Columns.Remove("PropertyState");
                    dataTable.Columns.Remove("PropertyZip");
                    dataTable.Columns.Remove("Flood_Determination_Date");
                }

                if (!gosClients.Select(x => x.Id).Intersect(clientId).Any())
                {
                    dataTable.Columns.Remove("ProductCode");
                }

                if (dataTable.Columns.Contains("LoanNumber") && !businessFriendlyIdName.IsNullOrWhiteSpace())
                {
                    dataTable.Columns["LoanNumber"].ColumnName = businessFriendlyIdName;
                }

                var memoryStream = ExcelHelper.ExportDataSet(dataTable);
                log.Info("Excel file generated successfully.");
                memoryStream.Seek(0, SeekOrigin.Begin);
                log.Info("Generated report size: " + memoryStream.Length);


                string fileGuid = Guid.NewGuid().ToString();
                TempData[fileGuid] = memoryStream.ToArray();
                string fileName = string.Empty;
                if (!startDate.IsNullOrWhiteSpace())
                {
                    string[] fileNameArray = { "ReportWizard", exactStartDate.Value.ToString("MMddyyyy"), "To", exactEndDate.Value.ToString("MMddyyyy") };
                    fileName = string.Join("_", fileNameArray);
                }
                else
                {
                    string[] fileNameArray = { "ReportWizard", DateTime.UtcNow.AddMinutes(-timezoneOffset).ToString("MMddyyyy") };
                    fileName = string.Join("_", fileNameArray);
                }
                fileName = string.Format("{0}.xlsx", fileName);
                var attachmentData = TempData[fileGuid] as byte[];

                var userId = User.Identity.GetUserId();
                var user = await userManager.FindByIdAsync(userId);
                var userEmail = user.Email;

                if (attachmentData != null)
                {
                    string to = userEmail;
                    string subject = $"{Config.Common_Environment} : Report Generated Through Report Wizard"; 
                    string body = "Please find the generated report attached."; 

                    await InternalEmail.SendMail(to,subject, body, attachmentData, fileName);
                }

                return Json(new { Success = true, message = "Report generated and emailed successfully." });

            }
            catch (Exception e)
            {
                log.Error("Error in generating report: " + e.Message, e);
                return Json(JsonResponseFactory.ErrorResponse(e.Message.ToString()), JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult DownloadReportWizard(string fileGuid, string fileName)
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

        public ActionResult PVReconReport()
        {
            if (User.IsInRole(AMRoles.Admin))
            {
                var list = new List<SelectListItem>();
                var clientGroup = homeSvc.GetAllActiveClient().GroupBy(s => s.GroupAlias);
                foreach (var group in clientGroup)
                {
                    var groupName = new SelectListGroup { Name = group.Key };
                    foreach (var value in group)
                    {
                        SelectListItem item = new SelectListItem { Text = value.Name, Value = value.Id.ToString(), Group = groupName };
                        list.Add(item);
                    }
                }
                ViewBag.ClientId = list;
            }
            else
            {
                ViewBag.ClientId = User.Identity.GetClientId();
            }

            return View();
        }

        [HttpPost]
        public async Task<ActionResult> _DownloadPVReconReport(string printVendorCode, string startDate, string endDate, string fileName, int timezoneOffset, List<int> clientIds, string status, List<string> shippingMethods)
        {
            //Commented this to set start and end date to match NORDIS cutoff.
            //var exactStartDate = DateTime.ParseExact(startDate, "MM/dd/yy", CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces).ToStartDateTime().EstToUtc();
            //var exactEndDate = DateTime.ParseExact(endDate, "MM/dd/yy", CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces).ToEndDateTime().EstToUtc();
            
            //Set start and end date to match NORDIS cutoff.
            var exactStartDate = DateTime.ParseExact(startDate, "MM/dd/yy", CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces).ToStartDateTime();
            var exactEndDate = DateTime.ParseExact(endDate, "MM/dd/yy", CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces).ToEndDateTime();

            var data = repoSvc.GetPVReconReport(printVendorCode, exactStartDate.EstToUtc(), exactEndDate.EstToUtc(), clientIds, status, shippingMethods);
            var model = Mapper.Map<List<MonthlyReconReportViewModel>>(data);
            model.ForEach(m =>
            {
                m.ReceivedOn = m.ReceivedOn.HasValue ? m.ReceivedOn.Value.AddMinutes(-timezoneOffset) : m.ReceivedOn;
                m.ShippedOn = m.ShippedOn.HasValue ? m.ShippedOn.Value.AddMinutes(-timezoneOffset) : m.ShippedOn;
                m.SubmittedOn = m.SubmittedOn.HasValue ? m.SubmittedOn.Value.AddMinutes(-timezoneOffset) : m.SubmittedOn;
                m.PulledBackOn = m.PulledBackOn.HasValue ? m.PulledBackOn.Value.AddMinutes(-timezoneOffset) : m.PulledBackOn;
                m.DeliveryDate = m.DeliveryDate.HasValue ? m.DeliveryDate.Value.AddMinutes(-timezoneOffset) : m.DeliveryDate;
                m.ImpressionCount = m.Channel.ToLower().Contains("credco") ? (m.TotalSheets * 2).ToString() : "NA";
            });
            var dataTable = ExcelHelper.ToDataTable<MonthlyReconReportViewModel>(model);
            if (User.IsInRole(AM.Entities.AMRoles.BAOffShore))
            {
                List<string> columnsToDelete = new List<string>() { "FirstName", "LastName", "Address", "City", "ZIP", "State" };
                foreach (var column in columnsToDelete)
                    dataTable.Columns.Remove(column);
            }
            var memoryStream = ExcelHelper.ExportDataSet(dataTable);
            memoryStream.Seek(0, SeekOrigin.Begin);

            string fileGuid = Guid.NewGuid().ToString();
            TempData[fileGuid] = memoryStream.ToArray();

            if (!startDate.IsNullOrWhiteSpace())
            {
                string[] fileNameArray = { "PrintVendorRecon Report", exactStartDate.ToString("MMddyyyy"), "To", exactEndDate.ToString("MMddyyyy") };
                fileName = string.Join("_", fileNameArray);
            }
            else
            {
                string[] fileNameArray = { "PrintVendorRecon Report", DateTime.UtcNow.AddMinutes(-timezoneOffset).ToString("MMddyyyy") };
                fileName = string.Join("_", fileNameArray);
            }


            fileName = string.Format("{0}.xlsx", fileName);
            var attachmentData = TempData[fileGuid] as byte[];

            var userId = User.Identity.GetUserId();
            var user = await userManager.FindByIdAsync(userId);
            var userEmail = user.Email;

            if (attachmentData != null)
            {
                string to = userEmail;
                string subject = $"{Config.Common_Environment} : Print Vendor Recon report generated";
                string body = "Please find the generated report attached.";

                await InternalEmail.SendMail(to, subject, body, attachmentData, fileName);
            }

            return Json(new { Success = true, message = "Report generated and emailed successfully." });
        }

        public ActionResult DownloadPVReconReport(string fileGuid, string fileName)
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
        public ActionResult EnvelopesUsageReport()
        {
            ViewBag.Envelopes = new SelectList(invSvc.GetEnvelopes(), "Id", "EnvelopeName");
            return View();
        }
        [HttpPost]
        public ActionResult _GetEnvelopesUsageReport(int? envelopeId, string printVendorCode, string startDate, string endDate)
        {
            var exactStartDate = DateTime.ParseExact(startDate, "MM/dd/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces);
            var exactEndDate = DateTime.ParseExact(endDate, "MM/dd/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces);
            var model = repoSvc.GetEnvelopesUsageReport(User.Identity.GetClientId().Value, envelopeId, printVendorCode, exactStartDate, exactEndDate);
            return Json(JsonResponseFactory.SuccessResponse(new { model = model.OrderByDescending(s => s.SubmittedOn) }), JsonRequestBehavior.AllowGet);
        }

        public ActionResult DuplicateReport()
        {
            return View();
        }
        [HttpPost]
        public ActionResult _GetDuplicateReport(string startDate, string endDate, string channel)
        {
            try
            {
                var exactStartDate = DateTime.ParseExact(startDate, "MM/dd/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces);
                var exactEndDate = DateTime.ParseExact(endDate, "MM/dd/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces);
                var data = repoSvc.GetDuplicateReportByDate(User.Identity.GetClientId().Value, exactStartDate, exactEndDate, channel);
                var model = Mapper.Map<List<MonthlyReconReportViewModel>>(data);
                return Json(JsonResponseFactory.SuccessResponse(new { model }), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(JsonResponseFactory.ErrorResponse(e.ToString()), JsonRequestBehavior.AllowGet);
            }
        }
    }
}
