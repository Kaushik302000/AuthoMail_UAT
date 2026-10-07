using AM.Entities;
using AM.Lib;
using AM.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace AM.Web.Controllers
{
    [Authorize(Roles = AMRoles.Admin + "," + AMRoles.EVault)]
    public class EVaultController : BaseController
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        IHomeService homeSvc;
        IEDeliveryService edvSvc;

        public EVaultController(IEDeliveryService edvSvc, IHomeService homeSvc)
        {
            this.homeSvc = homeSvc;
            this.edvSvc = edvSvc;
        }

        // GET: EVault
        [HttpGet]
        public ActionResult EVaultPackages()
        {
            ViewBag.DocType = new SelectList(homeSvc.GetDocTypeList(User.Identity.GetClientId().Value), "Id", "DocTypeName");
            ViewBag.BusinessId = homeSvc.GetBusinessFriendlyIdNameByclientId(User.Identity.GetClientId().Value);
            var estToday = DateTime.UtcNow.UtcToEst().Date;
            // EST Week
            ViewBag.startDate = (estToday + new TimeSpan(0, 0, 0)).ToString("MM/dd/yy");
            ViewBag.endDate = (estToday + new TimeSpan(23, 59, 59)).ToString("MM/dd/yy");

            return View();
        }

        [HttpGet]
        public async Task<ActionResult> DownloadEVaultPackage(string fileName, string appCode, int id)
        {
            try
            {
                var filePath = Path.Combine(Config.Common_AuthoMailRootFolder, HttpUtility.UrlEncode(appCode), id.ToString(), "EVault", HttpUtility.UrlEncode(fileName));
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Add("Id", "dc4c269c-0eea-424a-8be9-0cc75eb527e9");
                HttpResponseMessage response = new HttpResponseMessage();
                response = await client.GetAsync($@"{Config.Web_Pdf_API_Url}\DownloadEVaultPdf?fileName={filePath}");
                if (response.IsSuccessStatusCode)
                {
                    var stream = await response.Content.ReadAsByteArrayAsync();
                    var pdfFileName = Path.GetFileName(fileName);
                    return File(stream, "application/pdf", pdfFileName);
                }
                else
                {
                    return Json(JsonResponseFactory.ErrorResponse("File not found."));
                }
            }
            catch (Exception ex)
            {
                log.Error(ex.ToString());
                return Json(JsonResponseFactory.ErrorResponse(ex.Message));
            }
        }

        [HttpPost]
        public ActionResult _GetEVaultMailPieceList(string startDate, string endDate)
        {
            var exactStartDate = DateTime.ParseExact(startDate, "MM/dd/yy", CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces);
            var exactEndDate = DateTime.ParseExact(endDate, "MM/dd/yy", CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces);

            //Ref only. remove it. also check doctype is required or not.
            var model = edvSvc.GetESignedDocList(exactStartDate.ToStartDateTime().EstToUtc(), exactEndDate.ToEndDateTime().EstToUtc(), User.Identity.GetClientId().Value);

            model.ForEach(x =>
            {
                x.IsWithinRetentionPeriod = CheckWithinRetentionPeriod(x.RetentionPeriod, Common.Now, x.ReceivedOn.Value, x.ClientId.Value);
            });
            return Json(JsonResponseFactory.SuccessResponse(new { model = model.OrderByDescending(s => s.ReceivedOn) }), JsonRequestBehavior.AllowGet);
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
    }
}