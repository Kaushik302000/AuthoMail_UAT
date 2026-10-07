using AM.Entities;
using AM.Reporting.TrackingShipmentService;
using AM.Services;
using log4net;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Services.Protocols;

namespace AM.Reporting
{
    class CheckFedExTrackingInfo
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public static string WebAuthenticationKey { get; private set; }
        public static string WebPassword { get; private set; }
        public static string WebAccountNumber { get; private set; }
        public static string WebMeterNumber { get; private set; }

        static CheckFedExTrackingInfo()
        {
            using (IFedExService fedExSvc = new FedExService())
            {
                FedExAccountInfo fedExAccountDetails = fedExSvc.GetFedExAccountDetailsByAccountCode(AppConfig.FedExAccountCode);
                if (fedExAccountDetails != null)
                {
                    WebAuthenticationKey = fedExAccountDetails.WebAuthenticationKey;
                    WebPassword = fedExAccountDetails.WebAuthenticationPassword;
                    WebAccountNumber = fedExAccountDetails.AccountNumber;
                    WebMeterNumber = fedExAccountDetails.MeterNumber;
                }
            }
        }

        public bool GetTrackingInformation(List<MailPieces> mailPieces)
        {
            int totalMailPieceCount = mailPieces.Count;
            int checkedMailPieceCount = 0;
            double loopCount = Math.Ceiling(mailPieces.Count / 30.0f);

            for (double i = 0; i < loopCount; i++)
            {
                var takenMailPieces = mailPieces.Skip(checkedMailPieceCount).Take(30).ToList();
                checkedMailPieceCount += 30;


                TrackRequest request = CreateTrackRequest(takenMailPieces);
                TrackPortTypeClient service = new TrackPortTypeClient();
                try
                {
                    // Call the Track web service passing in a TrackRequest and returning a TrackReply
                    TrackReply reply = service.track(request);
                    if (reply.HighestSeverity == NotificationSeverityType.SUCCESS || reply.HighestSeverity == NotificationSeverityType.NOTE || reply.HighestSeverity == NotificationSeverityType.WARNING)
                    {
                        GetTrackReplyAndUpdateValue(reply);
                    }
                }
                catch (SoapException ex)
                {
                    log.Error("SOAP Error occured while requesting FedEx Tracking Details : ", ex);
                }
                catch (Exception ex)
                {
                    log.Error("Error occured while requesting FedEx Tracking Details : ", ex);
                }
            }

            return false;
        }

        private void GetTrackReplyAndUpdateValue(TrackReply reply)
        {
            List<FedExPickedPackagesDetail> fedExPickedPackageDetails = new List<FedExPickedPackagesDetail>();
            List<FedExNotPickedPackagesDetail> fedExNotPickedPackageDetails = new List<FedExNotPickedPackagesDetail>();
            // Track details for each package
            foreach (CompletedTrackDetail completedTrackDetail in reply.CompletedTrackDetails)
            {
                foreach (TrackDetail trackDetail in completedTrackDetail.TrackDetails)
                {
                    if (trackDetail.Events != null)
                    {
                        var trackEvent = trackDetail.Events.Where(c => c.EventType == "PU").FirstOrDefault();
                        if (trackEvent != null)
                        {
                            FedExPickedPackagesDetail pickupDetail = new FedExPickedPackagesDetail();
                            pickupDetail.TrackingNo = trackDetail.TrackingNumber;
                            pickupDetail.PickedTimeStamp = trackEvent.Timestamp.ToUniversalTime();
                            fedExPickedPackageDetails.Add(pickupDetail);
                        }
                    }
                }
            }

            try
            {
                using (IMailPieceService mailPieceSvc = new MailPieceService())
                {
                    foreach (var pickedUpDetail in fedExPickedPackageDetails)
                    {
                        int retVal = mailPieceSvc.UpdateMailPiecePickedUpDetailsFromTrackingNo(pickedUpDetail);
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("Error occured while updating picked up details", ex);
            }
        }

        private static TrackRequest CreateTrackRequest(List<MailPieces> takenMailPieces)
        {
            TrackRequest request = new TrackRequest();
            //Credentials
            request.WebAuthenticationDetail = new WebAuthenticationDetail();
            request.WebAuthenticationDetail.UserCredential = new WebAuthenticationCredential();
            request.WebAuthenticationDetail.UserCredential.Key = WebAuthenticationKey;
            request.WebAuthenticationDetail.UserCredential.Password = WebPassword;

            request.ClientDetail = new ClientDetail();
            request.ClientDetail.AccountNumber = WebAccountNumber;
            request.ClientDetail.MeterNumber = WebMeterNumber;

            request.TransactionDetail = new TransactionDetail();
            request.TransactionDetail.CustomerTransactionId = AppConfig.FedExAccountCode;

            request.Version = new VersionId();
            List<TrackSelectionDetail> trackSelections = new List<TrackSelectionDetail>();
            // Tracking information
            foreach (var mailPiece in takenMailPieces)
            {
                TrackSelectionDetail trackSelection = new TrackSelectionDetail();
                trackSelection.PackageIdentifier = new TrackPackageIdentifier();
                trackSelection.PackageIdentifier.Value = mailPiece.TrackingNo;
                trackSelection.PackageIdentifier.Type = TrackIdentifierType.TRACKING_NUMBER_OR_DOORTAG;
                trackSelections.Add(trackSelection);
            }
            request.SelectionDetails = trackSelections.ToArray();

            request.ProcessingOptions = new TrackRequestProcessingOptionType[1];
            request.ProcessingOptions[0] = TrackRequestProcessingOptionType.INCLUDE_DETAILED_SCANS;
            return request;
        }
    }
}
