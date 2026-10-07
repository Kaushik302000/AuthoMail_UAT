using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AM.Web.Models
{
    public class ReportViewModel
    {
        public int Key { get; set; }//batch id
        public string Value { get; set; }// time with batch id (24 hr h:m )
    }
}