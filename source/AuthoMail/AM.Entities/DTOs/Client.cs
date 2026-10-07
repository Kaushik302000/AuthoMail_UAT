using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AM.Entities
{
    public class Client
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string AppCode { get; set; }
        public int BusinessUnit { get; set; }
        public bool IsAMActive { get; set; }
        public bool IsPnSActive { get; set; }
        public string TimeZone { get; set; }
        public int SigningRoomExpiryDays { get; set; }
        public bool IsReqBlankPageValidation { get; set; }
        public bool IsEncrypted { get; set; }
        public int RetentionPeriod { get; set; }
        public string GroupAlias { get; set; }
        public string Contact1Email { get; set; }
        public string Contact2Email { get; set; }
        public bool IsResponseRequired { get; set; }
        public bool IsHardAddrValidationRequired { get; set; }
        public bool IsRejectDupRecord { get; set; }
        public bool IsEDeliveryEnabled { get; set; }
    }

    public class ClientSFTChannel
    {
        public int ClientId { get; set; }
        public int SFTChannelId { get; set; }
    }

    public class SFTChannel
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public string Title { get; set; }
        public string Protocol { get; set; }
        public int? Port { get; set; }
        public string Host { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string ToADEDir { get; set; }
        public string ArchiveDir { get; set; }
        public string PostDownloadAction { get; set; }
        public string APITokenURL { get; set; }
        public string APIResponseURL { get; set; }
        public int APIMaxResponseCount { get; set; }
        public string APITokenHeaders { get; set; }
        public string ServerIP { get; set; }
        public Dictionary<string, string> GetHeaders()
        {
            return APITokenHeaders
                    .Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(line => line.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries))
                    .ToDictionary(pair => pair[0].Trim(), pair => pair[1].Trim());
        }

        public string GetChannelCode()
        {
            if (!string.IsNullOrWhiteSpace(Code))
                return Code.Substring(0, Code.LastIndexOf("_"));
            return null;
        }
    }

    public class ClientBusinessIdentifier
    {
        public int ClientId { get; set; }
        public string BusinessIdName { get; set; }
    }

    public class Deliverable
    {
        public int Id { get; set; }
        public int ClientId { get; set; }
        public string ReportName { get; set; }
        public string ReportType { get; set; }
        public string DeliveryType { get; set; }
        public string ReportQuery { get; set; }
        public string Delimeter { get; set; }
        public string FileNameExpression { get; set; }
        public bool IsSFTUploadRequired { get; set; }
        public string UploadPath { get; set; }
        public int? EmailChannelId { get; set; }
        public int? ScheduleId { get; set; }
        public EmailChannel EmailChannel { get; set; }
    }

    public class EmailChannel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string To { get; set; }
        public string Cc { get; set; }
        public string Bcc { get; set; }
        public string Subject { get; set; }
        public string BodyTemplate { get; set; }
        public bool IsBodyHTML { get; set; }
    }

    public class ClientAMCCustIdMap
    {
        public int ClientId { get; set; }
        public string AMCCustId { get; set; }
    }

    public class DallasNotificationMap
    {
        public string AppCode { get; set; }
        public int SeqNo { get; set; }
    }
}
