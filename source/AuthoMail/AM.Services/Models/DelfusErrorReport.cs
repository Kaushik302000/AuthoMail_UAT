using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AM.Services
{
    public class DelfusErrorReport
    {
        public string RD_CUST_ID { get; set; }
        public string RD_LOAN_ID { get; set; }
        public string DocumentType { get; set; }
        public string PdfFileName { get; set; }
        public string DisqualifyReason { get; set; }
        public DateTime? DownloadedOn { get; set; }
        public string RecipientName { get; set; }
        public string RD_SITUS_STR_NM { get; set; }
        public string RD_SITUS_CITY_NM { get; set; }
        public string RD_SITUS_STATE_ABBR { get; set; }
        public string RD_SITUS_ZIP_CD { get; set; }

    }
}
