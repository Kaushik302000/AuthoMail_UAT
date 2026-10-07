CREATE PROCEDURE [dbo].[SP_GetScanStats]
	@appcode varchar(128),
	@EstDate DATE,
	@startdate datetime,
	@enddate datetime
 AS
BEGIN
	declare @totalPending int, @scannedCount int, @clientid int
	
	-- Remove AUT from app code
	set @appcode = replace(@appcode, 'AUT','')

	-- Get client id
	select @clientid=Id from Common.Clients where AppCode=@appcode

	--Calculate Pending & Scan count

		select 
		@totalPending=count(case when MailPieceStatus = 'InProgress' and IsSubmitted=1 then 1 else null end),
		@scannedCount=count(case when MailPieceStatus <> 'InProgress' and (ProcessingTimeStamp between @startdate and @enddate) then 1 else null end)
		from dbo.ufn_GetDallasBatchDetails(@EstDate)
		where ClientId=@clientId and ((MailPieceStatus = 'InProgress' and IsSubmitted=1) or (MailPieceStatus <> 'InProgress' and (ProcessingTimeStamp between @startdate and @enddate)))

		-- Return result
	select @scannedCount as [ScanCount],@totalPending + @scannedCount as [TotalCount]

END
