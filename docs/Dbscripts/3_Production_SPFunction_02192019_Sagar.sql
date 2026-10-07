/****** Object:  UserDefinedFunction [dbo].[ufn_GetSweepTimings]    Script Date: 2/19/2019 12:17:46 PM ******/
DROP FUNCTION [dbo].[ufn_GetSweepTimings]
GO

/****** Object:  UserDefinedFunction [dbo].[ufn_GetSweepTimings]    Script Date: 2/19/2019 12:17:46 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
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
 values (1,'ALASKA','PCI','Weekday','OnSchedule','15:00:00')
 insert into @T (SrNo,AppCode,VendorCode,WeekDayType,SweepType,SweepTime) 
 values (2,'ALASKA','PCI','Weekday','OnSchedule','19:45:00')
 insert into @T (SrNo,AppCode,VendorCode,WeekDayType,SweepType,SweepTime) 
 values (1,'ALASKA','PCI','Saturday','OnSchedule','15:00:00')
 insert into @T (SrNo,AppCode,VendorCode,WeekDayType,SweepType,SweepTime) 
 values (2,'ALASKA','PCI','Saturday','OnSchedule','19:45:00')
 
 --USAA
 insert into @T (SrNo,AppCode,VendorCode,WeekDayType,SweepType,SweepTime) 
 values (1,'USAA','CL_DALLAS','Weekday','Continuous','00:00:00')
 insert into @T (SrNo,AppCode,VendorCode,WeekDayType,SweepType,SweepTime) 
 values (1,'USAA','CL_DALLAS','Saturday','Continuous','00:00:00')

 insert into @T (SrNo,AppCode,VendorCode,WeekDayType,SweepType,SweepTime) 
 values (1,'USAA','PCI','Weekday','OnSchedule','15:00:00')
 insert into @T (SrNo,AppCode,VendorCode,WeekDayType,SweepType,SweepTime) 
 values (2,'USAA','PCI','Weekday','OnSchedule','20:00:00')
 
 insert into @T (SrNo,AppCode,VendorCode,WeekDayType,SweepType,SweepTime) 
 values (1,'USAA','PCI','Saturday','OnSchedule','15:00:00')
 insert into @T (SrNo,AppCode,VendorCode,WeekDayType,SweepType,SweepTime) 
 values (2,'USAA','PCI','Saturday','OnSchedule','20:00:00')
 
 --HSBC
 insert into @T (SrNo,AppCode,VendorCode,WeekDayType,SweepType,SweepTime) 
 values (1,'HSBC','CL_DALLAS','Weekday','Continuous','00:00:00')
 insert into @T (SrNo,AppCode,VendorCode,WeekDayType,SweepType,SweepTime) 
 values (1,'HSBC','CL_DALLAS','Saturday','Continuous','00:00:00')
 
 --CITI
 insert into @T (SrNo,AppCode,VendorCode,WeekDayType,SweepType,SweepTime) 
 values (1,'CITIVM','PCI','Weekday','OnSchedule','15:00:00')
 insert into @T (SrNo,AppCode,VendorCode,WeekDayType,SweepType,SweepTime) 
 values (2,'CITIVM','PCI','Weekday','OnSchedule','19:30:00')
 
 insert into @T (SrNo,AppCode,VendorCode,WeekDayType,SweepType,SweepTime) 
 values (1,'CITIVM','PCI','Saturday','OnSchedule','15:00:00')
 insert into @T (SrNo,AppCode,VendorCode,WeekDayType,SweepType,SweepTime) 
 values (2,'CITIVM','PCI','Saturday','OnSchedule','19:30:00')

  --RPS
 insert into @T (SrNo,AppCode,VendorCode,WeekDayType,SweepType,SweepTime) 
 values (1,'RPS','PCI','Weekday','OnSchedule','15:00:00')
 insert into @T (SrNo,AppCode,VendorCode,WeekDayType,SweepType,SweepTime) 
 values (2,'RPS','PCI','Weekday','OnSchedule','19:30:00')
 
 insert into @T (SrNo,AppCode,VendorCode,WeekDayType,SweepType,SweepTime) 
 values (1,'RPS','PCI','Saturday','OnSchedule','15:00:00')
 insert into @T (SrNo,AppCode,VendorCode,WeekDayType,SweepType,SweepTime) 
 values (2,'RPS','PCI','Saturday','OnSchedule','19:30:00')

 --BOTW
 insert into @T (SrNo,AppCode,VendorCode,WeekDayType,SweepType,SweepTime) 
 values (1,'BOTW','CL_DALLAS','Weekday','Continuous','00:00:00')
 insert into @T (SrNo,AppCode,VendorCode,WeekDayType,SweepType,SweepTime) 
 values (1,'BOTW','CL_DALLAS','Saturday','Continuous','00:00:00')
 
 return
end

go
/****** Object:  StoredProcedure [dbo].[usp_GetMRDFResponseDummyDataByPrintTrailBatchId]    Script Date: 2/19/2019 12:27:14 PM ******/
DROP PROCEDURE [dbo].[usp_GetMRDFResponseDummyDataByPrintTrailBatchId]
GO

