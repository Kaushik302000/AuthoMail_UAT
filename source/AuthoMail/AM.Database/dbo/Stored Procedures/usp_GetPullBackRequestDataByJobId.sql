CREATE PROCEDURE [dbo].[usp_GetPullBackRequestDataByJobId]
@jobId int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	
	BEGIN TRAN
	--Pulled back logic
	UPDATE [PnS].[LogBatchTimeLine]
	SET
	[PCIBatchPrepEnd]=CAST('1753-1-1' AS DATETIME)
	WHERE BatchId=@jobId

	DECLARE @PnSBatchUniqueId UNIQUEIDENTIFIER,@AMMailBatchId INT

	SET @PnSBatchUniqueId=(SELECT BatchIdentifier FROM PnS.BatchSummary WHERE PrintBatch=@jobId)
	
	SET @AMMailBatchId = (SELECT MailBatchId FROM AM.PrintBatches WHERE UniqueId=@PnSBatchUniqueId)
	
	SELECT * FROM [AM].[PullBackRequests] 
	WHERE MailPieceUniqueId IN (SELECT UniqueId FROM AM.MailPieces WHERE MailBatchId=@AMMailBatchId)
	

	COMMIT TRAN

END
