CREATE PROCEDURE [dbo].[SP_GetRecentBatchesStats] 
	@appcode varchar(128),
	@EstDate DATE,
	@startdate datetime,
	@enddate datetime
 AS
BEGIN
	
	declare @clientid int

	--Get Client Id, and remove AUT from client appcode
	set @appcode=REPLACE(@appcode,'AUT','')
	select @clientid=Id from Common.Clients where AppCode=@appcode;
	
	-- Get Pending & Scan count batch(sweep) wise
	with t as
	(
	select distinct BatchName,
		count(case when MailPieceStatus = 'InProgress' and IsSubmitted=1 then 1 else null end) as PendingCount,
		count(case when MailPieceStatus <> 'InProgress' and (ProcessingTimeStamp between @startdate and @enddate) then 1 else null end) as ScannedCount
		from dbo.ufn_GetDallasBatchDetails(@EstDate)
		where ClientId=@clientId and ((MailPieceStatus = 'InProgress' and IsSubmitted=1) or (MailPieceStatus <> 'InProgress' and (ProcessingTimeStamp between @startdate and @enddate))) AND BatchName IS NOT NULL
		group by BatchName
	),		
	sw as
	(
		select distinct BatchName from dbo.ufn_GetDallasBatches(@EstDate) where AppCode=@appcode
	)
		
	select	ISNULL(t.BatchName,sw.BatchName) as BatchName,
			IsNULL(t.ScannedCount,0) as ScannedLoans,
			(ISNULL(t.PendingCount,0) + ISNULL(t.ScannedCount,0)) as TotalLoans 
			from t
			full outer join sw on t.BatchName = sw.BatchName
			order by BatchName
END

