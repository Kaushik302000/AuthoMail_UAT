using AM.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using AM.Lib;

namespace AM.Services
{
    public interface IDocTypeService : IDisposable
    {
        DocType GetDocTypeByDocTypeName(int clientId, string documentOrPackageName);

        DocType GetDocTypeByDocTypeName(int clientId, string documentOrPackageName, int pageCount);

        DocType GetDocTypeByDocTypeName(int clientId, string documentOrPackageName, int pageCount, DateTime estReceivedOn);
        DocType GetIntlDocTypeByDocTypeName(int clientId, string documentOrPackageName, int pageCount, DateTime estReceivedOn);

        DocType GetDocTypeByDocTypeName(int clientId, string documentOrPackageName, int pageCount, string shippingCarrier, string shippingServiceType);
        DocType GetIntlDocTypeByDocTypeName(int clientId, string documentOrPackageName, int pageCount, string shippingCarrier, string shippingServiceType);
        DocType GetFallBackDocTypeByPnSConfigId(int clientId, int docTypeId, DateTime estReceivedOn, string shippingCarrier, string shippingServiceType, int? mpId = 0);
        DocType GetDocTypeById(int docTypeId, int pnsConfigDetailId);
        DocType GetIntlDocTypeById(int docTypeId, int pnsConfigDetailId);
        List<PnSConfigDetailCoverLetter> GetDocumentCoverLetterTemplates(int pnSConfigDetailId);
        List<PnSConfigDetailCoverLetter> GetDocumentRearLetterTemplates(int pnSConfigDetailId);
        DocType GetDocTypeWithMaximumCutOffTimeByClientId(int clientId);
        bool AllowInsertsOnlyDocumentType(int clientId, string docType);
        List<DocTypeReport> GetDocTypeList();
    }

    public class DocTypeService : ServiceBase, IDocTypeService
    {
        public DocType GetDocTypeByDocTypeName(int clientId, string documentOrPackageName)
        {
            string query = "SELECT * FROM [AM].[DocTypes] D WHERE D.DocTypeName = @DocumentOrPackageName AND D.ClientId = @ClientId AND D.IsActive = 1";

            return db.QueryFirstOrDefault<DocType>(query, new { ClientId = clientId, DocumentOrPackageName = documentOrPackageName });
        }
        public DocType GetDocTypeByDocTypeName(int clientId, string documentOrPackageName, int pageCount)
        {
            string query = "SELECT * FROM [dbo].[v_DocTypes] D WHERE D.DocTypeName = @DocumentOrPackageName AND D.ClientId = @ClientId AND (1 - D.IsPrintReady + (CASE WHEN PrintFormat='Simplex' OR PrintFormat='Simplex_Color' THEN @PageCount ELSE (@PageCount / 2 + @PageCount % 2) END)) BETWEEN D.MinPage AND D.MaxPage AND D.IsFallBack = 0 AND D.IsActive = 1";

            return db.QuerySingleOrDefault<DocType>(query, new { ClientId = clientId, DocumentOrPackageName = documentOrPackageName, PageCount = pageCount });
        }

