using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AM.Entities
{
    public class FloodData
    {
        public string FloodCertNumber { get; set; }
        public string AccountNumber { get; set; }
        public string BorrowerFirstName { get; set; }
        public string BorrowerLastName { get; set; }
        public string BorrowerStreetAddress { get; set; }
        public string BorrowerStreetAddress2 { get; set; }
        public string BorrowerCity { get; set; }
        public string BorrowerState { get; set; }
        public string BorrowerZip { get; set; }
        public string BorrowerCountry { get; set; }
        public string BorrowerEmailId { get; set; }
        public string DocumentDate { get; set; }
        public string PropertyStreetAddress { get; set; }
        public string PropertyCity { get; set; }
        public string PropertyState { get; set; }
        public string PropertyZip { get; set; }
        public string CommunityName { get; set; }
        public string LoanNumber { get; set; }
        public string NoticeDate { get; set; }
        public string CommunityCode { get; set; }
        public string ClientId { get; set; }
        public string ClientCode { get; set; }
        public string RequestorName { get; set; }
        public string SendEDelivery { get; set; }
    }

    public class CustomTemplate
    {
        public int DocTypeId { get; set; }
        public string CustomTemplatePath { get; set; }
        public string CommunityCode { get; set; }
    }
}
