using AM.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using AM.Entities;
using AM.Lib;
using System.Data;

namespace AM.Services
{
    public interface IMailBatchService : IDisposable
    {
        int InsertBatchDetails(MailBatchDTO mailBatch);
        int InsertBatchDetails(MailBatchDTO mailBatch, List<MailBatchCustomInfo> mailBatchCustomInfo);
        List<MailBatchDTO> InsertBatchDetails(List<MailBatchDTO> mailBatches);
        int DeleteMailPiecesDetailsByMailBatchId(int mailBatchId);
        int UpdateBatchDetails(MailBatchDTO mailBatch);
        Task<int> MarkMailBatchAsInvalid(int id, string errorDesc);
        List<MailBatchDTO> GetBatchesByPipelineStatusAndClientId(string batchPipelineStatus, int? clientId = null);
        List<MailBatchDTO> GetBatchesByPipelineStatusAndSFTChannelId(string batchPipelineStatus, int? sftChannelId = null);
        List<MailBatchDTO> GetBatchesByPipelineStatusAndSFTChannelIdAndServerIP(string batchPipelineStatus, int sftChannelId, string serverIP);
        List<MailBatchDTO> GetBatchesByPipelineStatusAndServerIP(string batchPipelineStatus, string serverIP);
        BatchData GetMailBatchDetailsByBatchId(int mailBatchId);
        MailBatchDTO GetMailBatchDetailByBatchId(int mailBatchId);
        int InsertPrintBatchDetails(PrintBatch printBatch);
        List<SubmittedBatch> GetSubmittedPrintBatchDetails(string serverIP);
        List<SubmittedBatch> GetResponseReceivedPrintBatchDetails(string serverIP);
        List<SubmittedBatch> GetReportPreppedPrintBatchDetails(int sftChannelId, string serverIP);
        int GetMailBatchesReceivedCount(DateTime startTime, DateTime endTime, string status);
        int UpdatePrintBatchPipelineStatus(int printBatchId, string pipeLineStatus, DateTime timestamp);
        int RemainingPrintBatchesReportedBackCount(int mailBatchId);
        Task<int> InsertBatchDetails(MailBatch mailBatch, int clientId, SFTChannel sFTChannel);
        int InsertLastProcessTimeForUATBatches(DateTime lastProcessedDateTime);
        MailPieceDTO GetMailPiecesDataFromMailBatchIdAndMailPieceId(int mailBatchId, int mailPieceId);
        MailPieceDTO ProcessReshipData(MailPieceDTO mailPiece);
        int UpdateHoldBatchesInfo(int clientId, DateTime heldOn);
    }
    public class MailBatchService : ServiceBase, IMailBatchService //It implements Interfaces 
    {
        public int InsertBatchDetails(MailBatchDTO mailBatch)
        {
            string query = @"INSERT INTO [AM].[MailBatches](UniqueId, ClientId, SFTChannelId, ClientBatchId, ReceivedOn, BatchName, DocumentOrPackageType, Priority, BatchStatus, ValidationStatus, ValidationErrors, PipelineStatus, IsReturnInfoSpecified, RetRcptPrefix, RetRcptFirstName, RetRcptMiddleName, RetRcptLastName, RetRcptSuffix, RetRcptPhone, RetRcptOrganization, RetRcptAddrOptionalLine1AboveStreet, RetRcptAddrOptionalLine2AboveStreet, RetRcptAddressStreet, RetRcptAddressAptOrSuiteOrOther, RetRcptAddrOptionalLine1BelowStreet, RetRcptAddrOptionalLine2BelowStreet, RetRcptAddressCity, RetRcptAddressState, RetRcptAddressZIP, RetRcptAddressCountry, ServerIP) Output Inserted.Id VALUES (@UniqueId, @ClientId, @SFTChannelId, @ClientBatchId, CAST(CAST(@ReceivedOn AS DATETIME2(0)) AS DATETIME), @BatchName, @DocumentOrPackageType, @Priority, @BatchStatus, @ValidationStatus, @ValidationErrors, @PipelineStatus, @IsReturnInfoSpecified, @RetRcptPrefix, @RetRcptFirstName, @RetRcptMiddleName, @RetRcptLastName, @RetRcptSuffix, @RetRcptPhone, @RetRcptOrganization, @RetRcptAddrOptionalLine1AboveStreet, @RetRcptAddrOptionalLine2AboveStreet, @RetRcptAddressStreet, @RetRcptAddressAptOrSuiteOrOther, @RetRcptAddrOptionalLine1BelowStreet, @RetRcptAddrOptionalLine2BelowStreet, @RetRcptAddressCity, @RetRcptAddressState, @RetRcptAddressZIP, @RetRcptAddressCountry, @ServerIP)";

            return db.ExecuteScalar<int>(query, mailBatch);
        }

