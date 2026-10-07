using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AM.HealthAPI
{
    public class Config
    {        
        public string APIDownloadPath { get; set; }
        public long MinFreeGB { get; set; }
        public string AMRootFagosFTP { get; set; }
        public string Url { get; set; }
    }
}
