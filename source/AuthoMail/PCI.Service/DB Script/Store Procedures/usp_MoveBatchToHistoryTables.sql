/****** Object:  StoredProcedure [dbo].[usp_MoveBatchToHistoryTables]    Script Date: 08/15/2017 13:37:37 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		Khushboo Patel
-- Create date: 15 Aug 2017
-- Description:	Move MDRFRequest,MDRFResponse and BatchSummary to archive
-- =============================================
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_MoveBatchToHistoryTables]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[usp_MoveBatchToHistoryTables]
GO
CREATE PROCEDURE [dbo].[usp_MoveBatchToHistoryTables] 
	-- Add the parameters for the stored procedure here
	@batchId int
AS

	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

   
    DECLARE @ErrorCode  int  
    DECLARE @ErrorStep  varchar(200)
     SET @ErrorCode = @@ERROR

BEGIN TRY

    BEGIN TRAN
        
		 -- Copy from dbo.BatchSummary to history.BatchSummary
		 
		 SET @ErrorStep = 'Error in Copying from the BatchSummary';
		 
		 INSERT INTO history.BatchSummary 
		(PrintBatch,AppId,JobName,PipelineStatus,ValidationStatus,PrintFileName,AppPrintFormat,BarcodeLocation,OnValidationErrors,ValidationErrors,BatchStatus,ReceivedOn,DownloadedOn,
		PreppedOn,SubmittedOn,MRDFReceivedOn,ReportPreppedOn,ReportedBackOn,SuspendedOn,CancelledOn,JulianDate,Images,PaperStock1Name,PaperStock1Quantity,PaperStock2Name,PaperStock2Quantity,
		PaperStock3Name,PaperStock3Quantity,PaperStock4Name,PaperStock4Quantity,PrintFormat,BREPartNumber,BREQuantity,InsertStock2Name,InsertStock2Quantity,InsertStock3Name,InsertStock3Quantity,
		InsertStock4Name,InsertStock4Quantity,EnvelopeStockName,EnvelopeStockQty,TotalPostage,PresortOrPSI,DueDate,PackageType,Checks,CompanySite,ApplicationValue,TNumber,PrinterType,SLADays,
		ReprintType,Color,OneOrTwoUp,CustId,JobTag,LetterCode,SpecialInstructions)
		SELECT PrintBatch,AppId,JobName,PipelineStatus,ValidationStatus,PrintFileName,AppPrintFormat,BarcodeLocation,OnValidationErrors,ValidationErrors,BatchStatus,ReceivedOn,DownloadedOn,PreppedOn,
		SubmittedOn,MRDFReceivedOn,ReportPreppedOn,ReportedBackOn,SuspendedOn,CancelledOn,JulianDate,Images,PaperStock1Name,PaperStock1Quantity,PaperStock2Name,PaperStock2Quantity,PaperStock3Name,PaperStock3Quantity,PaperStock4Name,PaperStock4Quantity,PrintFormat
		,BREPartNumber,BREQuantity,InsertStock2Name,InsertStock2Quantity,InsertStock3Name,InsertStock3Quantity,InsertStock4Name,InsertStock4Quantity,EnvelopeStockName,EnvelopeStockQty,TotalPostage,PresortOrPSI,DueDate,PackageType,Checks,CompanySite,ApplicationValue,TNumber	,PrinterType,SLADays,ReprintType,Color,OneOrTwoUp,CustId,JobTag,LetterCode,SpecialInstructions
		FROM dbo.BatchSummary
		WHERE PrintBatch =@batchId
		
		-- Copy from dbo.MRDFRequest to history.MRDFRequest

        SET @ErrorStep = 'Error in Copying from the MRDFRequest';

        INSERT INTO history.MRDFRequest
		(JobId,PieceId,[Date],TotalSheets,InputWeight,Feeder3_Sel,Feeder4_Sel,Feeder5_Sel,LowCap_Sel,NoSeal_Sel,Feeder2_Sel,ZipMark_Sel,Address1,Address2,Address3,Address4,Address5,Address6,
		Zip5,Zip4,Zip2,DivertVS2,DivertVS3,DivertVS4,ReturnLogoBitMap,ReturnLine1,ReturnLine2,ReturnLine3,ReturnLine4,ReturnLine5,ReturnLine6,GroupNumber,SocialSecNumber,CheckAmount,PassThru,
		Expansion,EndofRecord,UniqueIdentifier,PDFFileName,ErrDesc,MailPieceStatus)
		SELECT JobId,PieceId,[Date],TotalSheets,InputWeight,Feeder3_Sel,Feeder4_Sel,Feeder5_Sel,LowCap_Sel,NoSeal_Sel,Feeder2_Sel,ZipMark_Sel,Address1,Address2,Address3,Address4,Address5,Address6,
		Zip5,Zip4,Zip2,DivertVS2,DivertVS3,DivertVS4,ReturnLogoBitMap,ReturnLine1,ReturnLine2,ReturnLine3,ReturnLine4,ReturnLine5,ReturnLine6,GroupNumber,SocialSecNumber,CheckAmount,PassThru,
		Expansion,EndofRecord,UniqueIdentifier,PDFFileName,ErrDesc,MailPieceStatus
		FROM dbo.MRDFRequest
		WHERE JobId=@batchId


         -- Copy from dbo.MRDFResponse to history.MRDFResponse
         
		SET @ErrorStep = 'Error in Copying from the MRDFResponse';
		
        INSERT INTO history.MRDFResponse
		(JobId,PieceId,[Date],Feeder2_Fed,OperatorId,MachineId,ProcessingTimeStamp,DetailedErrorId,Disposition,Input_Fed,Feeder3_Fed,Feeder4_Fed,Feeder5_Fed,ComputedWeight,MeterDate,StatusSource,
		PostageMetered,WeightMeasured,ExitLocation)
		SELECT JobId,PieceId,[Date],Feeder2_Fed,OperatorId,MachineId,ProcessingTimeStamp,DetailedErrorId,Disposition,Input_Fed,Feeder3_Fed,Feeder4_Fed,Feeder5_Fed,ComputedWeight,MeterDate,StatusSource,
		PostageMetered,WeightMeasured,ExitLocation 
		FROM dbo.MRDFResponse
		WHERE JobId=@batchId
		
         -- delete from dbo.MRDFRequest

        SET @ErrorStep = 'Error in deleting the items from the MRDFRequest'

        DELETE
        FROM dbo.MRDFRequest
        WHERE JobId = @batchId
        
        -- delete from dbo.MRDFResponse 

        SET @ErrorStep = 'Error in deleting the items from the MRDFResponse'

        DELETE
        FROM dbo.MRDFResponse
        WHERE JobId = @batchId
        
        -- delete from dbo.BatchSummary 

        SET @ErrorStep = 'Error in deleting the items from the BatchSummary'

        DELETE
        FROM dbo.BatchSummary
        WHERE PrintBatch = @batchId

    COMMIT TRAN

     SET @ErrorCode  = 0 -- @ErrorCode=0 if success,  <>0 if failure
     SET @ErrorStep='All data was moved over'   
    
        SELECT @ErrorCode As ErrorCode,@ErrorStep As ErrorMsg                       

END TRY

BEGIN CATCH

   --Get the Error Message for @@Error
    IF @@TRANCOUNT > 0 ROLLBACK
	SET @ErrorCode = ERROR_NUMBER()-- @ErrorCode=0 if success,  <>0 if failure
    SET @ErrorStep=@ErrorStep + ' '
        + cast(ERROR_NUMBER() as varchar(20)) + ' line: '
        + cast(ERROR_LINE() as varchar(20)) + ' ' 
        + ERROR_MESSAGE() + ' > ' 
        + ERROR_PROCEDURE()
    SELECT @ErrorCode As ErrorCode,@ErrorStep As ErrorMsg  

    
                               

END CATCH
GO


