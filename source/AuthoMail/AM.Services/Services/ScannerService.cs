using AM.Lib;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AM.Services
{
    public interface IScannerService : IDisposable
    {
        ScanDetails RegisterScan(string barcode, string clientCode, string username, string machineName, string batchName);
        Dictionary<string, ScanDetails> GetScanStats();
        List<RegisterScanDetails> GetScannedMailPieces(string clientCode, string batchName);
        List<GetAllBatchCountDetails> GetRecentBatchesStats();
        List<GetAllBatchCountDetails> GetRecentBatchesStatsWithAppCode(string appCode);
        List<ScanedDocument> GetScanReportData(string clientCode, string barcodeValue, string loanNumber, string printOnFrom, string printOnTo, string shippedOnFrom, string shippedOnTo);
        List<PendingScanning> GetPendingScanning(string clientCode);
        USPSScanDetails RegisterUSPSScan(int mailPieceId, string trackingNo, string clientCode);
        List<RegisterScanDetails> RegisterBulkScan(string barcode, string clientCode, string username, string machineName, string batchName);
        MailPieceDetails GetDocumentInfo(int mailPieceId);
        MailPieceDetails GetDocumentInfo(string uniqueId);
    }
    public class ScannerService : ServiceBase, IScannerService
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public ScanDetails RegisterScan(string barcode, string clientCode, string username, string machineName, string batchName)
        {
            barcode = barcode.Replace(clientCode, "");
            try
            {
                var estToday = DateTime.UtcNow.UtcToEst().Date;
                string query = string.Format("{0} @barcode, @clientCode, @operatorid, @machineid, @batchName, @estDate", "SP_RegisterScan");
                var registerScanData = db.QuerySingleOrDefault<RegisterScanDetails>(query, new { barcode = barcode, clientCode = clientCode, operatorid = username, machineid = machineName, batchName = batchName, estDate = estToday });
                // var batchWiseCount =GetRecentBatchesListStats(clientCode,db).Single(b => b.BatchName == batchName);
                db.Dispose();

                var scanCountData = new ScanDetails();// GetScanStats(clientCode);
                scanCountData.JobName = registerScanData.JobName;
                scanCountData.Code = registerScanData.Code;
                scanCountData.Message = registerScanData.Message;
                scanCountData.AppCode = clientCode;
                scanCountData.TrackingNo = registerScanData.TrackingNo;
                return scanCountData;

            }
            catch (Exception e)
            {
                log.Error(e.Message.ToString());
                throw e;
            }
        }
        public List<RegisterScanDetails> RegisterBulkScan(string barcode, string clientCode, string username, string machineName, string batchName)
        {
            try
            {
                var estToday = DateTime.UtcNow.UtcToEst().Date;
                string query = string.Format("{0} @barcode, @clientCode, @operatorid, @machineid, @batchName, @estDate", "SP_BulkRegisterScan");
                var registerScanDetails = db.Query<RegisterScanDetails>(query, new { barcode = barcode, clientCode = clientCode, operatorid = username, machineid = machineName, batchName = batchName, estDate = estToday }).ToList();
                db.Dispose();
                return registerScanDetails;
            }
            catch (Exception e)
            {
                log.Error(e.Message.ToString());
                throw e;
            }
        }
        public Dictionary<string, ScanDetails> GetScanStats()
        {
            try
            {
                string scanCountQuery = string.Format("{0} @estDate, @startdate, @enddate", "SP_GetScanStats_Temp");
                var estToday = DateTime.UtcNow.UtcToEst().Date;
                // EST Week
                var startDate = (estToday + new TimeSpan(0, 0, 0)).EstToUtc();
                var endDate = (estToday + new TimeSpan(23, 59, 59)).EstToUtc();
                var scanCountData = db.Query<ScanDetails>(scanCountQuery, new { estDate = estToday, startDate = startDate, endDate = endDate }).ToDictionary(sd => sd.AppCode, sd => sd);
                return scanCountData;
            }
            catch (Exception e)
            {
                log.Error(e.Message.ToString());
                throw e;
            }
        }

        public List<RegisterScanDetails> GetScannedMailPieces(string clientCode, string batchName)
        {
            try
            {
                string scanCountQuery = string.Format("{0} @appcode, @batchName, @startdate, @enddate, @estDate", "SP_GetScannedMailPieces");
                var estToday = DateTime.UtcNow.UtcToEst().Date;
                // EST Week
                var startDate = (estToday + new TimeSpan(0, 0, 0)).EstToUtc();
                var endDate = (estToday + new TimeSpan(23, 59, 59)).EstToUtc();
                var scannedMailPieceList = db.Query<RegisterScanDetails>(scanCountQuery, new { appcode = clientCode, batchName = batchName, startDate = startDate, endDate = endDate, estDate = estToday }).ToList();
                return scannedMailPieceList;

            }
            catch (Exception e)
            {
                log.Error(e.Message.ToString());
                throw e;
            }
        }

        public List<GetAllBatchCountDetails> GetRecentBatchesStats()
        {
            try
            {
                var batchList = GetRecentBatchesListStats();

                return batchList;

            }
            catch (Exception e)
            {
                log.Error(e.Message.ToString());
                throw e;
            }
        }
        public List<GetAllBatchCountDetails> GetRecentBatchesListStats()
        {
            string scanCountQuery = string.Format("{0} @estDate, @startdate, @enddate", "SP_GetRecentBatchesStats_Temp");
            var estToday = DateTime.UtcNow.UtcToEst().Date;
            // EST Week
            var startDate = (estToday + new TimeSpan(0, 0, 0)).EstToUtc();
            var endDate = (estToday + new TimeSpan(23, 59, 59)).EstToUtc();
            var batchList = db.Query<GetAllBatchCountDetails>(scanCountQuery, new { estDate = estToday, startDate = startDate, endDate = endDate }).ToList();
            return batchList;
        }
        public List<GetAllBatchCountDetails> GetRecentBatchesStatsWithAppCode(string appCode)
        {
            string scanCountQuery = string.Format("{0} @appcode, @estDate, @startdate, @enddate", "SP_GetRecentBatchesStats");
            var estToday = DateTime.UtcNow.UtcToEst().Date;
            // EST Week
            var startDate = (estToday + new TimeSpan(0, 0, 0)).EstToUtc();
            var endDate = (estToday + new TimeSpan(23, 59, 59)).EstToUtc();
            var batchList = db.Query<GetAllBatchCountDetails>(scanCountQuery, new { appcode = appCode, estDate = estToday, startDate = startDate, endDate = endDate }).ToList();
            return batchList;
        }
        public List<ScanedDocument> GetScanReportData(string clientCode, string barcodeValue, string loanNumber, string printOnFrom, string printOnTo, string shippedOnFrom, string shippedOnTo)
        {
            try
            {
                DateTime printOnStart = (DateTime.ParseExact(printOnFrom.Trim(), "MMMM d,yyyy", CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces).Date + new TimeSpan(0, 0, 0)).EstToUtc();

                DateTime printOnEnd = (DateTime.ParseExact(printOnTo.Trim(), "MMMM d,yyyy", CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces).Date + new TimeSpan(23, 59, 59)).EstToUtc();

                DateTime shippedOnStart = (DateTime.ParseExact(shippedOnFrom.Trim(), "MMMM d,yyyy", CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces).Date + new TimeSpan(0, 0, 0)).EstToUtc();

                DateTime shippedOnEnd = (DateTime.ParseExact(shippedOnTo.Trim(), "MMMM d,yyyy", CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces).Date + new TimeSpan(23, 59, 59)).EstToUtc();

                var estToday = Common.EstToday;
                string query = string.Format("{0} @loanNumber, @trackingNo, @appcode, @estDate, @substartdate, @subenddate, @scanstartDate,@scanendDate", "SP_GetScanReportData");

                var batchList = db.Query<ScanedDocument>(query, new { loanNumber = loanNumber, trackingNo = barcodeValue, appcode = clientCode, estDate = estToday, substartdate = shippedOnStart, subenddate = shippedOnEnd, scanstartDate = printOnStart, scanendDate = printOnEnd }).ToList();
                return batchList;

            }
            catch (Exception e)
            {
                log.Error(e.Message.ToString());
                return null;
            }
        }

        public List<PendingScanning> GetPendingScanning(string clientCode)
        {
            try
            {
                var estToday = DateTime.UtcNow.UtcToEst().Date;
                string query = string.Format("{0} @appcode, @estDate", "SP_GetPendingScanning");
                var batchList = db.Query<PendingScanning>(query, new { appcode = clientCode, estDate = estToday }).ToList();
                return batchList;
            }
            catch (Exception e)
            {
                log.Error(e.Message.ToString());
                return null;
            }
        }

        public USPSScanDetails RegisterUSPSScan(int mailPieceId, string trackingNo, string clientCode)
        {

            try
            {
                string query = string.Format("{0} @mailPieceId,@trackingNo", "SP_USPSScan");
                var registerScanData = db.QuerySingleOrDefault<USPSScanDetails>(query, new { mailPieceId = mailPieceId, trackingNo = trackingNo });
                registerScanData.AppCode = clientCode;
                db.Dispose();
                return registerScanData;

            }
            catch (Exception e)
            {
                log.Error(e.Message.ToString());
                throw e;
            }
        }

        public MailPieceDetails GetDocumentInfo(int mailPieceId)
        {

            try
            {
                var registerScanData = db.QuerySingleOrDefault<MailPieceDetails>(@"SELECT P.Id,P.FileName,B.ServerIP,C.AppCode,REQ.JobId FROM AM.MailPieces P INNER JOIN AM.MailBatches B ON B.Id=P.MailBatchId INNER JOIN PnS.MRDFRequest REQ ON REQ.UniqueId=P.UniqueId INNER JOIN Common.Clients C ON C.Id=B.ClientId WHERE P.Id=@MailPieceID", new { MailPieceID = mailPieceId });
                db.Dispose();
                return registerScanData;
            }
            catch (Exception e)
            {
                log.Error(e.Message.ToString());
                throw e;
            }
        }

        public MailPieceDetails GetDocumentInfo(string uniqueId)
        {

            try
            {
                var registerScanData = db.QuerySingleOrDefault<MailPieceDetails>(@"SELECT P.Id,P.FileName,B.ServerIP,C.AppCode,REQ.JobId FROM AM.MailPieces P INNER JOIN AM.MailBatches B ON B.Id=P.MailBatchId INNER JOIN PnS.MRDFRequest REQ ON REQ.UniqueId=P.UniqueId INNER JOIN Common.Clients C ON C.Id=B.ClientId WHERE P.UniqueId=@MPUniqueId", new { MPUniqueId = uniqueId });
                db.Dispose();
                return registerScanData;
            }
            catch (Exception e)
            {
                log.Error(e.Message.ToString());
                throw e;
            }
        }
    }
}
