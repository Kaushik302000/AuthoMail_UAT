using AM.Lib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AM.Entities
{
    public class FedExAccountInfo
    {
        public string AccountCode { get; set; }
        public string WebAuthenticationKey { get; set; }
        public string WebAuthenticationPassword { get; set; }
        public string AccountNumber { get; set; }
        public string MeterNumber { get; set; }
    }

    public class Holiday
    {
        public int Id { get; set; }
        public DateTime HolidayDate { get; set; }
        public string Description { get; set; }
        public int GroupId { get; set; }
    }

    public class ClientHolidayGroup
    {
        public int ClientId { get; set; }
        public int HolidayGroupId { get; set; }
    }

    public class PackagesHoldInfo
    {
        public int Id { get; set; }
        public int ClientId { get; set; }
        public string ClientCode { get; set; }
        public string DocumentOrPackageType { get; set; }
        public DateTime HoldOn { get; set; }
        public DateTime ProcessOn { get; set; }
    }

    public class FedExPickedPackagesDetail
    {
        public DateTime PickedTimeStamp { get; set; }
        public string TrackingNo { get; set; }
        public string ShipmentStatus { get; set; }
        public string DeliveryStatus { get; set; }
        public DateTime? DeliveryDate { get; set; }
    }

    public class FedExNotPickedPackagesDetail
    {
        public string TrackingNo { get; set; }
        public MailPieceDTO mailPiece { get; set; }
    }

    public class EDeliveryStatusInfoModel
    {
        public string StepName { get; set; }
        public string StepTimeStamp { get; set; }
        public DateTime? StepProcessingTimeStamp
        {
            get
            {
                if (StepName.Contains("On") && !StepTimeStamp.IsNullOrWhiteSpace())
                {
                    return DateTime.Parse(StepTimeStamp);
                }
                else
                    return null;
            }
        }
    }
}
