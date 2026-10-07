/****** Object:  StoredProcedure [dbo].[usp_GetBatchSummaryById]    Script Date: 11/1/2018 6:40:27 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
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

/****** Object:  StoredProcedure [dbo].[usp_GetAllClientAppDetail]    Script Date: 11/16/2017 17:20:05 ******/
SET ANSI_NULLS ON


/****** Object:  StoredProcedure [dbo].[usp_GetMRDFRequestDataByPrintTrailId]    Script Date: 11/2/2018 9:23:03 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		Sagar Golvelkar
-- Create date: 10/31/2018
-- Description:	Get all the mrdf request details by print trails id
-- =============================================
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


/****** Object:  StoredProcedure [dbo].[usp_GetMRDFResponseDataByPrintTrailBatchId]    Script Date: 11/1/2018 11:29:29 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
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

/****** Object:  StoredProcedure [dbo].[usp_GetSubmittedBatches]    Script Date: 11/1/2018 6:43:01 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND  name = 'usp_GetSubmittedBatches')
DROP PROCEDURE [dbo].[usp_GetSubmittedBatches]
GO

CREATE PROCEDURE [dbo].[usp_GetSubmittedBatches]
AS
BEGIN

--SELECT * FROM dbo.BatchSummary
SELECT B.Id as PrintTrailBatchId,B.PrintBatchName
FROM [PnS].PrintTrailBatches B
INNER JOIN Common.PrintTrails PT ON B.PrintTrailId=PT.Id
INNER JOIN Common.PrintVendors PV ON PT.PrintVendorId=PV.Id
WHERE B.IsSubmitted=1 AND B.IsMRDFReceived=0 

END


/****** Object:  StoredProcedure [dbo].[usp_GetPrintBatchDetailByPrintBatchTrailId]    Script Date: 11/1/2018 6:43:53 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		Sagar Golvelkar
-- Create date: 10/31/2018
-- Description:	Get PrintTrailBatch details
-- =============================================
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


/****** Object:  StoredProcedure [dbo].[usp_GetPrintTrailDetailByPrintTrailId]    Script Date: 11/1/2018 6:45:37 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		Sagar Golvelkar
-- Create date: 10/31/2018
-- Description:	Get Print Trail details by 
-- =============================================
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
	PV.Name as PrintVendorName,PV.BusinessCode ,PV.SFTChannelId,
	SC.Protocol,SC.Host,SC.UserName,SC.[Password],SC.ToADEDir,SC.FromADEDir 
	FROM Common.PrintTrails PT
	INNER JOIN Common.PrintVendors PV ON PV.Id=PT.PrintVendorId
	INNER JOIN Common.SFTChannels SC ON PV.SFTChannelId=SC.Id
	WHERE PT.Id=@printTrailId
END

GO

/****** Object:  StoredProcedure [dbo].[usp_GetPrintTrailDetailByPrintTrailName]    Script Date: 11/1/2018 6:46:04 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		Sagar Golvelkar
-- Create date: 10/31/2018
-- Description:	Get Print Trail details by 
-- =============================================
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
	PV.Name as PrintVendorName,PV.BusinessCode ,PV.SFTChannelId,
	SC.Protocol,SC.Host,SC.UserName,SC.[Password],SC.ToADEDir,SC.FromADEDir 
	FROM Common.PrintTrails PT
	INNER JOIN Common.PrintVendors PV ON PV.Id=PT.PrintVendorId
	INNER JOIN Common.SFTChannels SC ON PV.SFTChannelId=SC.Id
	WHERE PT.TrailName=@printTrailName
END

GO

/****** Object:  StoredProcedure [dbo].[usp_GetMRDFResponseDummyDataByPrintTrailBatchId]    Script Date: 11/2/2018 8:23:17 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
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



/****** Object:  StoredProcedure [dbo].[usp_GetMRDFRequestDataByPrintTrailBatchId]    Script Date: 11/1/2018 11:35:02 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
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
		SELECT REQ.*,MP.Id as MailPieceId,MP.EffectiveReceivedOnEst, BS.ClientId,MP.DocTypeId,D.ShippingCarrier,
		D.ShippingServiceType,C.AppCode
		FROM PnS.MRDFRequest REQ
		INNER JOIN PnS.BatchSummary BS ON REQ.JobId=BS.PrintBatch
		INNER JOIN AM.MailPieces MP ON REQ.UniqueId=MP.UniqueId
		INNER JOIN AM.DocTypes D ON MP.DocTypeId=D.Id
		INNER JOIN Common.Clients C ON C.Id=MP.ClientId
		WHERE REQ.PrintTrailBatchId=@printTrailBatchId AND ISNULL(MailPieceStatus,'')=@mailPieceStatus
	END
END
GO


/****** Object:  StoredProcedure [dbo].[usp_GetAllClientAppDetail]    Script Date: 11/1/2018 11:35:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
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


/****** Object:  StoredProcedure [dbo].[usp_GetClientAppDetailsById]    Script Date: 11/1/2018 11:35:48 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
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
----------------------------------------------------------------------------------------
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
c.AppCode,c.Name as ClientName,(select BatchName from db where EffectiveReceivedOnEst between StartDate AND EndDate AND ShippingCarrier=d.ShippingCarrier AND ShippingServiceType=d.ShippingServiceType AND AppCode=c.AppCode) BatchName
FROM [AM].[MailPieces] m
LEFT OUTER JOIN PnS.MRDFRequest req ON m.UniqueId = req.UniqueId
LEFT OUTER JOIN PnS.BatchSummary b ON req.JobId = b.PrintBatch
LEFT OUTER JOIN PnS.PrintTrailBatches preq ON req.PrintTrailBatchId=preq.Id
LEFT OUTER JOIN PnS.MRDFResponse res ON req.PrintTrailBatchId = res.JobId AND req.PieceId=res.PieceId
INNER JOIN AM.DocTypes d ON m.DocTypeId=d.Id
INNER JOIN Common.Clients c ON m.ClientId=c.Id
WHERE m.PrintLocation=(SELECT Value FROM Common.SystemSettings WHERE Id='Common_PrintLocationCL') 
AND m.EffectiveReceivedOnEst >=(select MIN(StartDate) from db)
)
select * from q where BatchName is not null 
)
-----------------------------------------------------------------------
/****** Object:  StoredProcedure [dbo].[SP_RegisterScan]    Script Date: 8/6/2018 10:39:18 AM ******/

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
		SET @printlocationcl=(select Value from [Common].[SystemSettings] where Id ='Common_PrintLocationCL')
		-- Get mailpiece, MRDFRequest data filter by trackingno & printlocation
	
		if(@btName is null)
		begin 
			select 'Invalid' as Code, NULL as TrackingNo, 'Batch name can not be null' as [Message], NULL as [JobName]
			return
		end
		select @mailpieceid= Id,@pieceid=PieceId,@jobid=JobId,@mrdfreqdate=MRDFReqDate,@ispullbackrequested =IsPullBackRequested,@isqcrequested=IsQCRequested,@totalsheet=TotalSheets,@pdffilename=PDFFileName,@jobName=JobName,@serviceType=ShippingServiceType,@clientAppCode=AppCode,@clientName=ClientName,@bt=BatchName,@PrintTrailBatchId=PrintTrailBatchId
		from dbo.ufn_GetDallasBatchDetails(@EstDate)
		where TrackingNo=@trackno and PrintLocation=@printlocationcl and IsSubmitted=1 and MailPieceStatus='InProgress'

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


