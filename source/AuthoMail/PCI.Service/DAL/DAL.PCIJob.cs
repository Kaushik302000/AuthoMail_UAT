using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.EnterpriseLibrary.Data;

namespace PCI.Service
{
    partial class DAL
    {
        public static List<SubmittedBatches> GetSubmittedBatches(string serverIP, string printVendorCode = "")
        {
            var submittedBatches = db.ExecuteDataSet("usp_GetSubmittedBatches", serverIP, printVendorCode).Tables[0];
            List<SubmittedBatches> printBatches = submittedBatches.AsEnumerable().Select(x => new SubmittedBatches
            {
                PrintTrailBatchId = x.Field<int>("PrintTrailBatchId"),
                PrintBatchName = x.Field<string>("PrintBatchName"),
                PrintVendorCode = x.Field<string>("PrintVendorCode"),
                ReceivedFileList = new List<string>()
            }).ToList();
            return printBatches;
        }

        //public static List<SubmittedBatches> GetMRDFReceivedBatches()
        //{
        //    var submittedBatches = db.ExecuteDataSet("usp_GetMRDFReceivedBatches").Tables[0];
        //    List<SubmittedBatches> printBatches = submittedBatches.AsEnumerable().Select(x => new SubmittedBatches
        //    {
        //        PrintTrailBatchId = x.Field<int>("PrintBatch"),
        //        PrintBatchName = "{0}-{1}-{2}-{3}-".Fill(x.Field<string>("CustCode"), x.Field<string>("BatchCode"), x.Field<string>("AppCode"), x.Field<string>("DocTypeCode")),
        //        ReceivedFileList = new List<string>()
        //    }).ToList();
        //    return printBatches;
        //}

        //public static List<MailPieceDetail> GetMailBatchReceivedOnDataBasedFromPrintBatchId(int pnsPrintBatchId)
        //{
        //    var amMailPiecesData = db.ExecuteDataSet("usp_GetMailBatchReceivedOnDataBasedFromPrintBatchId", pnsPrintBatchId).Tables[0];
        //    List<MailPieceDetail> amMailPieces = amMailPiecesData.AsEnumerable().Select(x => new MailPieceDetail
        //    {

        //        MailPieceId = x.Field<int>("MailPieceId"),
        //        EffectiveReceivedOnEst = x.Field<DateTime>("EffectiveReceivedOnEst"),
        //        UniqueId = x.Field<Guid>("UniqueId")
        //    }).ToList();

        //    return amMailPieces;
        //}

        public static List<MailPieceDetail> GetMRDFRequestDataByPrintTrailId(int printTrailId, string serverIP)
        {
            var data = db.ExecuteDataSet("usp_GetMRDFRequestDataByPrintTrailId", printTrailId, serverIP).Tables[0];

            List<MailPieceDetail> mrdfReqs = new List<MailPieceDetail>();
            foreach (DataRow row in data.Rows)
            {
                mrdfReqs.Add(new MailPieceDetail()
                {
                    ClientId = row.Field<int>("ClientId"),
                    AppCode = row.Field<string>("AppCode"),
                    Alias = row.Field<string>("Alias"),
                    DocTypeId = row.Field<int>("DocTypeId"),
                    Address1 = row.Field<string>("Address1"),
                    Date = row.Field<DateTime>("Date"),
                    DivertVS3 = int.Parse(row.Field<string>("DivertVS3")),
                    ErrDesc = row.Field<string>("ErrDesc"),
                    ErrId = row.Field<int>("ErrId"),
                    Expansion = row.Field<string>("Expansion"),
                    JobId = row.Field<int>("JobId"),
                    MailPieceStatus = row.Field<string>("MailPieceStatus"),
                    PDFFileName = row.Field<string>("PDFFileName"),
                    PieceId = row.Field<int>("PieceId"),
                    TotalPages = row.Field<int>("TotalPages"),
                    TotalSheets = row.Field<int>("TotalSheets"),
                    Feeder4_Sel = int.Parse(row.Field<string>("Feeder4_Sel")),
                    UniqueId = row.Field<Guid>("UniqueId"),
                    MailPieceId = row.Field<int?>("MailPieceId"),
                    EffectiveReceivedOnEst = row.Field<DateTime?>("EffectiveReceivedOnEst"),
                    ShippingCarrier = row.Field<string>("ShippingCarrier"),
                    ShippingServiceType = row.Field<string>("ShippingServiceType"),
                    MailPieceDueDate = row.Field<DateTime>("MailPieceDueDate"),
                    EnvelopeId = row.Field<int?>("EnvelopeId"),
                    CustomerId = row.Field<string>("CustomerId"),
                    IsForeignAddress = row.Field<bool>("IsForeignAddress"),
                    ShipperId = row.Field<int>("ShipperId"),
                    Inserts = row.Field<string>("Inserts"),
                    InsertsPartNos = row.Field<string>("InsertsPartNos"),
                    //Extra Properties to map for MRDF Request
                    InputWeight = row.Field<string>("InputWeight"),
                    Feeder3_Sel = row.Field<string>("Feeder3_Sel"),
                    Feeder5_Sel = row.Field<string>("Feeder5_Sel"),
                    LowCap_Sel = row.Field<string>("LowCap_Sel"),
                    NoSeal_Sel = row.Field<string>("NoSeal_Sel"),
                    Feeder2_Sel = row.Field<string>("Feeder2_Sel"),
                    ZipMark_Sel = row.Field<string>("ZipMark_Sel"),
                    Address2 = row.Field<string>("Address2"),
                    Zip5 = row.Field<string>("Zip5"),
                    Zip4 = row.Field<string>("Zip4"),
                    Zip2 = row.Field<string>("Zip2"),
                    DivertVS2 = row.Field<string>("DivertVS2"),
                    DivertVS4 = row.Field<string>("DivertVS4"),
                    ReturnLogoBitMap = row.Field<string>("ReturnLogoBitMap"),
                    ReturnLine1 = row.Field<string>("ReturnLine1"),
                    ReturnLine2 = row.Field<string>("ReturnLine2"),
                    ReturnLine3 = row.Field<string>("ReturnLine3"),
                    ReturnLine4 = row.Field<string>("ReturnLine4"),
                    ReturnLine5 = row.Field<string>("ReturnLine5"),
                    ReturnLine6 = row.Field<string>("ReturnLine6"),
                    GroupNumber = row.Field<string>("GroupNumber"),
                    SocialSecNumber = row.Field<string>("SocialSecNumber"),
                    CheckAmount = row.Field<string>("CheckAmount"),
                    PassThru = row.Field<string>("PassThru"),
                    EndofRecord = row.Field<string>("EndofRecord"),
                    ChecksPageNumbers = row.Field<string>("ChecksPageNumbers"),
                    IsChecks = row.Field<bool>("IsChecks"),
                });
            }

            return mrdfReqs;
        }

