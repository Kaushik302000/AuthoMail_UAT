using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace AM.Entities
{
    [Serializable]
    [XmlType(Namespace = "http://schemas.corelogic.com/ade/authomail/v1.18.0")]
    [XmlRoot(Namespace = "http://schemas.corelogic.com/ade/authomail/v1.18.0", IsNullable = false)]
    public class MailBatchResponse
    {
        [XmlArrayItem(IsNullable = false)]
        public List<MailPieceReport> MailPiecesReport { get; private set; }

        [XmlAttribute]
        public Guid Id { get; set; }
        [XmlAttribute]
        public string ClientCode { get; set; }
        [XmlAttribute]
        public string ClientBatchId { get; set; }
        [XmlAttribute]
        public string BatchName { get; set; }
        [XmlAttribute]
        public string ErrorDescription { get; set; }

        public MailBatchResponse()
        {
            this.MailPiecesReport = new List<MailPieceReport>();
        }
    }

    [Serializable]
    [XmlType(Namespace = "http://schemas.corelogic.com/ade/authomail/v1.18.0")]
    public class MailPieceReport
    {
        [XmlAttribute]
        public Guid Id { get; set; }
        [XmlAttribute]
        public string BusinessFriendlyIdName { get; set; }
        [XmlAttribute]
        public string BusinessFriendlyIdValue { get; set; }
        [XmlAttribute]
        public string DocumentOrPackageType { get; set; }
        [XmlAttribute]
        public string FileName { get; set; }

        /// <summary>
        /// TotalSheets in the mail-piece file printed by Print Vendor. 
        /// It would be useful for audit in-case of pullback and/or billing.
        /// </summary>
        [XmlAttribute]
        public int TotalSheets { get; set; }

        public bool ShouldSerializeTotalSheets()
        {
            return (TotalSheets > 0 && MailingStatus == MailingStatus.Shipped);
        }

        /// <summary>
        /// This is used for printing and mailing status
        /// </summary>
        [XmlAttribute]
        public MailingStatus MailingStatus { get; set; }

        /// <summary>
        /// This is for getting tracking number of the package
        /// </summary>
        [XmlAttribute]
        public string TrackingNumber { get; set; }
        public bool ShouldSerializeTrackingNumber()
        {
            return (!string.IsNullOrEmpty(TrackingNumber) && MailingStatus == MailingStatus.Shipped);
        }

        /// <summary>
        /// This is for getting date and time of print and ship
        /// </summary>
        [XmlAttribute]
        public DateTime ShippedOn { get; set; }
        public bool ShouldSerializeShippedOn()
        {
            return (!string.IsNullOrEmpty(ShippedOn.ToString()) && ShippedOn != DateTime.MinValue && MailingStatus == MailingStatus.Shipped);
        }
        /// <summary>
        /// This is for getting date and time of print and ship
        /// </summary>
        [XmlAttribute]
        public DateTime PulledBackOn { get; set; }
        public bool ShouldSerializePulledBackOn()
        {
            return (!string.IsNullOrEmpty(PulledBackOn.ToString()) && PulledBackOn != DateTime.MinValue && MailingStatus == MailingStatus.PulledBack);
        }

        /// <summary>
        /// This is used for Postage cost
        /// </summary>
        [XmlAttribute]
        public Decimal Postage { get; set; }
        public bool ShouldSerializePostage()
        {
            return (!string.IsNullOrEmpty(Postage.ToString()) && Postage != 0m && MailingStatus == MailingStatus.Shipped);
        }

        /// <summary>
        /// This is used for weight measured by print vendor
        /// </summary>
        [XmlAttribute]
        public Decimal WeightMeasured { get; set; }
        public bool ShouldSerializeWeightMeasured()
        {
            return (!string.IsNullOrEmpty(WeightMeasured.ToString()) && WeightMeasured != 0m && MailingStatus == MailingStatus.Shipped);
        }

        /// <summary>
        /// This is used when printstatus is Error and print vendor provides error description
        /// </summary>
        [XmlAttribute]
        public string ErrorCode { get; set; }
        public bool ShouldSerializeErrorCode()
        {
            return MailingStatus == MailingStatus.Error;
        }

        /// <summary>
        /// This is used when printstatus is Error and print vendor provides error description
        /// </summary>
        [XmlAttribute]
        public string ErrorDesc { get; set; }
        public bool ShouldSerializeErrorDesc()
        {
            return MailingStatus == MailingStatus.Error;
        }
    }

    [Serializable]
    [XmlType(Namespace = "http://schemas.corelogic.com/ade/authomail/v1.18.0")]
    public enum MailingStatus
    {
        Shipped,
        Error,
        PulledBack,
        eDelivered,
        eSigned
    }
}
