CREATE PROCEDURE [dbo].[SP_RegisterScan]
	-- Add the parameters for the stored procedure here
@trackno VARCHAR(255),
@clientCode VARCHAR(128),
@operatorid VARCHAR(50),
@machineid VARCHAR(255),
@btName VARCHAR(255),-- 05PM, 06PM, 07PM, Backlog
@EstDate DATE
AS
BEGIN
DECLARE	@jobid INT,
		@pieceid INT,
		@mrdfreqdate date,
		@ispullbackrequested BIT,
		@isqcrequested BIT,
		@disposition VARCHAR(50),
		@mailpiecestatus VARCHAR(255),
		@totalsheet INT = NULL,
		@pdffilename VARCHAR(255),
		@printlocationcl VARCHAR(1024),
		@mailpieceid INT,
		@code VARCHAR(50),
		@existjobid INT,
		@jobName VARCHAR(512),
		@clientAppCode VARCHAR(128),
		@clientName VARCHAR(255),
		@bt VARCHAR(255),
		@PrintTrailBatchId INT = NULL,
		@minDate DATETIME =NULL,
		@shippingCarrier VARCHAR(255),
		@shippingServiceType VARCHAR(255),
		@weightMeasured DECIMAL(7,4),
		@postageCost DECIMAL(7,4),
		@InsertCount INT = NULL,
		@Inserts VARCHAR(255) = NULL,
		@EnvelopeId INT
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
		
		--Get batches timings
		SELECT * into #dallasBatchesTimes FROM (SELECT * FROM [dbo].[ufn_GetDallasBatches](@EstDate)) AS t
		--initilize mindate
		SELECT @minDate= MIN(StartDate) FROM [dbo].[ufn_GetDallasBatches](@EstDate)
			
		-- getdata FROM DB
		SELECT @mailpieceid=m.Id, @ispullbackrequested=m.[IsPullBackRequested],@isqcrequested=m.[IsQCRequested],
		@totalsheet=m.[TotalSheets],
		@jobName=b.[JobName],
		@pieceid=req.[PieceId],@PrintTrailBatchId=req.[PrintTrailBatchId], @jobid=req.[JobId],@mrdfreqdate=req.[Date],@pdffilename=req.[PDFFileName],
		@clientAppCode=c.[AppCode],@clientName=c.[Name],
		@bt=(SELECT [BatchName] FROM #dallasBatchesTimes WHERE EffectiveReceivedOnEst between StartDate AND EndDate AND ShippingCarrier=PCD.ShippingCarrier AND ShippingServiceType=PCD.ShippingServiceType AND AppCode=c.AppCode)
		,@shippingCarrier=PCD.ShippingCarrier,@shippingServiceType=PCD.ShippingServiceType
		,@EnvelopeId = E.Id
		FROM [AM].[MailPieces] m
		INNER JOIN [AM].[DocTypes] d ON m.DocTypeId=d.Id 
		INNER JOIN Common.PnSConfigDetail PCD ON m.PnSConfigDetailId = PCD.Id
		INNER JOIN Common.Clients c ON m.ClientId=c.Id
		INNER JOIN Common.Envelopes E ON PCD.EnvelopeId=E.Id
		LEFT OUTER JOIN PnS.MRDFRequest req ON m.UniqueId = req.UniqueId
		left outer JOIN PnS.BatchSummary b ON req.JobId = b.PrintBatch
		WHERE m.PrintLocation=@printlocationcl 
		AND m.EffectiveReceivedOnEst >=@minDate AND IsSubmitted=1 AND m.ShippingStatus='InProgress' AND m.TrackingNo=@trackno
		

		SET @Inserts = (SELECT Inserts FROM AM.MailPieces WHERE Id=@mailpieceid)

		SET @InsertCount = CASE WHEN LEN(ISNULL(@Inserts,''))>0 THEN (SELECT COUNT(value) FROM STRING_SPLIT(@Inserts, ',')) ELSE 0 END

		--Drop Temp Table
		IF(OBJECT_ID('tempdb..#dallasBatchesTimes') Is Not Null)
		BEGIN
		DROP TABLE #dallasBatchesTimes
		END


		IF(@mailpieceid IS NULL) 
		BEGIN
			SELECT 'Invalid' AS Code, NULL AS TrackingNo, NULL AS [Message], NULL AS [JobName]
			RETURN
		END

		--Validate tracking no is assign to this client
		IF(@clientCode <> @clientAppCode)
		BEGIN
			SELECT 'Invalid' AS Code, @trackno AS TrackingNo, REPLACE('This tracking number belongs to @clientName client. Please check your document.','@clientName',@clientName) AS [Message], NULL AS [JobName]
			RETURN
		END

		-- Validate this mailpiece is assign to this batch or not.
		IF(@bt<>@btName)
		BEGIN
			SELECT 'Invalid' AS Code, @trackno AS TrackingNo, REPLACE('This document belongs to <span style="color:#000"> @bt </span> batch.','@bt',@bt) AS [Message], NULL AS [JobName]
			RETURN
		END
		
		IF EXISTS(SELECT 1 FROM PnS.MRDFResponse WHERE JobId = @PrintTrailBatchId AND PieceId = @pieceid) 
		BEGIN
			SELECT 'Rescan' AS Code, @trackno AS TrackingNo, NULL AS [Message], @jobName AS [JobName]
			RETURN
		END

		-- SET mailpiece status IF ispullbackrequested then UPDATE otherwise not
		SET @mailpiecestatus='Shipped'

		IF(@ispullbackrequested = 1) 
		BEGIN
			SET @code = 'Pulledback'
			SET @disposition='4' 
			SET @mailpiecestatus='PulledBack'
		END
		ELSE IF(@isqcrequested=1)
		BEGIN
			SET @disposition='8'
			SET @code = 'QC'
		END
		ELSE
		BEGIN
			SET @disposition='5'
			SET @code = 'Valid'
		END

		SET @postageCost =	CASE 
							WHEN @shippingCarrier='USPS' AND ( @shippingServiceType='IMB' OR @shippingServiceType='FIRSTCLASS') THEN 
								CASE 
								WHEN @InsertCount = 1 THEN (SELECT InsertOnePostageCost FROM Common.DallasPostages WHERE @totalsheet BETWEEN MinPage AND MaxPage AND EnvelopeId = @EnvelopeId)
								WHEN @InsertCount = 2 THEN (SELECT InsertTwoPostageCost FROM Common.DallasPostages WHERE @totalsheet BETWEEN MinPage AND MaxPage AND EnvelopeId = @EnvelopeId)
								WHEN @InsertCount = 3 THEN (SELECT InsertThreePostageCost FROM Common.DallasPostages WHERE @totalsheet BETWEEN MinPage AND MaxPage AND EnvelopeId = @EnvelopeId)
								ELSE (SELECT PostageCost FROM Common.DallasPostages WHERE @totalsheet BETWEEN MinPage AND MaxPage AND EnvelopeId = @EnvelopeId) END
							WHEN @shippingCarrier='USPS' AND @shippingServiceType='PRIORITY' THEN 7.400 
							ELSE 0.00 END

		SET @weightMeasured =	CASE 
								WHEN @shippingCarrier='USPS' AND ( @shippingServiceType='IMB' OR @shippingServiceType='FIRSTCLASS') THEN 
									CASE 
									WHEN @InsertCount = 1 THEN (SELECT InsertOneWeight FROM Common.DallasPostages WHERE @totalsheet BETWEEN MinPage AND MaxPage AND EnvelopeId = @EnvelopeId)
									WHEN @InsertCount = 2 THEN (SELECT InsertTwoWeight FROM Common.DallasPostages WHERE @totalsheet BETWEEN MinPage AND MaxPage AND EnvelopeId = @EnvelopeId)
									WHEN @InsertCount = 3 THEN (SELECT InsertThreeWeight FROM Common.DallasPostages WHERE @totalsheet BETWEEN MinPage AND MaxPage AND EnvelopeId = @EnvelopeId)
									ELSE (SELECT [Weight] FROM Common.DallasPostages WHERE @totalsheet BETWEEN MinPage AND MaxPage AND EnvelopeId = @EnvelopeId) END 
								ELSE 0.00 END

		-- Start Transaction
		BEGIN TRANSACTION	
			-- insert data into MRDFResponse table
			INSERT INTO [PnS].[MRDFResponse] ([JobId],[PieceId],[Date],[Feeder2_Fed],[OperatorId],[MachineId],[ProcessingTimeStamp],[DetailedErrorId],[Disposition],[Input_Fed],[Feeder3_Fed],[Feeder4_Fed],[Feeder5_Fed],[ComputedWeight],[MeterDate],[StatusSource],[PostageMetered],[WeightMeasured],[ExitLocation],[TotalSheets],[PDFFileName],[PackageTrackingNumber])
			VALUES(@PrintTrailBatchId ,@pieceid,@mrdfreqdate,0,@operatorid,@machineid,GETUTCDATE(),'4691'
			,@disposition,'00','0','0','0','0.000','00000000', @printlocationcl,ISNULL(@postageCost,0.00),ISNULL(@weightMeasured,0.00),@printlocationcl
			,@totalsheet,@pdffilename,NULL)				
			
				UPDATE [PnS].[MRDFRequest] SET MailPieceStatus = @mailpiecestatus
				WHERE PieceId = @pieceid AND PrintTrailBatchId=@PrintTrailBatchId AND JobId=@jobid

				-- Check IF we are yet to received response for any mail pieces
			IF((SELECT COUNT(*) FROM PnS.MRDFRequest WHERE JobId=@jobid AND MailPieceStatus='InProgress')=0)
			BEGIN
			--UPDATE Batchsummary pipeline status when all mailpieces's response is received
				UPDATE PnS.BatchSummary 
				SET PipelineStatus = 'MRDFReceived', MRDFReceivedOn = GETUTCDATE() 
				WHERE PrintBatch = @jobid

				IF((SELECT COUNT(*) FROM [PnS].[MRDFRequest] WHERE MailPieceStatus='InProgress' AND PrintTrailBatchId=@PrintTrailBatchId)=0)
				BEGIN
					-- UPDATE in PrintTrailBatch of mrdfreceivedon when all response is received.
					UPDATE PnS.PrintTrailBatches 
					SET MRDFReceivedOn=GETUTCDATE()
					WHERE Id=@PrintTrailBatchId					
				END
			END
		--END Transaction
		COMMIT
		
		SELECT @code AS Code, @trackno AS TrackingNo, NULL AS [Message], @jobName AS [JobName]
	END TRY
	BEGIN CATCH
		IF @@TRANCOUNT > 0
			ROLLBACK
	END CATCH
END
