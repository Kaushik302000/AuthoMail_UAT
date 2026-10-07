using AM.Lib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AM.Web.Models
{
    public class MailPieceDetailsViewModel
    {
        public int MailPieceId { get; set; }
        public int MailBatchId { get; set; }
        public string BatchName { get; set; }
        public Guid UniqueId { get; set; }
        public string AppCode { get; set; }
        public string BusinessFriendlyIdName { get; set; }
        public string BusinessFriendlyIdValue { get; set; }
        public string FileName { get; set; }
        public string DocumentOrPackageType { get; set; }
        public int? SLADays { get; set; }
        public DateTime? DueDate { get; set; }
        public string ShippingStatus { get; set; }
        public DateTime? ReceivedOn { get; set; }
        public string TrackingNo { get; set; }
        public int IsPullBackRequested { get; set; }
        public DateTime? PullBackRequestedOn { get; set; }
        public int IsPulledBack { get; set; }
        public DateTime? PulledBackOn { get; set; }
        public string PulledBackRequestedBy { get; set; }
        public int IsPrinted { get; set; }
        public DateTime? PrintedOn { get; set; }
        public int IsShipped { get; set; }
        public DateTime? ShippedOn { get; set; }
        public DateTime? PickedUpOn { get; set; }
        public decimal ShippingCost { get; set; }
        public decimal? Weight { get; set; }
        public bool IsQCRequested { get; set; }
        public bool IsQCPerformed { get; set; }
        public string SFTChannels { get; set; }
        public string DocTypeCode { get; set; }
        public string DisplayDocTypeCode
        {
            get
            {
                if (this.DocTypeCode.IsNullOrEmpty()) return "-";
                return this.DocTypeCode.Split('|').First();
            }
        }
        public bool IsEDeliveryRequested { get; set; }
        public bool IsESignRequested { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public int TotalPages { get; set; }
        public int TotalSheets { get; set; }
        public int TotalBlankPages { get; set; }
        public string BlankPageNumbers { get; set; }
        public string ErrorCode { get; set; }
        public string ErrorDesc { get; set; }
        public bool IsRcptInfoSpecified { get; set; }
        public string RcptName { get; set; }
        public string RcptAddress { get; set; }
        public string RcptPhone { get; set; }
        public string RcptOrganization { get; set; }
        public string RcptAddressCity { get; set; }
        public string RcptAddressState { get; set; }
        public string RcptAddressZIP { get; set; }
        public string RcptAddressCountry { get; set; }
        public string CoRcptName { get; set; }
        public string CoRcptPhone { get; set; }
        public string CoRcptOrganization { get; set; }
        public string RetRcptName { get; set; }
        public string RetRcptAddress { get; set; }
        public string RetRcptAddressCity { get; set; }
        public string RetRcptAddressState { get; set; }
        public string RetRcptAddressZIP { get; set; }
        public string RcptEmailId { get; set; }
        public string PrintLocation { get; set; }
        public string PrintBatch { get; set; }
        public string PrintFileName { get; set; }
        public string Inserts { get; set; }
        public string ShipmentStatus { get; set; }
        public DateTime? ShipmentStatusUpdatedOn { get; set; }
    }
}