using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AM.Services
{
    public class ReqStatus
    {
        public const string Verifying = "Verifying";
        public const string Queued = "Queued";

        public int Id { get; set; }
        public int HTTPStatusCode { get; set; }
    }

    public class RequestLog
    {
        public int Id { get; set; }
        public int ClientId { get; set; }
        public Guid? MailPieceId { get; set; }
        public string Status { get; set; }
        public int HTTPStatusCode { get; set; }
        public string APIResponse { get; set; }
        public int TotalPages { get; set; }
        public string ErrorLog { get; set; }

        public DateTime ReceivedOn { get; set; }

        public DateTime ProcessedOn
        {
            get { return Common.Now; }
        }

        public double RequestSize { get; set; }
        public double InitialChecks { get; set; }
        public double? Download { get; set; }
        public double? Validate { get; set; }
        public double? Merge { get; set; }
        public double? PostBatch { get; set; }
        public double? Cleanup { get; set; }

        public double TotalTime
        {
            get { return (double)ProcessedOn.Subtract(ReceivedOn).TotalSeconds; }
        }
    }

    public class AddPackageResult
    {
        public int Id { get; set; }
        public int HTTPStatusCode { get; set; }
    }
}
