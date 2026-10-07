CREATE PROCEDURE [dbo].[SP_GetScannedMailPieces]
	@appcode varchar(128),
	@batchName varchar(255),
	@startDate datetime,
	@endDate datetime,
	@EstDate DATE
	
AS
BEGIN
	declare @printlocationcl varchar(1024), @clientid int
	set @appcode=REPLACE(@appcode,'AUT','')
	-- Get ClientId
	select @clientid=Id from Common.Clients where AppCode=@appcode;
	-- Get PrintLocation
	select @printlocationcl=Name from Common.PrintVendors where VendorCode='CL_DALLAS';
	--select @printlocationcl=Value from [Common].[SystemSettings] where Id ='Common_PrintLocationCL';

	select TrackingNo,
	(case when IsPullBackRequested = 1 then 'Pulledback' when IsQCRequested = 1 then 'QC' else 'Valid' end) as [Code],JobName,PrintBatchName
	from dbo.ufn_GetDallasBatchDetails(@EstDate)
	where PrintLocation=@printlocationcl and ClientId=@clientId and BatchName=@batchName and MailPieceStatus <> 'InProgress'	and ProcessingTimeStamp between @startDate and @endDate and IsSubmitted=1

END


