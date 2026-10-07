print 'start execution'
Print 'CREATE TABLE [Common].[Envelopes]'
Go
CREATE TABLE [Common].[Envelopes](
 [Id] [VARCHAR](255) UNIQUE NOT NULL,
 [CurrentQuantity] [INT] NULL,
 [IsBREEnvelope] [bit] NOT NULL,
 CONSTRAINT [PK_Envelopes] PRIMARY KEY CLUSTERED 
(
 [Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
Print 'Add [EnvelopeId],[BREEnvelopeId] columns in doctypes'
ALTER TABLE [AM].[DocTypes]
ADD [EnvelopeId] VARCHAR(255) NULL,[BREEnvelopeId] VARCHAR(255) NULL,NoOfCopies INT NULL

print 'Add FK in EnvelopeId column'
ALTER TABLE [AM].[DocTypes]  WITH CHECK ADD CONSTRAINT [FK_DocTypes_Envelope] FOREIGN KEY([EnvelopeId])
REFERENCES [Common].[Envelopes] ([Id])
GO

print 'Add FK in [BREEnvelopeId] column'
ALTER TABLE [AM].[DocTypes]  WITH CHECK ADD CONSTRAINT [FK_DocTypes_BREEnvelope] FOREIGN KEY([BREEnvelopeId])
REFERENCES [Common].[Envelopes] ([Id])
GO

ALTER TABLE [AM].[DocTypes] CHECK CONSTRAINT [FK_DocTypes_Envelope]
ALTER TABLE [AM].[DocTypes] CHECK CONSTRAINT [FK_DocTypes_BREEnvelope]
GO
UPDATE AM.DocTypes
SET NoOfCopies=1

GO
ALTER TABLE AM.DocTypes
ALTER COLUMN NoOfCopies INT NOT NULL

GO
Print 'CREATE TABLE [Common].[EnvelopeInventory]'
CREATE TABLE [Common].[EnvelopeInventory](
 [Id] [INT] IDENTITY(10000,1) NOT NULL,
 [EnvelopeId] VARCHAR(255) NOT NULL,
 [Description] [varchar](2048) NULL,
 [ReceivedOn] [datetime] NULL,
 [ReceivedQuantity] [INT] NOT NULL,
 [UnitPrice] [DECIMAL](5,2) NULL,
 CONSTRAINT [PK_EnvelopeInventory] PRIMARY KEY CLUSTERED 
(
 [Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

print 'add creation and modification columns in envelopes'
Go
ALTER TABLE [Common].[Envelopes]
ADD [CreatedBy] NVARCHAR(128) not null,[ModifiedBy] NVARCHAR(128) not null,[CreatedOn] datetime not null,[ModifiedOn] datetime not null

ALTER TABLE [Common].[Envelopes]  WITH CHECK ADD CONSTRAINT [FK_CreatedBy_Envelopes_Users] FOREIGN KEY([CreatedBy])
REFERENCES [Profile].[Users] ([Id])
GO
ALTER TABLE [Common].[Envelopes]  WITH CHECK ADD CONSTRAINT [FK_ModifiedBy_Envelopes_Users] FOREIGN KEY([ModifiedBy])
REFERENCES [Profile].[Users] ([Id])
GO
print 'add creation and modification columns in envelopes'
Go
ALTER TABLE [Common].[EnvelopeInventory]
ADD [CreatedBy] NVARCHAR(128) not null,[ModifiedBy] NVARCHAR(128) not null,[CreatedOn] datetime not null,[ModifiedOn] datetime not null

ALTER TABLE [Common].[EnvelopeInventory]  WITH CHECK ADD CONSTRAINT [FK_CreatedBy_EnvelopeInventory_Users] FOREIGN KEY([CreatedBy])
REFERENCES [Profile].[Users] ([Id])
GO
ALTER TABLE [Common].[EnvelopeInventory]  WITH CHECK ADD CONSTRAINT [FK_ModifiedBy_EnvelopeInventory_Users] FOREIGN KEY([ModifiedBy])
REFERENCES [Profile].[Users] ([Id])
Go
ALTER TABLE [Common].[EnvelopeInventory]  WITH CHECK ADD CONSTRAINT [FK_EnvelopeId_EnvelopeInventory_Users] FOREIGN KEY([EnvelopeId])
REFERENCES [Common].[Envelopes] ([Id])
GO

print 'create table [AM].[ProjectRequest]'
create table [AM].[ProjectRequest]
(
	Id [int] identity(1,1) not null primary key,
	ClientName nvarchar(255),	
	SubmittedOn datetime,
	SubmittedBy varchar(255),
	UATStartOn datetime null,
	ProdStartOn datetime null,
	Description nvarchar(2048),
	FileTransmittedVia nvarchar(1024),
	NoOfEstSweepTimes varchar(255),
	ESTSweepTime nvarchar(1024),
	TypeOfFiles nvarchar(1024),
	TransmittedTime nvarchar(255),
	ServiceLevel varchar(512),
	EstimatedPagesPerPackage int,
	EstimatedPackagesDaily int,
	EstimatedPackagesMonthly int,
	EstimatedPackagesAnnually int,
	DailyPrintSchedule varchar(100), -- PipeLine seperated
	PaperPrintRequirements nvarchar(2048),
	PackagesSentToCustomer nvarchar(2048),
	IsSampleEnvelopeAttached bit,
	Envelopes nvarchar(2048),
	IsBRERequired bit,
	BusinessReplyMailEnvelope nvarchar(2048),
	IsCustomerSupplyEnvelope bit,
	IsCustomerProvideArtworkEnvelope bit,
	IsClientPostageAccount bit,
	IsScanInAuthoMailDallas bit,
	FilesTransmittedOther nvarchar(2048),
	TypeOfFilesOther nvarchar(2048),
	PaperAndPrintDetails nvarchar(2048),
	ShippingMailingDetail nvarchar(2048),
	MailingEnvelopeDetail nvarchar(2048),
	BusinessReplyMailEnvelopeDetail nvarchar(2048),
	ListOfDocTypesAndPages nvarchar(2048),
	PDFSample nvarchar(2048),
	InsertRequired bit,
	BookletRequired bit,
	BusinessReplyMailEnvelopeRequired bit,
	OtherMaterialRequired bit,
	InsertMaterialOrderedBy varchar(255),--Corelogic or Client
	AdditionalInfo nvarchar(max),
	CreatedOn datetime not null,
	CreatedBy nvarchar(128) not null,
	ModifiedOn datetime not null,	
	ModifiedBy nvarchar(128) not null,
	CONSTRAINT CreatedBy_ProjectRequest_Users FOREIGN KEY (CreatedBy)
    REFERENCES [Profile].[users](Id),
	CONSTRAINT ModifiedBy_ProjectRequest_Users FOREIGN KEY (ModifiedBy)
    REFERENCES [Profile].[users](Id)
) 

Go
print 'procedure [uspalert_GetDelayedBatchesToSubmit]'
Go
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND  name = 'uspalert_GetDelayedBatchesToSubmit')
DROP PROCEDURE [dbo].[uspalert_GetDelayedBatchesToSubmit] 
GO
CREATE PROCEDURE [dbo].[uspalert_GetDelayedBatchesToSubmit] 	
	@cutOffMinutes int
AS
BEGIN
 select mp.Id,mb.Id as MailBatchId,rq.UniqueId,bs.IsSubmitted,c.AppCode,mb.BatchName,mb.ReceivedOn,mb.PipelineStatus,mp.ShippingStatus,d.DocTypeCode,d.ShippingCarrier,d.ShippingServiceType,mp.ClientId,pv.Name as PrintLocation,pv.VendorCode,rq.MailPieceStatus  from AM.MailPieces mp
 inner join AM.MailBatches mb on mp.MailBatchId=mb.Id
 left outer join Common.PrintTrails pt on mp.PrintTrailId = pt.Id
 left outer join Common.PrintVendors pv on pt.PrintVendorId = pv.Id
 left outer join Common.Clients c on mb.ClientId=c.Id
 left outer join PnS.MRDFRequest rq on mp.UniqueId=rq.UniqueId
 left outer join PnS.BatchSummary bs on rq.JobId=bs.PrintBatch
 left outer join AM.DocTypes d on mp.DocTypeId = d.Id
 where mb.ReceivedOn between DATEADD(day,-7,GETUTCDATE()) and DATEADD(minute,-@cutOffMinutes,GETUTCDATE()) and (mp.ShippingStatus is null or (mp.ShippingStatus in ('InProgress','Shipped') and (rq.UniqueId is null or ISNULL(bs.IsSubmitted,0)=0)) or (mp.ShippingStatus = 'Error' and mp.ErrorDesc IS NULL) or (mp.ShippingStatus = 'PulledBack' and not exists (select Id from AM.PullBackRequests where MailPieceId = mp.Id)) or (mb.ClientId IS NULL))
 or (rq.MailPieceStatus='Shipped' and mp.IsPickedUp <> 1 and mp.DueDate < GETUTCDATE() and d.ShippingCarrier='FedEx')
END

Go
print 'procedure [uspweb_MonthlyReconReport]'
Go
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND  name = 'uspweb_MonthlyReconReport')
DROP PROCEDURE [dbo].[uspweb_MonthlyReconReport]
GO
CREATE PROCEDURE [dbo].[uspweb_MonthlyReconReport]
	@clientId int,
	@startDate datetime,
	@endDate datetime,
	@channel varchar(128),
	@shippingStatus varchar(255)
AS
	select m.Id as MailPieceId,m.BusinessFriendlyIdValue as BusinessIdValue,m.[FileName],m.RcptFirstName as FirstName,m.RcptLastName as LastName
	,m.RcptAddressStreet as [Address],m.RcptAddressCity as City, m.RcptAddressState as [State],m.RcptAddressZIP as Zip,
	b.ReceivedOn,m.PrintLocation,m.ShippingStatus as [Status],m.TrackingNo,m.TotalPages,m.PrintedOn,m.ShippedOn,
	m.PickedUpOn,d.ShippingCarrier,
	--CASE WHEN d.ShippingCarrier ='FedEx' THEN m.PickedUpOn ELSE null END as PickedUpOn,
	d.DocTypeCode as DocType,d.ShippingServiceType as ShippingMethod,
	ch.Title as Channel,c.AppCode,
	r.JobId,r.PieceId
	FROM [AM].[MailBatches] b, [Common].[SFTChannels] ch,[Common].[Clients] c,[AM].[MailPieces] m
	LEFT OUTER JOIN [AM].[DocTypes] d ON m.DocTypeId=d.Id
	LEFT OUTER JOIN [PnS].[MRDFRequest] r ON m.UniqueId = r.UniqueId AND r.IsPDFReceived=1
	WHERE m.[MailBatchId]=b.Id AND c.Id=b.ClientId AND ch.Id=b.SFTChannelId AND m.ClientId=@clientId
	AND m.EffectiveReceivedOnEst BETWEEN @startDate and @endDate
	AND ISNULL(ch.Code, '') = ISNULL(@channel, ISNULL(ch.Code, ''))
	AND m.ShippingStatus=@shippingStatus

	Go
	print '[uspweb_ErrorReportSLA]'
Go
	IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND  name = 'uspweb_ErrorReportSLA')
DROP PROCEDURE [dbo].[uspweb_ErrorReportSLA]
GO
CREATE PROCEDURE [dbo].[uspweb_ErrorReportSLA]
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
	where m.MailBatchId=b.Id and c.Id=b.ClientId and ch.Id=b.SFTChannelId AND m.ClientId=@clientId --AND m.SLADays < 0
	AND ISNULL(b.ReceivedOn,0) BETWEEN ISNULL(@startDateRec, ISNULL(b.ReceivedOn, 0)) AND ISNULL(@endDateRec, ISNULL(b.ReceivedOn, 0))
	AND ISNULL(m.DueDate,0) BETWEEN ISNULL(@startDateDue, ISNULL(m.DueDate, 0)) AND ISNULL(@endDateDue, ISNULL(m.DueDate, 0))
	AND ISNULL(m.MailBatchId, '') = ISNULL(@batchId, ISNULL(m.MailBatchId, ''))
	AND ISNULL(m.DocTypeId, '') = ISNULL(@docType, ISNULL(m.DocTypeId, ''))
	--AND m.ShippingStatus IN ('InProgress','Shipped')
	--AND ISNULL(m.ShippingStatus, '') IN (ISNULL('''InProgress'',''Shipped''', ISNULL(m.ShippingStatus, '')))
	END

	Go
	

	print 'Add contact columns in clients'
	ALTER TABLE [Common].[Clients]
ADD [Address] nvarchar(255),[Contact1Name] varchar(255),[Contact1Phone] varchar(255),[Contact1Email] varchar(255),
[Contact2Name] varchar(255),[Contact2Phone] varchar(255),[Contact2Email] varchar(255),[Contact3Name] varchar(255),
[Contact3Phone] varchar(255),[Contact3Email] varchar(255),[ITContactName] varchar(255),[ITContactPhone] varchar(255),
[ITContactEmail] varchar(255),[BillingContactName] varchar(255),[BillingContactPhone] varchar(255),[BillingContactEmail] varchar(255)
Go




print 'End execution'