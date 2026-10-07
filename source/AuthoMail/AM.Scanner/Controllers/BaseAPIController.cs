using AM.Scanner.Common;
using AM.Scanner.Models;
using AM.Services;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;


namespace AM.Scanner.Controllers
{

    [RoutePrefix("api/report")]
    //[Authorize]
    public class BaseAPIController : ApiController
    {
        IScannerService scannerSvc;
        public BaseAPIController()
        {
            this.scannerSvc = new ScannerService();
        }
        [HttpGet]
        [Route("ScanedDocumentList")]
        public List<Services.ScanedDocument> ScanedDocumentList(string clientCode, string barcodeValue, string loanNumber, string printOnFrom, string printOnTo, string shippedOnFrom, string shippedOnTo)
        {
            using (new PerfTimerLogger("ScanedDocumentList[BaseAPIController]: "))
            {
                var model = scannerSvc.GetScanReportData(clientCode, barcodeValue, loanNumber, printOnFrom, printOnTo, shippedOnFrom, shippedOnTo);
                return model;
            }
        }

        [HttpGet]
        [Route("PendingScanningList")]
        public List<Services.PendingScanning> PendingScanningList(string clientCode)
        {
            using (new PerfTimerLogger("PendingScanningList[BaseAPIController]: "))
            {
                var model = scannerSvc.GetPendingScanning(clientCode);
                return model;
            }
        }

        [HttpGet]
        [Route("GetAllBatchForScan1")]
        public List<AppCodeBatchDetails> GetAllBatchForScan()
        {
            using (new PerfTimerLogger("GetAllBatchForScan[BaseAPIController]: "))
            {
                var model = scannerSvc.GetRecentBatchesStats();
                List<AppCodeBatchDetails> appCodeBatchDetails = new List<AppCodeBatchDetails>();
                foreach (var appCode in model.GroupBy(s => s.AppCode))
                {
                    AppCodeBatchDetails appCodeBatchDetail = new AppCodeBatchDetails();
                    appCodeBatchDetail.AppCode = appCode.Key;
                    appCodeBatchDetail.Batches = new List<Services.GetAllBatchCountDetails>();
                    appCodeBatchDetail.Batches.AddRange(appCode.ToList());
                    appCodeBatchDetails.Add(appCodeBatchDetail);
                }
                return appCodeBatchDetails;
            }
        }
        [HttpGet]
        [Route("GetAllBatchForScan")]
        public object DashBoard()
        {
            using (new PerfTimerLogger("GetAllBatchForScan[BaseAPIController]: "))
            {
                var headers = new List<string>() { "Client", "Status", "IsBulk", "Day Total" };

                var batchWistCounts = scannerSvc.GetRecentBatchesStats();
                var dayWiseCounts = scannerSvc.GetScanStats();
                var batches = batchWistCounts.Select(s => s.BatchName).Distinct().ToList();
                batches = BuildOrder(batches);
                headers.AddRange(batches);

                var objList = new List<object>();
                var clientList = new HashSet<string>(System.Web.Configuration.WebConfigurationManager.AppSettings["ClientList"].Split(','));
                var bulkClientList = new HashSet<string>(System.Web.Configuration.WebConfigurationManager.AppSettings["BulkClientList"].Split(','));
                var clientWiseBatches = batchWistCounts.Where(m => clientList.Contains(m.AppCode)).GroupBy(s => s.AppCode);
                foreach (var appCode in clientWiseBatches)
                {
                    var values = new List<string>();
                    values.Add(appCode.Key);

                    var daySummary = dayWiseCounts.ContainsKey(appCode.Key) ? dayWiseCounts[appCode.Key] : null;

                    string dayTotal = null;
                    string status = null;

                    if (daySummary == null || daySummary.TotalCount == 0)
                    {
                        dayTotal = "0/0";
                        status = "No Production";
                    }
                    else if (daySummary.ScanCount != daySummary.TotalCount)
                    {
                        dayTotal = $"{daySummary.ScanCount}/{daySummary.TotalCount}";
                        status = "Production Incomplete";
                    }
                    else
                    {
                        dayTotal = $"{daySummary.ScanCount}/{daySummary.TotalCount}";
                        status = "Production Complete";
                    }

                    values.Add(status);
                    values.Add(bulkClientList.Contains(appCode.Key).ToString().ToLower());
                    values.Add(dayTotal);
                    foreach (var batch in batches)
                    {
                        var b = appCode.SingleOrDefault(a => a.BatchName == batch);
                        values.Add(b == null ? "--" : $"{b.ScannedLoans}/{b.TotalLoans}");
                    }

                    objList.Add(values);
                }

                return new
                {
                    headers = headers,
                    data = objList
                };
            }
        }
        private List<string> BuildOrder(List<string> batches)
        {
            //var dict = new Dictionary<string, TimeSpan>();
            //var list = new List<string>() { "Backlog" };

            //foreach (var batch in batches.Where(b => b != "Backlog"))
            //{
            //    DateTime dateTime = DateTime.ParseExact(batch,
            //                        "hhtt", CultureInfo.InvariantCulture);
            //    dict.Add(batch, dateTime.TimeOfDay);
            //}

            batches = batches
                .OrderBy(b =>
                {
                    if (b == "Backlog")
                        return DateTime.Today;
                    else
                        return DateTime.ParseExact(b, "hhtt", CultureInfo.InvariantCulture);
                }).ToList();
            return batches;

            //list.AddRange(dict.OrderBy(d => d.Value).Select(d => d.Key));
            //return list;

        }
        private static Dictionary<string, string> GetProperties(object obj)
        {
            var props = new Dictionary<string, string>();
            if (obj == null)
                return props;

            var type = obj.GetType();
            foreach (var prop in type.GetProperties())
            {
                var val = prop.GetValue(obj, new object[] { });
                var valStr = val == null ? "" : val.ToString();
                props.Add(prop.Name, valStr);
            }

            return props;
        }
    }
}