        public int InsertBatchDetails(MailBatchDTO mailBatch, bool isReshipped)
        {
            string query = @"INSERT INTO [AM].[MailBatches]([UniqueId], [ClientId], [SFTChannelId], [ClientBatchId], [BatchName], [DocumentOrPackageType], [ReceivedOn], [DownloadedOn], [ValidatedOn], [AcknowledgedOn], [Priority], [BatchStatus], [ValidationStatus], [ValidationErrors], [PipelineStatus], [IsReturnInfoSpecified], [RetRcptPrefix], [RetRcptFirstName], [RetRcptMiddleName], [RetRcptLastName], [RetRcptSuffix], [RetRcptPhone], [RetRcptOrganization], [RetRcptAddrOptionalLine1AboveStreet], [RetRcptAddrOptionalLine2AboveStreet], [RetRcptAddressStreet], [RetRcptAddressAptOrSuiteOrOther], [RetRcptAddrOptionalLine1BelowStreet], [RetRcptAddrOptionalLine2BelowStreet], [RetRcptAddressCity], [RetRcptAddressState], [RetRcptAddressZIP], [RetRcptAddressCountry], [IsReshippedBatch]) Output Inserted.Id VALUES (@UniqueId, @ClientId, @SFTChannelId, @ClientBatchId, @BatchName, @DocumentOrPackageType, @ReceivedOn, @DownloadedOn, @ValidatedOn, @AcknowledgedOn, @Priority, @BatchStatus, @ValidationStatus, @ValidationErrors, @PipelineStatus, @IsReturnInfoSpecified, @RetRcptPrefix, @RetRcptFirstName, @RetRcptMiddleName, @RetRcptLastName, @RetRcptSuffix, @RetRcptPhone, @RetRcptOrganization, @RetRcptAddrOptionalLine1AboveStreet, @RetRcptAddrOptionalLine2AboveStreet, @RetRcptAddressStreet, @RetRcptAddressAptOrSuiteOrOther, @RetRcptAddrOptionalLine1BelowStreet, @RetRcptAddrOptionalLine2BelowStreet, @RetRcptAddressCity, @RetRcptAddressState, @RetRcptAddressZIP, @RetRcptAddressCountry, 1)";

            return db.ExecuteScalar<int>(query, mailBatch);
        }
        public int InsertBatchDetails(MailBatchDTO mailBatch, List<MailBatchCustomInfo> mailBatchCustomInfo)
        {
            int mailBatchId = InsertBatchDetails(mailBatch);

            mailBatchCustomInfo.ForEach(mp =>
            {
                mp.MailBatchId = mailBatchId;
            });

            string query = @"INSERT INTO [AM].[MailBatchCustomInfo]([MailBatchId], [Key], [DataType], [Value]) VALUES (@MailBatchId, @Key, @DataType, @Value)";

            db.ExecuteAsync(query, mailBatchCustomInfo);

            return mailBatchId;
        }

