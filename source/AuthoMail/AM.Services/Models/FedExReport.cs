using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AM.Services
{
    public class TrackingReport
    {
        public int Id { get; set; }
        public Guid UniqueId { get; set; }
        public string AppCode { get; set; }
        public int ClientId { get; set; }
        public string PipelineStatus { get; set; }
        public string ShippingStatus { get; set; }
        public string DocTypeCode { get; set; }
        public string ShippingCarrier { get; set; }
        public string ShippingServiceType { get; set; }
        public string PrintLocation { get; set; }
        public string BatchName { get; set; }
        public string LoanNumber { get; set; }
        public string RcptFirstName { get; set; }
        public string RcptLastName { get; set; }
        public string RcptAddressStreet { get; set; }
        public string RcptAddressAptOrSuiteOrOther { get; set; }
        public string RcptAddressCity { get; set; }
        public string RcptAddressState { get; set; }
        public string RcptAddressZIP { get; set; }
        public string TrackingNo { get; set; }
        public DateTime? ReceivedOn { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? ShippedOn { get; set; }
        public string DocumentOrPackageType { get; set; }
        public int IsPickedUp { get; set; }
        public DateTime? PickedUpOn { get; set; }
        public string PickUpStatus { get; set; }
        public DateTime? ShipmentStatusUpdatedOn { get; set; }
        public string ShipmentStatus { get; set; }
        public string DeliveryStatus { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public string ReturnTrackingNo { get; set; }
        public string ReturnDeliveryStatus { get; set; }
        public DateTime? ReturnDeliveryDate { get; set; }
    }

    public class TrackingDump
    {
        public string TrackingNo { get; set; }

        public bool IsReturnTracking { get; set; }
        public string ShippingCarrier { get; set; }
        public string ShipmentStatus { get; set; }
        public DateTime? ShipmentStatusUpdatedOn { get; set; }
        public string DeliveryStatus { get; set; }
        public DateTime? DeliveryDate { get; set; }
    }
}
