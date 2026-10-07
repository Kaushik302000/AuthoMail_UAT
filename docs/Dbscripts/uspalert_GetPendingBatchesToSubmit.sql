SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Akshar Vala
-- Create date: 7 Sept 2018
-- Description:	to get list of batches which are not submitted to print vendor within specified cutOffMinutes
-- Execute : uspalert_GetPendingBatchesToSubmit 1
-- =============================================
CREATE PROCEDURE uspalert_GetPendingBatchesToSubmit 	
	@cutOffMinutes int
AS
BEGIN
	
	SELECT			
	X.PrintBatchId,X.AppCode,BS.PrintFileName,X.PrintLocation,BS.PipelineStatus,X.ReceivedOn,X.DownloadedOn,X.PreppedOn,
	X.SFTChannelId,BS.IsSubmitted,BS.IsReportedBack FROM
	(
		SELECT DISTINCT P.PrintBatchId,PB.UniqueId,B.ReceivedOn,B.DownloadedOn,B.PreppedOn,PB.AppCode,B.SFTChannelId,P.PrintLocation		FROM AM.MailPieces P  INNER JOIN AM.MailBatches B ON B.Id=P.MailBatchID 
		INNER JOIN AM.PrintBatches PB ON PB.Id=P.PrintBatchId 
	)X 
	LEFT OUTER JOIN PnS.BatchSummary BS ON X.UniqueId = BS.BatchIdentifier 
	WHERE (ISNULL(BS.IsSubmitted,0) = 0 AND ISNULL(BS.IsReportedBack,0) = 0) 
	AND DATEDIFF(minute, CASE WHEN DATENAME(DW, X.ReceivedOn) = 'Sunday' THEN X.DownloadedOn ELSE X.ReceivedOn END, GETUTCDATE()) >		@cutOffMinutes
END
GO
