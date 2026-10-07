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
		@inprogresscnt int,
		@mailpieceid int,
		@code varchar(50),
		@existjobid int,
		@jobName varchar(512),
		@serviceType varchar(255),
		@clientAppCode varchar(128),
		@clientName varchar(255),
		@bt varchar(255),
		@PrintTrailBatchId int = null
	
	BEGIN TRY
	
		set @trackno = replace(@trackno, 'AUT','')
		set @clientCode = replace(@clientCode, 'AUT','')
		--Get Print Location name
		select @printlocationcl=Name from Common.PrintVendors where VendorCode='CL_DALLAS';
		--SET @printlocationcl=(select Value from [Common].[SystemSettings] where Id ='Common_PrintLocationCL')
		-- Get mailpiece, MRDFRequest data filter by trackingno & printlocation
	
		if(@btName is null)
		begin 
			select 'Invalid' as Code, NULL as TrackingNo, 'Batch name can not be null' as [Message], NULL as [JobName]
			return
		end
		select @mailpieceid= Id,@pieceid=PieceId,@jobid=JobId,@mrdfreqdate=MRDFReqDate,@ispullbackrequested =IsPullBackRequested,@isqcrequested=IsQCRequested,@totalsheet=TotalSheets,@pdffilename=PDFFileName,@jobName=JobName,@serviceType=ShippingServiceType,@clientAppCode=AppCode,@clientName=ClientName,@bt=BatchName,@PrintTrailBatchId=PrintTrailBatchId
		from dbo.ufn_GetDallasBatchDetails(@EstDate)
		where TrackingNo=@trackno and PrintLocation=@printlocationcl and IsSubmitted=1 and ShippingStatus='InProgress'

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
			

			select @inprogresscnt= count(*) from [PnS].[MRDFRequest] where MailPieceStatus='InProgress' and PrintTrailBatchId=@PrintTrailBatchId
			if(@inprogresscnt = 0)
			begin
			-- Update in PrintTrailBatch of mrdfreceivedon when all response is received.
				update PnS.PrintTrailBatches 
				set MRDFReceivedOn=GETUTCDATE()
				where Id=@PrintTrailBatchId
				--Update Batchsummary pipeline status when all mailpieces's response is received
				update PnS.BatchSummary 
				set PipelineStatus = 'MRDFReceived', MRDFReceivedOn = GETUTCDATE() 
				where PrintBatch in (select distinct JobId from PnS.MRDFRequest where PrintTrailBatchId=@PrintTrailBatchId)
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
RETURNS TABLE 
AS
RETURN
(
with db as
(
 select * from [dbo].[ufn_GetDallasBatches](@EstDate)
),
q as
(
SELECT m.Id,m.BusinessFriendlyIdValue as LoanNumber,m.ClientId, m.PrintLocation, m.EffectiveReceivedOnEst, m.TrackingNo,m.IsPullBackRequested,m.IsQCRequested,m.TotalSheets, m.DueDate,m.TotalPages,m.ShippingStatus,
b.PrintBatch,b.IsSubmitted,b.PipelineStatus,preq.PrintBatchName,b.JobName,preq.SubmittedOn,--b.SubmittedOn,
req.PieceId,req.PrintTrailBatchId, req.JobId,req.[Date] as MRDFReqDate,req.PDFFileName, req.UniqueId,req.MailPieceStatus, res.ProcessingTimeStamp,
d.ShippingServiceType,d.ShippingCarrier,d.IsBRE,
c.AppCode,c.Name as ClientName,(select BatchName from db where EffectiveReceivedOnEst between StartDate AND EndDate AND ShippingCarrier=d.ShippingCarrier AND ShippingServiceType=d.ShippingServiceType  AND AppCode=c.AppCode) BatchName
FROM [AM].[MailPieces] m
LEFT OUTER JOIN PnS.MRDFRequest req ON m.UniqueId = req.UniqueId
LEFT OUTER JOIN PnS.BatchSummary b ON req.JobId = b.PrintBatch
LEFT OUTER JOIN PnS.PrintTrailBatches preq ON req.PrintTrailBatchId=preq.Id
LEFT OUTER JOIN PnS.MRDFResponse res ON req.PrintTrailBatchId = res.JobId AND req.PieceId=res.PieceId
INNER JOIN AM.DocTypes d ON m.DocTypeId=d.Id
INNER JOIN Common.Clients c ON m.ClientId=c.Id
WHERE m.PrintLocation=(select Name from Common.PrintVendors where VendorCode='CL_DALLAS') 
AND m.EffectiveReceivedOnEst >=(select MIN(StartDate) from db)
)
select * from q where BatchName is not null 
)

