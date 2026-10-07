using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AM.Web.Models
{
    public class FileDetails
    {
        public string FileName { get; set; }
        public long FileSize { get; set; }
        public bool Status { get; set; }
        public DateTime ModifiedOn { get; set; }
        public bool IsLargeTemplateExist { get; set; }
    }
}