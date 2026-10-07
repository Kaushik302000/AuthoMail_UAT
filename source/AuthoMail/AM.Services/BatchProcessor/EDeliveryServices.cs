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
using AM.USPSLib;

namespace AM.Services
{
    public interface IEDeliveryService : IDisposable
    {
        List<EDMailPiece> GetBatchesByPipelineStatusAndSFTChannelId(string batchPipelineStatus);
        int UpdateEDMailPiecePipelineStatus(int mailPieceId, string pipeLineStatus, DateTime timestamp);
        int UpdateEDMailPieceActivityStatus(int mailPieceId, string pipeLineStatus, DateTime timestamp);
        int UpdateEDMailPieceActivityStatus(int mailPieceId, bool isESignRequested, DateTime? consentAcceptedOn, DateTime? consentDeclinedOn, DateTime? docReviewedOn, DateTime? docEsignedOn, DateTime? docDownloadedOn, DateTime? expiryDate, string ipAddress);
        int UpdateEDMailPieceDetails(EDMailPiece mailPiece);
        int ErrorOutOrFallBackEDeliveryPackage(EDMailPiece edmp);
        int UpdateEDeliveredMPForReportingBack(int mailBatchId, DateTime reportPreppedTiming, string batchStatus);
        List<EsignDocConfig> GetEsignDocumentConfigurationByClientIdAndDocTypeId(int clientId, int docTypeId);
        List<EDMailPiece> GetESignedDocList();
        List<EDMailPiece> GetESignedDocList(DateTime startDate, DateTime endDate, int? clientId);
    }
    public class EDeliveryService : ServiceBase, IEDeliveryService
    {
        public List<EDMailPiece> GetBatchesByPipelineStatusAndSFTChannelId(string pipelineStatus)
        {
            //Sql Query
            string query = "SELECT ED.Id, ED.UniqueId, ED.ClientId, ED.MailBatchId, MP.BusinessFriendlyIdValue AS LoanNumber, MP.FileName, MP.ExpiryDate, ED.PipelineStatus, MP.RcptFirstName, MP.RcptMiddleName,MP.RcptLastName, MP.RcptEmailId, MP.IsFallBackRequested, MP.IsESignRequested, ED.TotalPages, ED.ErrorCode, ED.ErrorDesc, ED.SigningRoomId, ED.SigningRoomUniqueId, ED.SigningRoomName,ED.SRDocId,ED.SRDocGuId, ED.ReceivedOn, ED.IsReceived, ED.DownloadedOn, ED.IsDownloaded, ED.ValidatedOn, ED.IsValidated, ED.SigningRoomCreatedOn,ED.IsSigningRoomCreated, ED.InvitationSentOn, ED.IsInvitationSent, ED.eDeliveredOn, ED.IseDelivered, ED.ConsentAcceptedOn, ED.IsConsentAccepted,ED.ConsentDeclinedOn, ED.IsConsentDeclined, ED.DocReviewedOn, ED.IsDocReviewed, ED.DocDownloadedOn, ED.IsDocDownloaded, ED.ESignedOn, ED.IsESigned, C.AppCode AS ClientCode,B.SFTChannelId AS ChannelId,S.Code AS ChannelCode,B.BatchName,ED.DownloadedIPAddress, MP.DocTypeId FROM [AM].[EDeliveryMailPieces] ED INNER JOIN AM.MailBatches B ON ED.MailBatchId = B.Id INNER JOIN AM.MailPieces MP ON ED.UniqueId = MP.UniqueId AND ED.MailBatchId = MP.MailBatchId LEFT OUTER JOIN Common.Clients C ON ED.ClientId = C.Id LEFT OUTER JOIN Common.SFTChannels S ON B.SFTChannelId = S.Id WHERE ED.PipelineStatus = @PipelineStatus";

            switch (pipelineStatus)
            {
                case EDPipelineStatus.Validated:
                    query += " AND ED.IsValidated=1 AND ED.IsSigningRoomCreated=0";
                    break;
                case EDPipelineStatus.SigningRoomCreated:
                    query += " AND ED.IsSigningRoomCreated=1 AND ED.IsInvitationSent=0";
                    break;
                case EDPipelineStatus.InvitationSent:
                    query += " AND ED.IsInvitationSent=1";
                    break;
                case EDPipelineStatus.eDelivered:
                    query += " AND ED.IseDelivered=1 AND ED.IsReportedBack=0";
                    break;
                case EDPipelineStatus.Expired:
                    query += " AND ED.IsReportedBack=0";
                    break;
                default:
                    break;
            }

            return db.Query<EDMailPiece>(query, new { PipelineStatus = pipelineStatus }).ToList();
        }

