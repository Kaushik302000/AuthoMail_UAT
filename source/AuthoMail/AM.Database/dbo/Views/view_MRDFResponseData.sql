CREATE VIEW [dbo].[view_MRDFResponseData] AS
SELECT REQ.JobId
,REQ.PieceId
,ISNULL(REQ.ErrId,0) AS ErrId 
,REQ.ErrDesc
,RES.ProcessingTimeStamp AS ShippedOn 
,REQ.UniqueId
,REQ.PDFFileName
,REQ.MailPieceStatus
,ISNULL(RES.TotalSheets,0) AS TotalSheets
,REQ.DivertVS3
,RES.Disposition 
,ISNULL(RES.WeightMeasured,0.00) AS WeightMeasured 
,ISNULL(RES.PostageMetered,0.00) AS PostageCost 
,RES.PackageTrackingNumber 
FROM [PnS].MRDFRequest REQ
LEFT JOIN [PnS].MRDFResponse RES
ON REQ.PrintTrailBatchId =RES.JobId AND REQ.PieceId =RES.PieceId

