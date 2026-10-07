using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AM.Services
{
    public class AMSetting
    {
        public string Id { get; set; }
        public string DataType { get; set; }
        public string Value { get; set; }
        public string Description { get; set; }
        public bool IsSystem { get; set; }
        public string DefaultValue { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedOn { get; set; }
    }
}
