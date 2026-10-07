CREATE PROCEDURE [dbo].[usp_GetNextBatchIdByJobName] 
	-- Add the parameters for the stored procedure here
	@jobName varchar(50),
	@operation varchar(50),
	@serverIP varchar(50)
AS
BEGIN
	
	IF (@jobName = 'PnSJobMonitor' AND @operation = 'ReportBack')
	BEGIN
		SELECT PrintBatch AS BatchId FROM [PnS].BatchSummary WHERE PipelineStatus = 'ReportPrepped' AND  IsReportPrepped = 1 AND IsReportedBack = 0 AND ServerIP=@serverIP
	END
    ELSE IF(@jobName = 'PnSCoreService')
    BEGIN
		IF(@operation = 'Prep')
		BEGIN
			SELECT PrintBatch AS BatchId FROM PnS.BatchSummary WHERE PipelineStatus = 'Downloaded' AND  IsDownloaded = 1 AND IsPrepped = 0 AND ServerIP=@serverIP
		END
		ELSE IF(@operation = 'ProcessMRDF')
		BEGIN
			SELECT PrintBatch AS BatchId FROM PnS.BatchSummary WHERE PipelineStatus = 'MRDFReceived' AND  IsMRDFReceived = 1 AND IsReportPrepped = 0 AND ServerIP=@serverIP
		END
    END
	ELSE IF(@jobName = 'PCIJobMonitor' AND @operation = 'Submit')
    BEGIN
		SELECT PrintBatch AS BatchId FROM PnS.BatchSummary WHERE PipelineStatus = 'Prepped' AND  IsPrepped = 1 AND IsSubmitted = 0 AND ServerIP=@serverIP
    END
END
