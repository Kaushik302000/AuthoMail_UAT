-- Remove Only--
GO
IF EXISTS (SELECT * FROM sys.objects WHERE type IN ( N'FN', N'IF', N'TF', N'FS', N'FT' ) AND  name = 'ufn_GetPCIBundleSweepTimings')
DROP FUNCTION [dbo].[ufn_GetPCIBundleSweepTimings]
GO
-- End Remove Only--

/*==========================================================================================*/

--- Drop & Create Functions ---

GO
IF EXISTS (SELECT * FROM sys.objects WHERE type IN ( N'FN', N'IF', N'TF', N'FS', N'FT' ) AND  name = 'ufn_GetDallasBatchDetails')
DROP FUNCTION [dbo].[ufn_GetDallasBatchDetails]
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
GO


IF EXISTS (SELECT * FROM sys.objects WHERE type IN ( N'FN', N'IF', N'TF', N'FS', N'FT' ) AND  name = 'ufn_GetSweepTimings')
DROP FUNCTION [dbo].[ufn_GetSweepTimings]
GO
CREATE FUNCTION [dbo].[ufn_GetSweepTimings] 
( 
 
)
RETURNS @T TABLE
(
 SrNo int,
 AppCode VARCHAR(20),
 VendorCode VARCHAR(20),
 WeekDayType varchar(10),
 SweepType VARCHAR(20),
 SweepTime time 
) 
AS
begin
 --ALASKA
 insert into @T (SrNo,AppCode,VendorCode,WeekDayType,SweepType,SweepTime) 
 values (1,'ALASKA','PCI','Weekday','Continuous','00:00:00')
 insert into @T (SrNo,AppCode,VendorCode,WeekDayType,SweepType,SweepTime) 
 values (1,'ALASKA','PCI','Saturday','Continuous','00:00:00')
 
 --USAA
 insert into @T (SrNo,AppCode,VendorCode,WeekDayType,SweepType,SweepTime) 
 values (1,'USAA','CL_DALLAS','Weekday','Continuous','00:00:00')
 insert into @T (SrNo,AppCode,VendorCode,WeekDayType,SweepType,SweepTime) 
 values (1,'USAA','CL_DALLAS','Saturday','Continuous','00:00:00')

 insert into @T (SrNo,AppCode,VendorCode,WeekDayType,SweepType,SweepTime) 
 values (1,'USAA','PCI','Weekday','OnSchedule','15:00:00')
 insert into @T (SrNo,AppCode,VendorCode,WeekDayType,SweepType,SweepTime) 
 values (2,'USAA','PCI','Weekday','OnSchedule','19:30:00')
 insert into @T (SrNo,AppCode,VendorCode,WeekDayType,SweepType,SweepTime) 
 values (3,'USAA','PCI','Weekday','OnSchedule','20:00:00')
 
 insert into @T (SrNo,AppCode,VendorCode,WeekDayType,SweepType,SweepTime) 
 values (1,'USAA','PCI','Saturday','OnSchedule','15:00:00')
 insert into @T (SrNo,AppCode,VendorCode,WeekDayType,SweepType,SweepTime) 
 values (2,'USAA','PCI','Saturday','OnSchedule','19:30:00')
 insert into @T (SrNo,AppCode,VendorCode,WeekDayType,SweepType,SweepTime) 
 values (3,'USAA','PCI','Saturday','OnSchedule','20:00:00')
 
 --HSBC
 insert into @T (SrNo,AppCode,VendorCode,WeekDayType,SweepType,SweepTime) 
 values (1,'HSBC','PCI','Weekday','OnSchedule','15:00:00')
 insert into @T (SrNo,AppCode,VendorCode,WeekDayType,SweepType,SweepTime) 
 values (2,'HSBC','PCI','Weekday','OnSchedule','19:30:00')
 insert into @T (SrNo,AppCode,VendorCode,WeekDayType,SweepType,SweepTime) 
 values (3,'HSBC','PCI','Weekday','OnSchedule','20:00:00')
 return
end
Go