/****** Object:  StoredProcedure [dbo].[usp_GetMRDFResponseDummyDataByPrintTrailBatchId]    Script Date: 2/19/2019 12:27:14 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
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
		,LEFT(RIGHT('0'+CAST(CAST(ROUND(RAND(CHECKSUM(NEWID())) * (10), 2) AS decimal(5,2)) as VARCHAR),6)+'0',6) as PostageMetered,'000 ' as WeightMeasured,'Mailrun_Files            ' as ExitLocation
		,Req.PrintTrailBatchId,CASE WHEN P.IsPullBackRequested=1 AND P.PulledBackOn IS NULL THEN 'PulledBack' WHEN P.IsQCRequested=1 THEN 'Shipped' ELSE 'Shipped' END as PrintStatus,P.Id as MailPieceId
		FROM [PnS].[MRDFRequest] Req
		INNER JOIN [AM].[MailPieces] P ON P.UniqueId=Req.UniqueId
		WHERE Req.PrintTrailBatchId = @printTrailBatchId AND Req.MailPieceStatus='InProgress'
END


GO

/****** Object:  StoredProcedure [dbo].[usp_GetMRDFRequestDataByPrintTrailId]    Script Date: 2/19/2019 12:28:01 PM ******/
DROP PROCEDURE [dbo].[usp_GetMRDFRequestDataByPrintTrailId]
GO

/****** Object:  StoredProcedure [dbo].[usp_GetMRDFRequestDataByPrintTrailId]    Script Date: 2/19/2019 12:28:01 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE PROCEDURE [dbo].[usp_GetMRDFRequestDataByPrintTrailId] 
@printTrailId INT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT REQ.*,MP.Id as MailPieceId,MP.EffectiveReceivedOnEst, BS.ClientId,BS.DocTypeId,D.ShippingCarrier,
 D.ShippingServiceType,C.AppCode,MP.DueDate as MailPieceDueDate,D.EnvelopeId,D.BREEnvelopeId
 FROM PnS.MRDFRequest REQ
 INNER JOIN PnS.BatchSummary BS ON REQ.JobId=BS.PrintBatch
 LEFT OUTER JOIN AM.MailPieces MP ON REQ.UniqueId=MP.UniqueId
 INNER JOIN AM.DocTypes D ON BS.DocTypeId=D.Id
 INNER JOIN Common.Clients C ON C.Id=BS.ClientId
 WHERE BS.PipelineStatus='Prepped' AND  IsPrepped = 1 AND IsSubmitted = 0
 AND BS.PrintTrailId=@printTrailId
	
END

GO

/****** Object:  StoredProcedure [dbo].[usp_GetMRDFRequestDataByPrintTrailBatchId]    Script Date: 2/19/2019 12:28:32 PM ******/
DROP PROCEDURE [dbo].[usp_GetMRDFRequestDataByPrintTrailBatchId]
GO

