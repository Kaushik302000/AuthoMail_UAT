SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Akshar Vala	
-- Create date: 11 Aug 2017
-- Description:	Get Batch Summary Details by BatchId
-- EXECUTE : usp_GetBatchSummaryById 2
-- =============================================
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_GetBatchSummaryById]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].usp_GetBatchSummaryById
GO
CREATE PROCEDURE usp_GetBatchSummaryById
	-- Add the parameters for the stored procedure here
	@batchId int
AS
BEGIN

	SELECT [PrintBatch]
      ,[AppId]
      ,[JobName]
      ,[PipelineStatus]
      ,[ValidationStatus]
      ,[PrintFileName]
      ,[OnValidationErrors]
      ,[BarcodeLocation]
      ,[ValidationErrors]
      ,[BatchStatus]
      ,[IsReceived]
      ,[ReceivedOn]
      ,[IsDownloaded]
      ,[DownloadedOn]
      ,[IsPrepped]
      ,[PreppedOn]
      ,[IsSubmitted]
      ,[SubmittedOn]
      ,[IsMRDFReceived]
      ,[MRDFReceivedOn]
      ,[IsReportPrepped]
      ,[ReportPreppedOn]
      ,[IsReportedBack]
      ,[ReportedBackOn]
      ,[IsSuspended]
      ,[SuspendedOn]
      ,[IsCancelled]
      ,[CancelledOn]
      ,[JulianDate]
      ,[Images]
      ,[PaperStock1Name]
      ,[PaperStock1Quantity]
      ,[PaperStock2Name]
      ,[PaperStock2Quantity]
      ,[PaperStock3Name]
      ,[PaperStock3Quantity]
      ,[PaperStock4Name]
      ,[PaperStock4Quantity]
      ,[PrintFormat]
      ,[BREPartNumber]
      ,[BREQuantity]
      ,[InsertStock2Name]
      ,[InsertStock2Quantity]
      ,[InsertStock3Name]
      ,[InsertStock3Quantity]
      ,[InsertStock4Name]
      ,[InsertStock4Quantity]
      ,[EnvelopeStockName]
      ,[EnvelopeStockQty]
      ,[TotalPostage]
      ,[PresortOrPSI]
      ,[DueDate]
      ,[PackageType]
      ,[Checks]
      ,[CompanySite]
      ,[ApplicationValue]
      ,[TNumber]
      ,[PrinterType]
      ,[SLADays]
      ,[ReprintType]
      ,[Color]
      ,[OneOrTwoUp]
      ,[CustId]
      ,[JobTag]
      ,[LetterCode]
      ,[SpecialInstructions]
  FROM [dbo].[BatchSummary] WHERE PrintBatch = @batchId
END
GO