        public int UpdateEDMailPiecePipelineStatus(int mailPieceId, string pipeLineStatus, DateTime timestamp)
        {
            string query = "UPDATE [AM].[EDeliveryMailPieces] SET [{0}On] = @TimeStamp, PipelineStatus=@PipelineStatus WHERE Id=@Id".Fill(pipeLineStatus);

            return db.Execute(query, new { TimeStamp = timestamp, Id = mailPieceId, PipelineStatus = pipeLineStatus });
        }

        public int UpdateEDMailPieceActivityStatus(int mailPieceId, string pipeLineStatus, DateTime timestamp)
        {
            string query = "UPDATE [AM].[EDeliveryMailPieces] SET [{0}On] = @TimeStamp WHERE Id=@Id".Fill(pipeLineStatus);

            return db.Execute(query, new { TimeStamp = timestamp, Id = mailPieceId });
        }

        public int UpdateEDMailPieceActivityStatus(int mailPieceId, bool isESignRequested, DateTime? consentAcceptedOn, DateTime? consentDeclinedOn, DateTime? docReviewedOn, DateTime? docEsignedOn, DateTime? docDownloadedOn, DateTime? expiryDate, string ipAddress)
        {
            return db.Execute("[dbo].[usp_UpdateEDMailPieceActivityStatus]", new { @Id = mailPieceId, @IsESignRequested = isESignRequested, @ConsentAcceptedOn = consentAcceptedOn, @ConsentDeclinedOn = consentDeclinedOn, @DocReviewedOn = docReviewedOn, @DocEsignedOn = docEsignedOn, @DocDownloadedOn = docDownloadedOn, @ExpiryDate = expiryDate, @IpAddress = ipAddress }, commandType: CommandType.StoredProcedure);
        }

        public int UpdateEDMailPieceDetails(EDMailPiece mailPiece)
        {
            string query = @"UPDATE [AM].[EDeliveryMailPieces] SET  SRDocId=@SRDocId,SRDocGuId=@SRDocGuId, ErrorCode=@ErrorCode, ErrorDesc=@ErrorDesc, SigningRoomId=@SigningRoomId, SigningRoomUniqueId=@SigningRoomUniqueId, SigningRoomName=@SigningRoomName, IsESignedDocDownloaded = @IsESignedDocDownloaded WHERE Id=@Id";

            return db.Execute(query, mailPiece);
        }

