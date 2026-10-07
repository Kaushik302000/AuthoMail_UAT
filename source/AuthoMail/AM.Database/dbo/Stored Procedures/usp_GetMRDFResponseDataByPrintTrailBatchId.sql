
CREATE PROCEDURE [dbo].[usp_GetMRDFResponseDataByPrintTrailBatchId]
@printTrailBatchId int
AS
BEGIN
 -- SET NOCOUNT ON added to prevent extra result sets from
 -- interfering with SELECT statements.
 SET NOCOUNT ON;
 
 SELECT Res.JobId, Res.PieceId, Res.Date, Res.Feeder2_Fed, Res.OperatorId, Res.MachineId, Res.ProcessingTimeStamp, Res.DetailedErrorId
 , Res.Disposition, Res.Input_Fed, Res.Feeder3_Fed, Res.Feeder4_Fed, Res.Feeder5_Fed, Res.ComputedWeight, Res.MeterDate
 , Res.StatusSource, Res.PostageMetered, Res.WeightMeasured, Res.ExitLocation, Res.TotalSheets, Res.PDFFileName, Res.PackageTrackingNumber
 ,MP.Id as MailPieceId 
 FROM [PnS].[MRDFResponse] Res
 LEFT OUTER JOIN PnS.MRDFRequest Req ON Res.PieceId=Req.PieceId AND Res.JobId=Req.PrintTrailBatchId
 LEFT OUTER JOIN AM.MailPieces MP ON MP.UniqueId=Req.UniqueId
 WHERE  Req.PrintTrailBatchId=@printTrailBatchId
END

