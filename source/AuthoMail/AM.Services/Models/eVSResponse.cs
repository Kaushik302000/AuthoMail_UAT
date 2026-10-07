using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using AM.USPSLib;

namespace AM.Services
{
    public class eVSResponse : IeVSResponse
    {
        public string BarcodeNumber { get; set; }

        [XmlElement(ElementName = "LabelImage")]
        public string LabelImageString { get; set; }

        [XmlElement(ElementName = "ReceiptImage")]
        public string ReceiptImageString { get; set; }

        public string ToName { get; set; }

        public string ToFirm { get; set; }

        [XmlElement(ElementName = "ToAddress1")]
        public string ToAptOrSuiteorOther { get; set; }
        [XmlElement(ElementName = "ToAddress2")]
        public string ToStreetAddress { get; set; }

        public string ToCity { get; set; }

        public string ToState { get; set; }

        public string ToZip5 { get; set; }

        public string ToZip4 { get; set; }

        public string Postnet { get; set; }

        public string RDC { get; set; }

        public decimal Postage { get; set; }

        public List<ExtraService> ExtraServices { get; set; }

        public string Zone { get; set; }

        public string DimensionalWeight { get; set; }

        public string CarrierRoute { get; set; }

        public string PermitHolderName { get; set; }

        public string InductionType { get; set; }

        public string LogMessage { get; set; }

        public Commitment Commitment { get; set; }

        public USPSError Error { get; set; }

        [XmlIgnore]
        public bool HasError
        {
            get
            {
                if (this.Error == null) return false;
                return !(String.IsNullOrWhiteSpace(this.Error.Number) &&
                         String.IsNullOrWhiteSpace(this.Error.Description));
            }
            set { }
        }
    }
}
