using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using AM.Lib;
using System.Data;
using System.Net.Mail;
using System.IO;
using System.Web.Hosting;
namespace AM.Services
{
    public interface IPullBackService : IDisposable
    {
        List<PullBackViewModel> GetMailPieceData(List<string> mpIds);
        List<PullBackRequestForm> GetPullBackData(List<string> mpIds);
        int UpdatePullBackRequestedOn(string mpId, string userId, string status);
        int InsertPullBackRequestData(string mpId, int clientId, string status, string userId, string pbReasons);
        int UpdateAMBatchStatus(string mpId);
        int UpdateUSPSApiCancelErrorDesc(string mpId);
        string GetPnSBatchPipelineStatusByPrintBatchId(int printBatchId);
        List<MailPieceViewModel> GetPullBackRequestList(string businessFriendlyIdName, string businessFriendlyIdValue, int? docType, DateTime startDate, DateTime endDate, string channel, int clientId, string batchName);
        List<QuickPullBackResponse> GetQuickPullBackResponseFromBusinessFriendlyValue(string businessFriendlyIdName, string businessFriendlyIdValue, int clientId);
        List<QuickPullBackResponse> GetQuickPullBackResponse(List<string> mailPieceIds, int clientId);
        bool GeneratePullBackForm(List<PullBackRequestForm> pullBackData);
    }
    public class PullBackService : ServiceBase, IPullBackService
    {
        public List<PullBackViewModel> GetMailPieceData(List<string> mpIds)
        {
            string qry = string.Format(@"SELECT AMMP.MailBatchId,AMMP.[UniqueId] as MailPieceUniqueId, AMMP.ClientId, PM.PrintTrailBatchId,PB.PrintBatch,AMMP.TrackingNo,pcd.ShippingServiceType,AMMB.PipelineStatus AS AMBatchPipelineStatus,PB.PipelineStatus AS PnSBatchPipelineStatus,AMMP.ShippingStatus AS AMMailPieceStatus,PM.MailPieceStatus AS PnSMailPieceStatus,AMMP.PrintLocation,AMMP.ErrorDesc AS AMErrorDescription,PM.ErrDesc AS PnSErrorDescription,PLBT.PCIBatchPrepEnd,AMMP.IsEDeliveryRequested,EDMP.SigningRoomId,EDMP.SigningRoomName,EDMP.IsInvitationSent,AMMP.IsPullBackRequested,AMMP.PullBackRequestedOn,AMMP.IsPulledBack,AMMP.PulledBackOn,PU.UserName FROM AM.MailPieces AMMP LEFT OUTER JOIN AM.EDeliveryMailPieces EDMP ON AMMP.UniqueId = EDMP.UniqueId LEFT OUTER JOIN PnS.MRDFRequest PM ON AMMP.[UniqueId] = PM.UniqueId  
              left outer join [AM].[DocTypes] d ON AMMP.DocTypeId=d.Id 
              left outer JOIN Common.PnSConfigDetail pcd ON AMMP.PnSConfigDetailId = pcd.Id
              INNER JOIN AM.MailBatches AMMB ON AMMB.Id = AMMP.MailBatchId LEFT OUTER JOIN PnS.BatchSummary PB ON PB.PrintBatch = PM.JobId LEFT OUTER JOIN PnS.LogBatchTimeLine PLBT ON PLBT.BatchId = PB.PrintBatch LEFT OUTER JOIN[Profile].Users PU ON PU.Id = AMMP.PulledBackRequestedBy  WHERE AMMP.[UniqueId] IN @mailPieceIds");
            var data = db.Query<PullBackViewModel>(qry, new { mailPieceIds = mpIds }).ToList();
            return data;
        }
        public List<PullBackRequestForm> GetPullBackData(List<string> mpIds)
        {
            string qry = @";WITH 
                           printTrailBatch AS (SELECT Id,PrintBatchName,SubmittedOn,PrintTrailId FROM PnS.PrintTrailBatches),
                           mrdfReq AS (SELECT PrintTrailBatchId,PieceId,[UniqueId],PDFFileName FROM PnS.MRDFRequest),
                           printTrail AS (SELECT Id,TrailName,PrintVendorId FROM Common.PrintTrails),
                           printVendor AS (SELECT Id,Name,VendorCode FROM Common.PrintVendors),
                           mailPieces AS (SELECT Id as MailPieceId,UniqueId,ClientId,RcptFirstName,RcptLastName,DocTypeId,PrintTrailId FROM AM.MailPieces),
                           Client AS(SELECT Id,Name FROM Common.Clients),
                           RESULT AS (SELECT PTB.PrintBatchName,PMR.PDFFileName,PTB.SubmittedOn,PMR.PrintTrailBatchId,AMP.MailPieceId,PMR.PieceId,AMP.RcptFirstName,AMP.RcptLastName,CC.Name,PV.VendorCode FROM printTrailBatch PTB INNER JOIN mrdfReq PMR ON PTB.Id = PMR.PrintTrailBatchId INNER JOIN mailPieces AMP ON PMR.UniqueId = AMP.UniqueId INNER JOIN printTrail PT ON AMP.PrintTrailId = PT.Id INNER JOIN printVendor PV ON PT.PrintVendorId = PV.Id INNER JOIN  Client CC ON AMP.ClientId = CC.Id WHERE PMR.[UniqueId] IN @mailPieceIds)
                           (SELECT PrintBatchName+'.zip' as PrintFileName,SubmittedOn,PrintTrailBatchId AS PrintBatch,PieceId = STUFF((SELECT DISTINCT ', '+ CAST(CASE WHEN VendorCode = 'NORDIS' THEN MailPieceId ELSE pRES.PieceId END AS VARCHAR(255)) + CASE WHEN VendorCode = 'NORDIS' THEN '/' ELSE ':' END + pRES.PDFFileName+ CASE WHEN VendorCode = 'NORDIS' THEN '/' ELSE ':' END + ISNULL(pRES.RcptFirstName,'')+' '+ ISNULL(pRES.RcptLastName,'') FROM RESULT pRES WHERE pRES.PrintTrailBatchId = RES.PrintTrailBatchId FOR XML PATH('')),1,1,''),Name,VendorCode FROM RESULT RES GROUP BY RES.PrintTrailBatchId,PrintBatchName,SubmittedOn,Name,VendorCode)";

            var data = db.Query<PullBackRequestForm>(qry, new { mailPieceIds = mpIds }).ToList();
            return data;
        }

        public int UpdatePullBackRequestedOn(string mpId, string userId, string status)
        {
            var data = 0;
            if (string.IsNullOrEmpty(status))
            {
                string qry = string.Format(@"UPDATE AM.MailPieces SET PullBackRequestedOn = @utcDate,PulledBackRequestedBy = @UserId WHERE UniqueId = @id");
                data = db.Execute(qry, new { utcDate = DateTime.UtcNow, UserId = string.IsNullOrEmpty(userId) ? DBNull.Value.ToString() : userId, id = mpId });
            }
            if (status == "PulledBack")
            {
                string qry = string.Format(@"UPDATE AM.MailPieces SET ShippingStatus = @shippingStatus, PullBackRequestedOn = @utcDate, PulledBackOn = @pulledbackUtcDate,PulledBackRequestedBy = @UserId WHERE UniqueId = @id");
                data = db.Execute(qry, new { shippingStatus = status, utcDate = DateTime.UtcNow, pulledbackUtcDate = DateTime.UtcNow, UserId = string.IsNullOrEmpty(userId) ? DBNull.Value.ToString() : userId, id = mpId });
                db.Execute(@"UPDATE PnS.MRDFRequest SET MailPieceStatus = 'PulledBack', ErrId = '-4', ErrDesc = 'Mail piece was pulled back by client directly.' , TotalSheets = 0 WHERE UniqueId = @MailPieceUniqueId", new { MailPieceUniqueId = mpId });
                UpdateAMBatchStatus(mpId.ToString());
                db.Execute(@"UPDATE AM.EDeliveryMailPieces SET PipelineStatus = 'ReportedBack',ReportedBackOn = @ReportedBackOn WHERE UniqueId = @MailPieceUniqueId", new { ReportedBackOn = DateTime.UtcNow, MailPieceUniqueId = mpId });
                db.Execute(@"UPDATE AM.MailPieces SET PrintBatchId = null WHERE UniqueId = @MailPieceUniqueId", new { MailPieceUniqueId = mpId });
            }
            return data;
        }

        public int InsertPullBackRequestData(string mpId, int clientId, string status, string userId, string pbReason)
        {
            return db.Execute("[dbo].[usp_InsertPullBackRequestsData]", new { @MailPieceUniqueId = mpId, @ClientId = clientId, @Status = status, @RequestedOn = DateTime.UtcNow, @RequestedBy = string.IsNullOrEmpty(userId) ? DBNull.Value.ToString() : userId, @PBReasons = pbReason }, commandType: CommandType.StoredProcedure);
        }

        public int UpdateAMBatchStatus(string mpId)
        {
            return db.Execute("usp_UpdateAMMailBatchesStatus", new { @MailPieceUniqueId = mpId, @shippingResponseReceievedOn = DateTime.UtcNow }, commandType: CommandType.StoredProcedure);
        }

        public int UpdateUSPSApiCancelErrorDesc(string mpId)
        {
            return db.Execute(@"UPDATE AM.MailPieces SET ErrorDesc = 'USPS label is cancelled because mailpiece is pulledback.' WHERE UniqueId = @MailPieceUniqueId", new { MailPieceUniqueId = mpId });
        }

        //public List<MailPieces> GetPullBackRequestList(string businessFriendlyIdName, string businessFriendlyIdValue, string docType, DateTime startDate, DateTime endDate, int clientId)
        public List<MailPieceViewModel> GetPullBackRequestList(string businessFriendlyIdName, string businessFriendlyIdValue, int? docType, DateTime startDate, DateTime endDate, string channel, int clientId, string batchName)
        {
            string query = @"select m.BusinessFriendlyIdValue as BusinessIdValue,m.UniqueId,m.FileName,(m.RcptFirstName+ ' ' +m.RcptLastName) as Name,
                            m.RcptAddressStreet as Address,m.TotalPages,m.TotalBlankPages,m.IsPullBackRequested,m.ShippingStatus as Status,m.DueDate,m.TrackingNo,m.ErrorCode,m.ErrorDesc,IsEDeliveryRequested,IsESignRequested,
                            b.ReceivedOn,
                            d.DocTypeName as DocType,
                            c.AppCode 
                            from AM.MailPieces m 
                            inner join AM.MailBatches b on m.MailBatchId=b.Id
                            inner join Common.Clients c on m.ClientId=c.Id
                            LEFT OUTER JOIN Common.SFTChannels s on b.SFTChannelId=s.Id
                            LEFT OUTER JOIN [Profile].[Users] u on m.[PulledBackRequestedBy]=u.Id 
                            left outer join [AM].[DocTypes] d ON m.DocTypeId=d.Id 
                            left outer join Common.PnSConfigDetail pcd ON m.PnSConfigDetailId = pcd.Id
                            where m.ShippingStatus<>'Error' AND ((m.MailBatchId=b.Id and m.ClientId=c.Id) and b.ClientId=@clientId) ";

            DynamicParameters parameter = new DynamicParameters();
            parameter.Add("@clientId", clientId);
            if (!string.IsNullOrWhiteSpace(businessFriendlyIdName))
            {
                query = string.Format("{0} and m.BusinessFriendlyIdName=@BusinessFriendlyIdName", query);
                parameter.Add("@BusinessFriendlyIdName", businessFriendlyIdName);
            }
            if (!string.IsNullOrWhiteSpace(businessFriendlyIdValue))
            {
                query = string.Format("{0} and m.businessFriendlyIdValue=@businessFriendlyIdValue", query);
                parameter.Add("@businessFriendlyIdValue", businessFriendlyIdValue);
            }

            if (!batchName.IsNullOrWhiteSpace())
            {
                query = string.Format("{0} and b.BatchName=@batchName", query);
                parameter.Add("@batchName", batchName);
            }

            if (docType.HasValue)
            {
                query = string.Format("{0} and d.Id = @docTypeId", query);
                parameter.Add("@docTypeId", docType);
            }
            if (!channel.IsNullOrWhiteSpace())
            {
                query = string.Format("{0} and s.Code = @sftChannelId", query);
                parameter.Add("@sftChannelId", channel);
            }
            query = string.Format("{0} and b.ReceivedOn BETWEEN @startDate and @endDate order by b.ReceivedOn desc", query);
            parameter.Add("@startDate", startDate.ToStartDateTime().EstToUtc());
            parameter.Add("@endDate", endDate.ToEndDateTime().EstToUtc());
            var data = db.Query<MailPieceViewModel>(query, parameter).ToList();
            return data;
        }
        public List<QuickPullBackResponse> GetQuickPullBackResponseFromBusinessFriendlyValue(string businessFriendlyIdName, string businessFriendlyIdValue, int clientId)
        {
            string query = PullBackServiceQuery.GetQuickPullBackResponseFromBusinessFriendlyValue;
            var mailPieces = db.Query<QuickPullBackResponse>(query, new { businessFriendlyIdName = businessFriendlyIdName, businessFriendlyIdValue = businessFriendlyIdValue, clientId = clientId }).ToList();
            return mailPieces;
        }
        public List<QuickPullBackResponse> GetQuickPullBackResponse(List<string> mailPieceIds, int clientId)
        {
            string query = PullBackServiceQuery.GetQuickPullBackResponse;
            var response = db.Query<QuickPullBackResponse>(query, new { clientId = clientId, mailPieceIds = mailPieceIds }).ToList();
            return response;
        }

        public bool GeneratePullBackForm(List<PullBackRequestForm> pullBackData)
        {
            bool isGenerated = true;
            try
            {
                foreach (var item in pullBackData)
                {
                    if (item.VendorCode == "PCI" || item.VendorCode == "NORDIS")
                    {
                        Dictionary<string, string> dataForPullBackForm = new Dictionary<string, string>();
                        DateTime eastern = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, Config.Web_API_PB_ClientTimeZone);
                        dataForPullBackForm.Add("RequestDateTime", eastern.ToString());
                        dataForPullBackForm.Add("FileName", string.Format("{0} {1}", item.PrintFileName, item.SubmittedOn == null ? string.Empty : item.SubmittedOn.Value.ToString("MM/dd/yy")));
                        dataForPullBackForm.Add("JobNumber", item.PrintBatch.ToString());
                        dataForPullBackForm.Add("PackagesToPull", string.Format("{0}", item.PieceId.ToString()));
                        dataForPullBackForm.Add("Coordinator", string.Format("AuthoMail, ADE"));
                        dataForPullBackForm.Add("PkgInstruction", string.Empty);
                        dataForPullBackForm.Add("Comments", string.Empty);

                        byte[] output = null;
                        string attachmentPdfName = string.Format("{0}_{1}.pdf", item.PrintBatch.ToString(), DateTime.Now.ToString("ddMMyyyyhhmmss"));
                        //string outputPdfPath = Path.Combine(Config.API_PB_OutputDir, string.Format("{0}_{1}.pdf", item.PrintBatch.ToString(), DateTime.Now.ToString("ddMMyyyyhhmmss")));
                        //Helper.MailMerge(Config.API_PB_PullBackFormTemplatePath, dataForPullBackForm, outputPdfPath);
                        if (item.VendorCode == "PCI")
                        {
                            Helper.MailMerge(Config.Web_API_PB_PullBackForm_PCI, dataForPullBackForm, ref output);
                        }
                        else if (item.VendorCode == "NORDIS")
                        {
                            Helper.MailMerge(Config.Web_API_PB_PullBackForm_NORDIS, dataForPullBackForm, ref output);
                        }
                        bool isEmailSent = SendEmailWithPullBackForm(output, attachmentPdfName, item.VendorCode);
                    }

                }
            }
            catch (Exception ex)
            {
                isGenerated = false;
                throw ex;
            }
            return isGenerated;
        }

