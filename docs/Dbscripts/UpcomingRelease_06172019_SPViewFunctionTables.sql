
PRINT 'Added two new column in Common.Envelopes table'
ALTER TABLE Common.Envelopes
ADD ReorderDuration INT NULL--,MinStock INT NULL
go
PRINT 'Added new column in AM.MailPieces,PnS.MRDFRequest,PnS.PrintTrailBatches table'
ALTER TABLE AM.MailPieces
ADD PDFFileSize BIGINT NULL
go
ALTER TABLE PnS.MRDFRequest
ADD PDFFileSize BIGINT NULL
go
ALTER TABLE PnS.PrintTrailBatches
ADD ZipFileSize BIGINT NULL
go
PRINT 'Added new table PnS.EnvelopeUsage'
CREATE TABLE [PnS].[EnvelopeUsage](
	[PrintTrailBatchId] [int] NULL,
	[EnvelopeId] [varchar](255) NULL,
	[Quantity] [int] NULL,
	[InsertedOn] [datetime] NULL
) ON [PRIMARY]
GO

ALTER TABLE [PnS].[EnvelopeUsage]  WITH CHECK ADD  CONSTRAINT [FK_EnvelopeUsage_Envelopes] FOREIGN KEY([EnvelopeId])
REFERENCES [Common].[Envelopes] ([Id])
GO

ALTER TABLE [PnS].[EnvelopeUsage] CHECK CONSTRAINT [FK_EnvelopeUsage_Envelopes]
GO

ALTER TABLE [PnS].[EnvelopeUsage]  WITH CHECK ADD  CONSTRAINT [FK_EnvelopeUsage_PrintTrailBatches] FOREIGN KEY([PrintTrailBatchId])
REFERENCES [PnS].[PrintTrailBatches] ([Id])
GO

ALTER TABLE [PnS].[EnvelopeUsage] CHECK CONSTRAINT [FK_EnvelopeUsage_PrintTrailBatches]
GO

CREATE INDEX Idx_EnvelopeUsage_PrintTrailBatchId
ON [PnS].[EnvelopeUsage] (PrintTrailBatchId);
GO


PRINT 'Added new table AM.ErrorNotifications'

CREATE TABLE [AM].[ErrorNotifications](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ClientId] [int] NOT NULL,
	[MailBatchId] [int] NOT NULL,
	[TotalMPsCount] [int] NOT NULL,
	[ErrorMPsCount] [int] NOT NULL,
	[ReceivedOn] [datetime] NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
 CONSTRAINT [PK_ErrorNotifications] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

CREATE UNIQUE INDEX Idx_ErrorNotifications_MailBatchId
ON AM.ErrorNotifications (MailBatchId);
GO

CREATE INDEX Idx_ErrorNotifications_ClientId
ON AM.ErrorNotifications (ClientId);
GO
CREATE nonclustered INDEX Idx_PrintTrailBatches_SubmittedOn
ON PnS.PrintTrailBatches(SubmittedOn);
GO