go
print '[dbo].[ufn_GetDallasBatchDetails] created'
---------------------------------------------------------------------------------
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
	declare @printlocationcl varchar(1024), @totalPending int, @scannedCount int, @clientid int

	-- Set PrintLocation value
	select @printlocationcl=Name from Common.PrintVendors where VendorCode='CL_DALLAS';
	--select @printlocationcl=Name from Common.PrintVendors where VendorCode='CL_DALLAS';
	--select @printlocationcl = Value from [Common].[SystemSettings] where Id ='Common_PrintLocationCL'

	-- Remove AUT from app code
	set @appcode = replace(@appcode, 'AUT','')

	-- Get client id
	select @clientid=Id from Common.Clients where AppCode=@appcode

	-- Calculate Total Pending Scanned
		select @totalPending=count(Id) from dbo.ufn_GetDallasBatchDetails(@EstDate)
		where Printlocation=@printlocationcl and ClientId=@clientId
		and MailPieceStatus = 'InProgress' and IsSubmitted=1
		

	-- Calculate Total Scanned count
		select @scannedCount=count(Id) from dbo.ufn_GetDallasBatchDetails(@EstDate)
		where Printlocation=@printlocationcl and ClientId=@clientId
		and MailPieceStatus <> 'InProgress' and (ProcessingTimeStamp between @startdate and @enddate)
	
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
	
	declare @bn nvarchar(max),@st time, @et time,
	@printlocationcl varchar(1024),  @clientid int,
	@sd datetime, @ed datetime, @mindt datetime=null

	--Get Client Id, and remove AUT from client appcode
	set @appcode=REPLACE(@appcode,'AUT','')
	select @clientid=Id from Common.Clients where AppCode=@appcode;
	-- Get PrintLocation
	select @printlocationcl=Name from Common.PrintVendors where VendorCode='CL_DALLAS';
	--select @printlocationcl=Value from [Common].[SystemSettings] where Id ='Common_PrintLocationCL';
	-- Get Pending count batch(sweep) wise
	with pc as
	(
		select distinct BatchName,count(Id) as PendingCount from dbo.ufn_GetDallasBatchDetails(@EstDate)
		where Printlocation=@printlocationcl and ClientId=@clientId
		and MailPieceStatus = 'InProgress' and IsSubmitted=1
		group by BatchName
	),
	-- Get Scanned count batch(sweep) wise
	sc as
	(
		select distinct BatchName,count(Id) as ScannedCount from dbo.ufn_GetDallasBatchDetails(@EstDate)
		where Printlocation=@printlocationcl and ClientId=@clientId and IsSubmitted=1
		and MailPieceStatus <> 'InProgress' and ProcessingTimeStamp between @startdate and @enddate
		group by BatchName
	),
	sw as
	(
		select distinct BatchName from dbo.ufn_GetDallasBatches(@EstDate) where AppCode=@appcode
	),

	t as
	(	
		select	ISNULL(sc.BatchName,pc.BatchName) as BatchName,
				ISNULL(pc.PendingCount,0) as PendingCount,
				ISNULL(sc.ScannedCount,0) as ScannedCount
		from sc full outer join pc on sc.BatchName=pc.BatchName
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
------------------------------------------------------------------------------------
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND  name = 'uspalert_GetDelayedBatchesToSubmit')
DROP PROCEDURE [dbo].[uspalert_GetDelayedBatchesToSubmit]
go
print '[dbo].[uspalert_GetDelayedBatchesToSubmit] dropped'
GO
CREATE PROCEDURE [dbo].[uspalert_GetDelayedBatchesToSubmit] 	
	@cutOffMinutes int
AS
BEGIN
 SELECT mp.Id,mb.Id as MailBatchId,rq.UniqueId,bs.IsSubmitted,c.AppCode,mb.BatchName,mb.ReceivedOn,mb.PipelineStatus,mp.ShippingStatus,d.DocTypeCode,d.ShippingCarrier,d.ShippingServiceType,mp.ClientId,pv.Name as PrintLocation,pv.VendorCode,rq.MailPieceStatus  from AM.MailPieces mp
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
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND  name = 'uspalert_GetDelayedBatchesToSubmit')
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
AND m.EffectiveReceivedOnEst BETWEEN @startDate and @endDate
AND ISNULL(ch.Code, '') = ISNULL(@channel, ISNULL(ch.Code, ''))

go
print '[dbo].[uspweb_MonthlyReconReport] created'
go
