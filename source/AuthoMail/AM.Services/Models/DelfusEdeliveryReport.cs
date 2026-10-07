using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AM.Services
{
    public class DelfusEdeliveryReport
    {
        public string RD_LOAN_ID { get; set; }
        public string BatchName { get; set; }
        public string SubmittedDate { get; set; }
        public string EDeliveryStatus { get; set; }
        public DateTime? EDeliveryStatusDate { get; set; }
        public string RD_DLVR_EMAIL_ADDR_TXT { get; set; }        
        public string RD_CUST_ID { get; set; }
        public string RD_BRRW_FRST_NM { get; set; }
        public string RD_BRRW_LAST_NM { get; set; }
        public string RD_BRRW_ADDR1_TXT { get; set; }
        public string RD_BRRW_CITY_NM { get; set; }
        public string RD_BRRW_STATE_ABBR { get; set; }
        public string RD_BRRW_ZIP_CD { get; set; }
        public DateTime? DownloadedOn { get; set; }
        public bool IsEDeliveryRecord { get; set; }
        public string RD_TMPLT_CD { get; set; }
        public string PdfFileName { get; set; }
        public string DocumentType { get; set; }
        public string Disqualifyreason { get; set; }
        public string RecordStatus { get; set; }
    }
}