PRINT 'Added new table [AM].[AlertSubscribers]'
CREATE TABLE [AM].[AlertSubscribers](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[UserId] [nvarchar](128) NOT NULL,
	[ClientId] [int] NOT NULL,
	[AlertType] [varchar](255) NOT NULL,
 CONSTRAINT [PK_AlertSubscribers] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [AM].[AlertSubscribers]  WITH CHECK ADD  CONSTRAINT [FK_AlertSubscribers_Clients] FOREIGN KEY([ClientId])
REFERENCES [Common].[Clients] ([Id])
GO

ALTER TABLE [AM].[AlertSubscribers] CHECK CONSTRAINT [FK_AlertSubscribers_Clients]
GO

ALTER TABLE [AM].[AlertSubscribers]  WITH CHECK ADD  CONSTRAINT [FK_AlertSubscribers_Users] FOREIGN KEY([UserId])
REFERENCES [Profile].[Users] ([Id])
GO

ALTER TABLE [AM].[AlertSubscribers] CHECK CONSTRAINT [FK_AlertSubscribers_Users]
GO
-----------------------------------------------------------------------------------
-- Min Max Page Fields
ALTER TABLE AM.DocTypes
ADD MinPage INT NULL, MaxPage INT NULL
GO
-- New table Added ClientDocTypes
CREATE TABLE AM.ClientDocTypes
(
	Id INT IDENTITY(1,1) NOT NULL,
	ClientId INT NOT NULL,
	DocTypeName NVARCHAR(255) NOT NULL,
	CONSTRAINT [PK_ClientDocTypes_Id] PRIMARY KEY CLUSTERED (Id ASC),
	CONSTRAINT FK_ClientDocTypes_Clients FOREIGN KEY (ClientId) REFERENCES Common.Clients (Id) 
)
GO
CREATE INDEX Idx_ClientDocTypes_DocTypeName
ON AM.ClientDocTypes (DocTypeName);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_ClientDocTypes_ClientId_DocTypeName] ON [AM].[ClientDocTypes]
(
	[ClientId] ASC,
	[DocTypeName] ASC
)
GO
ALTER TABLE AM.DocTypes
ADD ClientDocTypeId INT NULL,IsFallBack BIT
GO
ALTER TABLE AM.DocTypes  WITH CHECK ADD  CONSTRAINT [FK_DocTypes_ClientDocTypes] FOREIGN KEY(ClientDocTypeId)
REFERENCES AM.ClientDocTypes ([Id])
GO
CREATE INDEX Idx_ClientDocTypes_ClientId
ON AM.ClientDocTypes (ClientId);
GO
CREATE INDEX Idx_DocTypes_ClientDocTypeId
ON AM.DocTypes (ClientDocTypeId);
GO
DROP INDEX IX_DocTypes_ClientId_Description
ON AM.DocTypes
GO
DROP INDEX IX_DocTypes_ClientId_DocTypeCode
ON AM.DocTypes
GO
--EXEC sp_rename 'AM.DocTypes.DocTypeCode', 'DocTypeCodeDelete', 'COLUMN';
--GO
--EXEC sp_rename 'AM.DocTypes.Description', 'DescriptionDelete', 'COLUMN';
--GO
--ALTER TABLE AM.DocTypes ALTER COLUMN DocTypeCodeDelete VARCHAR(255) NULL
--GO
--ALTER TABLE AM.DocTypes ALTER COLUMN DescriptionDelete VARCHAR(512) NULL
--GO		



--Change Mapping Of DocType Envelopes

ALTER TABLE [AM].[DocTypes] DROP CONSTRAINT [FK_DocTypes_Envelope]
GO
ALTER TABLE [AM].[DocTypes] DROP CONSTRAINT [FK_DocTypes_BREEnvelope]
GO
/****** Object:  Index [PK_Envelopes]    Script Date: 23-07-2019 04:18:24 PM ******/
ALTER TABLE [Common].[Envelopes] DROP CONSTRAINT [PK_Envelopes] WITH ( ONLINE = OFF )
GO

sp_RENAME '[Common].[Envelopes].[Id]' , 'EnvelopeName', 'COLUMN'
GO

ALTER TABLE Common.Envelopes
ADD Id INT IDENTITY(1,1) NOT NULL,PartNumber VARCHAR(255) NULL,EnvelopeDesc VARCHAR(255) NULL
GO

ALTER TABLE Common.Envelopes
ADD CONSTRAINT PK_Envelopes PRIMARY KEY(Id)
GO

sp_RENAME '[AM].[DocTypes].[EnvelopeId]' , 'EnvelopeName1', 'COLUMN'
GO

sp_RENAME '[AM].[DocTypes].[BREEnvelopeId]' , 'BREEnvelopeName1', 'COLUMN'
GO

ALTER TABLE [AM].[DocTypes]
ADD EnvelopeId INT NULL,BREEnvelopeId INT NULL
GO

ALTER TABLE [AM].[DocTypes]  WITH CHECK ADD  CONSTRAINT [FK_DocTypes_Envelope] FOREIGN KEY([EnvelopeId])
REFERENCES [Common].[Envelopes] ([Id])
GO

ALTER TABLE [AM].[DocTypes]  WITH CHECK ADD  CONSTRAINT [FK_DocTypes_BREEnvelope] FOREIGN KEY([EnvelopeId])
REFERENCES [Common].[Envelopes] ([Id])
GO

ALTER TABLE [PnS].[EnvelopeUsage] DROP CONSTRAINT [FK_EnvelopeUsage_Envelopes]
GO

sp_RENAME '[PnS].[EnvelopeUsage].[EnvelopeId]' , 'EnvelopeName1', 'COLUMN'
GO

