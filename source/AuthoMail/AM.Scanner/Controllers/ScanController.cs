using AM.Scanner.Common;
using AM.Scanner.Models;
using AM.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace AM.Scanner.Controllers
{
    [Authorize]
    public class ScanController : Controller
    {
        IScannerService scannerSvc;
        // GET: Scan
        readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public ScanController()
        {
            this.scannerSvc = new ScannerService();
        }
        public ActionResult Index(string clientCode, string batchTime)
        {
            return View();
        }

        public string GetIPAddress()
        {
            try
            {
                IPHostEntry Host = default(IPHostEntry);
                string Hostname = null;
                Hostname = System.Environment.MachineName;
                Host = Dns.GetHostEntry(Hostname);
                string ip = string.Empty;
                foreach (IPAddress IP in Host.AddressList)
                {
                    if (IP.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    {
                        ip = Convert.ToString(IP);
                    }
                }
                return ip;
            }
            catch (Exception e)
            {
                logger.Error(e.Message);
                return string.Empty;
            }
        }

        [HttpPost]
        public ActionResult RegisterScan(string barcode, string clientCode, string batchName)
        {
            using (new PerfTimerLogger("RegisterScan[ScanController]: "))
            {
                string machineName = GetIPAddress();
                Services.ScanDetails scanDetails = scannerSvc.RegisterScan(barcode, clientCode, User.Identity.Name, machineName, batchName);
                return Json(new { model = scanDetails }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost]
        public ActionResult RegisterBulkScan(string barcode, string clientCode, string batchName)
        {
            using (new PerfTimerLogger("RegisterBulkScan[ScanController]: "))
            {
                string machineName = GetIPAddress();
                var scanDetails = scannerSvc.RegisterBulkScan(barcode, clientCode, User.Identity.Name, machineName, batchName);
                return Json(new { model = scanDetails }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public ActionResult GetScannedDocuments(string clientCode, string batchName)
        {
            using (new PerfTimerLogger("GetScannedDocuments[ScanController]: "))
            {
                var getScannedList = scannerSvc.GetScannedMailPieces(clientCode, batchName);
                return Json(new { model = getScannedList }, JsonRequestBehavior.AllowGet);
            }

        }

        public ActionResult GetScanCountByBatchName(string clientCode, string batchName)
        {
            using (new PerfTimerLogger("GetScanCountByBatchName[ScanController]: "))
            {
                var model = scannerSvc.GetRecentBatchesStatsWithAppCode(clientCode);
                Services.ScanDetails scanDetails = new Services.ScanDetails();
                scanDetails.AppCode = clientCode;
                if (model != null)
                {
                    var batchWiseCount = model.Where(s => s.BatchName == batchName).FirstOrDefault();
                    if (batchWiseCount != null)
                    {
                        scanDetails.ScanCount = Convert.ToInt32(batchWiseCount.ScannedLoans);
                        scanDetails.TotalCount = Convert.ToInt32(batchWiseCount.TotalLoans);
                    }
                    else
                    {
                        scanDetails.ScanCount = 0;
                        scanDetails.TotalCount = 0;
                    }

                }
                else
                {
                    scanDetails.ScanCount = 0;
                    scanDetails.TotalCount = 0;
                }
                return Json(scanDetails, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public ActionResult USPSScan(string clientCode)
        {
            return View();
        }

        [HttpPost]
        public ActionResult RegisterUSPSScan(int mailPieceId, string trackingNo, string clientCode)
        {
            Services.USPSScanDetails scanDetails = scannerSvc.RegisterUSPSScan(mailPieceId, trackingNo, clientCode);
            return Json(new { model = scanDetails }, JsonRequestBehavior.AllowGet);
        }

        #region SingleScan
        public ActionResult SingleScan(string clientCode, string batchTime)
        {
            logger.Info("RegisterScan SingleScan");
            return View();
        }
        #endregion SingleScan

        #region Bulk Scan
        public ActionResult BulkScan(string clientCode, string batchTime)
        {
            logger.Info("Bulk Scan");
            return View();
        }
        #endregion
    }
}