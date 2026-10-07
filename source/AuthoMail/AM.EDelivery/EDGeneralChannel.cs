using AM.Entities;
using AM.Lib;
using AM.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Transactions;

namespace AM.EDelivery
{
    class EDGeneralChannel : EDChannel
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        protected IGeneralService generalSvc;

        public EDGeneralChannel()
            : base()
        {
            generalSvc = GetService<GeneralService>();
        }

        protected override void CreateSigningRoom()
        {
            log.Debug("EDelivery CreateSigningRoom job queue is started.");

            List<EDMailPiece> validatedMailPieces = eDeliverySvc.GetBatchesByPipelineStatusAndSFTChannelId(EDPipelineStatus.Validated);
            if (validatedMailPieces.Count == 0)
            {
                log.Debug("EDelivery CreateSigningRoom job queue is empty.");
                log.Debug("EDelivery CreateSigningRoom job queue is completed.");
                return;
            }

            foreach (EDMailPiece edmp in validatedMailPieces)
            {
                try
                {
                    MailPieceInfo mpi = new MailPieceInfo(edmp.ClientCode, Config.Common_AuthoMailRootFolder, edmp.MailBatchId, edmp.BatchName);
                    edmp.SigningRoomName = $"{edmp.Id}-{edmp.ClientCode}-{Path.GetFileNameWithoutExtension(edmp.FileName)}-{Common.Now.ToString("yyyyMMddhhmmss")}";
                    DateTime expiryDate = edmp.ExpiryDate.Value;
                    edmp.ExpiryDate = expiryDate;

                    SigningRoomRequest request = new SigningRoomRequest()
                    {
                        SigningRoomName = edmp.SigningRoomName,
                        Description = "",
                        ExpirationDate = expiryDate.UtcToEst(),
                        StartDate = Common.Now.Date.UtcToClientTimeZone("Pacific Standard Time")
                    };



                    DocumentInfo documentInfo = new DocumentInfo();
                    documentInfo.DocumentData = Convert.ToBase64String(File.ReadAllBytes(Path.Combine(mpi.AMRequestPath, edmp.FileName))).ToString();
                    documentInfo.DocumentName = Path.GetFileNameWithoutExtension(Path.Combine(mpi.AMRequestPath, edmp.FileName));
                    documentInfo.FileName = Path.GetFileName(Path.Combine(mpi.AMRequestPath, edmp.FileName));

                    if (edmp.IsESignRequested)
                    {
                        var eSignPageList = eDeliverySvc.GetEsignDocumentConfigurationByClientIdAndDocTypeId(edmp.ClientId.Value, edmp.DocTypeId);
                        var documentSigners = new List<DocumentSigners>();
                        var docAnnotations = new List<DocumentAnnotations>();
                        foreach (var item in eSignPageList)
                        {
                            docAnnotations.Add(new DocumentAnnotations() { AnnotationType = "Signature", Height = item.SignHeight, Width = item.SignWidth, XOffset = item.SignXOffset, YOffset = item.SignYOffset, Optional = item.Optional, PageNumber = item.SignPageNumber });
                        }

                        documentSigners.Add(new DocumentSigners() { FirstName = edmp.RcptFirstName, LastName = edmp.RcptLastName, InvitationEmail = edmp.RcptEmailId, DocumentAnnotations = docAnnotations });

                        documentInfo.DocumentSigners = documentSigners;
                    }

                    request.Documents = new List<DocumentInfo>() {
                        documentInfo
                    };

                    request.Participants = new List<Participant>() {
                        new Participant()
                        {
                            FirstName = edmp.RcptFirstName,
                            LastName = edmp.RcptLastName,
                            InvitationEmail = edmp.RcptEmailId
                        }
                    };


                    IEDeliveryAPIService eDeliveryAPISvc = new EDeliveryAPIService();
                    int reTrycnt = 0;
                    bool hasError = false;
                    while (reTrycnt < Config.Core_NoOfAttemptOnFailure)
                    {
                        var response = eDeliveryAPISvc.CreateSigningRoom(request, edmp.ClientId.Value).Result;
                        if (!response.HasError)
                        {
                            edmp.SigningRoomId = response.SigningRoom.SigningRoomId;
                            edmp.SigningRoomUniqueId = response.SigningRoom.SigningRoomGuid;
                            edmp.SRDocId = response.SigningRoom.Documents[0].DocumentId;
                            edmp.SRDocGuId = response.SigningRoom.Documents[0].DocumentGuid;
                            eDeliverySvc.UpdateEDMailPieceDetails(edmp);
                            eDeliverySvc.UpdateEDMailPiecePipelineStatus(edmp.Id, EDPipelineStatus.SigningRoomCreated, Common.Now);
                            hasError = false;
                            break;
                        }
                        else if (response.HasError && response.Error.Contains("already exists") || response.Error.Contains("expiration date must be greater than or equal to"))
                        {
                            edmp.ErrorCode = OutComeCodes.Issue_In_EDelivery;
                            edmp.ErrorDesc = response.Error;
                            hasError = true;
                            break;
                        }

                        var msg = "Error occured while creating signing room for Id {0} with Error".Fill(edmp.Id) + Environment.NewLine + response.Error;
                        log.Error(msg);
                        MailUtility.SendUnexpectedExceptionEmail(msg);
                        edmp.ErrorCode = OutComeCodes.Issue_In_EDelivery;
                        edmp.ErrorDesc = $"Error occured while creating signing room for Loan Number:{edmp.LoanNumber} with Error {response.Error}";
                        System.Threading.Thread.Sleep(Config.Core_FailureAttemptInterval);
                        hasError = true;
                        reTrycnt++;
                    }

                    if (hasError)
                    {
                        eDeliverySvc.UpdateEDMailPieceDetails(edmp);
                        #region Check ErrorOut or IsFallBackRequested Package
                        eDeliverySvc.ErrorOutOrFallBackEDeliveryPackage(edmp);
                        #endregion
                    }
                }
                catch (Exception ex)
                {
                    var msg = "Error occured while creating signing room for Id {0} for client {1} with below error <br/>{2}".Fill(edmp.Id, edmp.ClientCode, ex);
                    log.Error(msg);
                    MailUtility.SendUnexpectedExceptionEmail(msg);
                    continue;
                }
            }

            log.Debug("EDelivery CreateSigningRoom job queue is completed.");
        }

