
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Akshar vala
-- Create date: 10 AUg 2017
-- Description:	To Get Next Batch to Process by JOB Name
-- Execute : usp_GetNextBatchByJobName 'PnSJobMonitor',''
-- =============================================
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_GetNextBatchIdByJobName]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].usp_GetNextBatchIdByJobName
GO
CREATE PROCEDURE usp_GetNextBatchIdByJobName 
	-- Add the parameters for the stored procedure here
	@jobName varchar(50),
	@operation varchar(50)
AS
BEGIN
	
	IF (@jobName = 'PnSJobMonitor' AND @operation = 'ReportBack')
	BEGIN
		SELECT TOP 1 PrintBatch FROM dbo.BatchSummary WHERE PipelineStatus = 'ReportPrepped' AND  IsReportPrepped = 1 AND IsReportedBack = 0
	END
    ELSE IF(@jobName = 'PnSCoreService')
    BEGIN
		IF(@operation = 'Prep')
		BEGIN
			SELECT TOP 1 PrintBatch AS BatchId FROM dbo.BatchSummary WHERE PipelineStatus = 'Downloaded' AND  IsDownloaded = 1 AND IsPrepped = 0 
		END
		ELSE IF(@operation = 'ProcessMRDF')
		BEGIN
			SELECT TOP 1 PrintBatch AS BatchId FROM dbo.BatchSummary WHERE PipelineStatus = 'MRDFReceived' AND  IsMRDFReceived = 1 AND IsReportPrepped = 0
		END
		ELSE
		BEGIN
			SELECT NULL AS BatchId
		END
    END
	ELSE IF(@jobName = 'PCIJobMonitor' AND @operation = 'Submit')
    BEGIN
		SELECT TOP 1 PrintBatch AS BatchId FROM dbo.BatchSummary WHERE PipelineStatus = 'Prepped' AND  IsPrepped = 1 AND IsSubmitted = 0 
    END
	ELSE
    BEGIN
       SELECT NULL AS BatchId
    END

END
GO
