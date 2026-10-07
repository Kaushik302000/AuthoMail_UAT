using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AM.Entities;
using AM.Lib;
using AM.Services;
using System.Configuration;
using System.Net.Mail;
using System.IO;
using AM.Services.USPS.TrackV2;
using AM.USPSLib.TrackV2;

namespace AM.Reporting
{
    class CarrierTrackingInfo
    {
        private static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        internal static void CheckForShipmentTracking(string alias = null, List<string> clientCodes = null, DateTime? startDateTime = null, DateTime? endDateTime = null)
        {
            try
            {
                log.Info("FedEx Pickup Process Started");
                List<TrackingReport> mailPieces = new List<TrackingReport>();
                using (IFedExService fedExTrackingSvc = new FedExService())
                using (IReportService reportSvc = new ReportService())
                {
                    DateTime dueDateStartTime = startDateTime.HasValue ? startDateTime.Value.ToStartDateTime() : Common.Now.UtcToEst().AddDays(-10).ToStartDateTime();

                    //Here we are passing dates in EST format because we are looking for est dates column
                    DateTime dueDateEndTime = endDateTime.HasValue ? endDateTime.Value.ToEndDateTime() : Common.Now.UtcToEst().ToEndDateTime();

                    log.Info("FedEx Tracking Process For Date And Time {0}".Fill(dueDateEndTime));
                    //UPDATE Pickupon list before generating reports.
                    fedExTrackingSvc.UpdateFedExShipmentInfo(dueDateStartTime, dueDateEndTime, alias, clientCodes);
                }
                log.Info("FedEx Pickup Process Completed");
            }
            catch (Exception ex)
            {
                log.Error("Error occured while checkign for FedEx packages picked up or not", ex);
            }
        }

        internal static void CheckForUSPSeVSShipmentTracking(string alias = null, List<string> clientCodes = null, DateTime? startDateTime = null, DateTime? endDateTime = null)
        {
            try
            {
                log.Info("USPS eVS Tracking Process Started");

                using (IUSPSService uspsSvc = new USPSService())
                using (ICommonService commonSvc = new CommonService())
                using (IReportService reportSvc = new ReportService())
                using (IGeneralService generalSvc = new GeneralService())
                using (IFedExService fedExSvc = new FedExService())
                {
                    DateTime dueDateStartTime = startDateTime.HasValue ? startDateTime.Value.ToStartDateTime() : Common.Now.UtcToEst().AddDays(-10).ToStartDateTime();

                    //Here we are passing dates in EST format because we are looking for est dates column
                    DateTime dueDateEndTime = endDateTime.HasValue ? endDateTime.Value.ToEndDateTime() : Common.Now.UtcToEst().ToEndDateTime();
                    log.Info("USPS Tracking Process For Date And Time {0}".Fill(dueDateEndTime));

                    List<TrackingReport> mailPieces = reportSvc.GetUSPSeVSDataForTracking(dueDateStartTime, dueDateEndTime, alias, clientCodes);

                    if (mailPieces == null || mailPieces.Count == 0)
                        return;

                    var pendingUSPSTrackingMps = mailPieces.Where(mp => mp.DeliveryStatus.IsNullOrWhiteSpace() || (!mp.DeliveryStatus.IsNullOrWhiteSpace() && !mp.DeliveryStatus.ToLower().Contains("delivered"))).ToList();

                    generalSvc.USPSTrackingCall(pendingUSPSTrackingMps, false);

                    var returnUSPSTrackingInfo = mailPieces.Where(mp => !string.IsNullOrWhiteSpace(mp.ReturnTrackingNo) && mp.ReturnTrackingNo.StartsWith("9")).ToList();

                    var pendingUSPSReturnTrackingMps = returnUSPSTrackingInfo.Where(mp => mp.ReturnDeliveryStatus.IsNullOrWhiteSpace() || (!mp.ReturnDeliveryStatus.IsNullOrWhiteSpace() && !mp.ReturnDeliveryStatus.ToLower().Contains("delivered"))).ToList();

                    generalSvc.USPSTrackingCall(pendingUSPSReturnTrackingMps, true);

                    var returnFedExTrackingInfo = mailPieces.Where(mp => !string.IsNullOrWhiteSpace(mp.ReturnTrackingNo) && (mp.ReturnTrackingNo.StartsWith("3") || mp.ReturnTrackingNo.StartsWith("7") || mp.ReturnTrackingNo.StartsWith("2"))).ToList();

                    var pendingFedExReturnTrackingMps = returnFedExTrackingInfo.Where(mp => mp.ReturnDeliveryStatus.IsNullOrWhiteSpace() || (!mp.ReturnDeliveryStatus.IsNullOrWhiteSpace() && !mp.ReturnDeliveryStatus.ToLower().Contains("delivered"))).ToList();

                    fedExSvc.FedExTrackingCall(pendingFedExReturnTrackingMps, true);


                }
                log.Info("USPS eVS Tracking Process Completed");
            }
            catch (Exception ex)
            {
                log.Error("Error occured while checking for USPS packages picked up or not", ex);
            }
        }
    }
}