        protected override void SendInvitations()
        {
            log.Debug("EDelivery SendInvitations job queue is started.");

            List<EDMailPiece> signingRoomCreatedMailPieces = eDeliverySvc.GetBatchesByPipelineStatusAndSFTChannelId(EDPipelineStatus.SigningRoomCreated);

            if (signingRoomCreatedMailPieces.Count == 0)
            {
                log.Debug("EDelivery SendInvitations job queue is empty.");
                log.Debug("EDelivery SendInvitations job queue is completed.");
                return;
            }

            foreach (EDMailPiece edmp in signingRoomCreatedMailPieces)
            {
                try
                {
                    IEDeliveryAPIService eDeliveryAPISvc = new EDeliveryAPIService();
                    int reTrycnt = 0;
                    bool hasError = false;
                    while (reTrycnt < Config.Core_NoOfAttemptOnFailure)
                    {
                        bool isInvitationSent = eDeliveryAPISvc.SendInvitations(edmp.SigningRoomId.Value, edmp.ClientId.Value).Result;

                        if (isInvitationSent)
                        {
                            eDeliverySvc.UpdateEDMailPiecePipelineStatus(edmp.Id, EDPipelineStatus.InvitationSent, Common.Now);
                            hasError = false;
                            break;
                        }

                        var msg = "Error occured while sending invitation to participants of Id {0}";
                        log.Error(msg);
                        MailUtility.SendUnexpectedExceptionEmail(msg);
                        System.Threading.Thread.Sleep(Config.Core_FailureAttemptInterval);
                        hasError = true;
                        reTrycnt++;
                    }

                    if (hasError)
                    {
                        edmp.ErrorCode = OutComeCodes.Issue_In_EDelivery;
                        edmp.ErrorDesc = "Error occured while sending invitation to participant for Loan Number:{0}".Fill(edmp.LoanNumber);
                        eDeliverySvc.UpdateEDMailPieceDetails(edmp);

                        #region Check ErrorOut or IsFallBackRequested Package
                        eDeliverySvc.ErrorOutOrFallBackEDeliveryPackage(edmp);
                        #endregion
                    }
                }
                catch (Exception ex)
                {
                    var msg = "Error occured while sending invitation to participant Id {0} for client {1} with below error <br/>{2}".Fill(edmp.Id, edmp.ClientCode, ex);
                    log.Error(msg);
                    MailUtility.SendUnexpectedExceptionEmail(msg);
                    continue;
                }
            }

            log.Debug("EDelivery SendInvitations job queue is completed.");
        }