ALTER TABLE [PnS].[EnvelopeUsage]
ADD EnvelopeId INT NULL
GO

ALTER TABLE [PnS].[EnvelopeUsage]  WITH CHECK ADD  CONSTRAINT [FK_EnvelopeUsage_Envelopes] FOREIGN KEY([EnvelopeId])
REFERENCES [Common].[Envelopes] ([Id])
GO

ALTER TABLE [Common].[EnvelopeInventory] DROP CONSTRAINT [FK_EnvelopeId_EnvelopeInventory]
GO

sp_RENAME '[Common].[EnvelopeInventory].[EnvelopeId]' , 'EnvelopeName1', 'COLUMN'
GO

ALTER TABLE [Common].[EnvelopeInventory]
ADD EnvelopeId INT NULL
GO

ALTER TABLE [Common].[EnvelopeInventory]  WITH CHECK ADD  CONSTRAINT [FK_EnvelopeId_EnvelopeInventory] FOREIGN KEY([EnvelopeId])
REFERENCES [Common].[Envelopes] ([Id])
GO

ALTER TABLE AM.DocTypes
ADD RetRcptAddressCountry VARCHAR(255) NULL
GO
ALTER TABLE AM.MailBatches
ADD RetRcptAddressCountry VARCHAR(255) NULL
GO
ALTER TABLE AM.MailPieces
ADD RcptAddressCountry VARCHAR(255) NULL,RetRcptAddressCountry VARCHAR(255) NULL,ClientDocTypeId INT NULL
GO
ALTER TABLE [AM].[MailPieces]  WITH CHECK ADD  CONSTRAINT [FK_MailPiece_ClientDocTypes] FOREIGN KEY([ClientDocTypeId])
REFERENCES [AM].[ClientDocTypes] ([Id])
GO
ALTER TABLE [Common].[PrintTrails]
ADD PrintLocation VARCHAR(255) NULL
GO
ALTER TABLE [PnS].[MRDFRequest]
ADD FilePath VARCHAR(255) NULL
GO

print 'Add columns in ProjectRequest'
alter table AM.ProjectRequest
add 
--ServiceLevelOther nvarchar(1024) null,
--PaperPrintRequirementsOther nvarchar(1024) null,
--PackagesSentToCustomerOther nvarchar(1024) null,
--EnvelopesOther nvarchar(1024) null,
--BusinessReplyMailEnvelopeOther nvarchar(1024) null,
FilesTransmittedDetail nvarchar(1024) null,
TypeOfFilesDetail nvarchar(1024) null
Go
alter table profile.Users
add 
--LastLoginTimeUTC datetime null,
ChangePasswordDateUTC datetime not null default(getutcdate()),
StatusModifiedBy nvarchar(128) null,
StatusModifiedOn datetime null
Go
alter table profile.Users
add constraint FK_Users_StatusModifiedBy foreign key(StatusModifiedBy)
references profile.Users (Id)
Go
------------STORE Procedures---------------------

PRINT 'Added new sp	usp_InsertErrorNotificationData'
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND  name = 'usp_InsertErrorNotificationData')
DROP PROCEDURE [dbo].[usp_InsertErrorNotificationData]
go
CREATE PROCEDURE [dbo].[usp_InsertErrorNotificationData]
@mailBatchId int
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @errorMPs INT,@errorMPsInNotify INT,@errorNotifyId INT

	select @errorMPs=count(Id) from am.MailPieces where mailbatchid=@mailBatchId and ShippingStatus='Error';
	if(@errorMPs>0)
	begin
		select @errorMPsInNotify=ErrorMPsCount,@errorNotifyId=Id from AM.ErrorNotifications WHERE MailBatchId=@mailBatchId;
		if(@errorMPsInNotify is null)
		begin 
			insert into AM.ErrorNotifications(ClientId, MailBatchId, ErrorMPsCount, ReceivedOn, CreatedOn)
			SELECT ClientId,Id,@errorMPs,ReceivedOn,GETUTCDATE() FROM AM.MailBatches WHERE Id=@mailBatchId
		end
		else
		begin
			if(@errorMPs > @errorMPsInNotify)
			begin
				delete from AM.ErrorNotifications where Id=@errorNotifyId;

				insert into AM.ErrorNotifications(ClientId, MailBatchId, ErrorMPsCount, ReceivedOn, CreatedOn)
				SELECT ClientId,Id,@errorMPs,ReceivedOn,GETUTCDATE() FROM AM.MailBatches WHERE Id=@mailBatchId
			end
		end
	end
