using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AM.Entities
{
    public class DocGenResponse
    {
        public Guid TransactionId { get; set; }
        public byte[] DocumentData { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}
