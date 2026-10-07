using AM.Scanner.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Common;
using System.Linq;
using System.Web;
using Dapper;
using System.Globalization;
using AM.Lib;
namespace AM.Scanner.Common
{
    public class DBHelper
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        internal static DbConnection Create()
        {
            try
            {
                string connectionStringName = "AuthoMail";
                var cs = ConfigurationManager.ConnectionStrings[connectionStringName];

                if (cs == null)
                {
                    log.Error(string.Format("Connection string with name '{0}' does not exist.", connectionStringName));
                    throw new ArgumentException("Connection string with name '{0}' does not exist.", connectionStringName);
                }

                var factory = DbProviderFactories.GetFactory(cs.ProviderName);
                var connection = factory.CreateConnection();
                connection.ConnectionString = cs.ConnectionString;
                return connection;
            }
            catch (Exception e)
            {
                log.Error(e.Message.ToString());
                return null;
            }
        }

        public static ScanDetails RegisterScan(string barcode, string clientCode, string username, string machineName, string batchName)
        {
            barcode = barcode.Replace(clientCode, "");
            try
            {
                DbConnection db = Create();
                //DbCommand cmd = db.GetStoredProcCommand(Common.SP_RegisterScan, barcode);
                // DbCommand cmd = db.GetStoredProcCommand(Common.SP_RegisterScan, barcode, username, machineName);
                var estToday = DateTime.UtcNow.UtcToEst().Date;
                string query = string.Format("{0} @barcode, @clientCode, @operatorid, @machineid, @batchName, @estDate", Common.SP_RegisterScan);
                var registerScanData = db.QuerySingleOrDefault<RegisterScanDetails>(query, new { barcode = barcode, clientCode = clientCode, operatorid = username, machineid = machineName, batchName = batchName, estDate = estToday });
                // var batchWiseCount =GetRecentBatchesListStats(clientCode,db).Single(b => b.BatchName == batchName);
                db.Dispose();

                var scanCountData = new ScanDetails();// GetScanStats(clientCode);
                scanCountData.JobName = registerScanData.JobName;
                scanCountData.Code = registerScanData.Code;
                scanCountData.Message = registerScanData.Message;
                scanCountData.AppCode = clientCode;
                scanCountData.TrackingNo = registerScanData.TrackingNo;
                //scanCountData.ScanCount = Convert.ToInt32(batchWiseCount.ScannedLoans);
                //scanCountData.TotalCount = Convert.ToInt32(batchWiseCount.TotalLoans);
                return scanCountData;

            }
            catch (Exception e)
            {
                log.Error(e.Message.ToString());
                throw e;
            }
        }

        public static List<ScanDetails> GetScanStats()
        {
            try
            {
                DbConnection db = Create();
                string scanCountQuery = string.Format("{0} @estDate, @startdate, @enddate", Common.SP_GetScanStats);
                var estToday = DateTime.UtcNow.UtcToEst().Date;
                // EST Week
                var startDate = (estToday + new TimeSpan(0, 0, 0)).EstToUtc();
                var endDate = (estToday + new TimeSpan(23, 59, 59)).EstToUtc();
                var scanCountData = db.Query<ScanDetails>(scanCountQuery, new { estDate = estToday, startDate = startDate, endDate = endDate }).ToList();
                db.Dispose();
                return scanCountData;

            }
            catch (Exception e)
            {
                log.Error(e.Message.ToString());
                throw e;
            }
        }
        public static List<RegisterScanDetails> GetScannedMailPieces(string clientCode, string batchName)
        {
            try
            {
                DbConnection db = Create();
                string scanCountQuery = string.Format("{0} @appcode, @batchName, @startdate, @enddate, @estDate", Common.SP_GetScannedMailPieces);
                var estToday = DateTime.UtcNow.UtcToEst().Date;
                // EST Week
                var startDate = (estToday + new TimeSpan(0, 0, 0)).EstToUtc();
                var endDate = (estToday + new TimeSpan(23, 59, 59)).EstToUtc();
                var scannedMailPieceList = db.Query<RegisterScanDetails>(scanCountQuery, new { appcode = clientCode, batchName = batchName, startDate = startDate, endDate = endDate, estDate = estToday }).ToList();
                db.Dispose();
                return scannedMailPieceList;

            }
            catch (Exception e)
            {
                log.Error(e.Message.ToString());
                throw e;
            }
        }

