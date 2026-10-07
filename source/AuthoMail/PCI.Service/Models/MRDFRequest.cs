using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCI.Service
{
    public class MRDFRequest
    {
        public int JobId { get; set; }
        public int? PrintTrailBatchId { get; set; }
        public int PieceId { get; set; }
        public DateTime Date { get; set; }
        public int TotalSheets { get; set; }
        public int TotalPages { get; set; }
        public string Address1 { get; set; }
        public Guid UniqueId { get; set; }
        public string PDFFileName { get; set; }
        public string MailPieceStatus { get; set; }
        public int ErrId { get; set; }
        public string ErrDesc { get; set; }
        public string Expansion { get; set; }
        public int DivertVS3 { get; set; }
        public int Feeder4_Sel { get; set; }
        public bool HasError
        {
            get { return !ErrDesc.IsNullOrWhiteSpace(); }
        }
        public Int64? PDFFileSize { get; set; }
    }

    public class ErrorResponse
    {
        public int PieceId { get; set; }
        public string ErrorDesc { get; set; }
    }
}
