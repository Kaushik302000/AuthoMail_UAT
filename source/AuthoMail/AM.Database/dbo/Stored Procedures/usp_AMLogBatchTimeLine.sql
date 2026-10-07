


CREATE PROCEDURE [dbo].[usp_AMLogBatchTimeLine]
	-- Add the parameters for the stored procedure here
	@BatchId int,
	@ActivityName varchar(25),
	@TimeStamp datetime
AS
BEGIN
	
	IF(@ActivityName = 'DownloadStart')
	BEGIN
		INSERT INTO [AM].[LogBatchTimeLine] ([MailBatchId],[DownloadStart]) VALUES (@BatchId,@TimeStamp)
	END
	ELSE
	BEGIN
		UPDATE [AM].[LogBatchTimeLine] SET
		[DownloadEnd] = (CASE WHEN @ActivityName = 'DownloadEnd' THEN @TimeStamp ELSE [DownloadEnd] END),
		[ValidationStart] = (CASE WHEN @ActivityName = 'ValidationStart' THEN @TimeStamp ELSE [ValidationStart] END),
		[ValidationEnd] = (CASE WHEN @ActivityName = 'ValidationEnd' THEN @TimeStamp ELSE [ValidationEnd]  END),
		[AcknowledgeStart] = (CASE WHEN @ActivityName = 'AcknowledgeStart' THEN @TimeStamp ELSE [AcknowledgeStart] END),
		[AcknowledgeEnd] = (CASE WHEN @ActivityName = 'AcknowledgeEnd' THEN @TimeStamp ELSE [AcknowledgeEnd] END),
		[PreppedStart] = (CASE WHEN @ActivityName = 'PreppedStart' THEN @TimeStamp ELSE [PreppedStart] END),
		[PreppedEnd] = (CASE WHEN @ActivityName = 'PreppedEnd' THEN @TimeStamp ELSE [PreppedEnd] END),
		[SubmittedToShippingStart] = (CASE WHEN @ActivityName = 'SubmittedToShippingStart' AND [SubmittedToShippingStart] IS NULL THEN @TimeStamp ELSE [SubmittedToShippingStart] END),
		[SubmittedToShippingEnd] = (CASE WHEN @ActivityName = 'SubmittedToShippingEnd' THEN @TimeStamp ELSE [SubmittedToShippingEnd] END),
		[ShippingResponseReceivedStart] = (CASE WHEN @ActivityName = 'ShippingResponseReceivedStart' AND [ShippingResponseReceivedStart] IS NULL THEN @TimeStamp ELSE [ShippingResponseReceivedStart] END),
		[ShippingResponseReceivedEnd] = (CASE WHEN @ActivityName = 'ShippingResponseReceivedEnd' THEN @TimeStamp ELSE [ShippingResponseReceivedEnd] END),
		[ReportPrepStart] = (CASE WHEN @ActivityName = 'ReportPrepStart' THEN @TimeStamp ELSE [ReportPrepStart] END),
		[ReportPrepEnd] = (CASE WHEN @ActivityName = 'ReportPrepEnd' THEN @TimeStamp ELSE [ReportPrepEnd] END),
		[ReportBackStart] = (CASE WHEN @ActivityName = 'ReportBackStart' THEN @TimeStamp ELSE [ReportBackStart] END),
		[ReportBackEnd] = (CASE WHEN @ActivityName = 'ReportBackEnd' THEN @TimeStamp ELSE [ReportBackEnd] END)
		WHERE MailBatchId = @BatchId
	END
END





