        protected override void EDelivered()
        {
            log.Debug("EDelivery EDelivered job queue is started.");

            List<EDMailPiece> eDeliveredMPs = eDeliverySvc.GetBatchesByPipelineStatusAndSFTChannelId(EDPipelineStatus.eDelivered);

            if (eDeliveredMPs.Count == 0)
            {
                log.Debug("EDelivery EDelivered job queue is empty.");
                log.Debug("EDelivery EDelivered job queue is completed.");
                return;
            }

            foreach (EDMailPiece edmp in eDeliveredMPs)
            {
                try
                {
                    using (IMailPieceService mpSvc = new MailPieceService())
                    using (IReportService reportSvc = new ReportService())
                    {
                        using (TransactionScope scope = new TransactionScope())
                        {
                            try
                            {
                                MailPieces mailPiece = reportSvc.GetMailPiecesDataFromMailBatchIdAndUniqueId(edmp.MailBatchId, edmp.UniqueId);
                                mailPiece.PrintedOn = edmp.DocDownloadedOn.Value;
                                mailPiece.ShippedOn = edmp.DocDownloadedOn.Value;
                                mailPiece.TotalPages = edmp.TotalPages;
                                mailPiece.TotalSheets = edmp.TotalPages;
                                mailPiece.ShippingStatus = ShippingStatus.eDelivered.ToString();

                                mpSvc.UpdateBatchMailPieceShippingStatusDetails(mailPiece);
                                MailPieceInfo mpi = new MailPieceInfo(edmp.ClientCode, Config.Common_AuthoMailRootFolder, edmp.MailBatchId, edmp.BatchName);
                                GenerateDeliveryReport(edmp, mailPiece, mpi);

                                DateTime reportPrepTiming = Common.Now;

                                eDeliverySvc.UpdateEDeliveredMPForReportingBack(edmp.MailBatchId, reportPrepTiming, BatchStatus.eDelivered.ToString());
                                eDeliverySvc.UpdateEDMailPiecePipelineStatus(edmp.Id, EDPipelineStatus.ReportedBack, reportPrepTiming);
                                scope.Complete();
                            }
                            catch (Exception ex)
                            {
                                var msg = "Error occured while generating EDelivered report for EDMailPiece-Id {0} and BatchName='{1}.xml'".Fill(edmp.Id, edmp.BatchName);
                                log.Error(msg, ex);
                                MailUtility.SendUnexpectedExceptionEmail(msg + "<br/> Error Message is {0}".Fill(ex));
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    var msg = "Error occured while preparing EDelivered report for Id {0} for client {1} with below error <br/>{2}".Fill(edmp.Id, edmp.ClientCode, ex);
                    log.Error(msg);
                    MailUtility.SendUnexpectedExceptionEmail(msg);
                    continue;
                }
            }

            log.Debug("EDelivery EDelivered job queue is completed.");
        }

        protected override void ESigned()
        {
            log.Debug("ESigned job queue is started.");

            List<EDMailPiece> eSignedMPs = eDeliverySvc.GetBatchesByPipelineStatusAndSFTChannelId(EDPipelineStatus.eSigned);

            if (eSignedMPs.Count == 0)
            {
                log.Debug("ESigned job queue is empty.");
                log.Debug("ESigned job queue is completed.");
                return;
            }

            foreach (EDMailPiece edmp in eSignedMPs)
            {
                try
                {
                    using (IMailPieceService mpSvc = new MailPieceService())
                    using (IReportService reportSvc = new ReportService())
                    {
                        using (TransactionScope scope = new TransactionScope())
                        {
                            try
                            {
                                MailPieces mailPiece = reportSvc.GetMailPiecesDataFromMailBatchIdAndUniqueId(edmp.MailBatchId, edmp.UniqueId);
                                mailPiece.PrintedOn = edmp.ESignedOn.Value;
                                mailPiece.ShippedOn = edmp.ESignedOn.Value;
                                mailPiece.TotalPages = edmp.TotalPages;
                                mailPiece.TotalSheets = edmp.TotalPages;
                                mailPiece.ShippingStatus = ShippingStatus.eSigned.ToString();

                                mpSvc.UpdateBatchMailPieceShippingStatusDetails(mailPiece);
                                MailPieceInfo mpi = new MailPieceInfo(edmp.ClientCode, Config.Common_AuthoMailRootFolder, edmp.MailBatchId, edmp.BatchName);
                                GenerateDeliveryReport(edmp, mailPiece, mpi);

                                DateTime reportPrepTiming = Common.Now;

                                eDeliverySvc.UpdateEDeliveredMPForReportingBack(edmp.MailBatchId, reportPrepTiming, BatchStatus.eSigned.ToString());
                                eDeliverySvc.UpdateEDMailPiecePipelineStatus(edmp.Id, EDPipelineStatus.ReportedBack, reportPrepTiming);
                                scope.Complete();
                            }
                            catch (Exception ex)
                            {
                                var msg = "Error occured while generating ESigned report for EDMailPiece-Id {0} and BatchName='{1}.xml'".Fill(edmp.Id, edmp.BatchName);
                                log.Error(msg, ex);
                                MailUtility.SendUnexpectedExceptionEmail(msg + "<br/> Error Message is {0}".Fill(ex));
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    var msg = "Error occured while preparing ESigned report for Id {0} for client {1} with below error <br/>{2}".Fill(edmp.Id, edmp.ClientCode, ex);
                    log.Error(msg);
                    MailUtility.SendUnexpectedExceptionEmail(msg);
                    continue;
                }
            }

            log.Debug("ESigned job queue is completed.");
        }

        private void GenerateDeliveryReport(EDMailPiece eDeliveryMP, MailPieces mp, MailPieceInfo mpi)
        {
            string amResponsePath = mpi.AMResponsePath;

            switch (eDeliveryMP.GetChannelCode().ToUpper())
            {
                case "AMC":
                    using (IAMCService amcSvc = new AMCService())
                    {
                        StringBuilder deliveryReport = amcSvc.AMCDeliveryReport(new List<MailPieces>() { mp });

                        if (deliveryReport != null)
                        {
                            string deliveryReportFileName = "Delivery-{0}.txt".Fill(Common.Now.ToString("yyyyMMdd-HHmmss"));

                            string deliveryReportPath = Path.Combine(amResponsePath, deliveryReportFileName);

                            File.WriteAllText(deliveryReportPath, deliveryReport.ToString());
                        }
                    }
                    Thread.Sleep(1000);
                    break;
                case "API_ELLIEMAE":
                    using (IGeneralService generalSvc = new GeneralService())
                    {
                        var mailBatch = mp.MailBatches;
                        var client = mp.Clients;
                        mailBatch.Clients = client;
                        string responseReport = generalSvc.GeneralJsonReport(mp.UniqueId.ToString());

                        if (!string.IsNullOrWhiteSpace(responseReport))
                        {
                            string deliveryReportFileName = "{0}-{1}-{2}.json".Fill(mailBatch.Clients.AppCode, mp.MailBatchId, commonSvc.GetReportingSequence());
                            string deliveryReportPath = Path.Combine(amResponsePath, deliveryReportFileName);

                            File.WriteAllText(deliveryReportPath, responseReport);
                        }
                    }
                    Thread.Sleep(1000);
                    break;
                default:
                    using (IGeneralService generalSvc = new GeneralService())
                    {
                        var mailBatch = mp.MailBatches;
                        var client = mp.Clients;
                        mailBatch.Clients = client;
                        string responseReport = generalSvc.GeneralXMLReport(new List<MailPieces>() { mp }, mp.MailBatches);

                        if (!string.IsNullOrWhiteSpace(responseReport))
                        {
                            string deliveryReportFileName = "{0}-{1}-{2}.xml".Fill(mailBatch.Clients.AppCode, mp.MailBatchId, commonSvc.GetReportingSequence());
                            string deliveryReportPath = Path.Combine(amResponsePath, deliveryReportFileName);

                            File.WriteAllText(deliveryReportPath, responseReport);
                        }
                    }
                    break;
            }
        }

        protected override void Expired()
        {
            log.Debug("EDelivery Expired job queue is started.");

            List<EDMailPiece> eDeliveredMPs = eDeliverySvc.GetBatchesByPipelineStatusAndSFTChannelId(EDPipelineStatus.Expired);

            if (eDeliveredMPs.Count == 0)
            {
                log.Debug("EDelivery Expired job queue is empty.");
                log.Debug("EDelivery Expired job queue is completed.");
                return;
            }

            foreach (EDMailPiece edmp in eDeliveredMPs)
            {
                try
                {
                    using (TransactionScope scope = new TransactionScope())
                    {
                        try
                        {
                            edmp.ErrorCode = OutComeCodes.SigningRoomExpired;
                            edmp.ErrorDesc = "Signing room is expired for E-Delivery.";
                            #region Check ErrorOut or IsFallBackRequested Package
                            eDeliverySvc.ErrorOutOrFallBackEDeliveryPackage(edmp);
                            #endregion

                            scope.Complete();
                        }
                        catch (Exception ex)
                        {
                            var msg = "Error occured while processing expired packages EDMailPiece-Id {0} and BatchName='{1}.xml'".Fill(edmp.Id, edmp.BatchName);
                            log.Error(msg, ex);
                            MailUtility.SendUnexpectedExceptionEmail(msg + "<br/> Error Message is {0}".Fill(ex));
                        }
                    }
                }
                catch (Exception ex)
                {
                    var msg = "Error occured while processing expired packages for Id {0} for client {1} with below error <br/>{2}".Fill(edmp.Id, edmp.ClientCode, ex);
                    log.Error(msg);
                    MailUtility.SendUnexpectedExceptionEmail(msg);
                    continue;
                }
            }

            log.Debug("EDelivery Expired job queue is completed.");
        }

        protected override void Declined()
        {
            log.Debug("EDelivery Declined job queue is started.");

            List<EDMailPiece> declinedMPs = eDeliverySvc.GetBatchesByPipelineStatusAndSFTChannelId(EDPipelineStatus.Declined);

            if (declinedMPs.Count == 0)
            {
                log.Debug("EDelivery Declined job queue is empty.");
                log.Debug("EDelivery Declined job queue is completed.");
                return;
            }

            foreach (EDMailPiece edmp in declinedMPs)
            {
                try
                {
                    using (TransactionScope scope = new TransactionScope())
                    {
                        try
                        {
                            edmp.ErrorCode = OutComeCodes.ConsentDeclined;
                            edmp.ErrorDesc = "Consent is declined by participant.";

                            eDeliverySvc.UpdateEDMailPieceDetails(edmp);

                            #region Check ErrorOut or IsFallBackRequested Package
                            eDeliverySvc.ErrorOutOrFallBackEDeliveryPackage(edmp);
                            #endregion

                            scope.Complete();
                        }
                        catch (Exception ex)
                        {
                            var msg = "Error occured while processing Declined packages EDMailPiece-Id {0} and BatchName='{1}.xml'".Fill(edmp.Id, edmp.BatchName);
                            log.Error(msg, ex);
                            MailUtility.SendUnexpectedExceptionEmail(msg + "<br/> Error Message is {0}".Fill(ex));
                        }
                    }
                }
                catch (Exception ex)
                {
                    var msg = "Error occured while processing Declined packages for Id {0} for client {1} with below error <br/>{2}".Fill(edmp.Id, edmp.ClientCode, ex);
                    log.Error(msg);
                    MailUtility.SendUnexpectedExceptionEmail(msg);
                    continue;
                }
            }

            log.Debug("EDelivery Declined job queue is completed.");
        }
    }
}