IF EXISTS (SELECT * FROM sys.objects WHERE type IN ( N'FN', N'IF', N'TF', N'FS', N'FT' ) AND  name = 'ufn_GetDallasBatchTimings')
DROP FUNCTION [dbo].[ufn_GetDallasBatchTimings]
GO
 CREATE FUNCTION [dbo].[ufn_GetDallasBatchTimings]
 (
 @EstDate DATE
 )
 RETURNS @BatchTimings TABLE 
 (
  AppCode VARCHAR(255),
  BatchName VARCHAR(255),
  StartTime VARCHAR(255),
  EndTime VARCHAR(255),
  ShippingCarrier VARCHAR(255),
  ShippingServiceType VARCHAR(255)
 )
 AS
 BEGIN
  
  DECLARE @DayofWeek INT = DATEPART(dw,@EstDate)

  IF(@DayofWeek=7)
  BEGIN
   INSERT INTO @BatchTimings (AppCode, BatchName,StartTime,EndTime,ShippingCarrier,ShippingServiceType) 
   VALUES ('USAA','10AM','00:00:00','11:00:00','FedEx','PRIORITY_OVERNIGHT')   
   INSERT INTO @BatchTimings (AppCode, BatchName,StartTime,EndTime,ShippingCarrier,ShippingServiceType) 
   VALUES ('USAA','01PM','11:00:01','14:00:00','FedEx','PRIORITY_OVERNIGHT')

   INSERT INTO @BatchTimings (AppCode, BatchName,StartTime,EndTime,ShippingCarrier,ShippingServiceType) 
   VALUES ('USAA','10AM','00:00:00','11:00:00','USPS','PRIORITY_EXPRESS')   
   INSERT INTO @BatchTimings (AppCode, BatchName,StartTime,EndTime,ShippingCarrier,ShippingServiceType) 
   VALUES ('USAA','01PM','11:00:01','14:00:00','USPS','PRIORITY_EXPRESS')

   INSERT INTO @BatchTimings (AppCode, BatchName,StartTime,EndTime,ShippingCarrier,ShippingServiceType) 
   VALUES ('USAA','10AM','00:00:00','11:00:00','USPS','PRIORITY')  
   INSERT INTO @BatchTimings (AppCode, BatchName,StartTime,EndTime,ShippingCarrier,ShippingServiceType) 
   VALUES ('USAA','01PM','11:00:01','14:00:00','USPS','PRIORITY')
  END
  ELSE
  BEGIN
   
   INSERT INTO @BatchTimings (AppCode, BatchName,StartTime,EndTime,ShippingCarrier,ShippingServiceType) 
   VALUES ('USAA','12PM','00:00:00','13:00:00','FedEx','PRIORITY_OVERNIGHT')
   INSERT INTO @BatchTimings (AppCode, BatchName,StartTime,EndTime,ShippingCarrier,ShippingServiceType) 
   VALUES ('USAA','05PM','13:00:01','18:00:00','FedEx','PRIORITY_OVERNIGHT')
   INSERT INTO @BatchTimings (AppCode, BatchName,StartTime,EndTime,ShippingCarrier,ShippingServiceType) 
   VALUES ('USAA','06PM','18:00:01','19:00:00','FedEx','PRIORITY_OVERNIGHT')
   INSERT INTO @BatchTimings (AppCode, BatchName,StartTime,EndTime,ShippingCarrier,ShippingServiceType) 
   VALUES ('USAA','07PM','19:00:01','20:00:00','FedEx','PRIORITY_OVERNIGHT')

   INSERT INTO @BatchTimings (AppCode, BatchName,StartTime,EndTime,ShippingCarrier,ShippingServiceType) 
   VALUES ('USAA','12PM','00:00:00','13:00:00','USPS','PRIORITY_EXPRESS')
   INSERT INTO @BatchTimings (AppCode, BatchName,StartTime,EndTime,ShippingCarrier,ShippingServiceType) 
   VALUES ('USAA','05PM','13:00:01','18:00:00','USPS','PRIORITY_EXPRESS')

   INSERT INTO @BatchTimings (AppCode, BatchName,StartTime,EndTime,ShippingCarrier,ShippingServiceType) 
   VALUES ('USAA','12PM','00:00:00','13:00:00','USPS','PRIORITY')
   INSERT INTO @BatchTimings (AppCode, BatchName,StartTime,EndTime,ShippingCarrier,ShippingServiceType) 
   VALUES ('USAA','05PM','13:00:01','18:00:00','USPS','PRIORITY')

   -- INSERT INTO @BatchTimings (AppCode, BatchName,StartTime,EndTime,ShippingCarrier,ShippingServiceType) 
   -- VALUES ('HSBC','12PM','00:00:00','13:00:00','USPS','IMB')
   -- INSERT INTO @BatchTimings (AppCode, BatchName,StartTime,EndTime,ShippingCarrier,ShippingServiceType) 
   -- VALUES ('HSBC','05PM','13:00:01','18:00:00','USPS','IMB')
   -- INSERT INTO @BatchTimings (AppCode, BatchName,StartTime,EndTime,ShippingCarrier,ShippingServiceType) 
   -- VALUES ('HSBC','06PM','18:00:01','19:00:00','USPS','IMB')
   -- INSERT INTO @BatchTimings (AppCode, BatchName,StartTime,EndTime,ShippingCarrier,ShippingServiceType) 
   -- VALUES ('HSBC','07PM','19:00:01','20:00:00','USPS','IMB')
  END
  
  RETURN
 END
 Go


 IF EXISTS (SELECT * FROM sys.objects WHERE type IN ( N'FN', N'IF', N'TF', N'FS', N'FT' ) AND  name = 'ufn_GetDallasBatches')