        public List<MailBatchDTO> InsertBatchDetails(List<MailBatchDTO> mailBatches)
        {
            //Insert batch record, mark batch as received and inprogress
            foreach (var mailBatch in mailBatches)
            {
                string query = @"INSERT INTO [AM].[MailBatches](UniqueId, ClientId, DocumentOrPackageType, SFTChannelId, ClientBatchId, BatchName, [Priority],[ReceivedOn], [PipelineStatus], [BatchStatus], [IsReturnInfoSpecified]) Output Inserted.Id VALUES (@UniqueId, @ClientId, @DocumentOrPackageType, @SFTChannelId, @ClientBatchId, @BatchName, @Priority,CAST(CAST(@ReceivedOn AS DATETIME2(0)) AS DATETIME), @PipelineStatus, @BatchStatus, @IsReturnInfoSpecified)";

                mailBatch.Id = db.ExecuteScalar<int>(query, mailBatch);
            }

            return mailBatches;
        }

        public int DeleteMailPiecesDetailsByMailBatchId(int mailBatchId)
        {
            return db.Execute("usp_DeleteMailPiecesDetailsByMailBatchId", new { mailBatchId = mailBatchId }, commandType: CommandType.StoredProcedure);
        }

        public int UpdateBatchDetails(MailBatchDTO mailBatch)
        {
            string query = "UPDATE [AM].[MailBatches] SET [UniqueId] = @UniqueId,[ClientId] = @ClientId,[DocumentOrPackageType] = @DocumentOrPackageType,[BatchName] = @BatchName ,[ValidationStatus] = @ValidationStatus ,[ValidationErrors] = @ValidationErrors,[ClientBatchId] = @ClientBatchId ,[IsReturnInfoSpecified] = IsReturnInfoSpecified,[RetRcptPrefix] = @RetRcptPrefix ,[RetRcptFirstName] =@RetRcptFirstName ,[RetRcptMiddleName] = @RetRcptMiddleName ,[RetRcptLastName] = @RetRcptLastName ,[RetRcptSuffix] = @RetRcptSuffix ,[RetRcptPhone] = @RetRcptPhone ,[RetRcptOrganization] = @RetRcptOrganization ,[RetRcptAddrOptionalLine1AboveStreet] = @RetRcptAddrOptionalLine1AboveStreet ,[RetRcptAddrOptionalLine2AboveStreet] = @RetRcptAddrOptionalLine2AboveStreet ,[RetRcptAddressStreet] = @RetRcptAddressStreet ,[RetRcptAddressAptOrSuiteOrOther] = @RetRcptAddressAptOrSuiteOrOther ,[RetRcptAddrOptionalLine1BelowStreet] = RetRcptAddrOptionalLine1BelowStreet ,[RetRcptAddrOptionalLine2BelowStreet] = RetRcptAddrOptionalLine2BelowStreet ,[RetRcptAddressCity] = @RetRcptAddressCity ,[RetRcptAddressState] = @RetRcptAddressState ,[RetRcptAddressZIP] = @RetRcptAddressZIP ,[Priority] = @Priority ,[SFTChannelId] = @SFTChannelId WHERE Id=@Id";

            return db.Execute(query, mailBatch);
        }

        public async Task<int> MarkMailBatchAsInvalid(int id, string errorDesc)
        {
            string query = "UPDATE [AM].[MailBatches] SET [ValidationStatus] = @ValidationStatus, [ValidationErrors] = @ValidationErrors WHERE Id=@Id";

            return await db.ExecuteAsync(query, new { ValidationStatus = BatchValidationStatus.Invalid, ValidationErrors = errorDesc, Id = id });
        }

