CREATE PROCEDURE [dbo].[SP_GetScanReportData]
	@loanNumber varchar(255),
	@trackingNo varchar(255),
	@appcode varchar(128),
	@EstDate DATE,
	@substartdate datetime,
	@subenddate datetime,
	@scanstartDate datetime,
	@scanendDate datetime
AS
BEGIN
	declare @printlocationcl varchar(255),@clientid int
	-- Get Print location 
	select @printlocationcl=Name from Common.PrintVendors where VendorCode='CL_DALLAS';
	--select @printlocationcl = Value from [Common].[SystemSettings] where Id ='Common_PrintLocationCL'
	-- Assign Client id value
	set @appcode = replace(@appcode, 'AUT','')
	select @clientid=Id from Common.Clients where AppCode=@appcode;
	
	select JobName,PrintBatchName,BatchName,TrackingNo as BarcodeValue,TotalPages,ProcessingTimeStamp as ShippedOn,LoanNumber,MailPieceStatus,SubmittedOn as ReceivedOn,PDFFileName as [FileName],FilePath from dbo.ufn_GetDallasBatchDetails(@EstDate)
	where ClientId=@clientId and PrintLocation=@printlocationcl and ProcessingTimeStamp is not null and IsSubmitted=1
	and SubmittedOn between ISNULL(@substartdate,SubmittedOn) and ISNULL(@subenddate,SubmittedOn)
	and ProcessingTimeStamp between ISNULL(@scanstartDate,ProcessingTimeStamp) and ISNULL(@scanendDate,ProcessingTimeStamp)
	and TrackingNo = ISNULL(@trackingNo,TrackingNo)
	and LoanNumber = ISNULL(@loanNumber,LoanNumber)
	order by SubmittedOn asc

END


