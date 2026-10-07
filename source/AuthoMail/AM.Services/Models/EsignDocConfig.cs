using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AM.Services
{
    public class EsignDocConfig
    {
        public int ClientId { get; set; }
        public int DocTypeId { get; set; }
        public int SignHeight { get; set; }
        public int SignWidth { get; set; }
        public int SignXOffset { get; set; }
        public int SignYOffset { get; set; }
        public bool Optional { get; set; }
        public int SignPageNumber { get; set; }
    }
}