        public List<MailBatchDTO> GetBatchesByPipelineStatusAndClientId(string batchPipelineStatus, int? clientId)
        {
            //Sql Query
            string query = $"SELECT TOP {Config.Common_MaxProcessingLimit} * FROM [AM].[MailBatches] WHERE PipelineStatus= @batchPipelineStatus";

            if (clientId.HasValue)
                query += " AND ClientId= @ClientId";

            switch (batchPipelineStatus)
            {
                case BatchPipelineStatus.Received:
                    query += " AND IsReceived=1 AND IsDownloaded=0";
                    break;
                case BatchPipelineStatus.Downloaded:
                    query += " AND IsDownloaded=1 AND IsValidated=0";
                    break;
                case BatchPipelineStatus.Validated:
                    query += " AND IsValidated=1 AND IsAcknowledged=0";
                    break;
                case BatchPipelineStatus.Acknowledged:
                    query += " AND IsAcknowledged=1 AND IsPrepped=0";
                    break;
                case BatchPipelineStatus.Prepped:
                    query += " AND IsPrepped=1 AND IsSubmittedToShipping=0";
                    break;
                case BatchPipelineStatus.SubmittedToShipping:
                    query += " AND IsSubmittedToShipping=1 AND IsShippingResponseReceived=0";
                    break;
                case BatchPipelineStatus.ShippingResponseReceived:
                    query += " AND IsShippingResponseReceived=1 AND IsReportPrepped=0";
                    break;
                case BatchPipelineStatus.ReportPrepped:
                    query += " AND IsReportPrepped=1 AND IsReportedBack=0";
                    break;
                default:
                    break;
            }

            var data = db.Query<MailBatchDTO>(query, new { batchPipelineStatus = batchPipelineStatus, ClientId = clientId }).ToList();
            return data;
        }

        public List<MailBatchDTO> GetBatchesByPipelineStatusAndSFTChannelId(string batchPipelineStatus, int? sftChannelId)
        {
            //Sql Query
            string query = $"SELECT TOP {Config.Common_MaxProcessingLimit} * FROM [AM].[MailBatches] WHERE PipelineStatus= @batchPipelineStatus";

            if (sftChannelId.HasValue)
                query += " AND SFTChannelId= @SFTChannelId";

            switch (batchPipelineStatus)
            {
                case BatchPipelineStatus.Received:
                    query += " AND IsReceived=1 AND IsDownloaded=0";
                    break;
                case BatchPipelineStatus.Downloaded:
                    query += " AND IsDownloaded=1 AND IsValidated=0";
                    break;
                case BatchPipelineStatus.Validated:
                    query += " AND IsValidated=1 AND IsAcknowledged=0";
                    break;
                case BatchPipelineStatus.Acknowledged:
                    query += " AND IsAcknowledged=1 AND IsPrepped=0";
                    break;
                case BatchPipelineStatus.Prepped:
                    query += " AND IsPrepped=1 AND IsSubmittedToShipping=0";
                    break;
                case BatchPipelineStatus.SubmittedToShipping:
                    query += " AND IsSubmittedToShipping=1 AND IsShippingResponseReceived=0";
                    break;
                case BatchPipelineStatus.ShippingResponseReceived:
                    query += " AND IsShippingResponseReceived=1 AND IsReportPrepped=0";
                    break;
                case BatchPipelineStatus.ReportPrepped:
                    query += " AND IsReportPrepped=1 AND IsReportedBack=0";
                    break;
                default:
                    break;
            }

            return db.Query<MailBatchDTO>(query, new { batchPipelineStatus = batchPipelineStatus, SFTChannelId = sftChannelId }).ToList();
        }

        public List<MailBatchDTO> GetBatchesByPipelineStatusAndSFTChannelIdAndServerIP(string batchPipelineStatus, int sftChannelId, string serverIP)
        {
            //Sql Query
            string query = $"SELECT TOP {Config.Common_MaxProcessingLimit} * FROM [AM].[MailBatches] WHERE PipelineStatus= @batchPipelineStatus AND SFTChannelId= @sftChannelId AND ServerIP = @serverIP";

            switch (batchPipelineStatus)
            {
                case BatchPipelineStatus.Received:
                    query += " AND IsReceived=1 AND IsDownloaded=0";
                    break;
                case BatchPipelineStatus.Downloaded:
                    query += " AND IsDownloaded=1 AND IsValidated=0";
                    break;
                case BatchPipelineStatus.Validated:
                    query += " AND IsValidated=1 AND IsAcknowledged=0";
                    break;
                case BatchPipelineStatus.Acknowledged:
                    query += " AND IsAcknowledged=1 AND IsPrepped=0";
                    break;
                case BatchPipelineStatus.Prepped:
                    query += " AND IsPrepped=1 AND IsSubmittedToShipping=0";
                    break;
                case BatchPipelineStatus.SubmittedToShipping:
                    query += " AND IsSubmittedToShipping=1 AND IsShippingResponseReceived=0";
                    break;
                case BatchPipelineStatus.ShippingResponseReceived:
                    query += " AND IsShippingResponseReceived=1 AND IsReportPrepped=0";
                    break;
                case BatchPipelineStatus.ReportPrepped:
                    query += " AND IsReportPrepped=1 AND IsReportedBack=0";
                    break;
                default:
                    break;
            }

            return db.Query<MailBatchDTO>(query, new { batchPipelineStatus, sftChannelId, serverIP }).ToList();
        }