        public static int InsertPrintTrailBatchDetail(PrintTrailBatch printTrailBatchDetail)
        {
            var printTrailBatchId = db.ExecuteSqlScalar<int>(@"INSERT INTO [PnS].[PrintTrailBatches]([PrintBatchName],[ShipperId],[SubmittedOn],[MRDFReceivedOn],[PrintTrailId],[ServerIP]) OUTPUT INSERTED.Id VALUES(@p1,@p2,@p3,@p4,@p5,@p6)", printTrailBatchDetail.PrintBatchName, printTrailBatchDetail.ShipperId, printTrailBatchDetail.SubmittedOn, printTrailBatchDetail.MRDFReceivedOn, printTrailBatchDetail.PrintTrailId, printTrailBatchDetail.ServerIP);

            return printTrailBatchId;
        }

        public static int UpdatedPrintTrailBatchIdInMRDFRequest(int printTrailBatchId, List<MailPieceDetail> mailPieceDetails)
        {
            foreach (var mailPiece in mailPieceDetails)
            {
                db.ExecuteSqlNonQuery(@"UPDATE [PnS].[MRDFRequest] SET [PrintTrailBatchId]= @p1,[PieceId] = @p2 WHERE JobId = @p3 AND PieceId = @p4", mailPiece.PrintTrailBatchId, mailPiece.SequenceNo, mailPiece.JobId, mailPiece.PieceId);
            }

            //For divert primary key issue we are using starting number as 90001 and then we will change that numbering again
            db.ExecuteSqlNonQuery(@"UPDATE [PnS].[MRDFRequest] SET [PieceId] = ([PieceId]-90000)  WHERE PrintTrailBatchId = @p1", printTrailBatchId);

            //Here we are updating address fields with only sequence number and jobid in address1 field.
            return db.ExecuteSqlNonQuery(@"UPDATE [PnS].[MRDFRequest] SET Address1 = REPLACE(REPLACE('Job: {0} Seq: {1}','{0}',PrintTrailBatchId ), '{1}' ,PieceId) WHERE PrintTrailBatchId = @p1 AND MailPieceStatus='InProgress'", printTrailBatchId);
        }

        public static int UpdatePrintTrailBatchDetail(PrintTrailBatch printTrailBatchDetail)
        {
            return db.ExecuteSqlNonQuery(@"UPDATE [PnS].[PrintTrailBatches] SET [PrintBatchName]= @p1,[SubmittedOn] = @p2,[MRDFReceivedOn] = @p3,[ValidationErrors]=@p4 WHERE Id = @p5", printTrailBatchDetail.PrintBatchName, printTrailBatchDetail.SubmittedOn, printTrailBatchDetail.MRDFReceivedOn, printTrailBatchDetail.ValidationErrors, printTrailBatchDetail.Id);
        }