        private bool SendEmailWithPullBackForm(byte[] attachmentPdf, string attachmentPdfName, string vendorCode)
        {
            bool isEmailSent = false;

            try
            {
                using (MailMessage msg = new MailMessage())
                {
                    msg.IsBodyHtml = true;
                    if (vendorCode == "PCI")
                    {
                        msg.To.Add(Config.Web_API_PB_Email_To_PCI);
                    }
                    else if (vendorCode == "NORDIS")
                    {
                        msg.To.Add(Config.Web_API_PB_Email_To_NORDIS);
                    }
                    msg.CC.Add(Config.Web_API_PB_Email_Cc);
                    msg.Subject = Config.Web_API_PB_Email_Subject;
                    msg.Priority = MailPriority.High;
                    msg.Body = string.Format("{0} {1}.", Config.Web_API_PB_Email_BodyMsg, attachmentPdfName);
                    msg.Attachments.Add(new Attachment(new MemoryStream(attachmentPdf), attachmentPdfName));
                    SmtpClient sc = new SmtpClient();
                    sc.Send(msg);
                    isEmailSent = true;
                }

            }
            catch (Exception err)
            {
                throw err;
            }

            return isEmailSent;
        }

        public string GetPnSBatchPipelineStatusByPrintBatchId(int printBatchId)
        {
            string qry = string.Format("SELECT PipelineStatus FROM PnS.BatchSummary WHERE PrintBatch = @PrintBatchId");
            string data = db.QuerySingle<string>(qry, new { PrintBatchId = printBatchId });
            return data;
        }

    }

