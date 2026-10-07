 CREATE PROCEDURE [dbo].[usp_GetFinalMRDFResponseDataByJobId]
@jobId int
AS
BEGIN
 -- SET NOCOUNT ON added to prevent extra result sets from
 -- interfering with SELECT statements.
 SET NOCOUNT ON;
 
 SELECT JobId, PostageCost, ErrId, case when ShippedOn is null and errid = -4 then CAST('1753-1-1' AS DATETIME) else ShippedOn end as ShippedOn, WeightMeasured,case when errid = -4 then '4' else Disposition end as Disposition
 , PieceId, UniqueId, PDFFileName, ErrDesc, MailPieceStatus,TotalSheets,DivertVS3, PackageTrackingNumber 
 FROM view_MRDFResponseData V WHERE V.JobId=@jobId --AND ErrId <> -4
END