END
GO

ALTER TABLE [PnS].[MRDFRequest]
ADD FilePath VARCHAR(255) NULL
GO

------------STORE Procedures---------------------


IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND  name = 'usp_EnvelopeInventoryStockCalculation')
DROP PROCEDURE [dbo].[usp_EnvelopeInventoryStockCalculation]
go

CREATE PROCEDURE [dbo].[usp_EnvelopeInventoryStockCalculation]
	
AS
BEGIN
	;with allenvelope as
(
	select EnvelopeId,cast(SubmittedOn as date) as SubmittedOn,Quantity,
ROW_NUMBER() OVER (PARTITION BY EnvelopeId order by cast(SubmittedOn as date) desc) as RowNum
	from PnS.EnvelopeUsage EU
	INNER JOIN PnS.PrintTrailBatches PTB ON EU.PrintTrailBatchId=PTB.Id
	where cast(SubmittedOn as date) < cast(GETUTCDATE() as date)
	GROUP BY cast(SubmittedOn as date),EnvelopeId,Quantity
),
monthAvg as
(
	select EnvelopeId,count(distinct(SubmittedOn)) as TotalDays,SUM(Quantity) AS TotalQuantityUsed
	from allenvelope
	where RowNum<=30
	GROUP BY EnvelopeId
),
weekAvg as
(
	select EnvelopeId,count(distinct(cast(SubmittedOn as date))) as TotalDays,SUM(Quantity) AS TotalQuantityUsed
	from allenvelope
	where RowNum<=7
	GROUP BY EnvelopeId
),
mergeAvg as
(
	select m.EnvelopeId,case when m.TotalQuantityUsed>isnull(w.TotalQuantityUsed,0) then m.TotalQuantityUsed else w.TotalQuantityUsed end as TotalQuantityUsed,
	case when m.TotalDays>isnull(w.TotalDays,0) then m.TotalDays else w.TotalDays end as TotalDays
	from monthAvg m
	left outer join weekAvg w on m.EnvelopeId=w.EnvelopeId
)
select c.*,e.CurrentQuantity,e.ReorderDuration,CEILING(cast(TotalQuantityUsed as float)/TotalDays) as AvgRate,CEILING(cast(e.CurrentQuantity as float)/(TotalQuantityUsed/TotalDays)) as EnvDays
from mergeAvg c
inner join Common.Envelopes e on c.EnvelopeId=e.Id
END
go
-----------------------------------------------------------------------------------------------------------
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND  name = 'uspalert_GetDelayedBatchesToSubmit')
DROP PROCEDURE [dbo].[uspalert_GetDelayedBatchesToSubmit]
go
CREATE PROCEDURE [dbo].[uspalert_GetDelayedBatchesToSubmit] 	
	@cutOffMinutes int
AS
BEGIN
 select mp.Id,mp.businessfriendlyidvalue as LoanNumber,mp.TrackingNo,mb.Id as MailBatchId,rq.UniqueId,bs.IsSubmitted,c.AppCode,
 mb.BatchName,mb.ReceivedOn,mb.PipelineStatus,mp.DueDate, mp.ShippingStatus,cd.DocTypeName as DocTypeCode,
 d.ShippingCarrier,d.ShippingServiceType,mp.ClientId,pv.Name as PrintLocation,pv.VendorCode,rq.MailPieceStatus,pt.TrailName as PrintTrail  
 from AM.MailPieces mp
 inner join AM.MailBatches mb on mp.MailBatchId=mb.Id
 left outer join Common.PrintTrails pt on mp.PrintTrailId = pt.Id
 left outer join Common.PrintVendors pv on pt.PrintVendorId = pv.Id
 left outer join Common.Clients c on mb.ClientId=c.Id
 left outer join PnS.MRDFRequest rq on mp.UniqueId=rq.UniqueId
 left outer join PnS.BatchSummary bs on rq.JobId=bs.PrintBatch
 left outer join AM.DocTypes d on mp.DocTypeId = d.Id
 left outer join AM.ClientDocTypes cd on d.ClientDocTypeId=cd.Id
 where mb.ReceivedOn between DATEADD(day,-7,GETUTCDATE()) and DATEADD(minute,-@cutOffMinutes,GETUTCDATE()) and (mp.ShippingStatus is null or (mp.ShippingStatus in ('InProgress','Shipped') and (rq.UniqueId is null or ISNULL(bs.IsSubmitted,0)=0)) or (mp.ShippingStatus = 'Error' and mp.ErrorDesc IS NULL) or (mp.ShippingStatus = 'PulledBack' and not exists (select Id from AM.PullBackRequests where MailPieceId = mp.Id)) or (mb.ClientId IS NULL))
 or (rq.MailPieceStatus='Shipped' and mp.IsPickedUp <> 1 and mp.DueDate < GETUTCDATE() and d.ShippingCarrier='FedEx' and mb.ReceivedOn>='2019-01-01 05:00:00')
