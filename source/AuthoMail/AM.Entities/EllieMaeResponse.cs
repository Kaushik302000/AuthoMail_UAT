using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AM.Entities
{
    public class EllieMaeResponse
    {
        public Guid Id { get; set; }
        public string Status { get; set; }
        public Error Error { get; set; }
    }

    public class Error
    {
        public string code { get; set; }
        public string summary { get; set; }
        public string details { get; set; }
    }
}