/****** Object:  StoredProcedure [dbo].[usp_GetMRDFRequestDataByPrintTrailBatchId]    Script Date: 2/19/2019 12:28:32 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
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
		SELECT Req.JobId, Req.PieceId, Req.[Date], Req.TotalSheets, Req.InputWeight, Req.Feeder3_Sel, Req.Feeder4_Sel
		, Req.Feeder5_Sel, Req.LowCap_Sel, Req.NoSeal_Sel, Req.Feeder2_Sel, Req.ZipMark_Sel, Req.Address1, Req.Address2
		, Req.Zip5, Req.Zip4, Req.Zip2, Req.DivertVS2, Req.DivertVS3
		, Req.DivertVS4, Req.ReturnLogoBitMap, Req.ReturnLine1, Req.ReturnLine2, Req.ReturnLine3, Req.ReturnLine4
		, Req.ReturnLine5, Req.ReturnLine6, Req.GroupNumber, Req.SocialSecNumber, Req.CheckAmount, Req.PassThru
		, Req.Expansion, Req.EndofRecord, Req.UniqueId, Req.PDFFileName,ErrId, Req.ErrDesc, Req.MailPieceStatus
		, Req.IsPDFReceived,PrintTrailBatchId,MP.Id as MailPieceId
		FROM [PnS].[MRDFRequest] Req
		INNER JOIN PnS.BatchSummary BS ON REQ.JobId=BS.PrintBatch
		LEFT OUTER JOIN AM.MailPieces MP ON REQ.UniqueId=MP.UniqueId
		WHERE Req.PrintTrailBatchId = @printTrailBatchId
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
		D.ShippingServiceType,C.AppCode,MP.DueDate as MailPieceDueDate,D.EnvelopeId,D.BREEnvelopeId
		FROM PnS.MRDFRequest REQ
		INNER JOIN PnS.BatchSummary BS ON REQ.JobId=BS.PrintBatch
		LEFT OUTER JOIN AM.MailPieces MP ON REQ.UniqueId=MP.UniqueId
		INNER JOIN AM.DocTypes D ON BS.DocTypeId=D.Id
		INNER JOIN Common.Clients C ON C.Id=BS.ClientId
		WHERE REQ.PrintTrailBatchId=@printTrailBatchId AND ISNULL(MailPieceStatus,'')=@mailPieceStatus 
	END
END

GO


/****** Object:  StoredProcedure [dbo].[usp_GetMRDFResponseDataByPrintTrailBatchId]    Script Date: 2/19/2019 12:50:20 PM ******/
DROP PROCEDURE [dbo].[usp_GetMRDFResponseDataByPrintTrailBatchId]
GO

/****** Object:  StoredProcedure [dbo].[usp_GetMRDFResponseDataByPrintTrailBatchId]    Script Date: 2/19/2019 12:50:20 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[usp_GetMRDFResponseDataByPrintTrailBatchId]
@printTrailBatchId int
AS
BEGIN
 -- SET NOCOUNT ON added to prevent extra result sets from
 -- interfering with SELECT statements.
 SET NOCOUNT ON;
 
 SELECT Res.JobId, Res.PieceId, Res.Date, Res.Feeder2_Fed, Res.OperatorId, Res.MachineId, Res.ProcessingTimeStamp, Res.DetailedErrorId
 , Res.Disposition, Res.Input_Fed, Res.Feeder3_Fed, Res.Feeder4_Fed, Res.Feeder5_Fed, Res.ComputedWeight, Res.MeterDate
 , Res.StatusSource, Res.PostageMetered, Res.WeightMeasured, Res.ExitLocation, Res.TotalSheets, Res.PDFFileName, Res.PackageTrackingNumber
 ,MP.Id as MailPieceId 
 FROM [Pns].[MRDFResponse] Res
 LEFT OUTER JOIN PnS.MRDFRequest Req ON Res.PieceId=Req.PieceId AND Res.JobId=Req.PrintTrailBatchId
 LEFT OUTER JOIN AM.MailPieces MP ON MP.UniqueId=Req.UniqueId
 WHERE  Req.PrintTrailBatchId=@printTrailBatchId
END

GO


/****** Object:  StoredProcedure [dbo].[usp_GetDocTypeDetailsByAppId]    Script Date: 2/19/2019 12:52:12 PM ******/
DROP PROCEDURE [dbo].[usp_GetDocTypeDetailsByAppId]
GO

/****** Object:  StoredProcedure [dbo].[usp_GetDocTypeDetailsByAppId]    Script Date: 2/19/2019 12:52:12 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


 CREATE PROCEDURE [dbo].[usp_GetDocTypeDetailsByAppId]
  @clientId int
 AS
 BEGIN
  -- SET NOCOUNT ON added to prevent extra result sets from
  -- interfering with SELECT statements.
  SET NOCOUNT ON;
  
  SELECT D.ClientId,D.[PrintSize],D.[ContentScalePercent],D.[QCPercent],D.[ClientEmails],D.[SupportEmails]
  ,D.[Description] as DocTypeDesc,D.DocTypeCode,D.IsActive,D.ModifiedOn,D.PrintFormat
  ,D.CreatedOn,D.Id AS DocTypeId,D.ShippingCarrier,D.ShippingServiceType,D.IsBRE
  FROM AM.DocTypes D
  INNER JOIN Common.Clients C ON C.Id=D.ClientId
  WHERE C.Id=@clientId
  
 END

GO


/****** Object:  UserDefinedFunction [dbo].[ufn_GetDallasBatchTimings]    Script Date: 2/19/2019 12:56:11 PM ******/
DROP FUNCTION [dbo].[ufn_GetDallasBatchTimings]
GO

