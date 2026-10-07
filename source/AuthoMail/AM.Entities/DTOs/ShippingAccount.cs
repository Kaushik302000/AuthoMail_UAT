using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AM.Entities
{
    public class ShippingAccount
    {
        public string AccountCode { get; set; }
        public string WebAuthenticationKey { get; set; }
        public string WebAuthenticationPassword { get; set; }
        public string AccountNumber { get; set; }
        public string MeterNumber { get; set; }
    }
}
