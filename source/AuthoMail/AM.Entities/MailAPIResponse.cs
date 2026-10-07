using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AM.Entities
{
    public class BatchResponse : MailPieceDTO
    {
        public string AppCode { get; set; }
    }

    public class BatchAPIResponse
    {
        public int MBId { get; set; }
        public MailAPIResponse APIResponse { get; set; }
    }

    public class MailAPIResponse
    {
        public Guid Id { get; set; }
        //public Guid UniqueId { get; set; }
        public string Message { get; set; }
        public DateTime ReceivedOn { get; set; }
        public string ClientCode { get; set; }
        public string BusinessFriendlyIdName { get; set; }
        public string BusinessFriendlyIdValue { get; set; }
        public string DocumentOrPackageType { get; set; }
        public DocumentOrPackage DocumentOrPackage { get; set; }
        public RecipientInfo RecipientInfo { get; set; }
        public List<APIMetaData> CustomInfo { get; set; }
        public string MailingStatus { get; set; }
        public string ErrorCode { get; set; }
        public string ErrorDesc { get; set; }
        public DateTime? ShippedOn { get; set; }
        public string ShippingMethod { get; set; }
        public string ShippingServiceType { get; set; }
        public string TrackingNumber { get; set; }
        public int TotalPages { get; set; }
        public int TotalSheets { get; set; }
        public DateTime? PulledBackOn { get; set; }

        //public bool ShouldSerializeId()
        //{
        //    return this.MailingStatus != ShippingStatus.Acknowledgement.ToString() && this.Id != Guid.Empty;
        //}
        //public bool ShouldSerializeUniqueId()
        //{
        //    return this.MailingStatus == ShippingStatus.Acknowledgement.ToString();
        //}
        public bool ShouldSerializeMessage()
        {
            return this.MailingStatus == ShippingStatus.Acknowledgement.ToString() || this.MailingStatus == ShippingStatus.NotFound.ToString();
        }
        public bool ShouldSerializeClientCode()
        {
            if (this.MailingStatus == ShippingStatus.NotFound.ToString() || this.MailingStatus == ShippingStatus.InProgress.ToString()) return false;
            return this.MailingStatus != ShippingStatus.Acknowledgement.ToString();
        }
        public bool ShouldSerializeReceivedOn()
        {
            if (this.MailingStatus == ShippingStatus.NotFound.ToString() || this.MailingStatus == ShippingStatus.InProgress.ToString()) return false;
            return this.MailingStatus != ShippingStatus.Acknowledgement.ToString();
        }
        public bool ShouldSerializeBusinessFriendlyIdName()
        {
            if (this.MailingStatus == ShippingStatus.NotFound.ToString() || this.MailingStatus == ShippingStatus.InProgress.ToString()) return false;
            return this.MailingStatus != ShippingStatus.Acknowledgement.ToString();
        }
        public bool ShouldSerializeBusinessFriendlyIdValue()
        {
            if (this.MailingStatus == ShippingStatus.NotFound.ToString() || this.MailingStatus == ShippingStatus.InProgress.ToString()) return false;
            return this.MailingStatus != ShippingStatus.Acknowledgement.ToString();
        }
        public bool ShouldSerializeDocumentOrPackageType()
        {
            if (this.MailingStatus == ShippingStatus.NotFound.ToString() || this.MailingStatus == ShippingStatus.InProgress.ToString()) return false;
            return this.MailingStatus != ShippingStatus.Acknowledgement.ToString();
        }
        public bool ShouldSerializeDocumentOrPackage()
        {
            if (this.MailingStatus == ShippingStatus.NotFound.ToString() || this.MailingStatus == ShippingStatus.InProgress.ToString()) return false;
            return this.MailingStatus != ShippingStatus.Acknowledgement.ToString();
        }
        public bool ShouldSerializeRecipientInfo()
        {
            if (this.MailingStatus == ShippingStatus.NotFound.ToString() || this.MailingStatus == ShippingStatus.InProgress.ToString()) return false;
            return this.MailingStatus != ShippingStatus.Acknowledgement.ToString();
        }

        public bool ShouldSerializeCustomInfo()
        {
            if (this.MailingStatus == ShippingStatus.NotFound.ToString() || this.MailingStatus == ShippingStatus.InProgress.ToString()) return false;
            else if (this.CustomInfo == null || this.CustomInfo.Count == 0)
                return false;
            return this.MailingStatus != ShippingStatus.Acknowledgement.ToString();
        }
        //public bool ShouldSerializeMailingStatus()
        //{
        //    return this.MailingStatus != null;
        //}
        public bool ShouldSerializeErrorCode()
        {
            if (this.MailingStatus == ShippingStatus.Acknowledgement.ToString() || this.MailingStatus == ShippingStatus.InProgress.ToString()) return false;
            return this.MailingStatus == ShippingStatus.Error.ToString() || this.MailingStatus == ShippingStatus.Unknown.ToString();
        }
        public bool ShouldSerializeErrorDesc()
        {
            if (this.MailingStatus == ShippingStatus.Acknowledgement.ToString() || this.MailingStatus == ShippingStatus.InProgress.ToString()) return false;
            return this.MailingStatus == ShippingStatus.Error.ToString() || this.MailingStatus == ShippingStatus.Unknown.ToString();
        }

        public bool ShouldSerializeShippedOn()
        {
            if (this.MailingStatus == ShippingStatus.Acknowledgement.ToString() || this.MailingStatus == ShippingStatus.InProgress.ToString()) return false;
            return this.MailingStatus == ShippingStatus.Shipped.ToString() || this.MailingStatus == ShippingStatus.eDelivered.ToString();
        }
        public bool ShouldSerializeShippingMethod()
        {
            if (this.MailingStatus == ShippingStatus.Acknowledgement.ToString() || this.MailingStatus == ShippingStatus.InProgress.ToString()) return false;
            return this.MailingStatus == ShippingStatus.Shipped.ToString() || this.MailingStatus == ShippingStatus.eDelivered.ToString();
        }
        public bool ShouldSerializeShippingServiceType()
        {
            if (this.MailingStatus == ShippingStatus.Acknowledgement.ToString() || this.MailingStatus == ShippingStatus.InProgress.ToString()) return false;
            return this.MailingStatus == ShippingStatus.Shipped.ToString() || this.MailingStatus == ShippingStatus.eDelivered.ToString();
        }
        public bool ShouldSerializeTrackingNumber()
        {
            if (this.MailingStatus == ShippingStatus.Acknowledgement.ToString() || this.MailingStatus == ShippingStatus.InProgress.ToString()) return false;
            return this.MailingStatus == ShippingStatus.Shipped.ToString() || this.MailingStatus == ShippingStatus.eDelivered.ToString();
        }
        public bool ShouldSerializeTotalPages()
        {
            if (this.MailingStatus == ShippingStatus.Acknowledgement.ToString() || this.MailingStatus == ShippingStatus.InProgress.ToString()) return false;
            return this.MailingStatus == ShippingStatus.Shipped.ToString() || this.MailingStatus == ShippingStatus.eDelivered.ToString();
        }
        public bool ShouldSerializeTotalSheets()
        {
            if (this.MailingStatus == ShippingStatus.Acknowledgement.ToString() || this.MailingStatus == ShippingStatus.InProgress.ToString()) return false;
            return this.MailingStatus == ShippingStatus.Shipped.ToString() || this.MailingStatus == ShippingStatus.eDelivered.ToString();
        }
        public bool ShouldSerializePulledBackOn()
        {
            if (this.MailingStatus == ShippingStatus.Acknowledgement.ToString() || this.MailingStatus == ShippingStatus.InProgress.ToString()) return false;
            return this.MailingStatus == ShippingStatus.PulledBack.ToString();
        }
    }

    public class APIMetaData
    {
        public string Key { get; set; }
        public string Value { get; set; }
    }
}