        public List<MailBatchDTO> GetBatchesByPipelineStatusAndServerIP(string batchPipelineStatus, string serverIP)
        {
            //Sql Query
            string query = $"SELECT TOP {Config.Common_MaxProcessingLimit} * FROM [AM].[MailBatches] WHERE PipelineStatus= @batchPipelineStatus AND ServerIP = @serverIP";

            switch (batchPipelineStatus)
            {
                case BatchPipelineStatus.Received:
                    query += " AND IsReceived=1 AND IsDownloaded=0";
                    break;
                case BatchPipelineStatus.Downloaded:
                    query += " AND IsDownloaded=1 AND IsValidated=0";
                    break;
                case BatchPipelineStatus.Validated:
                    query += " AND IsValidated=1 AND IsAcknowledged=0";
                    break;
                case BatchPipelineStatus.Acknowledged:
                    query += " AND IsAcknowledged=1 AND IsPrepped=0";
                    break;
                case BatchPipelineStatus.Prepped:
                    query += " AND IsPrepped=1 AND IsSubmittedToShipping=0";
                    break;
                case BatchPipelineStatus.SubmittedToShipping:
                    query += " AND IsSubmittedToShipping=1 AND IsShippingResponseReceived=0";
                    break;
                case BatchPipelineStatus.ShippingResponseReceived:
                    query += " AND IsShippingResponseReceived=1 AND IsReportPrepped=0";
                    break;
                case BatchPipelineStatus.ReportPrepped:
                    query += " AND IsReportPrepped=1 AND IsReportedBack=0";
                    break;
                default:
                    break;
            }

            return db.Query<MailBatchDTO>(query, new { batchPipelineStatus, serverIP }).ToList();
        }

        public BatchData GetMailBatchDetailsByBatchId(int mailBatchId)
        {
            BatchData data = null;
            string query = @"SELECT * FROM [AM].[MailBatches] WHERE Id=@mailBatchId 
                            SELECT * FROM [AM].[MailBatchCustomInfo] WHERE MailBatchId=@mailBatchId
                            SELECT P.*,PCD.IsPrintReady,E.EnvelopeName FROM [AM].[MailPieces] P LEFT OUTER JOIN AM.DocTypes D ON P.DocTypeId=D.Id LEFT OUTER JOIN Common.PnSConfigDetail PCD ON P.PnSConfigDetailId=PCD.Id INNER JOIN Common.Envelopes E ON PCD.EnvelopeId=E.Id WHERE MailBatchId=@mailBatchId
                            SELECT * FROM [AM].[MailPieceCustomInfo] WHERE MailPieceId IN (SELECT Id FROM [AM].[MailPieces] WHERE MailBatchId=@mailBatchId)";

            using (var multi = db.QueryMultiple(query, new { mailBatchId = mailBatchId }))
            {
                if (multi != null)
                {
                    data = new BatchData();
                    data.MailBatch = multi.Read<MailBatchDTO>().Single();
                    data.MailBatchCustomInfo = multi.Read<MailBatchCustomInfo>().ToList();
                    data.MailPieces = multi.Read<MailPieceDTO>().ToList();
                    data.MailPieceCustomInfo = multi.Read<MailPieceCustomInfo>().ToList();
                }
            }

            return data;
        }

        public MailBatchDTO GetMailBatchDetailByBatchId(int mailBatchId)
        {
            string query = @"SELECT * FROM [AM].[MailBatches] WHERE Id=@MailBatchId";

            return db.QuerySingle<MailBatchDTO>(query, new { MailBatchId = mailBatchId });
        }

