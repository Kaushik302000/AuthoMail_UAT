using Microsoft.Practices.EnterpriseLibrary.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCI.Service
{
    partial class DAL
    {
        private static Database db = DatabaseFactory.CreateDatabase();

        public static List<int> GetNextBatchIdByJobName(string jobName, string operation, string serverIP)
        {
            var batchIds = db.ExecuteDataSet("usp_GetNextBatchIdByJobName", jobName, operation, serverIP);

            if (batchIds != null && batchIds.Tables.Count > 0)
                return batchIds.Tables[0].AsEnumerable().Select(id => id.Field<int>("BatchId")).ToList();

            return null;
        }

        public static BatchSummary GetBatchSummaryById(int batchId)
        {
            var ds = GetBatchSummaryDataById(batchId);
            return ds.DataTableAccessor<BatchSummary>(false).FirstOrDefault();
        }

        public static DataSet GetBatchSummaryDataById(int batchId)
        {
            return db.ExecuteDataSet("usp_GetBatchSummaryById", batchId);
        }

        public static Client GetClientAppDetailsById(int appId)
        {
            var accessor = db.CreateSprocAccessor<Client>("usp_GetClientAppDetailsById");
            return accessor.Execute(appId).FirstOrDefault();
        }

        public static List<DocType> GetDocTypeDetailsByAppId(int appId)
        {
            var accessor = db.CreateSprocAccessor<DocType>("usp_GetDocTypeDetailsByAppId");
            return accessor.Execute(appId).ToList();
        }

        public static int UpdateBatchPipeLineStatus(int batchId, string pipeLineStatus, DateTime timestamp)
        {
            return db.ExecuteSqlNonQuery("UPDATE [PnS].[BatchSummary] SET PipelineStatus = @p1, {0}On = @p2 WHERE PrintBatch = @p3".Fill(pipeLineStatus), pipeLineStatus, timestamp, batchId);
        }

        public static int UpdateBatchStatus(int batchId, string batchStatus)
        {
            return db.ExecuteSqlNonQuery("UPDATE [PnS].[BatchSummary] SET BatchStatus = @p1 WHERE PrintBatch = @p2", batchStatus, batchId);
        }

        public static int UpdateFileReceivedStatus(int jobId, int pieceId)
        {
            return db.ExecuteSqlNonQuery("UPDATE [PnS].[MRDFRequest] SET IsPDFReceived = 1 WHERE [JobId] = @p1 AND [PieceId] = @p2", jobId, pieceId);
        }

        public static int UpdateFileReceivedBatchStatus(int batchId, DateTime timestamp)
        {
            return db.ExecuteSqlNonQuery("UPDATE [PnS].[BatchSummary] SET PDFsReceivedOn = @p1 WHERE PrintBatch = @p2", timestamp, batchId);
        }

        public static void LogBatchTimeLine(int batchId, string activityName, DateTime timeStamp)
        {
            db.ExecuteNonQuery("usp_LogBatchTimeLine", batchId, activityName, timeStamp);
        }

        public static string GetSystemSettingsValue(string keyId)
        {
            return db.ExecuteScalar("usp_GetSystemSettingsById", keyId).ToString();
        }

        public static List<AMSystemSetting> GetSystemSettings()
        {
            var sysData = db.ExecuteDataSet(CommandType.Text, "SELECT * FROM Common.SystemSettings");
            return sysData.DataTableAccessor<AMSystemSetting>(false).ToList();
        }

        public static EmailChannel GetEmailChannelByTitleName(string titleName)
        {
            var emailChannel = db.ExecuteDataSet(CommandType.Text, "SELECT * FROM Common.EmailChannels WHERE Title='{0}'".Fill(titleName));
            return emailChannel.DataTableAccessor<EmailChannel>(false).Single();
        }

        public static List<EmailChannel> GetEmailChannels()
        {
            var emailChannel = db.ExecuteDataSet(CommandType.Text, "SELECT * FROM Common.EmailChannels");
            return emailChannel.DataTableAccessor<EmailChannel>(false).ToList();
        }

        public static List<DallasBatch> GetDallasBatches(DateTime estToday)
        {
            var dallasTimings = db.ExecuteDataSet(CommandType.Text, "Select * from dbo.ufn_GetDallasBatches('" + estToday.ToString() + "')");
            return dallasTimings.DataTableAccessor<DallasBatch>(false).ToList();
        }

        public static List<DallasBatchTiming> GetDallasBatchTimings(DateTime estToday)
        {
            var dallasTimings = db.ExecuteDataSet("usp_GetDallasBatchTimingsPnS", estToday);
            return dallasTimings.DataTableAccessor<DallasBatchTiming>(false).ToList();
        }

        public static List<DallasBatchTiming> GetDallasBatchTimingsFromTable()
        {
            var dallasTimings = db.ExecuteDataSet(CommandType.Text, "SELECT * FROM Common.DallasBatchTimings");
            return dallasTimings.DataTableAccessor<DallasBatchTiming>(false).ToList();
        }

        public static int UpdatePrintTrailBatchStatus(int printTrailBatchId, string pipeLineStatus, DateTime timestamp)
        {
            return db.ExecuteSqlNonQuery("UPDATE [PnS].[PrintTrailBatches] SET {0}On = @p1 WHERE Id = @p2".Fill(pipeLineStatus), timestamp, printTrailBatchId);
        }

        public static PrintTrailDetail GetPrintTrailDetailByPrintTrailId(int printTrailId)
        {
            var printTrailDetail = db.ExecuteDataSet("usp_GetPrintTrailDetailByPrintTrailId", printTrailId);
            return printTrailDetail.DataTableAccessor<PrintTrailDetail>(false).Single();
        }

        public static PrintTrailDetail GetPrintTrailDetailByPrintTrailName(string printTrailName, string printVendorCode)
        {
            var printTrailDetail = db.ExecuteDataSet("usp_GetPrintTrailDetailByPrintTrailName", printTrailName, printVendorCode);
            return printTrailDetail.DataTableAccessor<PrintTrailDetail>(false).SingleOrDefault();
        }

        public static bool CheckBatchesInQueue(DateTime startDateTime, DateTime endDateTime, string serverIP)
        {
            int receivedCount = db.ExecuteSqlScalar<int>("SELECT COUNT(*) FROM [AM].[MailBatches] B WHERE B.ReceivedOn BETWEEN @p1 AND @p2 AND B.IsReceived=1 AND B.ServerIP = @p3", startDateTime, endDateTime, serverIP);

            int amSubmittedOrReportedCount = db.ExecuteSqlScalar<int>("SELECT COUNT(*) FROM [AM].[MailBatches] B WHERE B.ReceivedOn BETWEEN @p1 AND @p2 AND ( B.IsSubmittedToShipping=1 OR B.IsReportedBack=1 OR B.IsReportPrepped=1 OR B.PipelineStatus='SubmittedToEDelivery') AND B.ServerIP = @p3", startDateTime, endDateTime, serverIP);

            if (receivedCount != amSubmittedOrReportedCount)
                return false;

            int pendingBatchCount = db.ExecuteSqlScalar<int>(@"WITH
            AMSubmitted as
            (SELECT UniqueId FROM [AM].[PrintBatches] WHERE MailBatchId IN (SELECT Id FROM [AM].[MailBatches] B WHERE B.ReceivedOn BETWEEN @p1 AND @p2 AND ( B.IsSubmittedToShipping=1 ) AND B.ServerIP = @p3)),
            PnSPrepped as
            (SELECT BatchIdentifier FROM [PnS].[BatchSummary] WHERE ReceivedOn >= @p1 AND ( IsPrepped=1  OR IsSubmitted = 1 OR IsReportPrepped = 1 OR IsReportedBack = 1) AND ServerIP = @p3)
            select Count(*) from (
            select UniqueId from AMSubmitted 
            except
            select BatchIdentifier from PnSPrepped 
            ) t", startDateTime, endDateTime, serverIP);

            return pendingBatchCount == 0;
        }

        public static PrintVendor GetPrintVendorDetailByVendorCode(string printVendorCode)
        {
            var printTrailDetail = db.ExecuteDataSet(CommandType.Text, "SELECT TOP 1 PV.Id, PV.Name, SI.SFTChannelId, SI.PrintVendorCode, PV.BusinessCode, PV.VendorCode, SC.Protocol, SC.Host, SC.UserName, SC.[Password], SC.ToADEDir, SC.FromADEDir, SC.ArchiveDir, SC.PostDownloadAction FROM Common.ShipperInformation SI INNER JOIN Common.SFTChannels SC ON SI.SFTChannelId = SC.Id INNER JOIN Common.PrintVendors PV ON SI.PrintVendorId = PV.Id WHERE SI.PrintVendorCode = '{0}'".Fill(printVendorCode));

            return printTrailDetail.DataTableAccessor<PrintVendor>(false).Single();
        }

        public static int UpdateEnvelopeInventory(PrintTrailBatch printTrailBatch, Dictionary<int?, int> envelopeInventory)
        {
            foreach (var envelope in envelopeInventory)
            {
                db.ExecuteSqlNonQuery("INSERT INTO [PnS].[EnvelopeUsage]([PrintTrailBatchId],[EnvelopeId],[Quantity],[InsertedOn])VALUES(@p1,@p2,@p3,@p4)", printTrailBatch.Id, envelope.Key, envelope.Value, Common.GetCurrentTime());
                db.ExecuteSqlNonQuery("UPDATE [Common].[EnvelopeStock] SET CurrentQuantity = CurrentQuantity-{0} WHERE EnvelopeId = @p1 AND ShipperId= @p2".Fill(envelope.Value), envelope.Key, printTrailBatch.ShipperId);
            }
            return 1;
        }

        public static SFTChannel GetSFTChannelDetailsByCode(string sftChannelCode)
        {
            var sftChannelDetail = db.ExecuteDataSet(CommandType.Text, "SELECT Id, Code,Description,Title, Protocol, Host, UserName, [Password], ToADEDir, FromADEDir, ArchiveDir, PostDownloadAction FROM Common.SFTChannels WHERE Code='{0}'".Fill(sftChannelCode));

            return sftChannelDetail.DataTableAccessor<SFTChannel>(false).Single();
        }

        public static int GetMailPieceIdFromGuid(Guid uniqueId)
        {
            return db.ExecuteSqlScalar<int>("SELECT TOP 1 Id FROM AM.MailPieces WHERE UniqueId=@p1 ORDER BY Id DESC", uniqueId);
        }

        public static int UpdatePrintTrailBatchSize(int printTrailBatchId, Int64 zipFileSize)
        {
            return db.ExecuteSqlNonQuery("UPDATE [PnS].[PrintTrailBatches] SET ZipFileSize = @p1 WHERE Id = @p2", zipFileSize, printTrailBatchId);
        }
    }
}