        public int ErrorOutOrFallBackEDeliveryPackage(EDMailPiece edmp)
        {
            if (edmp.IsFallBackRequested)
            {
                using (ICommonService commonSvc = new CommonService())
                using (IClientService clientSvc = new ClientService())
                using (IMailPieceService mpSvc = new MailPieceService())
                using (IDocTypeService docTypeSvc = new DocTypeService())
                using (IEDeliveryService eDeliverySvc = new EDeliveryService())
                {
                    //Here system is making received on date and time update to latest after fallback.
                    var newReceivedOnDateTime = Common.Now;
                    commonSvc.UpdateBatchPipeLineStatus(edmp.MailBatchId, BatchPipelineStatus.Received, newReceivedOnDateTime).Wait();
                    commonSvc.UpdateBatchPipeLineStatus(edmp.MailBatchId, BatchPipelineStatus.Downloaded, newReceivedOnDateTime).Wait();

                    //System will calculate print trail,print location, duedate etc.
                    MailPieceDTO mailPieceDTO = mpSvc.GetMailPieceDataByUniqueIdAndMBId(edmp.UniqueId, edmp.MailBatchId);

                    Client client = clientSvc.GetClientById(edmp.ClientId.Value);

                    //NOTE : Reason to take document type based on Name to just put valid Id after fall back
                    bool IsCountryUS = mailPieceDTO.RcptAddressCountry.IsCountryUS();
                    DocType mpDocType = !IsCountryUS ? docTypeSvc.GetIntlDocTypeByDocTypeName(mailPieceDTO.ClientId, mailPieceDTO.DocumentOrPackageType, edmp.TotalPages, edmp.ReceivedOn.Value.UtcToEst()) : docTypeSvc.GetDocTypeByDocTypeName(mailPieceDTO.ClientId, mailPieceDTO.DocumentOrPackageType, edmp.TotalPages, edmp.ReceivedOn.Value.UtcToEst());
                    mailPieceDTO.DocTypeId = mpDocType.Id;
                    mailPieceDTO.PnSConfigDetailId = mpDocType.PnSConfigDetailId;
                    mailPieceDTO.PrintTrailId = mpDocType.PrintTrailId;
                    mailPieceDTO.IsEDeliveryRequested = false;
                    mailPieceDTO.IsESignRequested = false;
                    GetExactDocumentType(mpDocType, mailPieceDTO);
                    commonSvc.GetUpdatedDueDateAndPrintLocation(newReceivedOnDateTime, mailPieceDTO, mpDocType, client);
                    mpSvc.UpdateBatchMailPieceDetails(mailPieceDTO);

                    var newValidatedOnDateTime = Common.Now;
                    commonSvc.UpdateBatchPipeLineStatus(edmp.MailBatchId, BatchPipelineStatus.Acknowledged, newValidatedOnDateTime).Wait();
                    eDeliverySvc.UpdateEDMailPiecePipelineStatus(edmp.Id, EDPipelineStatus.ReportedBack, newValidatedOnDateTime);
                    return 1;
                }
            }
            else
            {
                string query = "UPDATE AM.MailPieces SET ShippingStatus='Error', ErrorCode=@ErrorCode, ErrorDesc=@ErrorDesc WHERE MailBatchId=@MailBatchId AND UniqueId=@UniqueId";
                var retVal = db.Execute(query, new { ErrorCode = edmp.ErrorCode, ErrorDesc = edmp.ErrorDesc, MailBatchId = edmp.MailBatchId, UniqueId = edmp.UniqueId });

                if (retVal == 1)
                {
                    using (ICommonService commonSvc = new CommonService())
                    using (IEDeliveryService eDeliverySvc = new EDeliveryService())
                    {
                        var newValidatedOnDateTime = Common.Now;
                        eDeliverySvc.UpdateEDMailPiecePipelineStatus(edmp.Id, EDPipelineStatus.ReportedBack, newValidatedOnDateTime);
                        commonSvc.UpdateBatchStatus(edmp.MailBatchId, BatchStatus.Failed.ToString()).Wait();
                        commonSvc.UpdateBatchValidationStatus(edmp.MailBatchId, BatchValidationStatus.Invalid, edmp.ErrorDesc);
                        commonSvc.UpdateBatchPipeLineStatus(edmp.MailBatchId, BatchPipelineStatus.Validated, newValidatedOnDateTime).Wait();
                        return 1;
                    }
                }
            }

            return 0;
        }

        private void GetExactDocumentType(DocType docType, MailPieceDTO mailPiece)
        {
            using (IDocTypeService docTypeSvc = new DocTypeService())
            using (IMailPieceService mailPieceSvc = new MailPieceService())
            {
                if (docType.ShippingCarrier == Carrier.FedEx.ToString())
                {
                    if (mailPiece.RcptAddressTypes.Contains(AddressType.Military.ToString()) || mailPiece.RcptAddressTypes.Contains(AddressType.POBOX.ToString()) || mailPiece.RcptAddressTypes.Contains(AddressType.Territory.ToString()))
                    {
                        var fbPriorityExpress = docTypeSvc.GetFallBackDocTypeByPnSConfigId(mailPiece.ClientId, mailPiece.DocTypeId.Value, mailPiece.ReceivedOn.UtcToEst(), Carrier.USPS.ToString(), ShippingServiceType.PRIORITY_EXPRESS.ToString(), mailPiece.Id);


                        if (fbPriorityExpress != null)
                        {
                            docType = fbPriorityExpress;
                        }
                        else
                        {
                            var fbPriority = docTypeSvc.GetFallBackDocTypeByPnSConfigId(mailPiece.ClientId, mailPiece.DocTypeId.Value, mailPiece.ReceivedOn.UtcToEst(), Carrier.USPS.ToString(), ShippingServiceType.PRIORITY.ToString(), mailPiece.Id);

                            if (fbPriority == null)
                            {
                                return;
                            }

                            docType = fbPriority;
                        }

                        mailPiece.DocTypeId = docType.Id;

                        if (!mailPiece.IsReturnInfoSpecified && docType.IsReturnInfoSpecified)
                        {
                            mailPiece.RetRcptFirstName = docType.RetRcptFirstName;
                            mailPiece.RetRcptLastName = docType.RetRcptLastName;
                            mailPiece.RetRcptMiddleName = docType.RetRcptMiddleName;
                            mailPiece.RetRcptOrganization = docType.RetRcptOrganization;
                            mailPiece.RetRcptPrefix = docType.RetRcptPrefix;
                            mailPiece.RetRcptSuffix = docType.RetRcptSuffix;
                            mailPiece.RetRcptPhone = docType.RetRcptPhone;

                            mailPiece.RetRcptAddrOptionalLine1AboveStreet = docType.RetRcptAddrOptionalLine1AboveStreet;
                            mailPiece.RetRcptAddrOptionalLine2AboveStreet = docType.RetRcptAddrOptionalLine2AboveStreet;
                            mailPiece.RetRcptAddressStreet = docType.RetRcptAddressStreet;
                            mailPiece.RetRcptAddressAptOrSuiteOrOther = docType.RetRcptAddressAptOrSuiteOrOther;
                            mailPiece.RetRcptAddrOptionalLine1BelowStreet = docType.RetRcptAddrOptionalLine1BelowStreet;
                            mailPiece.RetRcptAddrOptionalLine2BelowStreet = docType.RetRcptAddrOptionalLine2BelowStreet;
                            mailPiece.RetRcptAddressCity = docType.RetRcptAddressCity;
                            mailPiece.RetRcptAddressState = docType.RetRcptAddressState;
                            mailPiece.RetRcptAddressZIP = docType.RetRcptAddressZIP;
                        }
                    }
                }
            }
        }

