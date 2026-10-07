using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCI.Service
{
    class MRDFResponse
    {
        public int JobId { get; set; }
        public int PieceId { get; set; }
        public DateTime Date { get; set; }
        public int Feeder2_Fed { get; set; }
        public string OperatorId { get; set; }
        public string MachineId { get; set; }
        public DateTime ProcessingTimeStamp { get; set; }
        public string DetailedErrorId { get; set; }
        public string Disposition { get; set; }
        public string Input_Fed { get; set; }
        public string Feeder3_Fed { get; set; }
        public string Feeder4_Fed { get; set; }
        public string Feeder5_Fed { get; set; }
        public decimal ComputedWeight { get; set; }
        public string MeterDate { get; set; }
        public string StatusSource { get; set; }
        public decimal PostageMetered { get; set; }
        public decimal WeightMeasured { get; set; }
        public string ExitLocation { get; set; }
        public int TotalSheets { get; set; }
        public string PDFFileName { get; set; }
    }

    class ValidationDifference
    {
        public int PieceId { get; set; }
        public int TotalSheets { get; set; }
        public string PDFFileName { get; set; }
    }
}