DROP FUNCTION [dbo].[ufn_GetDallasBatches]
GO

 CREATE FUNCTION [dbo].[ufn_GetDallasBatches]
 (
 @EstDate DATE
 )
 RETURNS @sweep TABLE 
 (
  AppCode VARCHAR(255),
  BatchName varchar(255),
  StartDate datetime,
  EndDate datetime,
  ShippingCarrier varchar(255),
  ShippingServiceType varchar(255)
 )
 AS
 BEGIN
  DECLARE @todayDate DATE
  SELECT @todayDate=@EstDate
  
  INSERT INTO @sweep (AppCode, BatchName,StartDate,EndDate,ShippingCarrier,ShippingServiceType) 
  SELECT AppCode, BatchName,cast(@todayDate AS DATETIME)+cast(StartTime AS DATETIME),cast(@todayDate AS DATETIME)+cast(EndTime AS DATETIME),ShippingCarrier,ShippingServiceType from [dbo].[ufn_GetDallasBatchTimings](@EstDate)  
  
  
  INSERT INTO @sweep (AppCode, BatchName,StartDate,EndDate,ShippingCarrier,ShippingServiceType) 
  VALUES ('USAA','Backlog',DateAdd(day,-7, cast(@todayDate AS DATETIME)),DateAdd(second,-1, cast(@todayDate AS DATETIME)),'FedEx','PRIORITY_OVERNIGHT')
  
  INSERT INTO @sweep (AppCode, BatchName,StartDate,EndDate,ShippingCarrier,ShippingServiceType) 
  VALUES ('USAA','Backlog',DateAdd(day,-7, cast(@todayDate AS DATETIME)),DateAdd(second,-1, cast(@todayDate AS DATETIME)),'USPS','PRIORITY_EXPRESS')

  INSERT INTO @sweep (AppCode, BatchName,StartDate,EndDate,ShippingCarrier,ShippingServiceType) 
  VALUES ('USAA','Backlog',DateAdd(day,-7, cast(@todayDate AS DATETIME)),DateAdd(second,-1, cast(@todayDate AS DATETIME)),'USPS','PRIORITY')

  -- INSERT INTO @sweep (AppCode, BatchName,StartDate,EndDate,ShippingCarrier,ShippingServiceType) 
  -- VALUES ('HSBC','Backlog',DateAdd(day,-7, cast(@todayDate AS DATETIME)),DateAdd(second,-1, cast(@todayDate AS DATETIME)),'USPS','IMB')

  RETURN
 END
 Go

-- End Functions --
/*==========================================================================================*/


--- Drop & Create Stored Procedure ---

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND  name = 'usp_GetPullBackRequestDataByJobId')
DROP PROCEDURE [dbo].[usp_GetPullBackRequestDataByJobId]
GO
CREATE PROCEDURE [dbo].[usp_GetPullBackRequestDataByJobId]
@jobId int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	
	BEGIN TRAN
	--Pulled back logic
	UPDATE [PnS].[LogBatchTimeLine]
	SET
	[PCIBatchPrepEnd]=CAST('1753-1-1' AS DATETIME)
	WHERE BatchId=@jobId

	DECLARE @PnSBatchUniqueId UNIQUEIDENTIFIER,@AMMailBatchId INT

	SET @PnSBatchUniqueId=(SELECT BatchIdentifier FROM PnS.BatchSummary WHERE PrintBatch=@jobId)
	
	SET @AMMailBatchId = (SELECT MailBatchId FROM AM.PrintBatches WHERE UniqueID=@PnSBatchUniqueId)
	
	SELECT * FROM [AM].[PullBackRequests] 
	WHERE MailPieceUniqueId IN (SELECT UniqueId FROM AM.MailPieces WHERE MailBatchId=@AMMailBatchId)
	

	COMMIT TRAN

END
Go

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND  name = 'usp_UpdateAMMailBatchesStatus')
DROP PROCEDURE [dbo].[usp_UpdateAMMailBatchesStatus]
GO

CREATE PROCEDURE [dbo].[usp_UpdateAMMailBatchesStatus] 
	-- Add the parameters for the stored procedure here
	@MailPieceUniqueId uniqueidentifier,
	@shippingResponseReceievedOn datetime
