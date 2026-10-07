using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AM.Web.Models
{
    public class SummaryControlViewModel:DailyDetailReportViewModel
    {
        public DateTime? DueDate { get; set; }
        public string ErrorCode { get; set; }
        public string ErrorDesc { get; set; }
    }
}