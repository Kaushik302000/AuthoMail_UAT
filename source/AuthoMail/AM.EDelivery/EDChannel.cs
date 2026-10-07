using AM.Entities;
using AM.Lib;
using AM.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using System.Xml;
using System.Xml.Schema;

namespace AM.EDelivery
{
    abstract class EDChannel : Disposable
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        protected IClientService clientSvc;
        protected IEDeliveryService eDeliverySvc;
        protected ICommonService commonSvc;
        protected IDocTypeService docTypeSvc;

        protected List<ServiceBase> services;

        protected EDChannel()
        {
            services = new List<ServiceBase>();

            // Add services here
            clientSvc = GetService<ClientService>();
            commonSvc = GetService<CommonService>();
            eDeliverySvc = GetService<EDeliveryService>();
            docTypeSvc = GetService<DocTypeService>();
        }

        protected T GetService<T>() where T : ServiceBase, new()
        {
            var service = new T();
            services.Add(service);
            return service;
        }

        public void ProcessBatches()
        {
            try
            {
                this.CreateSigningRoom();
                this.SendInvitations();
                this.EDelivered();
                this.ESigned();
                this.Expired();
                this.Declined();
            }
            catch (Exception ex)
            {
                var msg = "Unexpected error occured in EDChannel : {0}".Fill(ex);

                // Log exception
                log.Error(msg);

                // Send email to support team
                MailUtility.SendUnexpectedExceptionEmail(msg);
            }
        }

        /// <summary>
        /// Create signing room for all mailpieces which are validated successfully
        /// </summary>
        protected abstract void CreateSigningRoom();
        /// <summary>
        /// Send invitaions to all recipients for which signing rooms are created successfully
        /// </summary>
        protected abstract void SendInvitations();
        /// <summary>
        /// Take all eDelivered records and generate reports
        /// </summary>
        protected abstract void EDelivered();
        /// <summary>
        /// Take all eSigned records and generate reports
        /// </summary>
        protected abstract void ESigned();
        /// <summary>
        /// Take all Expired records and generate reports
        /// </summary>
        protected abstract void Expired();
        /// <summary>
        /// Take all Consent Declined records and generate reports
        /// </summary>
        protected abstract void Declined();

        #region Check Activity Logs
        public static void CheckActivityLogs()
        {
            log.Debug("EDelivery Check ActivityLogs job queue is started.");

            using (IEDeliveryService eDeliverySvc = new EDeliveryService())
            {
                List<EDMailPiece> invitationSentMailPieces = eDeliverySvc.GetBatchesByPipelineStatusAndSFTChannelId(EDPipelineStatus.InvitationSent);

                if (invitationSentMailPieces.Count == 0)
                {
                    log.Debug("EDelivery Check ActivityLogs job queue is empty.");
                    log.Debug("EDelivery Check ActivityLogs job queue is completed.");
                    return;
                }

                foreach (EDMailPiece mp in invitationSentMailPieces)
                {
                    try
                    {
                        IEDeliveryAPIService eDeliveryAPISvc = new EDeliveryAPIService();

                        var activityLogs = eDeliveryAPISvc.GetSigningRoomActivityLog(mp.SigningRoomName, mp.ClientId.Value).Result;

                        foreach (var actlog in activityLogs)
                        {
                            switch (actlog.EDeliveryStatus)
                            {
                                case ActivityStatus.ESignConsentAccepted:
                                    mp.ConsentAcceptedOn = actlog.UpdatedDate.PstToUtc();
                                    break;
                                case ActivityStatus.ESignConsentDeclined:
                                    mp.ConsentDeclinedOn = actlog.UpdatedDate.PstToUtc();
                                    break;
                                case ActivityStatus.DocumentReviewed:
                                    mp.DocReviewedOn = actlog.UpdatedDate.PstToUtc();
                                    break;
                                case ActivityStatus.DocumentSigned:
                                    mp.ESignedOn = actlog.UpdatedDate.PstToUtc();
                                    break;
                                case ActivityStatus.DocumentDownloaded:
                                    mp.DocDownloadedOn = actlog.UpdatedDate.PstToUtc();
                                    mp.DownloadedIPAddress = actlog.IPAddress;
                                    break;
                                default:
                                    break;
                            }
                        }
                        using (TransactionScope scope = new TransactionScope())
                        {

                            int i = eDeliverySvc.UpdateEDMailPieceActivityStatus(mp.Id, mp.IsESignRequested, mp.ConsentAcceptedOn, mp.ConsentDeclinedOn, mp.DocReviewedOn, mp.ESignedOn, mp.DocDownloadedOn, mp.ExpiryDate, mp.DownloadedIPAddress);
                            scope.Complete();
                        }
                    }
                    catch (Exception ex)
                    {
                        var msg = "Error occured while checking for activity logs of Id {0} for client {1} with below error <br/>{2}".Fill(mp.Id, mp.ClientCode, ex);
                        log.Error(msg);
                        MailUtility.SendUnexpectedExceptionEmail(msg);
                        continue;
                    }
                }
            }

            log.Debug("EDelivery Check ActivityLogs job queue is started.");
        }
        #endregion


        #region Download E-Signed Docs
        public static void DownloadEsignDocs()
        {
            log.Debug("EDelivery DownloadEsignDocs job queue is started.");

            using (IEDeliveryService eDeliverySvc = new EDeliveryService())
            {
                List<EDMailPiece> eSignedDocList = eDeliverySvc.GetESignedDocList();

                if (eSignedDocList.Count == 0)
                {
                    log.Debug("EDelivery DownloadEsignDocs job queue is empty.");
                    log.Debug("EDelivery DownloadEsignDocs job queue is completed.");
                    return;
                }

                foreach (EDMailPiece mp in eSignedDocList)
                {
                    try
                    {
                        IEDeliveryAPIService eDeliveryAPISvc = new EDeliveryAPIService();

                        //Download Doc Logic
                        var downloadedDocDetails = eDeliveryAPISvc.DownloadEsignedDocument(mp.SRDocId.Value, mp.ClientId.Value).Result;
                        if (downloadedDocDetails.HasError)
                        {
                            var msg = $"Error occured while downloading EsignDocs of Id {mp.Id} for client {mp.ClientCode} with below error <br/>{downloadedDocDetails.ErrorDesc}";
                            log.Error(msg);
                            continue;
                        }

                        //Save EsignedPdf
                        string evaultPath = Path.Combine(Config.Common_AuthoMailRootFolder, mp.ClientCode, mp.MailBatchId.ToString(), "EVault");

                        if (!Directory.Exists(evaultPath))
                            Directory.CreateDirectory(evaultPath);

                        File.WriteAllBytes(Path.Combine(evaultPath, downloadedDocDetails.DocumentName), Convert.FromBase64String(downloadedDocDetails.Document));

                        mp.IsESignedDocDownloaded = true;
                        eDeliverySvc.UpdateEDMailPieceDetails(mp);
                    }
                    catch (Exception ex)
                    {
                        var msg = "Error occured while downloading EsignDocs of Id {0} for client {1} with below error <br/>{2}".Fill(mp.Id, mp.ClientCode, ex);
                        log.Error(msg);
                        MailUtility.SendUnexpectedExceptionEmail(msg);
                        continue;
                    }
                }
            }

            log.Debug("EDelivery DownloadEsignDocs job queue is completed.");
        }
        #endregion

        protected override void DisposeManagedObjects()
        {
            services.ForEach(svc => svc.Dispose());
        }

        protected override void DisposeUnManagedObjects()
        {
            return;
        }
    }
}