/****** Object:  UserDefinedFunction [dbo].[ufn_GetDallasBatchTimings]    Script Date: 2/19/2019 12:56:11 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
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

   INSERT INTO @BatchTimings (AppCode, BatchName,StartTime,EndTime,ShippingCarrier,ShippingServiceType) 
   VALUES ('USAA','07PM','15:00:01','20:00:00','USPS','IMB')
  END
  ELSE
  BEGIN
   
   INSERT INTO @BatchTimings (AppCode, BatchName,StartTime,EndTime,ShippingCarrier,ShippingServiceType) 
   VALUES ('USAA','07PM','15:00:01','20:00:00','USPS','IMB')

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

   INSERT INTO @BatchTimings (AppCode, BatchName,StartTime,EndTime,ShippingCarrier,ShippingServiceType) 
   VALUES ('HSBC','12PM','00:00:00','13:00:00','USPS','IMB')
   INSERT INTO @BatchTimings (AppCode, BatchName,StartTime,EndTime,ShippingCarrier,ShippingServiceType) 
   VALUES ('HSBC','04PM','13:00:01','17:00:00','USPS','IMB')
   INSERT INTO @BatchTimings (AppCode, BatchName,StartTime,EndTime,ShippingCarrier,ShippingServiceType) 
   VALUES ('HSBC','05PM','17:00:01','18:00:00','USPS','IMB')
   INSERT INTO @BatchTimings (AppCode, BatchName,StartTime,EndTime,ShippingCarrier,ShippingServiceType) 
   VALUES ('HSBC','06PM','18:00:01','19:00:00','USPS','IMB')
   INSERT INTO @BatchTimings (AppCode, BatchName,StartTime,EndTime,ShippingCarrier,ShippingServiceType) 
   VALUES ('HSBC','07PM','19:00:01','20:00:00','USPS','IMB')

   INSERT INTO @BatchTimings (AppCode, BatchName,StartTime,EndTime,ShippingCarrier,ShippingServiceType) 
   VALUES ('BOTW','12PM','00:00:00','13:00:00','USPS','IMB')
   INSERT INTO @BatchTimings (AppCode, BatchName,StartTime,EndTime,ShippingCarrier,ShippingServiceType) 
   VALUES ('BOTW','04PM','13:00:01','17:00:00','USPS','IMB')
   INSERT INTO @BatchTimings (AppCode, BatchName,StartTime,EndTime,ShippingCarrier,ShippingServiceType) 
   VALUES ('BOTW','05PM','17:00:01','18:00:00','USPS','IMB')
   INSERT INTO @BatchTimings (AppCode, BatchName,StartTime,EndTime,ShippingCarrier,ShippingServiceType) 
   VALUES ('BOTW','06PM','18:00:01','19:00:00','USPS','IMB')
   INSERT INTO @BatchTimings (AppCode, BatchName,StartTime,EndTime,ShippingCarrier,ShippingServiceType) 
   VALUES ('BOTW','07PM','19:00:01','20:00:00','USPS','IMB')
  END
  
  RETURN
 END

GO


/****** Object:  UserDefinedFunction [dbo].[ufn_GetDallasBatches]    Script Date: 2/19/2019 12:58:44 PM ******/
DROP FUNCTION [dbo].[ufn_GetDallasBatches]
GO

/****** Object:  UserDefinedFunction [dbo].[ufn_GetDallasBatches]    Script Date: 2/19/2019 12:58:44 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
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

   INSERT INTO @sweep (AppCode, BatchName,StartDate,EndDate,ShippingCarrier,ShippingServiceType) 
  VALUES ('USAA','Backlog',DateAdd(day,-7, cast(@todayDate AS DATETIME)),DateAdd(second,-1, cast(@todayDate AS DATETIME)),'USPS','IMB')

  INSERT INTO @sweep (AppCode, BatchName,StartDate,EndDate,ShippingCarrier,ShippingServiceType) 
  VALUES ('HSBC','Backlog',DateAdd(day,-7, cast(@todayDate AS DATETIME)),DateAdd(second,-1, cast(@todayDate AS DATETIME)),'USPS','IMB')

   INSERT INTO @sweep (AppCode, BatchName,StartDate,EndDate,ShippingCarrier,ShippingServiceType) 
  VALUES ('BOTW','Backlog',DateAdd(day,-7, cast(@todayDate AS DATETIME)),DateAdd(second,-1, cast(@todayDate AS DATETIME)),'USPS','IMB')

  RETURN
 END

GO