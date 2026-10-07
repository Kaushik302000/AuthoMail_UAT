
CREATE PROCEDURE [dbo].[usp_UpdateAMMailBatchesStatus] 
	-- Add the parameters for the stored procedure here
	@MailPieceUniqueId uniqueidentifier,
	@shippingResponseReceievedOn datetime
AS
BEGIN
	DECLARE @mailBatchId int
	DECLARE @totalMailPieces int
	DECLARE @totalErrorMailPieces int
	DECLARE @totalPulledBackMailPieces int
	DECLARE @printBatchId int
	DECLARE @totalPulledBackPrintBatchesMailPieces int
	DECLARE @totalPrintBatchesMailPieces int
	DECLARE @batchSummaryId int
	DECLARE @printbatchUniqueId uniqueidentifier
	DECLARE @totalBatchSummaryMailPieces int
	DECLARE @totalPulledBackBatchSummaryMailPieces int

	SELECT @mailBatchId = MailBatchId,@printBatchId = PrintBatchId FROM AM.MailPieces WHERE UniqueId = @MailPieceUniqueId
	
	SELECT @printbatchUniqueId = UniqueId FROM AM.PrintBatches WHERE Id = @printBatchId

	SELECT @batchSummaryId = PrintBatch FROM PnS.BatchSummary WHERE BatchIdentifier = @printbatchUniqueId

	SELECT @totalMailPieces = COUNT(*) FROM AM.MailPieces WHERE MailBatchId = @mailBatchId

	SELECT @totalErrorMailPieces = COUNT(*) FROM AM.MailPieces WHERE MailBatchId = @mailBatchId AND ShippingStatus='Error'

	--Print @totalMailPieces

	SELECT @totalPulledBackMailPieces = COUNT(*) FROM AM.MailPieces WHERE MailBatchId = @mailBatchId AND IsPulledBack = 1

	SELECT @totalPulledBackPrintBatchesMailPieces = COUNT(*) FROM AM.MailPieces WHERE PrintBatchId = @printBatchId AND IsPulledBack = 1
	
	SELECT @totalPrintBatchesMailPieces = COUNT(*) FROM AM.MailPieces WHERE PrintBatchId = @printBatchId

	SELECT @totalBatchSummaryMailPieces = COUNT(*) FROM PnS.MRDFRequest WHERE JobId = @batchSummaryId

	SELECT @totalPulledBackBatchSummaryMailPieces = COUNT(*) FROM PnS.MRDFRequest WHERE JobId = @batchSummaryId AND MailPieceStatus = 'PulledBack'
	
	
	IF (@totalBatchSummaryMailPieces = @totalPulledBackBatchSummaryMailPieces and @totalBatchSummaryMailPieces <> 0)
	BEGIN	
		UPDATE PnS.BatchSummary SET BatchStatus = 'PulledBack',ReportPreppedOn = @shippingResponseReceievedOn,ReportedBackOn = @shippingResponseReceievedOn,PipelineStatus = 'ReportedBack' WHERE PrintBatch = @batchSummaryId
	END
		
	IF (@totalPrintBatchesMailPieces = @totalPulledBackPrintBatchesMailPieces)
	BEGIN
		UPDATE AM.PrintBatches SET BatchStatus = 'PulledBack',ShippingResponseReceivedOn = @shippingResponseReceievedOn,ReportPreppedOn = @shippingResponseReceievedOn,ReportedBackOn = @shippingResponseReceievedOn WHERE Id = @printBatchId	
	END
	
	IF(@totalMailPieces= @totalPulledBackMailPieces+@totalErrorMailPieces)
	BEGIN
		UPDATE AM.MailBatches SET BatchStatus = 'Failed',ShippingResponseReceivedOn = @shippingResponseReceievedOn,PipelineStatus = 'ShippingResponseReceived' WHERE Id = @mailBatchId		
	END
	
	IF (@totalMailPieces = @totalPulledBackMailPieces)
	BEGIN	
		UPDATE AM.MailBatches SET BatchStatus = 'PulledBack',ShippingResponseReceivedOn = @shippingResponseReceievedOn,PipelineStatus = 'ShippingResponseReceived' WHERE Id = @mailBatchId		
	END

END

