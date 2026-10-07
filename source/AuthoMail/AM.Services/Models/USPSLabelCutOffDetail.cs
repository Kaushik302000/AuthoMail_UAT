using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AM.Services
{
    public class USPSLabelCutOffDetail
    {
        public string USPSUserId { get; set; }
        public TimeSpan? BizStartTime { get; set; }
        public TimeSpan? BizEndTime { get; set; }
        public TimeSpan? SatStartTime { get; set; }
        public TimeSpan? SatEndTime { get; set; }
        public string PrintVendorLocation { get; set; }
    }

    public class USPSCredentialInfo
    {
        public string UserId { get; set; }
        public string PassKey { get; set; }
        public string PrintLocation { get; set; }
    }
}
