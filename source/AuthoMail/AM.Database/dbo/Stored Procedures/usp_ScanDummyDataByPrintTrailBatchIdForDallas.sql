CREATE PROCEDURE [dbo].[usp_ScanDummyDataByPrintTrailBatchIdForDallas]
@PrintTrailBatchId INT
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @ShippedOn DATETIME = GETUTCDATE()

	INSERT INTO PnS.MRDFResponse (JobId, PieceId, Date, Feeder2_Fed, OperatorId, MachineId, ProcessingTimeStamp, DetailedErrorId, Disposition, Input_Fed, Feeder3_Fed, Feeder4_Fed, Feeder5_Fed, ComputedWeight, MeterDate, StatusSource, PostageMetered, WeightMeasured, ExitLocation, TotalSheets, PDFFileName, PackageTrackingNumber)
	SELECT PrintTrailBatchId, PieceId, CAST(@ShippedOn AS DATE), 0, 'Admin', 'Admin', @ShippedOn, NULL, '5', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0.0000, 0.0000, NULL, TotalSheets, PDFFileName, NULL FROM PnS.MRDFRequest WHERE PrintTrailBatchId = @PrintTrailBatchId

	UPDATE PnS.MRDFRequest
	SET
	MailPieceStatus='Shipped'
	WHERE PrintTrailBatchId=@PrintTrailBatchId


	;WITH CB AS 
	(
		SELECT B.PrintBatch
		,(SELECT COUNT(1) FROM PnS.MRDFRequest WHERE MailPieceStatus='InProgress' AND JobId=B.PrintBatch) as InProgressCount 
		FROM PnS.BatchSummary B 
		WHERE B.PipelineStatus = 'Submitted' 
		AND PrintBatch IN (SELECT DISTINCT JobId FROM PnS.MRDFRequest WHERE PrintTrailBatchId = @PrintTrailBatchId)
	) 
	UPDATE B 
	SET 
	B.PipelineStatus = 'MRDFReceived',
	B.MRDFReceivedOn=GETUTCDATE() 
	FROM PnS.BatchSummary B 
	INNER JOIN CB ON B.PrintBatch = CB.PrintBatch 
	WHERE CB.InProgressCount = 0

	DECLARE @ShippedMPCount INT, @TotalRowCount INT
	
	SET @ShippedMPCount = (SELECT COUNT(*) FROM PnS.MRDFRequest WHERE PrintTrailBatchId = @PrintTrailBatchId AND (MailPieceStatus='Shipped' OR MailPieceStatus='PulledBack'))

	SET @TotalRowCount = (SELECT COUNT(*) FROM PnS.MRDFRequest WHERE PrintTrailBatchId = @PrintTrailBatchId)

	IF(@TotalRowCount=@ShippedMPCount)
	BEGIN
		UPDATE PnS.PrintTrailBatches
		SET
		MRDFReceivedOn=GETUTCDATE()
		WHERE Id=@PrintTrailBatchId
	END	
END


