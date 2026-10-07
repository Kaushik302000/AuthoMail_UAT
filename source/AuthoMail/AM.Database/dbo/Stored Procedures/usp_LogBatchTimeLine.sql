
-- =============================================
-- Author:		Akshar
-- Create date: 5 Sep 2017
-- Description:	To track the batch timing history 
-- =============================================
CREATE PROCEDURE [dbo].[usp_LogBatchTimeLine]
	-- Add the parameters for the stored procedure here
	@BatchId int,
	@ActivityName varchar(25),
	@TimeStamp datetime
AS
BEGIN
	
	IF(@ActivityName = 'DownloadStart')
	BEGIN
		INSERT INTO [PnS].[LogBatchTimeLine] ([BatchId],[DownloadStart]) VALUES (@BatchId,@TimeStamp)
	END
	ELSE
	BEGIN
		UPDATE [PnS].[LogBatchTimeLine] SET
		[DownloadEnd] = (CASE WHEN @ActivityName = 'DownloadEnd' THEN @TimeStamp ELSE [DownloadEnd] END),
		[PCIBatchPrepStart] = (CASE WHEN @ActivityName = 'PCIBatchPrepStart' THEN @TimeStamp ELSE [PCIBatchPrepStart] END),
		[PCIBatchPrepEnd] = (CASE WHEN @ActivityName = 'PCIBatchPrepEnd' THEN @TimeStamp ELSE [PCIBatchPrepEnd]  END),
		[IEReportPrepStart] = (CASE WHEN @ActivityName = 'IEReportPrepStart' THEN @TimeStamp ELSE [IEReportPrepStart] END),
		[IEReportPrepEnd] = (CASE WHEN @ActivityName = 'IEReportPrepEnd' THEN @TimeStamp ELSE [IEReportPrepEnd] END),
		[SubmitToPCIStart] = (CASE WHEN @ActivityName = 'SubmitToPCIStart' THEN @TimeStamp ELSE [SubmitToPCIStart] END),
		[SubmitToPCIEnd] = (CASE WHEN @ActivityName = 'SubmitToPCIEnd' THEN @TimeStamp ELSE [SubmitToPCIEnd] END),
		[MRDFDownloadStart] = (CASE WHEN @ActivityName = 'MRDFDownloadStart' AND [MRDFDownloadStart] IS NULL THEN @TimeStamp ELSE [MRDFDownloadStart] END),
		[MRDFDownloadEnd] = (CASE WHEN @ActivityName = 'MRDFDownloadEnd' THEN @TimeStamp ELSE [MRDFDownloadEnd] END),
		[ReportPrepStart] = (CASE WHEN @ActivityName = 'ReportPrepStart' THEN @TimeStamp ELSE [ReportPrepStart] END),
		[ReportPrepEnd] = (CASE WHEN @ActivityName = 'ReportPrepEnd' THEN @TimeStamp ELSE [ReportPrepEnd] END),
		[ReportBackStart] = (CASE WHEN @ActivityName = 'ReportBackStart' THEN @TimeStamp ELSE [ReportBackStart] END),
		[ReportBackEnd] = (CASE WHEN @ActivityName = 'ReportBackEnd' THEN @TimeStamp ELSE [ReportBackEnd] END)
		WHERE BatchId = @BatchId
	END
END



























