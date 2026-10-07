


CREATE PROCEDURE [dbo].[usp_GetMailBatchReceivedOnDataBasedFromPrintBatchId]
@pnsPrintBatchId int
AS
BEGIN
 -- SET NOCOUNT ON added to prevent extra result sets from
 -- interfering with SELECT statements.
 SET NOCOUNT ON;
 
 SELECT P.Id AS MailPieceId,P.EffectiveReceivedOnEst,P.UniqueId as UniqueId,m.PDFFileName as MailPieceFileName FROM AM.MailPieces P
 INNER JOIN [PnS].MRDFRequest M ON P.UniqueId=M.UniqueId
 WHERE M.JobId=@pnsPrintBatchId AND M.MailPieceStatus='InProgress'
 
END












































