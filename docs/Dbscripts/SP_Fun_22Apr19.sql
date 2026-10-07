GO
IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_MRDFRequest_PrintTrailBatchId')
DROP INDEX [IX_MRDFRequest_PrintTrailBatchId] ON [PnS].[MRDFRequest]
go
CREATE NONCLUSTERED INDEX [IX_MRDFRequest_PrintTrailBatchId] ON [PnS].[MRDFRequest]
(
	[PrintTrailBatchId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
GO

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND  name = 'SP_RegisterScan')
DROP PROCEDURE [dbo].[SP_RegisterScan]
go
print '[dbo].[SP_RegisterScan] dropped'
GO

CREATE PROCEDURE [dbo].[SP_RegisterScan]
	-- Add the parameters for the stored procedure here
@trackno varchar(255),
@clientCode varchar(128),
@operatorid varchar(50),
@machineid varchar(255),
@btName varchar(255),-- 05PM, 06PM, 07PM, Backlog
@EstDate DATE
AS
BEGIN
declare	@jobid int,
		@pieceid int,
		@mrdfreqdate date,
		@ispullbackrequested bit,
		@isqcrequested bit,
		@disposition varchar(50),
		@mailpiecestatus varchar(255),
		@totalsheet int = null,
		@pdffilename varchar(255),
		@printlocationcl varchar(1024),
		@mailpieceid int,
		@code varchar(50),
		@existjobid int,
		@jobName varchar(512),
		@clientAppCode varchar(128),
		@clientName varchar(255),
		@bt varchar(255),
		@PrintTrailBatchId int = null,
		@minDate datetime =null
	BEGIN TRY
	
		set @trackno = replace(@trackno, 'AUT','')
		set @clientCode = replace(@clientCode, 'AUT','')
		--Get Print Location name
		select @printlocationcl=Name from Common.PrintVendors where VendorCode='CL_DALLAS';
		
		-- Get mailpiece, MRDFRequest data filter by trackingno & printlocation
	
		if(@btName is null)
		begin 
			select 'Invalid' as Code, NULL as TrackingNo, 'Batch name can not be null' as [Message], NULL as [JobName]
			return
		end
		
		--Get batches timings
		select * into #dallasBatchesTimes from (select * from [dbo].[ufn_GetDallasBatches](@EstDate)) as t
		--initilize mindate
		select @minDate= MIN(StartDate) from #dallasBatchesTimes
			
		-- getdata from DB
		SELECT @mailpieceid=m.Id, @ispullbackrequested=m.[IsPullBackRequested],@isqcrequested=m.[IsQCRequested],
		@totalsheet=m.[TotalSheets],
		@jobName=b.[JobName],
		@pieceid=req.[PieceId],@PrintTrailBatchId=req.[PrintTrailBatchId], @jobid=req.[JobId],@mrdfreqdate=req.[Date],@pdffilename=req.[PDFFileName],
		@clientAppCode=c.[AppCode],@clientName=c.[Name],
		@bt=(select [BatchName] from #dallasBatchesTimes where EffectiveReceivedOnEst between StartDate AND EndDate AND ShippingCarrier=d.ShippingCarrier AND ShippingServiceType=d.ShippingServiceType AND AppCode=c.AppCode)
		FROM [AM].[MailPieces] m
		INNER JOIN AM.DocTypes d ON m.DocTypeId=d.Id
		INNER JOIN Common.Clients c ON m.ClientId=c.Id
		left outer join PnS.MRDFRequest req ON m.UniqueId = req.UniqueId
		left outer JOIN PnS.BatchSummary b ON req.JobId = b.PrintBatch
		WHERE m.PrintLocation=@printlocationcl 
		AND m.EffectiveReceivedOnEst >=@minDate and IsSubmitted=1 and m.ShippingStatus='InProgress' and m.TrackingNo=@trackno
			
		--Drop Temp Table
		If(OBJECT_ID('tempdb..#dallasBatchesTimes') Is Not Null)
		Begin
		Drop Table #dallasBatchesTimes
		End


		if(@mailpieceid IS NULL) 
		begin
			select 'Invalid' as Code, NULL as TrackingNo, NULL as [Message], NULL as [JobName]
			return
		end

		--Validate tracking no is assign to this client
		if(@clientCode <> @clientAppCode)
		begin
			select 'Invalid' as Code, @trackno as TrackingNo, REPLACE('This tracking number belongs to @clientName client. Please check your document.','@clientName',@clientName) as [Message], NULL as [JobName]
			return
		end

		-- Validate this mailpiece is assign to this batch or not.
		if(@bt<>@btName)
		begin
			select 'Invalid' as Code, @trackno as TrackingNo, REPLACE('This document belongs to <span style="color:#000"> @bt </span> batch.','@bt',@bt) as [Message], NULL as [JobName]
			return
		end
		
		if exists(select 1 from PnS.MRDFResponse where JobId = @PrintTrailBatchId and PieceId = @pieceid) 
		begin
			select 'Rescan' as Code, @trackno as TrackingNo, NULL as [Message], @jobName as [JobName]
			return
		end

		-- set mailpiece status if ispullbackrequested then update otherwise not
		SET @mailpiecestatus='Shipped'

		if(@ispullbackrequested = 1) 
		begin
			SET @code = 'Pulledback'
			SET @disposition='4' 
			SET @mailpiecestatus='PulledBack'
		end
		else if(@isqcrequested=1)
		begin
			SET @disposition='8'
			SET @code = 'QC'
		end
		else
		begin
			SET @disposition='5'
			SET @code = 'Valid'
		end

		-- Start Transaction
		BEGIN TRANSACTION	
			-- insert data into MRDFResponse table
			INSERT INTO [PnS].[MRDFResponse] ([JobId],[PieceId],[Date],[Feeder2_Fed],[OperatorId],[MachineId],									[ProcessingTimeStamp],[DetailedErrorId],[Disposition],[Input_Fed],[Feeder3_Fed],[Feeder4_Fed],[Feeder5_Fed],[ComputedWeight],[MeterDate],[StatusSource],[PostageMetered]
			,[WeightMeasured],[ExitLocation],[TotalSheets],[PDFFileName],[PackageTrackingNumber])
			VALUES(@PrintTrailBatchId ,@pieceid,@mrdfreqdate,0,@operatorid,@machineid,GETUTCDATE(),'4691'
			,@disposition,'00','0','0','0','0.000','00000000', @printlocationcl,'0.000','0.000',@printlocationcl
			,@totalsheet,@pdffilename,NULL)				
			
				Update [PnS].[MRDFRequest] SET MailPieceStatus = @mailpiecestatus
				where PieceId = @pieceid and PrintTrailBatchId=@PrintTrailBatchId and JobId=@jobid

				-- Check if we are yet to received response for any mail pieces
			if(not exists(select 1 from PnS.MRDFRequest where jobid=@jobid and MailPieceStatus='InProgress'))
			begin
			--Update Batchsummary pipeline status when all mailpieces's response is received
				update PnS.BatchSummary 
				set PipelineStatus = 'MRDFReceived', MRDFReceivedOn = GETUTCDATE() 
				where PrintBatch = @jobid

				if(not exists(select 1 from [PnS].[MRDFRequest] where MailPieceStatus='InProgress' and PrintTrailBatchId=@PrintTrailBatchId))
				begin
					-- Update in PrintTrailBatch of mrdfreceivedon when all response is received.
					update PnS.PrintTrailBatches 
					set MRDFReceivedOn=GETUTCDATE()
					where Id=@PrintTrailBatchId					
				end
			end
		--End Transaction
		COMMIT
		
		select @code as Code, @trackno as TrackingNo, NULL as [Message], @jobName as [JobName]
	END TRY
	BEGIN CATCH
		IF @@TRANCOUNT > 0
			ROLLBACK
	END CATCH
END
go
print '[dbo].[SP_RegisterScan] created'
GO

--------------------function---------------------
DROP FUNCTION [dbo].[ufn_GetDallasBatchDetails]
go
print '[dbo].[ufn_GetDallasBatchDetails] dropped'
GO
CREATE FUNCTION [dbo].[ufn_GetDallasBatchDetails]
(
@EstDate DATE
)
RETURNS @MPs TABLE 
(
	Id int,
	LoanNumber varchar(255) null,
	ClientId int null,
	PrintLocation varchar(255) null,
	EffectiveReceivedOnEst datetime null,
	TrackingNo varchar(255) null,
	IsPullBackRequested bit,
	IsQCRequested bit,
	TotalSheets int,
	DueDate datetime null,
	TotalPages int,
	ShippingStatus varchar(255) null,
	PrintBatch int,
	IsSubmitted bit,
	PipelineStatus varchar(512),
	PrintBatchName varchar(255),
	JobName varchar(512) null,
	SubmittedOn datetime null,
	PieceId int null,
	PrintTrailBatchId int null,
	JobId int null,
	MRDFReqDate datetime null,
	PDFFileName varchar(255)null,
	UniqueId uniqueidentifier null,
	MailPieceStatus varchar(255) null,
	ProcessingTimeStamp datetime null,
	ShippingServiceType varchar(255) null,
	ShippingCarrier varchar(255) null,
	IsBRE bit,
	AppCode varchar(128),
	ClientName varchar(255) null,
	BatchName varchar(128) null
)
AS
begin

	declare @minDate datetime,@pl varchar(255);
	declare @dallasBatchesTimes Table
	(
		AppCode VARCHAR(255),
		BatchName varchar(255),
		StartDate datetime,
		EndDate datetime,
		ShippingCarrier varchar(255),
		ShippingServiceType varchar(255)
	)
	select @pl=Name from Common.PrintVendors where VendorCode='CL_DALLAS'
	--Get batches timings
	--select * into #dallasBatchesTimes from (select * from [dbo].[ufn_GetDallasBatches](@EstDate)) as t
	insert into @dallasBatchesTimes
	select * from [dbo].[ufn_GetDallasBatches](@EstDate)

	--Assign value to mindate
	select @minDate= MIN(StartDate) from @dallasBatchesTimes

	;with q1 as
	(
	SELECT m.Id,m.BusinessFriendlyIdValue as LoanNumber,m.ClientId, m.PrintLocation, m.EffectiveReceivedOnEst, m.TrackingNo,m.IsPullBackRequested,
	m.IsQCRequested,m.TotalSheets, m.DueDate,m.TotalPages,m.ShippingStatus,
	b.PrintBatch,b.IsSubmitted,b.PipelineStatus,preq.PrintBatchName,b.JobName,preq.SubmittedOn,
	req.PieceId,req.PrintTrailBatchId, req.JobId,req.[Date] as MRDFReqDate,req.PDFFileName, req.UniqueId,req.MailPieceStatus, 
	res.ProcessingTimeStamp,
	d.ShippingServiceType,d.ShippingCarrier,d.IsBRE,
	c.AppCode,c.Name as ClientName,
	(select BatchName from @dallasBatchesTimes where EffectiveReceivedOnEst between StartDate AND EndDate AND ShippingCarrier=d.ShippingCarrier AND ShippingServiceType=d.ShippingServiceType  AND AppCode=c.AppCode) BatchName
	FROM [AM].[MailPieces] m
	LEFT OUTER JOIN PnS.MRDFRequest req ON m.UniqueId = req.UniqueId
	LEFT OUTER JOIN PnS.BatchSummary b ON req.JobId = b.PrintBatch
	LEFT OUTER JOIN PnS.PrintTrailBatches preq ON req.PrintTrailBatchId=preq.Id
	LEFT OUTER JOIN PnS.MRDFResponse res ON req.PrintTrailBatchId = res.JobId AND req.PieceId=res.PieceId
	INNER JOIN AM.DocTypes d ON m.DocTypeId=d.Id
	INNER JOIN Common.Clients c ON m.ClientId=c.Id
	WHERE m.PrintLocation=@pl
	AND m.EffectiveReceivedOnEst >=@minDate
	)
	insert into @MPs
	select * from q1 where BatchName is not null

	return
end
go
print '[dbo].[ufn_GetDallasBatchDetails] created'
---------------------------------------------------------------------------------
go
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND  name = 'SP_GetScanStats')
DROP PROCEDURE [dbo].[SP_GetScanStats]
go
print '[dbo].[SP_GetScanStats] dropped'
GO
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
go
print '[dbo].[SP_GetScanStats] created'
go
------------------------------------------------------------------------------------
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND  name = 'SP_GetRecentBatchesStats')
DROP PROCEDURE [dbo].[SP_GetRecentBatchesStats]
go
print '[dbo].[SP_GetRecentBatchesStats] dropped'
GO
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
		where ClientId=@clientId and ((MailPieceStatus = 'InProgress' and IsSubmitted=1) or (MailPieceStatus <> 'InProgress' and (ProcessingTimeStamp between @startdate and @enddate)))
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

