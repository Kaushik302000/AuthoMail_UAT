CREATE PROCEDURE [dbo].[SP_GetRecentBatchesStats_Temp] 

	@EstDate DATE,
	@startdate datetime,
	@enddate datetime
 AS
BEGIN
	
	declare @clientid int

	
	-- Get Pending & Scan count batch(sweep) wise
	;with t as
	(
	select distinct BatchName,
		count(case when MailPieceStatus = 'InProgress' and IsSubmitted=1 --and CAST(EffectiveReceivedOnEst AS DATE)=@EstDate 
		then 1 else null end) as PendingCount,
		count(case when MailPieceStatus <> 'InProgress' and (ProcessingTimeStamp between @startdate and @enddate) then 1 else null end) as ScannedCount,AppCode
		from dbo.ufn_GetDallasBatchDetails(@EstDate)
		where ((MailPieceStatus = 'InProgress' and IsSubmitted=1) or (MailPieceStatus <> 'InProgress' and (ProcessingTimeStamp between @startdate and @enddate))) AND BatchName IS NOT NULL
		group by BatchName,AppCode
	),		
	sw as
	(
		select distinct BatchName,AppCode from dbo.ufn_GetDallasBatches(@EstDate)
	)
		
	select	ISNULL(t.AppCode,sw.AppCode) as AppCode, ISNULL(t.BatchName,sw.BatchName) as BatchName,
			IsNULL(t.ScannedCount,0) as ScannedLoans,
			(ISNULL(t.PendingCount,0) + ISNULL(t.ScannedCount,0)) as TotalLoans 
			from t
			full outer join sw on t.BatchName = sw.BatchName AND t.AppCode=sw.AppCode
			ORDER BY ISNULL(t.AppCode,sw.AppCode), ISNULL(t.BatchName,sw.BatchName)
END