using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AM.USPSLib
{
    public class eVSCustomsFormsData : IeVSCustomsFormsData
    {
        public List<ItemDetail> ShippingContents { get; set; }
        public CustomsContentType? CustomsContentType { get; set; }
        public string ContentComments { get; set; }
        public RestrictionType? RestrictionType { get; set; }
        public string RestrictionComments { get; set; }
        public string AESITN { get; set; }
        public string ImportersReference { get; set; }
        public string ImportersContact { get; set; }
        public string ExportersReference { get; set; }
        public string ExportersContact { get; set; }
        public string InvoiceNumber { get; set; }
        public string LicenseNumber { get; set; }
        public string CertificateNumber { get; set; }
        public NonDeliveryOption? NonDeliveryOption { get; set; }
        public string AltReturnAddress1 { get; set; }
        public string AltReturnAddress2 { get; set; }
        public string AltReturnAddress3 { get; set; }
        public string AltReturnAddress4 { get; set; }
        public string AltReturnAddress5 { get; set; }
        public string AltReturnAddress6 { get; set; }
        public string AltReturnCountry { get; set; }
        public string LabelImportType { get; set; }
        public ePostageMailerReporting? ePostageMailerReporting { get; set; }
        public string SenderFirstName { get; set; }
        public string SenderLastName { get; set; }
        public string SenderBusinessName { get; set; }
        public string SenderAddress1 { get; set; }
        public string SenderCity { get; set; }
        public string SenderState { get; set; }
        public string SenderZip5 { get; set; }
        public string SenderPhone { get; set; }
        public string SenderEmail { get; set; }

        public bool ShouldSerializeCustomsContentType() { return this.CustomsContentType.HasValue; }
        public bool ShouldSerializeRestrictionType() { return this.RestrictionType.HasValue; }
        public bool ShouldSerializeNonDeliveryOption() { return this.NonDeliveryOption.HasValue; }
        public bool ShouldSerializeePostageMailerReporting() { return this.ePostageMailerReporting.HasValue; }
    }
}