AS
BEGIN
	DECLARE @mailBatchId int
	DECLARE @totalMailPieces int
	DECLARE @totalPulledBackMailPieces int
	DECLARE @printBatchId int
	DECLARE @totalPulledBackPrintBatchesMailPieces int
	DECLARE @totalPrintBatchesMailPieces int
	DECLARE @batchSummaryId int
	DECLARE @printbatchUniqueId uniqueidentifier
	DECLARE @totalBatchSummaryMailPieces int
	DECLARE @totalPulledBackBatchSummaryMailPieces int

	SELECT @mailBatchId = MailBatchId,@printBatchId = PrintBatchId FROM AM.MailPieces WHERE UniqueId = @MailPieceUniqueId
	
	SELECT @printbatchUniqueId = UniqueId FROM AM.PrintBatches WHERE Id = @printBatchId

	SELECT @batchSummaryId = PrintBatch FROM PnS.BatchSummary WHERE BatchIdentifier = @printbatchUniqueId

	SELECT @totalMailPieces = COUNT(*) FROM AM.MailPieces WHERE MailBatchId = @mailBatchId

	--Print @totalMailPieces

	SELECT @totalPulledBackMailPieces = COUNT(*) FROM AM.MailPieces WHERE MailBatchId = @mailBatchId AND IsPulledBack = 1

	SELECT @totalPulledBackPrintBatchesMailPieces = COUNT(*) FROM AM.MailPieces WHERE PrintBatchId = @printBatchId AND IsPulledBack = 1
	
	SELECT @totalPrintBatchesMailPieces = COUNT(*) FROM AM.MailPieces WHERE PrintBatchId = @printBatchId

	SELECT @totalBatchSummaryMailPieces = COUNT(*) FROM PnS.MrdfRequest WHERE JobId = @batchSummaryId

	SELECT @totalPulledBackBatchSummaryMailPieces = COUNT(*) FROM PnS.MrdfRequest WHERE JobId = @batchSummaryId AND MailPieceStatus = 'PulledBack'
	
	
	IF (@totalBatchSummaryMailPieces = @totalPulledBackBatchSummaryMailPieces and @totalBatchSummaryMailPieces <> 0)
	BEGIN	
		UPDATE PnS.BatchSummary SET BatchStatus = 'PulledBack',ReportPreppedOn = @shippingResponseReceievedOn,ReportedBackOn = @shippingResponseReceievedOn,PipelineStatus = 'ReportedBack' WHERE PrintBatch = @batchSummaryId
	END
		
	IF (@totalPrintBatchesMailPieces = @totalPulledBackPrintBatchesMailPieces)
	BEGIN
		UPDATE AM.PrintBatches SET BatchStatus = 'PulledBack',ShippingResponseReceivedOn = @shippingResponseReceievedOn,ReportPreppedOn = @shippingResponseReceievedOn,ReportedBackOn = @shippingResponseReceievedOn WHERE Id = @printBatchId	
	END
	
	IF (@totalMailPieces = @totalPulledBackMailPieces)
	BEGIN	
		UPDATE AM.MailBatches SET BatchStatus = 'PulledBack',ShippingResponseReceivedOn = @shippingResponseReceievedOn,PipelineStatus = 'ShippingResponseReceived' WHERE Id = @mailBatchId		
	END

END

GO
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND  name = 'usp_GetBatchSummaryById')
DROP PROCEDURE [dbo].[usp_GetBatchSummaryById]
GO

CREATE PROCEDURE [dbo].[usp_GetBatchSummaryById]
	-- Add the parameters for the stored procedure here
	@batchId int
AS
BEGIN

	SELECT PrintBatch, ClientId, JobName, PrintFileName, BatchIdentifier, PipelineStatus, ValidationStatus, OnValidationErrors,
 ValidationErrors, BatchStatus, IsReceived, ReceivedOn, IsDownloaded, DownloadedOn, IsPrepped, PreppedOn, IsSubmitted,
 SubmittedOn, IsMRDFReceived, MRDFReceivedOn, IsReportPrepped, ReportPreppedOn, IsReportedBack, ReportedBackOn, IsSuspended, 
 SuspendedOn, IsCancelled, CancelledOn, JulianDate, Images, DocTypeId,SLADays,DueDate,PrintLocation,PrintTrailId
  FROM [PnS].[BatchSummary] WHERE PrintBatch = @batchId
END
Go

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND  name = 'usp_GetMRDFRequestDataByPrintTrailId')
DROP PROCEDURE [dbo].[usp_GetMRDFRequestDataByPrintTrailId]
GO

CREATE PROCEDURE [dbo].[usp_GetMRDFRequestDataByPrintTrailId] 
@printTrailId INT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT REQ.*,MP.Id as MailPieceId,MP.EffectiveReceivedOnEst, BS.ClientId,BS.DocTypeId,D.ShippingCarrier,
 D.ShippingServiceType,C.AppCode
 FROM PnS.MRDFRequest REQ
 INNER JOIN PnS.BatchSummary BS ON REQ.JobId=BS.PrintBatch
 LEFT OUTER JOIN AM.MailPieces MP ON REQ.UniqueId=MP.UniqueId
 INNER JOIN AM.DocTypes D ON BS.DocTypeId=D.Id
 INNER JOIN Common.Clients C ON C.Id=BS.ClientId
 WHERE BS.PipelineStatus='Prepped' AND  IsPrepped = 1 AND IsSubmitted = 0
 AND BS.PrintTrailId=@printTrailId
	
END
GO


IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND  name = 'usp_GetMRDFResponseDataByPrintTrailBatchId')
DROP PROCEDURE [dbo].[usp_GetMRDFResponseDataByPrintTrailBatchId]
GO
CREATE PROCEDURE [dbo].[usp_GetMRDFResponseDataByPrintTrailBatchId]
@printTrailBatchId int
AS
BEGIN
 -- SET NOCOUNT ON added to prevent extra result sets from
 -- interfering with SELECT statements.
 SET NOCOUNT ON;
 
 SELECT JobId, PieceId, Date, Feeder2_Fed, OperatorId, MachineId, ProcessingTimeStamp, DetailedErrorId
 , Disposition, Input_Fed, Feeder3_Fed, Feeder4_Fed, Feeder5_Fed, ComputedWeight, MeterDate
 , StatusSource, PostageMetered, WeightMeasured, ExitLocation, TotalSheets, PDFFileName, PackageTrackingNumber 
 FROM [Pns].[MRDFResponse] MS 
 WHERE MS.JobId=@printTrailBatchId
END
Go

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND  name = 'usp_GetSubmittedBatches')
DROP PROCEDURE [dbo].[usp_GetSubmittedBatches]
GO

CREATE PROCEDURE [dbo].[usp_GetSubmittedBatches]
AS
BEGIN

SELECT B.Id as PrintTrailBatchId,B.PrintBatchName
FROM [PnS].PrintTrailBatches B
INNER JOIN Common.PrintTrails PT ON B.PrintTrailId=PT.Id
INNER JOIN Common.PrintVendors PV ON PT.PrintVendorId=PV.Id
WHERE B.IsSubmitted=1 AND B.IsMRDFReceived=0 

END
GO

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND  name = 'usp_GetPrintBatchDetailByPrintBatchTrailId')
DROP PROCEDURE [dbo].[usp_GetPrintBatchDetailByPrintBatchTrailId]
GO
CREATE PROCEDURE [dbo].[usp_GetPrintBatchDetailByPrintBatchTrailId]
@printTrailBatchId INT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

   SELECT * 
   FROM PnS.PrintTrailBatches
   WHERE Id=@printTrailBatchId
END

GO

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND  name = 'usp_GetPrintTrailDetailByPrintTrailId')
DROP PROCEDURE [dbo].[usp_GetPrintTrailDetailByPrintTrailId]
GO
CREATE PROCEDURE [dbo].[usp_GetPrintTrailDetailByPrintTrailId] 
@printTrailId INT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	
	SELECT TOP 1 PT.id as PrintTrailId, PT.PrintVendorId,PT.TrailName, PT.SimulateTrail,
	PV.Name as PrintVendorName,PV.BusinessCode,PV.VendorCode ,PV.SFTChannelId,
	SC.Protocol,SC.Host,SC.UserName,SC.[Password],SC.ToADEDir,SC.FromADEDir 
	FROM Common.PrintTrails PT
	INNER JOIN Common.PrintVendors PV ON PV.Id=PT.PrintVendorId
	INNER JOIN Common.SFTChannels SC ON PV.SFTChannelId=SC.Id
	WHERE PT.Id=@printTrailId
END

GO
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND  name = 'usp_GetPrintTrailDetailByPrintTrailName')
DROP PROCEDURE [dbo].[usp_GetPrintTrailDetailByPrintTrailName]
GO
CREATE PROCEDURE [dbo].[usp_GetPrintTrailDetailByPrintTrailName] 
@printTrailName VARCHAR(255)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	
	SELECT TOP 1 PT.id as PrintTrailId, PT.PrintVendorId,PT.TrailName, PT.SimulateTrail,
	PV.Name as PrintVendorName,PV.BusinessCode,PV.VendorCode ,PV.SFTChannelId,
	SC.Protocol,SC.Host,SC.UserName,SC.[Password],SC.ToADEDir,SC.FromADEDir 
	FROM Common.PrintTrails PT
	INNER JOIN Common.PrintVendors PV ON PV.Id=PT.PrintVendorId
	INNER JOIN Common.SFTChannels SC ON PV.SFTChannelId=SC.Id
	WHERE PT.TrailName=@printTrailName
END