END
go
---------------------------------------------------------------------------------------------------------
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND  name = 'usp_GetEnvelopeInventoryUsage')
DROP PROCEDURE [dbo].[usp_GetEnvelopeInventoryUsage]
go
Create PROCEDURE [dbo].[usp_GetEnvelopeInventoryUsage]
	@Id int
	AS
	BEGIN
		-- SET NOCOUNT ON added to prevent extra result sets from
		-- interfering with SELECT statements.
		SET NOCOUNT ON;
		
;with cte as
(
select  distinct(cast(SubmittedOn as date)) as Date ,SUM(Quantity) AS UsageStock
	from PnS.EnvelopeUsage EU
	INNER JOIN PnS.PrintTrailBatches PTB ON EU.PrintTrailBatchId=PTB.Id
	WHERE EU.EnvelopeId=@Id
	GROUP BY cast(SubmittedOn as date)  
)
select top 30 * from cte
order by date desc
	END
GO
---------------------------------------------------------------------------------------------------------
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND  name = 'uspweb_ErrorReportSLA')
DROP PROCEDURE [dbo].[uspweb_ErrorReportSLA]
go
Create PROCEDURE [dbo].[uspweb_ErrorReportSLA]
	@clientId int,
	@startDateRec datetime,
	@endDateRec datetime,
	@startDateDue datetime,
	@endDateDue datetime,
	@batchId int,
	@docType varchar(128)
AS
BEGIN

select *	
	from [AM].[MailBatches] b, [Common].[SFTChannels] ch, [Common].[Clients] c,[AM].[MailPieces] m
	left outer join [AM].[DocTypes] d on m.DocTypeId=d.Id
	left outer join AM.ClientDocTypes cd on d.ClientDocTypeId=cd.Id
	where m.MailBatchId=b.Id and c.Id=b.ClientId and ch.Id=b.SFTChannelId AND m.ClientId=@clientId --AND m.SLADays < 0
	AND ISNULL(b.ReceivedOn,0) BETWEEN ISNULL(@startDateRec, ISNULL(b.ReceivedOn, 0)) AND ISNULL(@endDateRec, ISNULL(b.ReceivedOn, 0))
	AND ISNULL(m.DueDate,0) BETWEEN ISNULL(@startDateDue, ISNULL(m.DueDate, 0)) AND ISNULL(@endDateDue, ISNULL(m.DueDate, 0))
	AND ISNULL(m.MailBatchId, '') = ISNULL(@batchId, ISNULL(m.MailBatchId, ''))
	AND ISNULL(d.ClientDocTypeId, '') = ISNULL(@docType, ISNULL(d.ClientDocTypeId, ''))
	--AND m.ShippingStatus IN ('InProgress','Shipped')
	--AND ISNULL(m.ShippingStatus, '') IN (ISNULL('''InProgress'',''Shipped''', ISNULL(m.ShippingStatus, '')))
	END
	go
-----------------------------------------------------------------------------------------------------
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND  name = 'uspweb_MonthlyReconReport')
DROP PROCEDURE [dbo].[uspweb_MonthlyReconReport]
go
create PROCEDURE [dbo].[uspweb_MonthlyReconReport]
	@clientId int,
	@startDate datetime,
	@endDate datetime,
	@channel varchar(128)