go
print '[dbo].[SP_GetRecentBatchesStats] created'
go

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'V' AND  name = 'view_FedExData')
DROP VIEW [dbo].[view_FedExData]
print '[dbo].[view_FedExData] dropped'
go
/****** Object:  View [dbo].[view_FedExData]    Script Date: 4/22/2019 12:04:06 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE VIEW [dbo].[view_FedExData]
AS
SELECT        MP.Id, MP.UniqueId, C.AppCode, MB.PipelineStatus, MP.ShippingStatus, D.DocTypeCode, D.ShippingCarrier, D.ShippingServiceType, MP.ClientId, MP.PrintLocation, 
                         MB.BatchName, MP.BusinessFriendlyIdValue AS LoanNumber, MP.RcptFirstName, MP.RcptLastName, MP.RcptAddressStreet, MP.RcptAddressAptOrSuiteOrOther, 
                         MP.RcptAddressCity, MP.RcptAddressState, MP.RcptAddressZIP, MP.TrackingNo, MB.ReceivedOn, MP.DocumentOrPackageType, MP.IsPickedUp, MP.PickedUpOn, 
                         MP.DueDate, MP.ShippedOn, MP.EffectiveReceivedOnEst
FROM            AM.MailPieces AS MP INNER JOIN
                         AM.MailBatches AS MB ON MB.Id = MP.MailBatchId INNER JOIN
                         AM.DocTypes AS D ON D.Id = MP.DocTypeId INNER JOIN
                         Common.Clients AS C ON MB.ClientId = C.Id
WHERE        (MP.ShippingStatus IN ('InProgress', 'Shipped')) AND (D.ShippingCarrier = 'FedEx') AND (MB.ReceivedOn >= '2019-01-01 05:00:00')

GO
print '[dbo].[view_FedExData] created'
go
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND  name = 'uspweb_FedExPickedUpReport')
DROP PROCEDURE [dbo].[uspweb_FedExPickedUpReport]
print '[dbo].[uspweb_FedExPickedUpReport] dropped'
go

---------------------------------------------------------------------------------
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND  name = 'uspweb_MonthlyReconReport')
DROP PROCEDURE [dbo].[uspweb_MonthlyReconReport]
go
print '[dbo].[uspweb_MonthlyReconReport] dropped'
GO
CREATE PROCEDURE [dbo].[uspweb_MonthlyReconReport]
	@clientId int,
	@startDate datetime,
	@endDate datetime,
	@channel varchar(128)
AS
	select m.UniqueId,b.BatchName,m.BusinessFriendlyIdValue as LoanNumber,m.[FileName],m.RcptFirstName as FirstName,m.RcptLastName as LastName
	,m.RcptAddressStreet as [Address],m.RcptAddressCity as City, m.RcptAddressState as [State],m.RcptAddressZIP as Zip,
	b.ReceivedOn,m.PrintLocation,m.ShippingStatus as [Status],m.TrackingNo,m.TotalPages,m.PrintedOn,m.ShippedOn,
	m.PickedUpOn,d.ShippingCarrier,m.ErrorDesc,
	d.DocTypeCode as DocType,d.ShippingCarrier +' '+d.ShippingServiceType as ShippingMethod,
	ch.Title as Channel,c.AppCode,
	r.JobId,r.PieceId
	FROM [AM].[MailBatches] b, [Common].[SFTChannels] ch,[Common].[Clients] c,[AM].[MailPieces] m
	LEFT OUTER JOIN [AM].[DocTypes] d ON m.DocTypeId=d.Id
	LEFT OUTER JOIN [PnS].[MRDFRequest] r ON m.UniqueId = r.UniqueId AND r.IsPDFReceived=1
	WHERE m.[MailBatchId]=b.Id AND c.Id=b.ClientId AND ch.Id=b.SFTChannelId AND m.ClientId=@clientId
	AND b.ReceivedOn BETWEEN @startDate and @endDate
	AND ISNULL(ch.Code, '') = ISNULL(@channel, ISNULL(ch.Code, ''))
go
print '[dbo].[uspweb_MonthlyReconReport] created'
---------------------------------------------------------------------------------
go
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND  name = 'uspalert_GetDelayedBatchesToSubmit')
DROP PROCEDURE [dbo].[uspalert_GetDelayedBatchesToSubmit]
go
print '[dbo].[uspalert_GetDelayedBatchesToSubmit] dropped'
GO
CREATE PROCEDURE [dbo].[uspalert_GetDelayedBatchesToSubmit] 	
	@cutOffMinutes int
AS
BEGIN
 select mp.Id,mp.businessfriendlyidvalue as LoanNumber,mp.TrackingNo,mb.Id as MailBatchId,rq.UniqueId,bs.IsSubmitted,c.AppCode,mb.BatchName,mb.ReceivedOn,mb.PipelineStatus,mp.ShippingStatus,d.DocTypeCode,d.ShippingCarrier,d.ShippingServiceType,mp.ClientId,pv.Name as PrintLocation,pv.VendorCode,rq.MailPieceStatus  
 from AM.MailPieces mp
 inner join AM.MailBatches mb on mp.MailBatchId=mb.Id
 left outer join Common.PrintTrails pt on mp.PrintTrailId = pt.Id
 left outer join Common.PrintVendors pv on pt.PrintVendorId = pv.Id
 left outer join Common.Clients c on mb.ClientId=c.Id
 left outer join PnS.MRDFRequest rq on mp.UniqueId=rq.UniqueId
 left outer join PnS.BatchSummary bs on rq.JobId=bs.PrintBatch
 left outer join AM.DocTypes d on mp.DocTypeId = d.Id
 where mb.ReceivedOn between DATEADD(day,-7,GETUTCDATE()) and DATEADD(minute,-@cutOffMinutes,GETUTCDATE()) and (mp.ShippingStatus is null or (mp.ShippingStatus in ('InProgress','Shipped') and (rq.UniqueId is null or ISNULL(bs.IsSubmitted,0)=0)) or (mp.ShippingStatus = 'Error' and mp.ErrorDesc IS NULL) or (mp.ShippingStatus = 'PulledBack' and not exists (select Id from AM.PullBackRequests where MailPieceId = mp.Id)) or (mb.ClientId IS NULL))
 or (rq.MailPieceStatus='Shipped' and mp.IsPickedUp <> 1 and mp.DueDate < GETUTCDATE() and d.ShippingCarrier='FedEx' and mb.ReceivedOn>='2019-01-01 05:00:00')
END
go
print '[dbo].[uspalert_GetDelayedBatchesToSubmit] created'
go
------------------------------------------------------------------------------------

if not exists (select * from Common.SystemSettings where id='Web_Search_DisplayLimit')
begin
INSERT INTO [Common].[SystemSettings] ([Id] ,[DataType] ,[Value] ,[Description] ,[IsSystem] ,[DefaultValue] ,[ModifiedBy] ,[ModifiedOn])     VALUES ('Web_Search_DisplayLimit','Int32','1200','No of records to be display in search page.',0,'1200',(select Id from Profile.Users where UserName='system@milestonetechnoconsulting.com'),GETUTCDATE())
print 'insert Web_Search_DisplayLimit'
end

if not exists (select * from Common.SystemSettings where id='Web_Search_DownloadLimit')
begin
INSERT INTO [Common].[SystemSettings] ([Id] ,[DataType] ,[Value] ,[Description] ,[IsSystem] ,[DefaultValue] ,[ModifiedBy] ,[ModifiedOn])     VALUES ('Web_Search_DownloadLimit','Int32','10000','No of records to be download in search page.',0,'10000',(select Id from Profile.Users where UserName='system@milestonetechnoconsulting.com'),GETUTCDATE())
print 'insert Web_Search_DownloadLimit'
end

if not exists (select * from Common.SystemSettings where id='Web_Login_PasswordExpirationIsEnable')
begin
INSERT INTO [Common].[SystemSettings] ([Id] ,[DataType] ,[Value] ,[Description] ,[IsSystem] ,[DefaultValue] ,[ModifiedBy] ,[ModifiedOn])     VALUES ('Web_Login_PasswordExpirationIsEnable','Boolean','true','Is password expired policy enable?',0,'true',(select Id from Profile.Users where UserName='system@milestonetechnoconsulting.com'),GETUTCDATE())
print 'insert Web_Login_PasswordExpirationIsEnable'
end
if not exists (select * from Common.SystemSettings where id='Web_Login_PasswordExpirationDays')
begin
INSERT INTO [Common].[SystemSettings] ([Id] ,[DataType] ,[Value] ,[Description] ,[IsSystem] ,[DefaultValue] ,[ModifiedBy] ,[ModifiedOn])     VALUES ('Web_Login_PasswordExpirationDays','Int32',50,'No of password expired days.',0,50,(select Id from Profile.Users where UserName='system@milestonetechnoconsulting.com'),GETUTCDATE())
print 'insert Web_Login_PasswordExpirationDays'
end
go
print 'Add columns in ProjectRequest'
alter table AM.ProjectRequest
add ServiceLevelOther nvarchar(1024) null,
PaperPrintRequirementsOther nvarchar(1024) null,
PackagesSentToCustomerOther nvarchar(1024) null,
EnvelopesOther nvarchar(1024) null,
BusinessReplyMailEnvelopeOther nvarchar(1024) null,
FilesTransmittedDetail nvarchar(1024) null,
TypeOfFilesDetail nvarchar(1024) null
Go
alter table profile.Users
add LastLoginTimeUTC datetime null,
ChangePasswordDateUTC datetime not null default(getutcdate()),
StatusModifiedBy nvarchar(128) null,
StatusModifiedOn datetime null
Go
alter table profile.Users
add constraint FK_Users_StatusModifiedBy foreign key(StatusModifiedBy)
references profile.Users (Id)
Go