GO

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND  name = 'usp_GetMRDFResponseDummyDataByPrintTrailBatchId')
DROP PROCEDURE [dbo].[usp_GetMRDFResponseDummyDataByPrintTrailBatchId]
GO
CREATE PROCEDURE [dbo].[usp_GetMRDFResponseDummyDataByPrintTrailBatchId]
@printTrailBatchId int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT Req.PrintTrailBatchId as JobId, Req.PieceId, Req.[Date], Req.TotalSheets, Req.InputWeight, Req.Feeder3_Sel, Req.Feeder4_Sel
		, Req.Feeder5_Sel, Req.LowCap_Sel, Req.NoSeal_Sel, Req.Feeder2_Sel, Req.ZipMark_Sel, Req.Address1, Req.Address2
		, Req.Zip5, Req.Zip4, Req.Zip2, Req.DivertVS2, Req.DivertVS3
		, Req.DivertVS4, Req.ReturnLogoBitMap, Req.ReturnLine1, Req.ReturnLine2, Req.ReturnLine3, Req.ReturnLine4
		, Req.ReturnLine5, Req.ReturnLine6, Req.GroupNumber, Req.SocialSecNumber, Req.CheckAmount, Req.PassThru
		, Req.Expansion, Req.EndofRecord, Req.UniqueId, Req.PDFFileName, Req.ErrId, Req.ErrDesc, Req.MailPieceStatus, Req.IsPDFReceived 
		, Req.Feeder2_Sel as Feeder2_Fed
		,NULL AS PackageTrackingNumber
		,'RichardP' as OperatorId,'PCI09   ' as MachineId
		,GETUTCDATE() as ProcessingTimeStamp
		,'4691 ' as DetailedErrorId,'00' as Input_Fed,'0' as Feeder3_Fed,'0' as Feeder4_Fed,'0' as Feeder5_Fed
		,CASE WHEN P.IsPullBackRequested=1 AND P.PulledBackOn IS NULL THEN '4 ' WHEN P.IsQCRequested=1 THEN '8 ' ELSE '5 ' END as Disposition
		,'0.00 ' as ComputedWeight, '00000000' as MeterDate,'Mailrun_Files            ' as StatusSource
		,'0.0000' as PostageMetered,'000 ' as WeightMeasured,'Mailrun_Files            ' as ExitLocation
		,Req.PrintTrailBatchId
		FROM [PnS].[MRDFRequest] Req
		INNER JOIN [AM].[MailPieces] P ON P.UniqueId=Req.UniqueId
		WHERE Req.PrintTrailBatchId = @printTrailBatchId AND Req.MailPieceStatus='InProgress'
END

GO

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND  name = 'usp_GetMRDFRequestDataByPrintTrailBatchId')
DROP PROCEDURE [dbo].[usp_GetMRDFRequestDataByPrintTrailBatchId]
GO
CREATE PROCEDURE [dbo].[usp_GetMRDFRequestDataByPrintTrailBatchId]
@printTrailBatchId int,
@mailPieceStatus varchar(255)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	If(@mailPieceStatus='ALL')
	BEGIN
		SELECT JobId, PieceId, [Date], TotalSheets, InputWeight, Feeder3_Sel, Feeder4_Sel
		, Feeder5_Sel, LowCap_Sel, NoSeal_Sel, Feeder2_Sel, ZipMark_Sel, Address1, Address2
		, Zip5, Zip4, Zip2, DivertVS2, DivertVS3
		, DivertVS4, ReturnLogoBitMap, ReturnLine1, ReturnLine2, ReturnLine3, ReturnLine4
		, ReturnLine5, ReturnLine6, GroupNumber, SocialSecNumber, CheckAmount, PassThru
		, Expansion, EndofRecord, UniqueId, PDFFileName,ErrId, ErrDesc, MailPieceStatus, IsPDFReceived,PrintTrailBatchId 
		FROM [PnS].[MRDFRequest] WHERE PrintTrailBatchId = @printTrailBatchId
	END
	ELSE If(@mailPieceStatus='INTERNAL')--This will be used for getting only submitted records to PCI
	BEGIN
		SELECT JobId, PieceId, [Date], TotalSheets, InputWeight, Feeder3_Sel, Feeder4_Sel
		, Feeder5_Sel, LowCap_Sel, NoSeal_Sel, Feeder2_Sel, ZipMark_Sel, Address1, Address2
		, Zip5, Zip4, Zip2, DivertVS2, DivertVS3
		, DivertVS4, ReturnLogoBitMap, ReturnLine1, ReturnLine2, ReturnLine3, ReturnLine4
		, ReturnLine5, ReturnLine6, GroupNumber, SocialSecNumber, CheckAmount, PassThru
		, Expansion, EndofRecord, UniqueId, PDFFileName,ErrId, ErrDesc, MailPieceStatus, IsPDFReceived,PrintTrailBatchId 
		FROM [PnS].[MRDFRequest] WHERE PrintTrailBatchId = @printTrailBatchId AND ErrId NOT IN (-1,-2,-3)
	END
	ELSE
	BEGIN
		SELECT REQ.*,MP.Id as MailPieceId,MP.EffectiveReceivedOnEst, BS.ClientId,BS.DocTypeId,D.ShippingCarrier,
  D.ShippingServiceType,C.AppCode
  FROM PnS.MRDFRequest REQ
  INNER JOIN PnS.BatchSummary BS ON REQ.JobId=BS.PrintBatch
  LEFT OUTER JOIN AM.MailPieces MP ON REQ.UniqueId=MP.UniqueId
  INNER JOIN AM.DocTypes D ON BS.DocTypeId=D.Id
  INNER JOIN Common.Clients C ON C.Id=BS.ClientId
  WHERE REQ.PrintTrailBatchId=@printTrailBatchId AND ISNULL(MailPieceStatus,'')=@mailPieceStatus 
	END
