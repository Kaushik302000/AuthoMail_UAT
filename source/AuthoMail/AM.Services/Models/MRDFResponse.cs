using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AM.Services
{
    public class MRDFResponse
    {
        public string PrintFileName { get; set; }
        public string Date { get; set; }
        public string JobId { get; set; }
        public string PieceId { get; set; }
        public string TotalSheets { get; set; }
        public string PDFFileName { get; set; }
        public string OperatorId { get; set; }
        public string MachineId { get; set; }
        public string ProcessingTimeStamp { get; set; }
        public string DetailedErrorId { get; set; }
        public string Disposition { get; set; }
        public string Common { get; set; }
        public string PackageTrackingNumber { get; set; }
    }
}
