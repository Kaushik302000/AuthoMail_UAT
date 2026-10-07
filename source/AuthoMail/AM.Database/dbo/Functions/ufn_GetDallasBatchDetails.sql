CREATE FUNCTION [dbo].[ufn_GetDallasBatchDetails]
(
@EstDate DATE
)
RETURNS TABLE 
AS
RETURN

SELECT m.Id,m.BusinessFriendlyIdValue AS LoanNumber,m.ClientId, m.PrintLocation, m.EffectiveReceivedOnEst, m.TrackingNo,m.IsPullBackRequested,
m.IsQCRequested,m.TotalSheets, m.DueDate,m.TotalPages,m.ShippingStatus,
b.PrintBatch,b.IsSubmitted,b.PipelineStatus,preq.PrintBatchName,b.JobName,preq.SubmittedOn,
req.PieceId,req.PrintTrailBatchId, req.JobId,req.[Date] AS MRDFReqDate,req.PDFFileName, req.UniqueId,req.MailPieceStatus, 
res.ProcessingTimeStamp,
PCD.ShippingServiceType,PCD.ShippingCarrier
,CASE WHEN m.Inserts IS NOT NULL AND LEN(RTRIM(LTRIM(m.Inserts)))>0 THEN CAST(1 AS BIT) WHEN d.IsBRE=1 THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END AS IsBRE,
c.AppCode,c.Name AS ClientName
,(SELECT BatchName FROM [dbo].[ufn_GetDallasBatches](@EstDate) WHERE EffectiveReceivedOnEst between StartDate AND EndDate AND ShippingCarrier=PCD.ShippingCarrier AND ShippingServiceType=PCD.ShippingServiceType  AND AppCode=c.AppCode) BatchName,
req.FilePath,m.Inserts,E.EnvelopeName
FROM [AM].[MailPieces] m
LEFT OUTER JOIN PnS.MRDFRequest req ON m.UniqueId = req.UniqueId
LEFT OUTER JOIN PnS.BatchSummary b ON req.JobId = b.PrintBatch
LEFT OUTER JOIN PnS.PrintTrailBatches preq ON req.PrintTrailBatchId=preq.Id
LEFT OUTER JOIN PnS.MRDFResponse res ON req.PrintTrailBatchId = res.JobId AND req.PieceId=res.PieceId
INNER JOIN [AM].DocTypes d ON m.DocTypeId=d.Id
INNER JOIN Common.PnSConfigDetail PCD ON m.PnSConfigDetailId=PCD.Id
INNER JOIN Common.Clients c ON m.ClientId=c.Id
INNER JOIN Common.Envelopes E ON PCD.EnvelopeId=E.Id
WHERE m.PrintLocation='Dallas CL'
AND m.EffectiveReceivedOnEst >=(SELECT MIN(StartDate) FROM [dbo].[ufn_GetDallasBatches](@EstDate))