        public DocType GetDocTypeByDocTypeName(int clientId, string documentOrPackageName, int pageCount, string shippingCarrier, string shippingServiceType)
        {
            string query = "SELECT * FROM [dbo].[v_DocTypes] D WHERE D.DocTypeName = @DocumentOrPackageName AND D.ClientId = @ClientId AND (1 - D.IsPrintReady + (CASE WHEN PrintFormat='Simplex' OR PrintFormat='Simplex_Color' THEN @PageCount ELSE (@PageCount / 2 + @PageCount % 2) END)) BETWEEN D.MinPage AND D.MaxPage AND D.IsActive = 1 AND D.ShippingCarrier=@ShippingCarrier AND D.ShippingServiceType=@ShippingServiceType".Fill(pageCount);

            return db.QuerySingleOrDefault<DocType>(query, new { ClientId = clientId, DocumentOrPackageName = documentOrPackageName, PageCount = pageCount, ShippingCarrier = shippingCarrier, ShippingServiceType = shippingServiceType });
        }
        public DocType GetIntlDocTypeByDocTypeName(int clientId, string documentOrPackageName, int pageCount, string shippingCarrier, string shippingServiceType)
        {
            string query = "SELECT * FROM [dbo].[v_IntlDocTypes] D WHERE D.DocTypeName = @DocumentOrPackageName AND D.ClientId = @ClientId AND (1 - D.IsPrintReady + (CASE WHEN PrintFormat='Simplex' OR PrintFormat='Simplex_Color' THEN @PageCount ELSE (@PageCount / 2 + @PageCount % 2) END)) BETWEEN D.MinPage AND D.MaxPage AND D.IsActive = 1 AND D.ShippingCarrier=@ShippingCarrier AND D.ShippingServiceType=@ShippingServiceType".Fill(pageCount);

            return db.QuerySingleOrDefault<DocType>(query, new { ClientId = clientId, DocumentOrPackageName = documentOrPackageName, PageCount = pageCount, ShippingCarrier = shippingCarrier, ShippingServiceType = shippingServiceType });
        }
        public DocType GetFallBackDocTypeByPnSConfigId(int clientId, int docTypeId, DateTime estReceivedOn, string shippingCarrier, string shippingServiceType, int? mpId = 0)
        {
            using (ICommonService commonSvc = new CommonService())
            {
                string query = "SELECT D.* FROM [dbo].[v_DocTypes] D WHERE D.ClientId=@ClientId AND D.Id=@DocTypeId AND D.IsActive=1 AND D.IsFallBack=1 AND D.ShippingCarrier=@ShippingCarrier AND D.ShippingServiceType=@ShippingServiceType";

                var docTypeList = db.Query<DocType>(query, new { ClientId = clientId, DocTypeId = docTypeId, ShippingCarrier = shippingCarrier, ShippingServiceType = shippingServiceType }).ToList();

                if (docTypeList.Count == 0)
                    return null;

                var lstDocType = GetDocTypeByDocTypeName(clientId, docTypeList, estReceivedOn);

                if (lstDocType.Count == 1)
                    return lstDocType.SingleOrDefault();

                if (lstDocType.Count > 1)
                {
                    if (lstDocType.Any(x => x.PrintVendorCode == "CL_DALLAS") && commonSvc.GetDallasBatchCounts(mpId) <= Config.Common_DallasVolumeCap)
                    {
                        return lstDocType.SingleOrDefault(d => d.PrintVendorCode == "CL_DALLAS");
                    }

                    return lstDocType.SingleOrDefault(d => d.PrintVendorCode != "CL_DALLAS");
                }
                return null;
            }
        }

        public DocType GetDocTypeById(int docTypeId, int pnsConfigDetailId)
        {
            string query = "SELECT D.* FROM [dbo].[v_DocTypes] D WHERE D.Id=@DocTypeId AND D.PnSConfigDetailId=@PnSConfigDetailId AND D.IsActive=1";

            return db.QuerySingleOrDefault<DocType>(query, new { DocTypeId = docTypeId, PnsConfigDetailId = pnsConfigDetailId });
        }
        public DocType GetIntlDocTypeById(int docTypeId, int pnsConfigDetailId)
        {
            string query = "SELECT D.* FROM [dbo].[v_IntlDocTypes] D WHERE D.Id=@DocTypeId AND D.PnSConfigDetailId=@PnSConfigDetailId AND D.IsActive=1";

            return db.QuerySingleOrDefault<DocType>(query, new { DocTypeId = docTypeId, PnsConfigDetailId = pnsConfigDetailId });
        }
        public List<PnSConfigDetailCoverLetter> GetDocumentCoverLetterTemplates(int pnSConfigDetailId)
        {
            string query = "SELECT * FROM [Common].[PnSConfigDetailCoverLetters] WHERE PnSConfigDetailId=@PnSConfigDetailId AND SeqId<=100 ORDER BY SeqId";

            return db.Query<PnSConfigDetailCoverLetter>(query, new { PnSConfigDetailId = pnSConfigDetailId }).ToList();
        }

        public List<PnSConfigDetailCoverLetter> GetDocumentRearLetterTemplates(int pnSConfigDetailId)
        {
            string query = "SELECT * FROM [Common].[PnSConfigDetailCoverLetters] WHERE PnSConfigDetailId=@PnSConfigDetailId AND SeqId > 100 ORDER BY SeqId";

            return db.Query<PnSConfigDetailCoverLetter>(query, new { PnSConfigDetailId = pnSConfigDetailId }).ToList();
        }