        public int UpdateEDeliveredMPForReportingBack(int mailBatchId, DateTime reportPreppedTiming, string batchStatus)
        {
            using (ICommonService commonSvc = new CommonService())
            {
                commonSvc.UpdateBatchStatus(mailBatchId, batchStatus).Wait();
                return commonSvc.UpdateBatchPipeLineStatus(mailBatchId, BatchPipelineStatus.ReportPrepped, reportPreppedTiming).Result;
            }
        }

        public List<EsignDocConfig> GetEsignDocumentConfigurationByClientIdAndDocTypeId(int clientId, int docTypeId)
        {
            string query = "SELECT [ClientId], [DocTypeId], [SignHeight], [SignWidth], [SignXOffset], [SignYOffset], [Optional], [SignPageNumber] FROM [AM].[EsignDocConfig] WHERE ClientId = @ClientId AND DocTypeId = @DoctypeId";
            return db.Query<EsignDocConfig>(query, new { ClientId = clientId, DocTypeId = docTypeId }).ToList();
        }

        public List<EDMailPiece> GetESignedDocList(DateTime startDate, DateTime endDate, int? clientId)
        {
            string query = "";
            if (clientId != null)
            {
                query = "SELECT ED.Id, ED.UniqueId, ED.ClientId, ED.MailBatchId, MP.BusinessFriendlyIdValue AS LoanNumber, MP.FileName, MP.ExpiryDate, ED.PipelineStatus, MP.RcptFirstName, MP.RcptMiddleName,MP.RcptLastName, MP.RcptEmailId, MP.IsFallBackRequested, MP.IsESignRequested, ED.TotalPages, ED.ErrorCode, ED.ErrorDesc, ED.SigningRoomId, ED.SigningRoomUniqueId, ED.SigningRoomName,ED.SRDocId,ED.SRDocGuId, ED.ReceivedOn, ED.IsReceived, ED.DownloadedOn, ED.IsDownloaded, ED.ValidatedOn, ED.IsValidated, ED.SigningRoomCreatedOn,ED.IsSigningRoomCreated, ED.InvitationSentOn, ED.IsInvitationSent, ED.eDeliveredOn, ED.IseDelivered, ED.ConsentAcceptedOn, ED.IsConsentAccepted,ED.ConsentDeclinedOn, ED.IsConsentDeclined, ED.DocReviewedOn, ED.IsDocReviewed, ED.DocDownloadedOn, ED.IsDocDownloaded, ED.ESignedOn, ED.IsESigned, C.AppCode AS ClientCode,B.SFTChannelId AS ChannelId,S.Code AS ChannelCode,B.BatchName,ED.DownloadedIPAddress, MP.DocTypeId,ED.IsESignedDocDownloaded, C.RetentionPeriod FROM [AM].[EDeliveryMailPieces] ED INNER JOIN AM.MailBatches B ON ED.MailBatchId = B.Id INNER JOIN AM.MailPieces MP ON ED.UniqueId = MP.UniqueId AND ED.MailBatchId = MP.MailBatchId LEFT OUTER JOIN Common.Clients C ON ED.ClientId = C.Id LEFT OUTER JOIN Common.SFTChannels S ON B.SFTChannelId = S.Id WHERE ED.IsESigned = 1 AND ED.IsESignedDocDownloaded=1 AND ED.ClientId = @ClientId AND ED.ReceivedOn BETWEEN @StartDate AND @EndDate";
                return db.Query<EDMailPiece>(query, new { ClientId = clientId.Value, StartDate = startDate, EndDate = endDate }).ToList();
            }
            else
            {
                query = "SELECT ED.Id, ED.UniqueId, ED.ClientId, ED.MailBatchId, MP.BusinessFriendlyIdValue AS LoanNumber, MP.FileName, MP.ExpiryDate, ED.PipelineStatus, MP.RcptFirstName, MP.RcptMiddleName,MP.RcptLastName, MP.RcptEmailId, MP.IsFallBackRequested, MP.IsESignRequested, ED.TotalPages, ED.ErrorCode, ED.ErrorDesc, ED.SigningRoomId, ED.SigningRoomUniqueId, ED.SigningRoomName,ED.SRDocId,ED.SRDocGuId, ED.ReceivedOn, ED.IsReceived, ED.DownloadedOn, ED.IsDownloaded, ED.ValidatedOn, ED.IsValidated, ED.SigningRoomCreatedOn,ED.IsSigningRoomCreated, ED.InvitationSentOn, ED.IsInvitationSent, ED.eDeliveredOn, ED.IseDelivered, ED.ConsentAcceptedOn, ED.IsConsentAccepted,ED.ConsentDeclinedOn, ED.IsConsentDeclined, ED.DocReviewedOn, ED.IsDocReviewed, ED.DocDownloadedOn, ED.IsDocDownloaded, ED.ESignedOn, ED.IsESigned, C.AppCode AS ClientCode,B.SFTChannelId AS ChannelId,S.Code AS ChannelCode,B.BatchName,ED.DownloadedIPAddress, MP.DocTypeId,ED.IsESignedDocDownloaded, C.RetentionPeriod FROM [AM].[EDeliveryMailPieces] ED INNER JOIN AM.MailBatches B ON ED.MailBatchId = B.Id INNER JOIN AM.MailPieces MP ON ED.UniqueId = MP.UniqueId AND ED.MailBatchId = MP.MailBatchId LEFT OUTER JOIN Common.Clients C ON ED.ClientId = C.Id LEFT OUTER JOIN Common.SFTChannels S ON B.SFTChannelId = S.Id WHERE ED.IsESigned = 1 AND ED.IsESignedDocDownloaded=1 AND ED.ReceivedOn BETWEEN @StartDate AND @EndDate";
                return db.Query<EDMailPiece>(query, new { ClientId = clientId.Value, StartDate = startDate, EndDate = endDate }).ToList();
            }
        }

