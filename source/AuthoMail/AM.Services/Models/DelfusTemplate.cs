using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AM.Services
{
    public class DelfusTemplate
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedOn { get; set; }
        public string Status { get; set; }
        public string Reason { get; set; }
    }
    public class DelfusResult
    {
        public bool Success { get; set; }
        public object Object { get; set; }
        public string ErrorMessage { get; set; }
    }
}
