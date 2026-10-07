using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace AM.USPSLib
{
    public interface IeVSResponseCommon
    {
        string BarcodeNumber { get; set; }

        string LabelImageString { get; set; }

        string ReceiptImageString { get; set; }

        string ToName { get; set; }

        string ToFirm { get; set; }

        string ToAptOrSuiteorOther { get; set; }

        string ToStreetAddress { get; set; }

        string ToCity { get; set; }

        string ToState { get; set; }

        string ToZip5 { get; set; }

        string ToZip4 { get; set; }

        string Postnet { get; set; }

        string RDC { get; set; }

        decimal Postage { get; set; }

        List<ExtraService> ExtraServices { get; set; }

        string Zone { get; set; }

        string DimensionalWeight { get; set; }

        string CarrierRoute { get; set; }

        string PermitHolderName { get; set; }

        string InductionType { get; set; }

        string LogMessage { get; set; }

        USPSError Error { get; set; }
    }
}