/****** Object:  View [dbo].[view_MRDFResponseData]    Script Date: 11/2/2018 9:02:52 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
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
	
	SELECT @mailBatchId = MailBatchId,@printBatchId = PrintBatchId FROM AM.MailPieces WHERE UniqueId = @MailPieceUniqueId

	SELECT @totalMailPieces = COUNT(*) FROM AM.MailPieces WHERE MailBatchId = @mailBatchId

	--Print @totalMailPieces

	SELECT @totalPulledBackMailPieces = COUNT(*) FROM AM.MailPieces WHERE MailBatchId = @mailBatchId AND IsPulledBack = 1

	SELECT @totalPulledBackPrintBatchesMailPieces = COUNT(*) FROM AM.MailPieces WHERE PrintBatchId = @printBatchId AND IsPulledBack = 1
	--PRINT @totalPulledBackMailPieces
	SELECT @totalPrintBatchesMailPieces = COUNT(*) FROM AM.MailPieces WHERE PrintBatchId = @printBatchId
	
	IF (@totalPrintBatchesMailPieces = @totalPulledBackPrintBatchesMailPieces)
	BEGIN
		UPDATE AM.PrintBatches SET BatchStatus = 'PulledBack',ShippingResponseReceivedOn = @shippingResponseReceievedOn,ReportPreppedOn = @shippingResponseReceievedOn,ReportedBackOn = @shippingResponseReceievedOn WHERE Id = @printBatchId	
	END

	IF (@totalMailPieces = @totalPulledBackMailPieces)
	BEGIN	
		UPDATE AM.MailBatches SET BatchStatus = 'PulledBack',ShippingResponseReceivedOn = @shippingResponseReceievedOn,PipelineStatus = 'ShippingResponseReceived' WHERE Id = @mailBatchId
		
	END

	UPDATE AM.MailPieces SET PrintBatchId = null WHERE UniqueId = @MailPieceUniqueId
END


Go
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
	where ClientId=@clientid and PrintLocation=(select Value from [Common].[SystemSettings] where Id		='Common_PrintLocationCL') and MailPieceStatus='InProgress' and ProcessingTimeStamp is null and IsSubmitted=1

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
	select @printlocationcl=Value from [Common].[SystemSettings] where Id ='Common_PrintLocationCL';

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
	select @printlocationcl = Value from [Common].[SystemSettings] where Id ='Common_PrintLocationCL'
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