        public static PrintTrailBatch GetPrintBatchDetailByPrintBatchTrailId(int printTrailBatchId)
        {
            var printTrailBatchDetail = db.ExecuteDataSet("usp_GetPrintBatchDetailByPrintBatchTrailId", printTrailBatchId).Tables[0];
            return printTrailBatchDetail.AsEnumerable().Select(pt => new PrintTrailBatch()
            {
                Id = pt.Field<int>("Id"),
                PrintBatchName = pt.Field<string>("PrintBatchName"),
                PrintTrailId = pt.Field<int>("PrintTrailId"),
                SubmittedOn = pt.Field<DateTime?>("SubmittedOn"),
                IsSubmitted = pt.Field<int>("IsSubmitted") == 1 ? true : false,
                MRDFReceivedOn = pt.Field<DateTime?>("MRDFReceivedOn"),
                IsMRDFReceived = pt.Field<int>("IsMRDFReceived") == 1 ? true : false,
                SuspendedOn = pt.Field<DateTime?>("SuspendedOn"),
                IsSuspended = pt.Field<int>("IsSuspended") == 1 ? true : false,
                ValidationErrors = pt.Field<string>("ValidationErrors")
            }).SingleOrDefault();
        }

        public static List<PrintTrailBatch> GetNotSubmittedPrintBatchDetailByPrintVendorCode(string pvCode, string serverIP)
        {
            string sqlQuery = $"SELECT PTB.*,PT.IsSinglePdf,PT.[IsManifestRequired] FROM PnS.PrintTrailBatches PTB INNER JOIN Common.PrintTrails PT ON PTB.PrintTrailId = PT.Id INNER JOIN Common.PrintVendors PV ON PT.PrintVendorId = PV.Id WHERE PTB.IsSubmitted=0 AND PT.PrintVendorCode = '{pvCode}' AND PTB.ServerIP = '{serverIP}'";
            return db.ExecuteSqlStringAccessor<PrintTrailBatch>(sqlQuery).ToList();
        }

        public static void UpdateSubmittedForAllResReceivedBatch(int ptBatchId, DateTime currentDateTime)
        {
            string sqlQuery = @"WITH CB AS (SELECT DISTINCT req.JobId FROM PnS.MRDFRequest req WHERE req.PrintTrailBatchId={0}),PTB AS (SELECT IsSubmitted FROM PnS.PrintTrailBatches WHERE Id={0})UPDATE B SET B.PipelineStatus = 'Submitted',B.SubmittedOn=@p1 FROM PnS.BatchSummary B INNER JOIN CB ON B.PrintBatch = CB.JobId WHERE (SELECT IsSubmitted FROM PTB)=0".Fill(ptBatchId);

            db.ExecuteSqlNonQuery(sqlQuery, currentDateTime);
        }

        public static int UpdateFilePathForDallas(MailPieceDetail mailPiece, string filePath)
        {
            return db.ExecuteSqlNonQuery(@"UPDATE [PnS].[MRDFRequest] SET [FilePath]= @p1 WHERE JobId = @p2 AND PieceId= @p3", filePath, mailPiece.JobId, mailPiece.PieceId);
        }

        public static int UpdateBulkFilePathForDallas(List<MailPieceDetail> mpDetails, string filePath)
        {
            var data = mpDetails.Select(x => new { JobId = x.JobId, PieceId = x.PieceId }).ToList();
            var jobGroupedData = data.GroupBy(x => x.JobId);
            foreach (var t in jobGroupedData)
            {
                db.ExecuteSqlNonQuery($@"UPDATE [PnS].[MRDFRequest] SET [FilePath]= @p1 WHERE JobId = (@p2) AND PieceId IN ({string.Join(",", t.Select(x => x.PieceId))})", filePath, t.Key);
            }
            return 1;
        }

        public static int UpdateBarcodeNumberForDallas(List<MailPieceDetail> mpDetails, string barcode)
        {
            var data = mpDetails.Select(x => new { JobId = x.JobId, PieceId = x.PieceId }).ToList();
            var jobGroupedData = data.GroupBy(x => x.JobId);
            foreach (var t in jobGroupedData)
            {
                db.ExecuteSqlNonQuery($@"UPDATE [PnS].[MRDFRequest] SET BarcodeNumber=@p1 WHERE JobId = (@p2) AND PieceId IN ({string.Join(",", t.Select(x => x.PieceId))})", barcode, t.Key);
            }
            return 1;
        }
    }
}
