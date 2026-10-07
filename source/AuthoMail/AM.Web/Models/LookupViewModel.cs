using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AM.Web.Models
{
    public class LookupViewModel
    {
        public string Value { get; set; }
        public List<string> LookupList { get; set; }
    }
}