    public static class PullBackServiceQuery
    {

        public static string GetQuickPullBackResponseFromBusinessFriendlyValue = "select b.ClientBatchId,c.TimeZone as [ClientTimeZone], m.Id as [MailPieceId], m.UniqueId as [MailPieceUniqueId],b.Id as [BatchId],b.UniqueId as [BatchUniqueId],b.BatchName,b.ReceivedOn,m.BusinessFriendlyIdName,m.BusinessFriendlyIdValue,m.[FileName],m.[ShippingStatus],m.IsPullBackRequested from [AM].[MailBatches] b,[AM].[MailPieces] m, [Common].[Clients] c where b.Id = m.MailBatchId and m.BusinessFriendlyIdName=@businessFriendlyIdName and m.ClientId=c.Id  m.BusinessFriendlyIdValue=@businessFriendlyIdValue AND b.ClientId=@clientId";
        public static string GetQuickPullBackResponse = "select b.ClientBatchId,c.TimeZone as [ClientTimeZone], m.Id as [MailPieceId], m.UniqueId as [MailPieceUniqueId],b.Id as [BatchId],b.UniqueId as [BatchUniqueId],b.BatchName,b.ReceivedOn,m.BusinessFriendlyIdName,m.BusinessFriendlyIdValue,m.[FileName]m.ShippingStatus,m.IsPullBackRequested from [AM].[MailBatches] b,[AM].[MailPieces] m, [Common].[Clients] c where b.Id =m.MailBatchId and m.ClientId=c.Id and m.UniqueId=@mailPieceIds AND b.ClientId=@clientId";
    }
}
