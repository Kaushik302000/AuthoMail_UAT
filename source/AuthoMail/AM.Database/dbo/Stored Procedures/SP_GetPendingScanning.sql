CREATE PROCEDURE [dbo].[SP_GetPendingScanning]
	@appcode varchar(128),
	@EstDate DATE
AS
BEGIN
	declare @clientid int
	set @appcode = replace(@appcode, 'AUT','')
	select @clientid=Id from Common.Clients where AppCode=@appcode;

	select TrackingNo,LoanNumber,SubmittedOn,PDFFileName as [FileName],JobName,PrintBatchName,MailPieceStatus,BatchName,DueDate,FilePath 
	from dbo.ufn_GetDallasBatchDetails(@EstDate)
	where ClientId=@clientid and MailPieceStatus='InProgress' and ProcessingTimeStamp is null and IsSubmitted=1

END


