CREATE PROCEDURE [dbo].[SP_GetScanStats_Temp]	
	@EstDate DATE,
	@startdate datetime,
	@enddate datetime
 AS
BEGIN
	declare @totalPending int, @scannedCount int, @clientid int
	
	-- Remove AUT from app code
	--set @appcode = replace(@appcode, 'AUT','')

	-- Get client id
	--select @clientid=Id from Common.Clients where AppCode=@appcode

	--Calculate Pending & Scan count
	 ;with cte1 as
	 (
		select count(case when MailPieceStatus = 'InProgress' and IsSubmitted=1 and CAST(EffectiveReceivedOnEst AS DATE)=@EstDate then 1 else null end) as totalPending,
		count(case when MailPieceStatus <> 'InProgress' and (ProcessingTimeStamp between @startdate and @enddate) then 1 else null end) as			scannedCount,AppCode,ClientId
		from dbo.ufn_GetDallasBatchDetails(@EstDate)
		where ((MailPieceStatus = 'InProgress' and IsSubmitted=1) or (MailPieceStatus <> 'InProgress' and (ProcessingTimeStamp between @startdate and @enddate))) group by AppCode,ClientId
		)

		-- Return result
	select scannedCount as [ScanCount],totalPending + scannedCount as [TotalCount],AppCode FROM cte1

END