AS
begin
	select m.UniqueId,b.BatchName,m.BusinessFriendlyIdValue as LoanNumber,m.[FileName],m.RcptFirstName as FirstName,m.RcptLastName as LastName
	,m.RcptAddressStreet as [Address],m.RcptAddressCity as City, m.RcptAddressState as [State],m.RcptAddressZIP as Zip,
	b.ReceivedOn,m.PrintLocation,m.ShippingStatus as [Status],m.TrackingNo,m.TotalPages,m.PrintedOn,m.ShippedOn,
	m.PickedUpOn,d.ShippingCarrier,m.ErrorDesc,
	cd.DocTypeName as DocType,d.ShippingCarrier +' '+d.ShippingServiceType as ShippingMethod,
	ch.Title as Channel,c.AppCode,
	r.JobId,r.PieceId
	FROM [AM].[MailBatches] b, [Common].[SFTChannels] ch,[Common].[Clients] c,[AM].[MailPieces] m
	LEFT OUTER JOIN [AM].[DocTypes] d ON m.DocTypeId=d.Id
	left outer join AM.ClientDocTypes cd on d.ClientDocTypeId=cd.Id
	LEFT OUTER JOIN [PnS].[MRDFRequest] r ON m.UniqueId = r.UniqueId AND r.IsPDFReceived=1
	WHERE m.[MailBatchId]=b.Id AND c.Id=b.ClientId AND ch.Id=b.SFTChannelId AND m.ClientId=@clientId
	AND b.ReceivedOn BETWEEN @startDate and @endDate
	AND ISNULL(ch.Code, '') = ISNULL(@channel, ISNULL(ch.Code, ''))
end
go
-------------------------------------------------------------------------------------------------
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND  name = 'usp_GetMRDFRequestDataByPrintTrailId')
DROP PROCEDURE [dbo].[usp_GetMRDFRequestDataByPrintTrailId]
go

CREATE PROCEDURE [dbo].[usp_GetMRDFRequestDataByPrintTrailId] 
@printTrailId INT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT REQ.*,MP.Id as MailPieceId,MP.EffectiveReceivedOnEst, BS.ClientId,BS.DocTypeId,D.ShippingCarrier,
 D.ShippingServiceType,C.AppCode,MP.DueDate as MailPieceDueDate,D.EnvelopeId,D.BREEnvelopeId,E.EnvelopeName,BE.EnvelopeName AS BREEnvelopeName
  ,E.PartNumber AS EnvelopePartNo,BE.PartNumber AS BREPartNo,E.EnvelopeDesc AS EnvelopeDesc,BE.EnvelopeDesc AS BREDesc
 FROM PnS.MRDFRequest REQ
 INNER JOIN PnS.BatchSummary BS ON REQ.JobId=BS.PrintBatch
 LEFT OUTER JOIN AM.MailPieces MP ON REQ.UniqueId=MP.UniqueId
 INNER JOIN AM.DocTypes D ON BS.DocTypeId=D.Id
 INNER JOIN Common.Clients C ON C.Id=BS.ClientId
 LEFT OUTER JOIN Common.Envelopes E ON D.EnvelopeId=E.Id
 LEFT OUTER JOIN Common.Envelopes BE ON D.BREEnvelopeId=BE.Id
 WHERE BS.PipelineStatus='Prepped' AND  IsPrepped = 1 AND IsSubmitted = 0 AND REQ.PrintTrailBatchId IS NOT NULL
 AND BS.PrintTrailId=@printTrailId
	
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
  D.ShippingServiceType,C.AppCode,MP.DueDate as MailPieceDueDate,D.EnvelopeId,D.BREEnvelopeId,E.EnvelopeName,BE.EnvelopeName AS BREEnvelopeName
  ,E.PartNumber AS EnvelopePartNo,BE.PartNumber AS BREPartNo,E.EnvelopeDesc AS EnvelopeDesc,BE.EnvelopeDesc AS BREDesc
  FROM PnS.MRDFRequest REQ
  INNER JOIN PnS.BatchSummary BS ON REQ.JobId=BS.PrintBatch
  LEFT OUTER JOIN AM.MailPieces MP ON REQ.UniqueId=MP.UniqueId
  INNER JOIN AM.DocTypes D ON BS.DocTypeId=D.Id
  INNER JOIN Common.Clients C ON C.Id=BS.ClientId
  LEFT OUTER JOIN Common.Envelopes E ON D.EnvelopeId=E.Id
  LEFT OUTER JOIN Common.Envelopes BE ON D.BREEnvelopeId=BE.Id
  WHERE REQ.PrintTrailBatchId=@printTrailBatchId AND ISNULL(MailPieceStatus,'')=@mailPieceStatus 
 END
