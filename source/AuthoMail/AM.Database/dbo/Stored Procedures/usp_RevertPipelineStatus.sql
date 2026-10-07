


-- =============================================
-- Author:		Akshar Vala
-- Create date: 17 Oct 2017
-- Description:	Revert pipeline status from current status
-- =============================================
CREATE PROCEDURE [dbo].[usp_RevertPipelineStatus]
	@batchId int,
	@pipelineStatus varchar(512)
AS
BEGIN
	IF(@pipelineStatus = 'Downloaded')
	BEGIN
		 UPDATE [PnS].[BatchSummary] SET PipelineStatus = 'Received',DownloadedOn = NULL,PreppedOn = NULL,SubmittedOn = NULL,
		 MRDFReceivedOn = NULL,ReportPreppedOn = NULL,ReportedBackOn = NULL,SuspendedOn=NULL
		 WHERE PrintBatch = @batchId
	END
	ELSE IF(@pipelineStatus = 'Prepped')
	BEGIN
		 UPDATE [PnS].[BatchSummary] SET PipelineStatus = 'Downloaded',PreppedOn = NULL,SubmittedOn = NULL,MRDFReceivedOn = NULL,
		 ReportPreppedOn = NULL,ReportedBackOn = NULL,ValidationStatus=NULL,ValidationErrors=NULL,BatchStatus=NULL
		 ,SuspendedOn=NULL
		 WHERE PrintBatch = @batchId
	END
	ELSE IF(@pipelineStatus = 'Submitted')
	BEGIN
		 UPDATE [PnS].[BatchSummary] SET PipelineStatus = 'Prepped',SubmittedOn = NULL,MRDFReceivedOn = NULL,ReportPreppedOn = NULL,
		 ReportedBackOn = NULL,ValidationStatus=NULL,ValidationErrors=NULL,BatchStatus=NULL,SuspendedOn=NULL
		 WHERE PrintBatch = @batchId
	END
	ELSE IF(@pipelineStatus = 'MRDFReceived')
	BEGIN
		 UPDATE [PnS].[BatchSummary] SET PipelineStatus = 'Submitted',MRDFReceivedOn = NULL,ReportPreppedOn = NULL
		 ,ReportedBackOn = NULL,SuspendedOn=NULL,ValidationErrors=NULL
		 WHERE PrintBatch = @batchId
		 --This is needs to be done because if some of the MRDF has been received and mailpieces are updated 
		 --then need to update MailPieceStatus to InProgress
		 UPDATE [PnS].MRDFRequest
		 SET MailPieceStatus='InProgress',ErrId=0,ErrDesc=NULL
		 WHERE JobId=@batchId AND (ErrId=7 OR ErrId=0) AND MailPieceStatus!='Aborted'
		 --If we are reverting to Submitted status then need to delete mrdf response at batch level
		 DELETE FROM [PnS].MRDFResponse WHERE JobId=@batchId
	END
	ELSE IF(@pipelineStatus = 'ReportPrepped')
	BEGIN
		 UPDATE [PnS].[BatchSummary] SET PipelineStatus = 'MRDFReceived',ReportPreppedOn = NULL,ReportedBackOn = NULL
		 ,SuspendedOn=NULL
		 WHERE PrintBatch = @batchId
	END
	ELSE IF(@pipelineStatus = 'ReportedBack')
	BEGIN
		 UPDATE [PnS].[BatchSummary] SET PipelineStatus = 'ReportPrepped',ReportedBackOn = NULL,SuspendedOn=NULL
		 WHERE PrintBatch = @batchId
	END
	ELSE IF(@pipelineStatus = 'Reprint')
	BEGIN
		 UPDATE [PnS].[BatchSummary] SET PipelineStatus = 'Submitted',MRDFReceivedOn = NULL,ReportPreppedOn = NULL,ReportedBackOn = NULL,
		 ValidationStatus=NULL,ValidationErrors=NULL,BatchStatus=NULL
		 WHERE PrintBatch = @batchId
		 DELETE FROM [PnS].MRDFResponse WHERE Disposition LIKE '%7%'
	END
END



























