using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AM.USPSLib
{
    //Todo take it from below
    //Source: 2.1.3 eVS Customs Forms and Data Requirements 
    public interface IeVSCustomsFormsData
    {
        List<ItemDetail> ShippingContents { get; set; }
        CustomsContentType? CustomsContentType { get; set; }
        string ContentComments { get; set; }
        RestrictionType? RestrictionType { get; set; }
        string RestrictionComments { get; set; }
        string AESITN { get; set; }
        string ImportersReference { get; set; }
        string ImportersContact { get; set; }
        string ExportersReference { get; set; }
        string ExportersContact { get; set; }
        string InvoiceNumber { get; set; }
        string LicenseNumber { get; set; }
        string CertificateNumber { get; set; }
        NonDeliveryOption? NonDeliveryOption { get; set; }
        string AltReturnAddress1 { get; set; }
        string AltReturnAddress2 { get; set; }
        string AltReturnAddress3 { get; set; }
        string AltReturnAddress4 { get; set; }
        string AltReturnAddress5 { get; set; }
        string AltReturnAddress6 { get; set; }
        string AltReturnCountry { get; set; }
        string LabelImportType { get; set; }
        ePostageMailerReporting? ePostageMailerReporting { get; set; }
        string SenderFirstName { get; set; }
        string SenderLastName { get; set; }
        string SenderBusinessName { get; set; }
        string SenderAddress1 { get; set; }
        string SenderCity { get; set; }
        string SenderState { get; set; }
        string SenderZip5 { get; set; }
        string SenderPhone { get; set; }
        string SenderEmail { get; set; }
    }
}