        public int InsertPrintBatchDetails(PrintBatch printBatch)
        {
            //Sql Query
            string query = "INSERT INTO [AM].[PrintBatches](UniqueId, AppCode, PrintTrailId, DocTypeId,PnSConfigDetailId, MailBatchId) Output Inserted.Id VALUES (@UniqueId, @AppCode, @PrintTrailId, @DocTypeId, @PnSConfigDetailId, @MailBatchId)";

            var printBatchId = db.ExecuteScalar<int>(query, printBatch);
            foreach (var mailPiece in printBatch.MailPieces)
            {
                string updateQuery = "UPDATE [AM].[MailPieces] SET [PrintBatchId]=@PrintBatchId,IsQCRequested=@IsQCRequested WHERE Id=@Id";

                var data = db.Execute(updateQuery, new { PrintBatchId = printBatchId, Id = mailPiece.Id, IsQCRequested = mailPiece.IsQCRequested });
            }
            return printBatchId;

        }

        public List<SubmittedBatch> GetSubmittedPrintBatchDetails(string serverIP)
        {
            //Sql Query
            string query = $@"SELECT PB.Id AS PrintBatchId, PB.AppCode +'_'+ CONVERT(VARCHAR(10),PB.Id) AS SubmittedBatchName,PB.UniqueId,PB.DocTypeId,PB.AppCode FROM AM.PrintBatches PB INNER JOIN AM.MailBatches B ON B.Id = PB.MailBatchId WHERE PB.IsSubmittedToShipping=1 AND PB.IsShippingResponseReceived=0 AND B.ServerIP = @serverIP";

            return db.Query<SubmittedBatch>(query, new { serverIP }).ToList();
        }

        public List<SubmittedBatch> GetResponseReceivedPrintBatchDetails(string serverIP)
        {
            //Sql Query
            string query = $@"SELECT TOP {Config.Common_MaxProcessingLimit} PB.Id AS PrintBatchId,PB.MailBatchId, PB.AppCode +'_'+ CONVERT(VARCHAR(10),PB.Id) AS SubmittedBatchName,PB.UniqueId,PB.DocTypeId,PB.AppCode,MB.SFTChannelId FROM AM.PrintBatches PB INNER JOIN AM.MailBatches MB ON PB.MailBatchId=MB.Id WHERE PB.IsSubmittedToShipping=1 AND PB.IsShippingResponseReceived=1 AND PB.BatchStatus=@BatchStatus AND MB.ServerIP = @serverIP";

            return db.Query<SubmittedBatch>(query, new { BatchStatus = BatchStatus.InProgress.ToString(), serverIP }).ToList();
        }

        public List<SubmittedBatch> GetReportPreppedPrintBatchDetails(int sftChannelId, string serverIP)
        {
            //Sql Query
            string query = @"SELECT DISTINCT PB.Id AS PrintBatchId,PB.MailBatchId, PB.AppCode +'_'+ CONVERT(VARCHAR(10),PB.Id) AS SubmittedBatchName,PB.UniqueId,PB.DocTypeId,PB.AppCode,MB.SFTChannelId FROM AM.PrintBatches PB INNER JOIN AM.MailBatches MB ON PB.MailBatchId=MB.Id WHERE PB.IsSubmittedToShipping=1 AND PB.IsReportPrepped=1 AND PB.IsReportedBack=0 AND MB.SFTChannelId = @sftChannelId AND MB.ServerIP = @serverIP";

            return db.Query<SubmittedBatch>(query, new { sftChannelId, serverIP }).ToList();
        }

        public int GetMailBatchesReceivedCount(DateTime startDateTime, DateTime endDateTime, string status)
        {
            string query = @"SELECT COUNT(*) FROM [AM].[MailBatches] WHERE ReceivedOn >=@StartDate AND ReceivedOn <=@EndDate";

            if (status == "Received")
                query += " AND IsReceived=1";
            else
                query += " AND (IsPrepped = 1 OR IsReportPrepped = 1 OR IsReportedBack = 1 OR PipelineStatus='SubmittedToEDelivery')";

            return db.ExecuteScalar<int>(query, new { StartDate = startDateTime, EndDate = endDateTime });
        }

