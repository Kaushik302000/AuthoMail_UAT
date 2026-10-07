CREATE PROCEDURE [dbo].[SP_BulkRegisterScan]
@trackno VARCHAR(255),
@clientCode VARCHAR(128),
@operatorid VARCHAR(50),
@machineid VARCHAR(255),
@btName VARCHAR(255),-- 05PM, 06PM, 07PM, Backlog
@EstDate DATE
AS
BEGIN
	DECLARE	@minDate DATETIME =NULL,@jobid INT,@printlocationcl VARCHAR(1024), @mpCounts INT,@mailpiecestatus VARCHAR(255),@code VARCHAR(255)
	BEGIN TRY
	
		SET @trackno = REPLACE(@trackno, 'AUT','')
		SET @clientCode = REPLACE(@clientCode, 'AUT','')
		--Get Print Location name
		SELECT @printlocationcl=Name FROM Common.PrintVendors WHERE VendorCode='CL_DALLAS';
		
		-- Get mailpiece, MRDFRequest data filter by trackingno & printlocation
	
		IF(@btName is NULL)
		BEGIN 
			SELECT 'Invalid' AS Code, NULL AS TrackingNo, 'Batch name can not be NULL' AS [Message], NULL AS [JobName]
			RETURN
		END

		--initilize mindate
		SELECT @minDate= MIN(StartDate) FROM [dbo].[ufn_GetDallasBatches](@EstDate)

		--Get batches timings
		SELECT * into #dallasBatchesTimes FROM (SELECT * FROM [dbo].[ufn_GetDallasBatches](@EstDate)) AS t

		SELECT * INTO #MPData FROM
		(
			SELECT m.Id AS MailPieceId, m.[IsPullBackRequested],m.[IsQCRequested],m.[TotalSheets],b.[JobName],req.[PieceId],
			req.[PrintTrailBatchId],req.[JobId],req.[Date],req.[PDFFileName],c.[AppCode],c.[Name],
			(SELECT [BatchName] FROM #dallasBatchesTimes WHERE EffectiveReceivedOnEst between StartDate AND EndDate AND ShippingCarrier=PCD.ShippingCarrier AND ShippingServiceType=PCD.ShippingServiceType AND AppCode=c.AppCode) AS BatchName
			,PCD.ShippingCarrier,PCD.ShippingServiceType,E.Id AS EnvelopeId,m.TrackingNo
			FROM [AM].[MailPieces] m
			INNER JOIN [AM].[DocTypes] d ON m.DocTypeId=d.Id 
			INNER JOIN Common.PnSConfigDetail PCD ON m.PnSConfigDetailId = PCD.Id
			INNER JOIN Common.Clients c ON m.ClientId=c.Id
			INNER JOIN Common.Envelopes E ON PCD.EnvelopeId=E.Id
			LEFT OUTER JOIN PnS.MRDFRequest req ON m.UniqueId = req.UniqueId
			left outer JOIN PnS.BatchSummary b ON req.JobId = b.PrintBatch
			WHERE m.PrintLocation=@printlocationcl 
			AND m.EffectiveReceivedOnEst >=@minDate AND IsSubmitted=1 AND m.ShippingStatus='InProgress' AND req.BarcodeNumber=@trackno
		) AS MP
		
		
		SET @mpCounts = (SELECT ISNULL(COUNT(*),0) FROM #MPData)

		--Drop Temp Table
		IF(OBJECT_ID('tempdb..#dallasBatchesTimes') Is Not Null)
		BEGIN
		DROP TABLE #dallasBatchesTimes
		END

		IF(@mpCounts IS NULL OR @mpCounts = 0) 
		BEGIN
			SELECT 'Invalid' AS Code, NULL AS TrackingNo, 'This barcode number is not valid.' AS [Message], NULL AS [JobName]
			RETURN
		END		

		
		IF ((SELECT COUNT(*) FROM PnS.MRDFResponse WHERE JobId IN (SELECT Distinct PrintTrailBatchId FROM PnS.MRDFRequest WHERE BarcodeNumber = @trackno)) >0)
		BEGIN
			SELECT 'Rescan' AS Code, @trackno AS TrackingNo, NULL AS [Message], NULL AS [JobName]
			RETURN
		END

		-- SET mailpiece status IF ispullbackrequested then UPDATE otherwise not
		SET @mailpiecestatus='Shipped'		
		SET @code = 'Valid'
		

		-- Start Transaction
		BEGIN TRANSACTION	
			-- insert data into MRDFResponse table
			INSERT INTO [PnS].[MRDFResponse] ([JobId],[PieceId],[Date],[Feeder2_Fed],[OperatorId],[MachineId],[ProcessingTimeStamp],[DetailedErrorId],[Disposition],[Input_Fed],[Feeder3_Fed],[Feeder4_Fed],[Feeder5_Fed],[ComputedWeight],[MeterDate],[StatusSource],[PostageMetered],[WeightMeasured],[ExitLocation],[TotalSheets],[PDFFileName],[PackageTrackingNumber])
			SELECT PrintTrailBatchId,PieceId,CAST(GETUTCDATE() AS DATE),0,@operatorid,@machineid,GETUTCDATE(),'4691','5'
			,'00','0','0','0','0.000','00000000', @printlocationcl,0.00,0.00
			,@printlocationcl,TotalSheets,PDFFileName,NULL
			FROM #MPData					
			
			UPDATE [PnS].[MRDFRequest] SET MailPieceStatus = @mailpiecestatus
			WHERE BarcodeNumber = @trackno

			-- Check IF we are yet to received response for any mail pieces
			IF((SELECT COUNT(*) FROM PnS.MRDFRequest WHERE BarcodeNumber = @trackno AND MailPieceStatus='InProgress')=0)
			BEGIN
			--UPDATE Batchsummary pipeline status when all mailpieces's response is received
				UPDATE PnS.BatchSummary 
				SET PipelineStatus = 'MRDFReceived', MRDFReceivedOn = GETUTCDATE() 
				WHERE PrintBatch IN (SELECT JobId FROM PnS.MRDFRequest WHERE BarcodeNumber = @trackno)

				IF((SELECT COUNT(*) FROM [PnS].[MRDFRequest] WHERE BarcodeNumber = @trackno AND MailPieceStatus='InProgress')=0)
				BEGIN
					-- UPDATE in PrintTrailBatch of mrdfreceivedon when all response is received.
					UPDATE PnS.PrintTrailBatches 
					SET MRDFReceivedOn=GETUTCDATE()
					WHERE Id IN (SELECT PrintTrailBatchId FROM PnS.MRDFRequest WHERE BarcodeNumber = @trackno)					
				END
			END
		COMMIT

		SELECT @code AS Code,ISNULL(TrackingNo,'NoTracking-'+CAST(MailPieceId AS VARCHAR(128))) AS TrackingNo,NULL AS [Message], BatchName AS [JobName] FROM #MPData
	END TRY
	BEGIN CATCH
		IF @@TRANCOUNT > 0
			ROLLBACK
	END CATCH
	--Drop Temp Table
	IF(OBJECT_ID('tempdb..#MPData') Is Not Null)
	BEGIN
	DROP TABLE #MPData
	END
END