END
GO

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND  name = 'usp_GetAllClientAppDetail')
DROP PROCEDURE [dbo].[usp_GetAllClientAppDetail]
GO
CREATE PROCEDURE [dbo].[usp_GetAllClientAppDetail]
	-- Add the parameters for the stored procedure here
	
AS
BEGIN
	SELECT 
	C.Id, C.Name, C.AppCode, C.BusinessUnit, C.IsAMActive, C.IsPnSActive, C.ModifiedOn, C.ModifiedBy
	, C.CreatedOn, C.CreatedBy, C.TimeZone
	,B.Id As BUId
	,B.CustCode
	,B.[BusinessUnit]
  FROM [Common].[Clients] C INNER JOIN [Common].[BusinessUnit] B ON B.Id = C.BusinessUnit
END
GO

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND  name = 'usp_GetClientAppDetailsById')
DROP PROCEDURE [dbo].[usp_GetClientAppDetailsById]
GO

CREATE PROCEDURE [dbo].[usp_GetClientAppDetailsById]
	-- Add the parameters for the stored procedure here
	@appId int
AS
BEGIN
	SELECT C.[Id]
	  ,B.Id AS BUId
	  ,B.BusinessUnit
	  ,B.CustCode
      ,[AppCode]
      ,[Name]
	  ,[TimeZone]
      ,[IsAMActive]
	  ,[IsPnSActive]
      ,[ModifiedOn]
      ,[ModifiedBy]
      ,[CreatedOn]
      ,[CreatedBy]
  FROM [Common].[Clients] C
  INNER JOIN Common.BusinessUnit B ON B.Id=C.BusinessUnit
  WHERE C.Id = @appId
END


GO
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND  name = 'SP_GetRecentBatchesStats')
DROP PROCEDURE [dbo].[SP_GetRecentBatchesStats]
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
GO


IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND  name = 'SP_GetScanStats')
DROP PROCEDURE [dbo].[SP_GetScanStats]
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

GO
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND  name = 'SP_GetPendingScanning')
DROP PROCEDURE [dbo].[SP_GetPendingScanning]
GO
CREATE PROCEDURE [dbo].[SP_GetPendingScanning]
	@appcode varchar(128),
	@EstDate DATE
AS
BEGIN
	declare @clientid int
	set @appcode = replace(@appcode, 'AUT','')
	select @clientid=Id from Common.Clients where AppCode=@appcode;

	select TrackingNo,LoanNumber,SubmittedOn,PDFFileName as [FileName],JobName,PrintBatchName,MailPieceStatus,BatchName,DueDate 
	from dbo.ufn_GetDallasBatchDetails(@EstDate)
	where ClientId=@clientid and PrintLocation=(select Name from Common.PrintVendors where VendorCode='CL_DALLAS') and MailPieceStatus='InProgress' and ProcessingTimeStamp is null and IsSubmitted=1

END

GO
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND  name = 'SP_RegisterScan')
DROP PROCEDURE [dbo].[SP_RegisterScan]
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

GO
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND  name = 'SP_GetScannedMailPieces')
DROP PROCEDURE [dbo].[SP_GetScannedMailPieces]
GO
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
	where Printlocation=@printlocationcl and ClientId=@clientId and BatchName=@batchName and MailPieceStatus <> 'InProgress'	and ProcessingTimeStamp between @startDate and @endDate and IsSubmitted=1

END


GO
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND  name = 'SP_GetScanReportData')
DROP PROCEDURE [dbo].[SP_GetScanReportData]
GO
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
	
	select JobName,PrintBatchName,BatchName,TrackingNo as BarcodeValue,TotalPages,ProcessingTimeStamp as ShippedOn,LoanNumber,MailPieceStatus,SubmittedOn as ReceivedOn from dbo.ufn_GetDallasBatchDetails(@EstDate)
	where ClientId=@clientId and PrintLocation=@printlocationcl and ProcessingTimeStamp is not null and IsSubmitted=1
	and SubmittedOn between ISNULL(@substartdate,SubmittedOn) and ISNULL(@subenddate,SubmittedOn)
	and ProcessingTimeStamp between ISNULL(@scanstartDate,ProcessingTimeStamp) and ISNULL(@scanendDate,ProcessingTimeStamp)
	and TrackingNo = ISNULL(@trackingNo,TrackingNo)
	and LoanNumber = ISNULL(@loanNumber,LoanNumber)
	order by SubmittedOn asc

END

GO
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND  name = 'uspalert_GetDelayedBatchesToSubmit')
DROP PROCEDURE [dbo].[uspalert_GetDelayedBatchesToSubmit]
GO
CREATE PROCEDURE [dbo].[uspalert_GetDelayedBatchesToSubmit] 	
	@cutOffMinutes int
