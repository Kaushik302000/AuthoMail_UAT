using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;

namespace AM.Services.Services
{
    public interface IDelfusTemplateService : IDisposable
    {
        Task<KeyValuePair<bool, string>> SaveTemplate(DelfusTemplate delfusTemplate);
        Task<IEnumerable<DelfusTemplate>> DelfusTemplatesByFileName(string fileName);
        Task<IEnumerable<string>> GetDelfusMergeList();
        Task<IEnumerable<DelfusTemplate>> DelfusTemplateSummary();
        Task<IEnumerable<DelfusErrorReport>> GetTaxCommercialErrorData(DateTime startDate, DateTime endDate);
        Task<IEnumerable<DelfusEdeliveryReport>> GetTaxCommercialEDeliveryData(DateTime startDate, DateTime endDate);
    }
    public class DelfusTemplateService : DelfusServiceBase, IDelfusTemplateService
    {
        public async Task<KeyValuePair<bool, string>> SaveTemplate(DelfusTemplate delfusTemplate)
        {
            try
            {
                string query = @"INSERT INTO Templates (FileName ,FilePath ,ModifiedBy ,ModifiedOn ,Status, Reason)
                    VALUES 
                  (@FileName ,@FilePath ,@ModifiedBy ,@ModifiedOn ,@Status, @Reason)";
                await db.ExecuteAsync(query, delfusTemplate);
                return new KeyValuePair<bool, string>(true, "File uploaded successfully.");
            }
            catch (Exception ex)
            {
                return new KeyValuePair<bool, string>(false, ex.Message);
            }
        }
        public async Task<IEnumerable<DelfusTemplate>> DelfusTemplatesByFileName(string fileName)
        {
            return await db.QueryAsync<DelfusTemplate>(@"select * from Templates
                    where FilePath=@FileName order by ModifiedOn desc", new { FileName = fileName });
        }

        public async Task<IEnumerable<string>> GetDelfusMergeList()
        {
            return await db.QueryAsync<string>("SELECT TemplateFieldMapName FROM [Schema].[Request] WHERE RecordType IN ('L','C') AND TemplateFieldMapName IS NOT NULL");

        }
        public async Task<IEnumerable<DelfusTemplate>> DelfusTemplateSummary()
        {
            return await db.QueryAsync<DelfusTemplate>(@"
            with cte as
(
	select max(Id) as Id from Templates
	group by FileName
)

         select * from Templates  
			where Id in (select Id from cte)
            order by ModifiedOn desc");
        }

        public async Task<IEnumerable<DelfusErrorReport>> GetTaxCommercialErrorData(DateTime startDate, DateTime endDate)
        {
            return await db.QueryAsync<DelfusErrorReport>(@"SELECT l.RD_CUST_ID, RD_LOAN_ID, l.DocumentType,l.PdfFileName, l.DisqualifyReason, b.DownloadedOn, l.RD_BRRW_FRST_NM + ' '+l.RD_BRRW_LAST_NM as [Recipient Name], l.RD_SITUS_STR_NM, l.RD_SITUS_CITY_NM,l.RD_SITUS_STATE_ABBR,l.RD_SITUS_ZIP_CD from dls.LienData l
INNER JOIN dls.Batches b
ON l.BatchId = b.Id
WHERE l.RecordStatus = 'Disqualified' AND b.DownloadedOn BETWEEN @startDate AND @endDate", new { startDate = startDate, endDate = endDate });
        }

        public async Task<IEnumerable<DelfusEdeliveryReport>> GetTaxCommercialEDeliveryData(DateTime startDate, DateTime endDate)
        {
            return await db.QueryAsync<DelfusEdeliveryReport>(@"SELECT B.BatchName, L.RD_LOAN_ID, L.RecordStatus, RD_BRRW_FRST_NM,L.RD_BRRW_LAST_NM,L.RD_BRRW_ADDR1_TXT,L.RD_BRRW_CITY_NM,L.RD_BRRW_STATE_ABBR,L.RD_BRRW_ZIP_CD, L.RD_DLVR_EMAIL_ADDR_TXT,L.DocumentType, B.DownloadedOn,L.EDeliveryStatusDate,L.EDeliveryStatus,L.PdfFileName, L.IsEDeliveryRecord, L.Disqualifyreason,  L.SubmittedDate, L.RD_CUST_ID, L.RD_TMPLT_CD FROM dls.LienData L
INNER JOIN dls.Batches B ON L.BatchId=B.Id
WHERE B.FileType='PdfOnly' AND DownloadedOn BETWEEN @startDate AND @endDate", new { startDate = startDate, endDate = endDate });
        }
    }
}