        public static List<GetAllBatchCountDetails> GetRecentBatchesStats(string clientCode)
        {
            try
            {
                DbConnection db = Create();
                var batchList = GetRecentBatchesListStats(clientCode, db);
                db.Dispose();
                return batchList;

            }
            catch (Exception e)
            {
                log.Error(e.Message.ToString());
                throw e;
            }
        }
        public static List<GetAllBatchCountDetails> GetRecentBatchesListStats(string clientCode, DbConnection db)
        {
            string scanCountQuery = string.Format("{0} @appcode, @estDate, @startdate, @enddate", Common.SP_GetRecentBatchesStats);
            var estToday = DateTime.UtcNow.UtcToEst().Date;
            // EST Week
            var startDate = (estToday + new TimeSpan(0, 0, 0)).EstToUtc();
            var endDate = (estToday + new TimeSpan(23, 59, 59)).EstToUtc();
            var batchList = db.Query<GetAllBatchCountDetails>(scanCountQuery, new { appcode = clientCode, estDate = estToday, startDate = startDate, endDate = endDate }).ToList();
            return batchList;
        }
        public static List<ScanedDocument> GetScanReportData(string clientCode, string barcodeValue, string loanNumber, string printOnFrom, string printOnTo, string shippedOnFrom, string shippedOnTo)
        {
            try
            {
                //barcodeValue = string.IsNullOrEmpty(barcodeValue.Trim()) ? null : barcodeValue.Trim();
                //loanNumber = string.IsNullOrEmpty(loanNumber.Trim()) ? null : loanNumber.Trim();
                //printOnFrom = string.IsNullOrEmpty(printOnFrom.Trim()) ? null : printOnFrom.Trim();
                //printOnTo = string.IsNullOrEmpty(printOnTo.Trim()) ? null : printOnTo.Trim();
                //shippedOnFrom = string.IsNullOrEmpty(shippedOnFrom.Trim()) ? null : shippedOnFrom.Trim();
                //shippedOnTo = string.IsNullOrEmpty(shippedOnTo.Trim()) ? null : shippedOnTo.Trim();

                DateTime printOnStart = (DateTime.ParseExact(printOnFrom.Trim(), "MMMM d,yyyy", CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces).Date + new TimeSpan(0, 0, 0)).EstToUtc();

                DateTime printOnEnd = (DateTime.ParseExact(printOnTo.Trim(), "MMMM d,yyyy", CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces).Date + new TimeSpan(23, 59, 59)).EstToUtc();

                DateTime shippedOnStart = (DateTime.ParseExact(shippedOnFrom.Trim(), "MMMM d,yyyy", CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces).Date + new TimeSpan(0, 0, 0)).EstToUtc();

                DateTime shippedOnEnd = (DateTime.ParseExact(shippedOnTo.Trim(), "MMMM d,yyyy", CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces).Date + new TimeSpan(23, 59, 59)).EstToUtc();

                DbConnection db = Create();
                var estToday = DateTime.UtcNow.UtcToEst().Date;
                string query = string.Format("{0} @loanNumber, @trackingNo, @appcode, @estDate, @substartdate, @subenddate, @scanstartDate,@scanendDate", Common.SP_GetScanReportData);

                var batchList = db.Query<ScanedDocument>(query, new { loanNumber = loanNumber, trackingNo = barcodeValue, appcode = clientCode, estDate = estToday, substartdate = shippedOnStart, subenddate = shippedOnEnd, scanstartDate = printOnStart, scanendDate = printOnEnd }).ToList();
                db.Dispose();
                return batchList;

            }
            catch (Exception e)
            {
                log.Error(e.Message.ToString());
                return null;
            }
        }

        public static List<PendingScanning> GetPendingScanning(string clientCode)
        {
            try
            {
                DbConnection db = Create();
                var estToday = DateTime.UtcNow.UtcToEst().Date;
                string query = string.Format("{0} @appcode, @estDate", Common.SP_GetPendingScanning);
                var batchList = db.Query<PendingScanning>(query, new { appcode = clientCode, estDate = estToday }).ToList();
                db.Dispose();
                return batchList;

            }
            catch (Exception e)
            {
                log.Error(e.Message.ToString());
                return null;
            }
        }

        public static USPSScanDetails RegisterUSPSScan(int mailPieceId, string trackingNo, string clientCode)
        {

            try
            {
                DbConnection db = Create();
                //DbCommand cmd = db.GetStoredProcCommand(Common.SP_RegisterScan, barcode);
                // DbCommand cmd = db.GetStoredProcCommand(Common.SP_RegisterScan, barcode, username, machineName);
                string query = string.Format("{0} @mailPieceId,@trackingNo", Common.SP_RegisterUSPSScan);
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
    }
}