        public List<EDMailPiece> GetESignedDocList()
        {
            //Sql Query
            string query = "SELECT ED.Id, ED.UniqueId, ED.ClientId, ED.MailBatchId, MP.BusinessFriendlyIdValue AS LoanNumber, MP.FileName, MP.ExpiryDate, ED.PipelineStatus, MP.RcptFirstName, MP.RcptMiddleName,MP.RcptLastName, MP.RcptEmailId, MP.IsFallBackRequested, MP.IsESignRequested, ED.TotalPages, ED.ErrorCode, ED.ErrorDesc, ED.SigningRoomId, ED.SigningRoomUniqueId, ED.SigningRoomName,ED.SRDocId,ED.SRDocGuId, ED.ReceivedOn, ED.IsReceived, ED.DownloadedOn, ED.IsDownloaded, ED.ValidatedOn, ED.IsValidated, ED.SigningRoomCreatedOn,ED.IsSigningRoomCreated, ED.InvitationSentOn, ED.IsInvitationSent, ED.eDeliveredOn, ED.IseDelivered, ED.ConsentAcceptedOn, ED.IsConsentAccepted,ED.ConsentDeclinedOn, ED.IsConsentDeclined, ED.DocReviewedOn, ED.IsDocReviewed, ED.DocDownloadedOn, ED.IsDocDownloaded, ED.ESignedOn, ED.IsESigned, C.AppCode AS ClientCode,B.SFTChannelId AS ChannelId,S.Code AS ChannelCode,B.BatchName,ED.DownloadedIPAddress, MP.DocTypeId FROM [AM].[EDeliveryMailPieces] ED INNER JOIN AM.MailBatches B ON ED.MailBatchId = B.Id INNER JOIN AM.MailPieces MP ON ED.UniqueId = MP.UniqueId AND ED.MailBatchId = MP.MailBatchId LEFT OUTER JOIN Common.Clients C ON ED.ClientId = C.Id LEFT OUTER JOIN Common.SFTChannels S ON B.SFTChannelId = S.Id WHERE ED.IsESigned = 1 AND ED.IsESignedDocDownloaded = 0";

            return db.Query<EDMailPiece>(query).ToList();
        }
    }
}
