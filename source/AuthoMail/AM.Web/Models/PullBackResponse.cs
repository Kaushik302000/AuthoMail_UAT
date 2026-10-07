using AM.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AM.Web.Models
{
    public class PullBackResponse
    {
        public string MailPieceId { get; set; }
        public string MailingStatus { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; }
        public DateTime? PullBackRequestedOn { get; set; }
        public string PulledBackRequestedBy { get; set; }       
        public string ErrorCode { get; set; }
        public string ErrorDescription { get; set; }

        public bool ShouldSerializeSuccess()
        {
            return false;
        }
        public bool ShouldSerializeMessage()
        {
            return this.ErrorCode != OutComeCodes.Unknown_Error;
        }
        public bool ShouldSerializePullBackRequestedOn()
        {
            return this.Success;
        }
        public bool ShouldSerializePulledBackRequestedBy()
        {
            return this.Success;
        }
        public bool ShouldSerializeErrorCode()
        {
            return false;
        }
        public bool ShouldSerializeErrorDescription()
        {
            return this.MailingStatus == ShippingStatus.Error.ToString();
        }
    }

    public class PullBackResponseOld
    {
        public string MailPieceId { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; }
        public DateTime? PullBackRequestedOn { get; set; }
        public string PulledBackRequestedBy { get; set; }
        public string MailPieceStatus { get; set; }
        public string ErrorDescription { get; set; }
    }
}