END

GO



-------------------------------------------------------------------------------------------------
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'TF' AND  name = 'ufn_GetDallasBatchDetails')
DROP function [dbo].[ufn_GetDallasBatchDetails]
go
Create FUNCTION [dbo].[ufn_GetDallasBatchDetails](@EstDate DATE)RETURNS @MPs TABLE (	Id int,	LoanNumber varchar(255) null,	ClientId int null,	PrintLocation varchar(255) null,	EffectiveReceivedOnEst datetime null,	TrackingNo varchar(255) null,	IsPullBackRequested bit,	IsQCRequested bit,	TotalSheets int,	DueDate datetime null,	TotalPages int,	ShippingStatus varchar(255) null,	PrintBatch int,	IsSubmitted bit,	PipelineStatus varchar(512),	PrintBatchName varchar(255),	JobName varchar(512) null,	SubmittedOn datetime null,	PieceId int null,	PrintTrailBatchId int null,	JobId int null,	MRDFReqDate datetime null,	PDFFileName varchar(255)null,	UniqueId uniqueidentifier null,	MailPieceStatus varchar(255) null,	ProcessingTimeStamp datetime null,	ShippingServiceType varchar(255) null,	ShippingCarrier varchar(255) null,	IsBRE bit,	AppCode varchar(128),	ClientName varchar(255) null,	BatchName varchar(128) null,	FilePath varchar(1024) null)ASbegin	declare @minDate datetime,@pl varchar(255);	declare @dallasBatchesTimes Table	(		AppCode VARCHAR(255),		BatchName varchar(255),		StartDate datetime,		EndDate datetime,		ShippingCarrier varchar(255),		ShippingServiceType varchar(255)	)	select @pl=Name from Common.PrintVendors where VendorCode='CL_DALLAS'	--Get batches timings	--select  into #dallasBatchesTimes from (select  from [dbo].[ufn_GetDallasBatches](@EstDate)) as t	insert into @dallasBatchesTimes	select * from [dbo].[ufn_GetDallasBatches](@EstDate)	--Assign value to mindate	select @minDate= MIN(StartDate) from @dallasBatchesTimes	insert into @MPs	SELECT m.Id,m.BusinessFriendlyIdValue as LoanNumber,m.ClientId, m.PrintLocation, m.EffectiveReceivedOnEst, m.TrackingNo,m.IsPullBackRequested,	m.IsQCRequested,m.TotalSheets, m.DueDate,m.TotalPages,m.ShippingStatus,	b.PrintBatch,b.IsSubmitted,b.PipelineStatus,preq.PrintBatchName,b.JobName,preq.SubmittedOn,	req.PieceId,req.PrintTrailBatchId, req.JobId,req.[Date] as MRDFReqDate,req.PDFFileName, req.UniqueId,req.MailPieceStatus, 	res.ProcessingTimeStamp,	d.ShippingServiceType,d.ShippingCarrier,d.IsBRE,	c.AppCode,c.Name as ClientName,(select BatchName from @dallasBatchesTimes where EffectiveReceivedOnEst between StartDate AND EndDate AND ShippingCarrier=d.ShippingCarrier AND ShippingServiceType=d.ShippingServiceType  AND AppCode=c.AppCode) BatchName,	req.FilePath	FROM [AM].[MailPieces] m	LEFT OUTER JOIN PnS.MRDFRequest req ON m.UniqueId = req.UniqueId	LEFT OUTER JOIN PnS.BatchSummary b ON req.JobId = b.PrintBatch	LEFT OUTER JOIN PnS.PrintTrailBatches preq ON req.PrintTrailBatchId=preq.Id	LEFT OUTER JOIN PnS.MRDFResponse res ON req.PrintTrailBatchId = res.JobId AND req.PieceId=res.PieceId	INNER JOIN AM.DocTypes d ON m.DocTypeId=d.Id	INNER JOIN Common.Clients c ON m.ClientId=c.Id	WHERE m.PrintLocation=@pl	AND m.EffectiveReceivedOnEst >=@minDate	returnend