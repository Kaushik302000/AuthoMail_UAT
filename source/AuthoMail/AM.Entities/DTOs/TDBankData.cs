using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AM.Entities
{
    public class TDBankData
    {
        [DisplayName("Letter ID")]
        public string LetterID { get; set; }
        [DisplayName("Current Date")]
        public DateTime? CurrentDate { get; set; }
        [DisplayName("Customer Name (1)")]
        public string CustomerName1 { get; set; }
        [DisplayName("Customer Name (2)")]
        public string CustomerName2 { get; set; }
        [DisplayName("Mailing Address (1)")]
        public string MailingAddress1 { get; set; }
        [DisplayName("City (1)")]
        public string City1 { get; set; }
        [DisplayName("State (1)")]
        public string State1 { get; set; }
        [DisplayName("Zip Code (1)")]
        public string ZipCode1 { get; set; }
        [DisplayName("Mailing Address (2)")]
        public string MailingAddress2 { get; set; }
        [DisplayName("City (2)")]
        public string City2 { get; set; }
        [DisplayName("State (2)")]
        public string State2 { get; set; }
        [DisplayName("Zip Code (2)")]
        public string ZipCode2 { get; set; }
        [DisplayName("Accounting Ending In")]
        public string AccountEndingIn { get; set; }
        [DisplayName("Property Address")]
        public string PropertyAddress { get; set; }
        [DisplayName("Greeting")]
        public string Greeting { get; set; }
    }
}
