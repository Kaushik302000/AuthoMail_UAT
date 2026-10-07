using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AM.Entities
{
    public class ClientDocType
    {
        public int Id { get; set; }
        public int ClientId { get; set; }
        public string DocTypeName { get; set; }
    }

    public class DocType
    {
        public int Id { get; set; }
        public int? PnSConfigId { get; set; }
        public int? IntlPnSConfigId { get; set; }
        public int? PnSConfigDetailId { get; set; }
        public string DocTypeName { get; set; }
        public bool IsBRE { get; set; }
        public string PrintSize { get; set; }
        public int ContentScalePercent { get; set; }
        public bool IsActive { get; set; }
        public string ShippingCarrier { get; set; }
        public string ShippingServiceType { get; set; }
        public string ShippingAccountCode { get; set; }
        public bool IsReturnInfoSpecified { get; set; }
        public string RetRcptPrefix { get; set; }
        public string RetRcptFirstName { get; set; }
        public string RetRcptMiddleName { get; set; }
        public string RetRcptLastName { get; set; }
        public string RetRcptSuffix { get; set; }
        public string RetRcptPhone { get; set; }
        public string RetRcptOrganization { get; set; }
        public string RetRcptAddrOptionalLine1AboveStreet { get; set; }
        public string RetRcptAddrOptionalLine2AboveStreet { get; set; }
        public string RetRcptAddressStreet { get; set; }
        public string RetRcptAddressAptOrSuiteOrOther { get; set; }
        public string RetRcptAddrOptionalLine1BelowStreet { get; set; }
        public string RetRcptAddrOptionalLine2BelowStreet { get; set; }
        public string RetRcptAddressCity { get; set; }
        public string RetRcptAddressState { get; set; }
        public string RetRcptAddressZIP { get; set; }
        public string RetRcptAddressCountry { get; set; }
        public string ClientEmails { get; set; }
        public string ClientContactNo { get; set; }
        public string SupportEmails { get; set; }
        public string SupportContactNo { get; set; }
        public int ClientId { get; set; }
        public float QCPercent { get; set; }
        public string BizdayCutOff { get; set; }
        public string BizdaySLAType { get; set; }
        public string BizdayTimeSpan { get; set; }
        public string SatCutOff { get; set; }
        public string SatSLAType { get; set; }
        public string SatTimeSpan { get; set; }
        public string SunCutOff { get; set; }
        public string SunSLAType { get; set; }
        public string SunTimeSpan { get; set; }
        public DateTime ModifiedOn { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public string CreatedBy { get; set; }
        public string ShippingSpecialService { get; set; }
        public int EnvelopeId { get; set; }
        public int NoOfCopies { get; set; }
        public string PrintFormat { get; set; }
        public int MinPage { get; set; }
        public int MaxPage { get; set; }
        public bool IsFallBack { get; set; }
        public bool IsPrintReady { get; set; }
        public bool AllowInsertsOnly { get; set; }
        public string PrintVendorCode { get; set; }
        public TimeSpan? BizdayStartTime { get; set; }
        public TimeSpan? BizdayEndTime { get; set; }
        public TimeSpan? SatStartTime { get; set; }
        public TimeSpan? SatEndTime { get; set; }
        public TimeSpan? SunStartTime { get; set; }
        public TimeSpan? SunEndTime { get; set; }
        public int PrintTrailId { get; set; }
        public string PrintLocation { get; set; }
        public string AppCode { get; set; }
        public string EnvelopeName { get; set; }
        public string EnvelopePartNumber { get; set; }
        public string BREName { get; set; }
        public string BREPartNumber { get; set; }
        public bool IsChecks { get; set; }
        public string BREEnvelopeName_PartNumber
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(BREPartNumber) && !string.IsNullOrWhiteSpace(BREName))
                {
                    return BREName + " / " + BREPartNumber;
                }
                return "NA";
            }
        }
        public bool HasReturnLabel { get; set; }
        public string ReturnShippingCarrier { get; set; }
        public string ReturnShippingServiceType { get; set; }
        public string ReturnShippingAccountCode { get; set; }
    }
    public class DocTypeVM
    {
        public int Id { get; set; }
        public int ClientId { get; set; }
        public int? PnSConfigId { get; set; }
        public string AppCode { get; set; }
        public string ClientName { get; set; }
        public string DocTypeName { get; set; }
        public bool IsBRE { get; set; }
        public string PrintSize { get; set; }
        public bool IsActive { get; set; }
        public float QCPercent { get; set; }
        public int BREEnvelopeId { get; set; }
        public int NoOfCopies { get; set; }
        public string PrintFormat { get; set; }
        public string AllowInsertsOnly { get; set; }
        public string BREName { get; set; }
        public string BREPartNumber { get; set; }
        public bool IsReturnInfoSpecified { get; set; }
        public string BREEnvelopeName_PartNumber
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(BREPartNumber) && !string.IsNullOrWhiteSpace(BREName))
                {
                    return BREName + " / " + BREPartNumber;
                }
                return "NA";
            }
        }

        public string RetRcptPrefix { get; set; }
        public string RetRcptFirstName { get; set; }
        public string RetRcptMiddleName { get; set; }
        public string RetRcptLastName { get; set; }
        public string RetRcptSuffix { get; set; }
        public string RetRcptPhone { get; set; }
        public string RetRcptOrganization { get; set; }
        public string RetRcptAddrOptionalLine1AboveStreet { get; set; }
        public string RetRcptAddrOptionalLine2AboveStreet { get; set; }
        public string RetRcptAddressStreet { get; set; }
        public string RetRcptAddressAptOrSuiteOrOther { get; set; }
        public string RetRcptAddrOptionalLine1BelowStreet { get; set; }
        public string RetRcptAddrOptionalLine2BelowStreet { get; set; }
        public string RetRcptAddressCity { get; set; }
        public string RetRcptAddressState { get; set; }
        public string RetRcptAddressZIP { get; set; }
        public string RetRcptAddressCountry { get; set; }
        public string ClientEmails { get; set; }
        public string ClientContactNo { get; set; }
        public string SupportEmails { get; set; }
        public string SupportContactNo { get; set; }
        public string PaperType { get; set; }
    }
    public class DocTypeReport
    {
        public string AppCode { get; set; }
        public string DocTypeName { get; set; }
        public string PrintSize { get; set; }
        public string PrintFormat { get; set; }
        public int MinPage { get; set; }
        public int MaxPage { get; set; }
        public string PrintTrailName { get; set; }
        public string PaperType { get; set; }
        public string PrintVendor { get; set; }
        public string ShippingCarrier { get; set; }
        public string ShippingServiceType { get; set; }
        public string ShippingAccountCode { get; set; }
        public string ShippingSpecialService { get; set; }
        public bool IsFallBack { get; set; }
        public bool IsPrintReady { get; set; }
        public bool AllowInsertsOnly { get; set; }
        public string IsBRE { get; set; }
        public bool IsActive { get; set; }
        public float QCPercent { get; set; }
        public int ContentScalePercent { get; set; }
        public string ConfigName { get; set; }
        public string DocumentCode { get; set; }
        public string BizdayCutOff { get; set; }
        public string BizdaySLAType { get; set; }
        public string BizdayTimeSpan { get; set; }
        public TimeSpan? BizdayStartTime { get; set; }
        public TimeSpan? BizdayEndTime { get; set; }
        public string SatCutOff { get; set; }
        public string SatSLAType { get; set; }
        public string SatTimeSpan { get; set; }
        public TimeSpan? SatStartTime { get; set; }
        public TimeSpan? SatEndTime { get; set; }
        public string SunCutOff { get; set; }
        public string SunSLAType { get; set; }
        public string SunTimeSpan { get; set; }
        public TimeSpan? SunStartTime { get; set; }
        public TimeSpan? SunEndTime { get; set; }
        public string EnvelopeName { get; set; }
        public string EnvelopePartNumber { get; set; }
        public string BREName { get; set; }
        public string BREPartNumber { get; set; }
        public int NoOfCopies { get; set; }
        public bool IsReturnInfoSpecified { get; set; }
        public string RetRcptPrefix { get; set; }
        public string RetRcptFirstName { get; set; }
        public string RetRcptMiddleName { get; set; }
        public string RetRcptLastName { get; set; }
        public string RetRcptSuffix { get; set; }
        public string RetRcptPhone { get; set; }
        public string RetRcptOrganization { get; set; }
        public string RetRcptAddrOptionalLine1AboveStreet { get; set; }
        public string RetRcptAddrOptionalLine2AboveStreet { get; set; }
        public string RetRcptAddressStreet { get; set; }
        public string RetRcptAddressAptOrSuiteOrOther { get; set; }
        public string RetRcptAddrOptionalLine1BelowStreet { get; set; }
        public string RetRcptAddrOptionalLine2BelowStreet { get; set; }
        public string RetRcptAddressCity { get; set; }
        public string RetRcptAddressState { get; set; }
        public string RetRcptAddressZIP { get; set; }
        public string RetRcptAddressCountry { get; set; }
        public string ClientEmails { get; set; }
        public string ClientContactNo { get; set; }
        public string SupportEmails { get; set; }
        public string SupportContactNo { get; set; }
    }

    public class PnSConfigDetailCoverLetter
    {
        public int PnSConfigDetailId { get; set; }
        public int SeqId { get; set; }
        public string CoverLetterTemplatePath { get; set; }
    }

    public class PrintVendor
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int SFTChannelId { get; set; }
        public string BusinessCode { get; set; }
        public string VendorCode { get; set; }
        public int ShipperId { get; set; }
        public bool IsActive { get; set; }
    }

    public class PrintVendorLocations
    {
        public string PrintVendorCode { get; set; }
        public string PrintVendorName { get; set; }
        public string PrintVendorNameCode { get { return $"{PrintVendorName}-{PrintVendorCode}"; } }
    }

    public class PrintTrail
    {
        public int Id { get; set; }
        public string TrailName { get; set; }
        public bool SimulateTrail { get; set; }
        public int PrintVendorId { get; set; }
        public string PrintVendor { get; set; }
    }

    public class ShipperInformation
    {
        public int Id { get; set; }
        public int PrintVendorId { get; set; }
        public string PrintLocation { get; set; }
        public string PrintVendorName { get; set; }
        public string PrintVendorStreet { get; set; }
        public string PrintVendorCity { get; set; }
        public string PrintVendorState { get; set; }
        public string PrintVendorZip { get; set; }
    }

    public class PnSConfigDetailPrintTrailMap
    {
        public int PnSConfigDetailId { get; set; }
        public int PrintTrailId { get; set; }
        public TimeSpan? BizDayStartTime { get; set; }
        public TimeSpan? BizDayEndTime { get; set; }
        public TimeSpan? SatStartTime { get; set; }
        public TimeSpan? SatEndTime { get; set; }
        public TimeSpan? SunStartTime { get; set; }
        public TimeSpan? SunEndTime { get; set; }
    }
}
