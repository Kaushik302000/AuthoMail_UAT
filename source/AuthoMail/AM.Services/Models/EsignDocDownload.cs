using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AM.Services
{
    public class EsignDocDownload
    {
        public int DocumentId { get; set; }
        public string Document { get; set; }
        public string DocumentName { get; set; }
        public bool HasError { get; set; }
        public string ErrorDesc { get; set; }
    }
}