        public int UpdatePrintBatchPipelineStatus(int printBatchId, string pipeLineStatus, DateTime timestamp)
        {
            string query = "UPDATE [AM].[PrintBatches] SET [{0}On] = @TimeStamp WHERE Id=@Id".Fill(pipeLineStatus);

            return db.Execute(query, new { TimeStamp = timestamp, Id = printBatchId });
        }

        public int RemainingPrintBatchesReportedBackCount(int mailBatchId)
        {
            string query = @"SELECT COUNT(*) FROM AM.PrintBatches PB WHERE PB.MailBatchId=@MailBatchId AND PB.IsReportedBack=0";

            return db.ExecuteScalar<int>(query, new { MailBatchId = mailBatchId });
        }

        public async Task<int> InsertBatchDetails(MailBatch mailBatch, int clientId, SFTChannel sFTChannel)
        {
            string query = @"INSERT INTO [AM].[MailBatches](UniqueId, ClientId, SFTChannelId, ClientBatchId,  BatchName, Priority) Output Inserted.Id VALUES (@UniqueId, @ClientId, @SFTChannelId, @ClientBatchId, @BatchName, @Priority)";

            return await db.ExecuteScalarAsync<int>(query, new { UniqueId = mailBatch.Id, ClientId = clientId, SFTChannelId = sFTChannel.Id, ClientBatchId = mailBatch.ClientBatchId, BatchName = mailBatch.ClientBatchId, Priority = Priority.Low.ToString() });
        }

        public int InsertLastProcessTimeForUATBatches(DateTime lastProcessedDateTime)
        {
            string query = "INSERT INTO dbo.ProductionBridge(LastProcessedOn) VALUES (@LastProcessedDateTimeOn)";
            return db.Execute(query, new { LastProcessedDateTimeOn = lastProcessedDateTime });
        }

        public MailPieceDTO GetMailPiecesDataFromMailBatchIdAndMailPieceId(int mailBatchId, int mailPieceId)
        {
            var mailPieceDictionary = new Dictionary<int, MailPieceDTO>();

            string query = @"SELECT * FROM AM.MailPieces M INNER JOIN AM.MailBatches B ON M.MailBatchId=B.Id INNER JOIN Common.Clients C ON M.ClientId=C.Id LEFT OUTER JOIN v_DocTypes D ON M.DocTypeId=D.Id AND M.PnSConfigDetailId = D.PnSConfigDetailId LEFT OUTER JOIN AM.MailPieceCustomInfo CI ON M.Id=CI.MailPieceId  WHERE M.MailBatchId=@MailBatchId AND M.Id=@MailPieceId";

            var data = db.Query<MailPieceDTO, MailBatchDTO, Client, DocType, MailPieceCustomInfo, MailPieceDTO>(query,
               (m, b, c, d, ci) =>
               {
                   MailPieceDTO mp;

                   if (!mailPieceDictionary.TryGetValue(m.Id, out mp))
                   {
                       mp = m;
                       mp.MailBatch = b;
                       mp.Client = c;
                       mp.DocType = d;
                       mp.MailPieceCustomInfo = new List<MailPieceCustomInfo>();
                       mailPieceDictionary.Add(mp.Id, mp);
                   }

                   mp.MailPieceCustomInfo.Add(ci);
                   return mp;
               },
               new
               {
                   MailBatchId = mailBatchId,
                   MailPieceId = mailPieceId
               }, splitOn: "Id,Id,MailPieceId").Distinct().ToList();
            return data.SingleOrDefault();
        }