        public DocType GetDocTypeWithMaximumCutOffTimeByClientId(int clientId)
        {
            string query = "SELECT TOP 1 D.* FROM [dbo].[v_DocTypes] D WHERE BizdayCutOff=(SELECT MAX(CAST(BizdayCutOff as time)) as maxCutOff FROM [dbo].[v_DocTypes] WHERE ClientId=@ClientId AND IsActive=1) AND ClientId=@ClientId AND IsActive=1";

            return db.QuerySingleOrDefault<DocType>(query, new { ClientId = clientId });
        }

        public DocType GetDocTypeByDocTypeName(int clientId, string documentOrPackageName, int pageCount, DateTime estReceivedOn)
        {
            using (ICommonService commonSvc = new CommonService())
            {
                string query = "SELECT * FROM [dbo].[v_DocTypes] D WHERE D.DocTypeName = @DocumentOrPackageName AND D.ClientId = @ClientId AND (1 - D.IsPrintReady + (CASE WHEN PrintFormat='Simplex' OR PrintFormat='Simplex_Color' THEN @PageCount ELSE (@PageCount / 2 + @PageCount % 2) END)) BETWEEN D.MinPage AND D.MaxPage AND D.IsFallBack = 0 AND D.IsActive = 1";

                var docTypeList = db.Query<DocType>(query, new { ClientId = clientId, DocumentOrPackageName = documentOrPackageName, PageCount = pageCount }).ToList();

                if (docTypeList.Count == 0)
                    return null;

                var lstDocType = GetDocTypeByDocTypeName(clientId, docTypeList, estReceivedOn);

                if (lstDocType.Count == 1)
                    return lstDocType.SingleOrDefault();

                if (lstDocType.Count > 1)
                {
                    if (lstDocType.Any(x => x.PrintVendorCode == "CL_DALLAS") && commonSvc.GetDallasBatchCounts() < Config.Common_DallasVolumeCap)
                    {
                        return lstDocType.SingleOrDefault(d => d.PrintVendorCode == "CL_DALLAS");
                    }

                    return lstDocType.SingleOrDefault(d => d.PrintVendorCode != "CL_DALLAS");
                }

                return null;
            }
        }
        public DocType GetIntlDocTypeByDocTypeName(int clientId, string documentOrPackageName, int pageCount, DateTime estReceivedOn)
        {
            using (ICommonService commonSvc = new CommonService())
            {
                string query = "SELECT * FROM [dbo].[v_IntlDocTypes] D WHERE D.DocTypeName = @DocumentOrPackageName AND D.ClientId = @ClientId AND (1 - D.IsPrintReady + (CASE WHEN PrintFormat='Simplex' OR PrintFormat='Simplex_Color' THEN @PageCount ELSE (@PageCount / 2 + @PageCount % 2) END)) BETWEEN D.MinPage AND D.MaxPage AND D.IsFallBack = 0 AND D.IsActive = 1";

                var docTypeList = db.Query<DocType>(query, new { ClientId = clientId, DocumentOrPackageName = documentOrPackageName, PageCount = pageCount }).ToList();

                if (docTypeList.Count == 0)
                    return null;

                var lstDocType = GetDocTypeByDocTypeName(clientId, docTypeList, estReceivedOn);

                if (lstDocType.Count == 1)
                    return lstDocType.SingleOrDefault();

                if (lstDocType.Count > 1)
                {
                    if (lstDocType.Any(x => x.PrintVendorCode == "CL_DALLAS") && commonSvc.GetDallasBatchCounts() < Config.Common_DallasVolumeCap)
                    {
                        return lstDocType.SingleOrDefault(d => d.PrintVendorCode == "CL_DALLAS");
                    }

                    return lstDocType.SingleOrDefault(d => d.PrintVendorCode != "CL_DALLAS");
                }

                return null;
            }
        }
        private List<DocType> GetDocTypeByDocTypeName(int clientId, List<DocType> docTypeList, DateTime estReceivedOn)
        {
            var estReceivedOnDate = estReceivedOn.Date;
            bool isSatWorkDay = true;
            bool isSunWorkDay = true;

            DayOfWeek dayOfWeek = estReceivedOn.DayOfWeek;
            string slaCutOff = string.Empty;
            string slaType = string.Empty;
            string maxDocTypeCutOffTime = "";

            if (dayOfWeek == DayOfWeek.Sunday)
            {
                maxDocTypeCutOffTime = docTypeList.OrderByDescending(dt => dt.SunCutOff).First().SunCutOff;

                if (maxDocTypeCutOffTime.IsNullOrWhiteSpace())
                {
                    estReceivedOn = estReceivedOn.Date.AddDays(1);
                    return GetDocTypeByDocTypeName(clientId, docTypeList, estReceivedOn);
                }
            }
            else if (dayOfWeek == DayOfWeek.Saturday)
            {
                maxDocTypeCutOffTime = docTypeList.OrderByDescending(dt => dt.SatCutOff).First().SatCutOff;
                if (maxDocTypeCutOffTime.IsNullOrWhiteSpace())
                {
                    estReceivedOn = estReceivedOn.Date.AddDays(1);
                    return GetDocTypeByDocTypeName(clientId, docTypeList, estReceivedOn);
                }
            }
            else
                maxDocTypeCutOffTime = docTypeList.OrderByDescending(dt => dt.BizdayCutOff).First().BizdayCutOff;

            var maxCutOffDateTimeEst = estReceivedOnDate + TimeSpan.Parse(maxDocTypeCutOffTime);

            using (ICommonService commonSvc = new CommonService())
            {
                if (!commonSvc.IsBusinessDay(estReceivedOnDate, Config.GetHolidaysByClientId(clientId), isSatWorkDay, isSunWorkDay) || (estReceivedOn > maxCutOffDateTimeEst))
                {
                    estReceivedOn = commonSvc.GetNextWorkingDay(estReceivedOnDate, Config.GetHolidaysByClientId(clientId), isSatWorkDay, isSunWorkDay);

                    //Recalculate same function for updated document type with new effective received date;
                    return GetDocTypeByDocTypeName(clientId, docTypeList, estReceivedOn);
                }

                estReceivedOnDate = estReceivedOn.Date;

                dayOfWeek = estReceivedOn.DayOfWeek;

                List<DocType> maxCutOffDocType = new List<DocType>();

                if (dayOfWeek == DayOfWeek.Sunday)
                    maxCutOffDocType = docTypeList.Where(dt => estReceivedOn >= estReceivedOnDate + dt.SunStartTime && estReceivedOn <= estReceivedOnDate + dt.SunEndTime).ToList();
                else if (dayOfWeek == DayOfWeek.Saturday)
                    maxCutOffDocType = docTypeList.Where(dt => estReceivedOn >= estReceivedOnDate + dt.SatStartTime && estReceivedOn <= estReceivedOnDate + dt.SatEndTime).ToList();
                else
                    maxCutOffDocType = docTypeList.Where(dt => estReceivedOn >= estReceivedOnDate + dt.BizdayStartTime && estReceivedOn <= estReceivedOnDate + dt.BizdayEndTime).ToList();

                return maxCutOffDocType;
            }
        }