AS
BEGIN
 select mp.Id,mb.Id as MailBatchId,rq.UniqueId,bs.IsSubmitted,c.AppCode,mb.BatchName,mb.ReceivedOn,mb.PipelineStatus,mp.ShippingStatus,d.DocTypeCode,d.ShippingCarrier,d.ShippingServiceType,mp.ClientId,pv.Name as PrintLocation,pv.VendorCode from AM.MailPieces mp
 inner join AM.MailBatches mb on mp.MailBatchId=mb.Id
 left outer join Common.PrintTrails pt on mp.PrintTrailId = pt.Id
 left outer join Common.PrintVendors pv on pt.PrintVendorId = pv.Id
 left outer join Common.Clients c on mb.ClientId=c.Id
 left outer join PnS.MRDFRequest rq on mp.UniqueId=rq.UniqueId
 left outer join PnS.BatchSummary bs on rq.JobId=bs.PrintBatch
 left outer join AM.DocTypes d on mp.DocTypeId = d.Id
 where mb.ReceivedOn between DATEADD(day,-7,GETUTCDATE()) and DATEADD(minute,-@cutOffMinutes,GETUTCDATE()) and (mp.ShippingStatus is null or (mp.ShippingStatus in ('InProgress','Shipped') and (rq.UniqueId is null or ISNULL(bs.IsSubmitted,0)=0)) or (mp.ShippingStatus = 'Error' and mp.ErrorDesc IS NULL) or (mp.ShippingStatus = 'PulledBack' and not exists (select Id from AM.PullBackRequests where MailPieceId = mp.Id)) or (mb.ClientId IS NULL))
 
END

Go

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND  name = 'usp_GetDocTypeDetailsByAppId')
DROP PROCEDURE [dbo].[usp_GetDocTypeDetailsByAppId]
GO

 CREATE PROCEDURE [dbo].[usp_GetDocTypeDetailsByAppId]
  @clientId int
 AS
 BEGIN
  -- SET NOCOUNT ON added to prevent extra result sets from
  -- interfering with SELECT statements.
  SET NOCOUNT ON;
  
  SELECT D.ClientId,D.[PrintSize],D.[ContentScalePercent],D.[QCPercent],D.[ClientEmails],D.[SupportEmails]
  ,D.[Description] as DocTypeDesc,D.DocTypeCode,D.IsActive,D.ModifiedOn
  ,D.CreatedOn,D.Id AS DocTypeId,D.ShippingCarrier,D.ShippingServiceType,D.IsBRE
  FROM AM.DocTypes D
  INNER JOIN Common.Clients C ON C.Id=D.ClientId
  WHERE C.Id=@clientId
  
 END
 Go

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND  name = 'usp_GetFinalMRDFResponseDataByJobId')
DROP PROCEDURE [dbo].[usp_GetFinalMRDFResponseDataByJobId]
GO
 CREATE PROCEDURE [dbo].[usp_GetFinalMRDFResponseDataByJobId]
@jobId int
AS
BEGIN
 -- SET NOCOUNT ON added to prevent extra result sets from
 -- interfering with SELECT statements.
 SET NOCOUNT ON;
 
 SELECT JobId, PostageCost, ErrId, case when ShippedOn is null and errid = -4 then CAST('1753-1-1' AS DATETIME) else ShippedOn end as ShippedOn, WeightMeasured,case when errid = -4 then '4' else Disposition end as Disposition
 , PieceId, UniqueId, PDFFileName, ErrDesc, MailPieceStatus,TotalSheets,DivertVS3, PackageTrackingNumber 
 FROM view_MRDFResponseData V WHERE V.JobId=@jobId --AND ErrId <> -4
END
-- End Stored Procedure --
/*==========================================================================================*/

--- Drop & Create View ---
GO

IF EXISTS(select * FROM sys.views where name = 'view_MRDFResponseData')
DROP VIEW [dbo].[view_MRDFResponseData]
GO
CREATE VIEW [dbo].[view_MRDFResponseData] AS
SELECT REQ.JobId
,REQ.PieceId
,ISNULL(REQ.ErrId,0) AS ErrId 
,REQ.ErrDesc
,RES.ProcessingTimeStamp AS ShippedOn 
,REQ.UniqueId
,REQ.PDFFileName
,REQ.MailPieceStatus
,ISNULL(RES.TotalSheets,0) AS TotalSheets
,REQ.DivertVS3
,RES.Disposition 
,ISNULL(RES.WeightMeasured,0.00) AS WeightMeasured 
,ISNULL(RES.PostageMetered,0.00) AS PostageCost 
,RES.PackageTrackingNumber 
FROM [PnS].MRDFRequest REQ
LEFT JOIN [PnS].MRDFResponse RES
ON REQ.PrintTrailBatchId =RES.JobId AND REQ.PieceId =RES.PieceId

GO

-- End View --