        public MailPieceDTO ProcessReshipData(MailPieceDTO mailPiece)
        {
            using (ICommonService commonSvc = new CommonService())
            using (IMailPieceService mailPieceSvc = new MailPieceService())
            using (IDocTypeService docTypeSvc = new DocTypeService())
            using (IClientService clientSvc = new ClientService())
            {
                MailPieceDTO newMailPiece = new MailPieceDTO();
                newMailPiece = mailPiece;

                var newBatch = newMailPiece.MailBatch;
                newBatch.BatchStatus = "InProgress";
                newBatch.PipelineStatus = BatchPipelineStatus.Acknowledged;
                newBatch.Priority = Priority.Medium.ToString();
                newBatch.ReceivedOn = Common.Now;
                newBatch.DownloadedOn = Common.Now;
                newBatch.ValidatedOn = Common.Now;
                newBatch.AcknowledgedOn = Common.Now;
                newBatch.UniqueId = Guid.NewGuid();
                newBatch.ValidationErrors = null;
                newBatch.ValidationStatus = BatchValidationStatus.Valid;
                newBatch.Id = InsertBatchDetails(newBatch, true);

                newMailPiece.ErrorCode = OutComeCodes.No_Error;
                newMailPiece.ErrorDesc = null;
                newMailPiece.MailBatchId = newBatch.Id;
                newMailPiece.MissedSLAReason = null;
                newMailPiece.PickedUpOn = null;
                newMailPiece.PrintBatchId = null;
                newMailPiece.PrintedOn = null;
                newMailPiece.PullBackRequestedBy = null;
                newMailPiece.PullBackRequestedOn = null;
                newMailPiece.PulledBackOn = null;
                newMailPiece.ReturnDeliveryDate = null;
                newMailPiece.ReturnDeliveryStatus = null;
                newMailPiece.ReturnTrackingNo = null;
                newMailPiece.ShippedOn = null;
                newMailPiece.ShippingCost = 0;
                newMailPiece.ShippingStatus = ShippingStatus.InProgress.ToString();
                newMailPiece.TrackingNo = null;
                newMailPiece.UniqueId = Guid.NewGuid();
                newMailPiece.Weight = 0;
                //Here we are storing originalUnique Id for reporting purpose.
                newMailPiece.MailPieceCustomInfo.Add(new MailPieceCustomInfo() { Key = "OriginalUniqueId", DataType = DataType.String, Value = mailPiece.UniqueId.ToString() });
                bool IsCountryUS = mailPiece.RcptAddressCountry.IsCountryUS();
                var docType = !IsCountryUS ? docTypeSvc.GetIntlDocTypeByDocTypeName(mailPiece.ClientId, mailPiece.DocumentOrPackageType, mailPiece.TotalPages, newBatch.ReceivedOn.Value.UtcToEst()) : docTypeSvc.GetDocTypeByDocTypeName(mailPiece.ClientId, mailPiece.DocumentOrPackageType, mailPiece.TotalPages, newBatch.ReceivedOn.Value.UtcToEst());

                if (docType != null)
                {
                    newMailPiece.DocTypeId = docType.Id;
                    newMailPiece.PnSConfigDetailId = docType.PnSConfigDetailId;
                    newMailPiece.PrintTrailId = docType.PrintTrailId;
                    newMailPiece.DocType = docType;
                }

                var client = clientSvc.GetClientById(newMailPiece.ClientId);
                newMailPiece.Client = client;

                commonSvc.GetUpdatedDueDateAndPrintLocation(newBatch.ReceivedOn.Value, newMailPiece, newMailPiece.DocType, newMailPiece.Client);
                newMailPiece.Id = mailPieceSvc.InsertBatchMailPieceDetails(newMailPiece, newMailPiece.MailPieceCustomInfo);
                mailPieceSvc.UpdateBatchMailPieceDetails(newMailPiece);
                newMailPiece.MailBatch = newBatch;
                return newMailPiece;
            }
        }

        public int UpdateHoldBatchesInfo(int clientId, DateTime heldOn)
        {
            string query = $"UPDATE AM.MailBatches SET PipelineStatus='{BatchPipelineStatus.Acknowledged}' WHERE ClientId=@ClientId AND PipelineStatus='{BatchPipelineStatus.Held}' AND CAST (HeldOn AS DATE) = @HeldOn";

            return db.Execute(query, new { ClientId = clientId, HeldOn = heldOn });
        }
    }
}