        public bool AllowInsertsOnlyDocumentType(int clientId, string docType)
        {
            string query = "SELECT AllowInsertsOnly FROM AM.DocTypes WHERE ClientId=@clientId AND IsActive=1 AND DocTypeName=@docType";

            return db.QuerySingleOrDefault<bool>(query, new { clientId, docType });
        }

        public List<DocTypeReport> GetDocTypeList()
        {
            string query = @"SELECT D.*,C.AppCode,E.EnvelopeName,E.PartNumber AS EnvelopePartNumber, BREName =STUFF((SELECT DISTINCT '|'+ CAST([EnvelopeName] AS VARCHAR(MAX)) FROM Common.Envelopes E INNER JOIN AM.DocTypeInserts DI ON DI.BREEnvelopeId=E.Id WHERE DI.DocTypeId = D.Id FOR XMl PATH('')),1,1,''), BREPartNumber =STUFF((SELECT DISTINCT '|'+ CAST([PartNumber] AS VARCHAR(MAX)) FROM Common.Envelopes E INNER JOIN AM.DocTypeInserts DI ON DI.BREEnvelopeId=E.Id WHERE DI.DocTypeId = D.Id FOR XMl PATH('')),1,1,''),
                PT.TrailName AS PrintTrailName, PV.VendorCode AS PrintVendor
                FROM [dbo].[v_DocTypes] D 
                LEFT OUTER JOIN Common.Clients C ON D.ClientId=C.Id
                LEFT OUTER JOIN Common.Envelopes E ON D.EnvelopeId=E.Id
                LEFT OUTER JOIN Common.PrintTrails PT ON D.PrintTrailId=PT.Id
                LEFT OUTER JOIN Common.PrintVendors PV ON PT.PrintVendorId=PV.Id
                WHERE C.Id>0 ORDER BY C.AppCode";
            return db.Query<DocTypeReport>(query).ToList();
        }
    }
}
