
-- create MRDFResponseHistory TABLE

CREATE TABLE [PnS].[MRDFResponseHistory](
 [JobId] [int] NOT NULL,
 [PieceId] [int] NOT NULL,
 [Date] [date] NULL,
 [Feeder2_Fed] [int] NULL,
 [OperatorId] [varchar](50) NULL,
 [MachineId] [varchar](50) NULL,
 [ProcessingTimeStamp] [datetime] NULL,
 [DetailedErrorId] [varchar](50) NULL,
 [Disposition] [varchar](50) NULL,
 [Input_Fed] [varchar](50) NULL,
 [Feeder3_Fed] [varchar](50) NULL,
 [Feeder4_Fed] [varchar](50) NULL,
 [Feeder5_Fed] [varchar](50) NULL,
 [ComputedWeight] [decimal](4, 3) NULL,
 [MeterDate] [varchar](50) NULL,
 [StatusSource] [varchar](50) NULL,
 [PostageMetered] [decimal](4, 3) NULL,
 [WeightMeasured] [decimal](4, 3) NULL,
 [ExitLocation] [varchar](50) NULL,
 [TotalSheets] [int] NULL,
 [PDFFileName] [varchar](255) NULL,
 [PackageTrackingNumber] [varchar](50) NULL,
 CONSTRAINT [PK_MRDFResponseHistory] PRIMARY KEY CLUSTERED 
(
 [JobId] ASC,
 [PieceId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO

ALTER TABLE [PnS].[MRDFResponseHistory]  WITH CHECK ADD  CONSTRAINT [FK_MRDFResponseHistory_BatchSummary] FOREIGN KEY([JobId])
REFERENCES [PnS].[BatchSummary] ([PrintBatch])
GO

ALTER TABLE [PnS].[MRDFResponseHistory] CHECK CONSTRAINT [FK_MRDFResponseHistory_BatchSummary]
GO


  

-----CHANGES FOR BRE
ALTER TABLE [AM].[DocTypes]
ADD [IsBRE] BIT NULL 

Go

UPDATE [AM].[DocTypes]
SET
IsBRE=0

Go

ALTER TABLE [AM].[DocTypes]
ALTER COLUMN [IsBRE] BIT NOT NULL 

ALTER TABLE [AM].[DocTypes] ADD  CONSTRAINT [DF_DocTypes_IsBRE]  DEFAULT ((0)) FOR [IsBRE]
  
  Go

CREATE TABLE [Common].[PrintVendors](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [varchar](255) NULL,
	[SFTChannelId] [int] NULL,
	[BusinessCode] [varchar](255) NULL,
	[VendorCode] [varchar](255) NULL,
 CONSTRAINT [PK_PrintVendors_Id] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

ALTER TABLE [Common].[PrintVendors]  WITH CHECK ADD  CONSTRAINT [FK_PrintVendors_SFTChannels] FOREIGN KEY([SFTChannelId])
REFERENCES [Common].[SFTChannels] ([Id])

ALTER TABLE [Common].[PrintVendors] CHECK CONSTRAINT [FK_PrintVendors_SFTChannels]



CREATE TABLE [Common].[PrintTrails](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[TrailName] [varchar](255) NULL,
	[PrintVendorId] [int] NOT NULL,
	[SimulateTrail] BIT NULL,
 CONSTRAINT [PK_PrintTrails_Id] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

ALTER TABLE [Common].[PrintTrails]  WITH CHECK ADD  CONSTRAINT [FK_PrintTrails_PrintVendors] FOREIGN KEY([PrintVendorId])
REFERENCES [Common].[PrintVendors] ([Id])
GO

ALTER TABLE [Common].[PrintTrails] CHECK CONSTRAINT [FK_PrintTrails_PrintVendors]
GO

CREATE TABLE [Common].[DocTypePrintTrailMaps](
	[DocTypeId] [int] NOT NULL,
	[PrintTrailId] [int] NOT NULL,
	[BizDayStartTime] [time](7) NULL,
	[BizDayEndTime] [time](7) NULL,
	[SatStartTime] [time](7) NULL,
	[SatEndTime] [time](7) NULL,
	[SunStartTime] [time](7) NULL,
	[SunEndTime] [time](7) NULL,
 CONSTRAINT [PK_DocTypeTrailMapsId] PRIMARY KEY CLUSTERED 
(
	[DocTypeId] ASC,
	[PrintTrailId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

ALTER TABLE [Common].[DocTypePrintTrailMaps]  WITH CHECK ADD  CONSTRAINT [FK_DocTypePrintTrailMaps_DocTypes] FOREIGN KEY([DocTypeId])
REFERENCES [AM].[DocTypes] ([Id])
GO

ALTER TABLE [Common].[DocTypePrintTrailMaps] CHECK CONSTRAINT [FK_DocTypePrintTrailMaps_DocTypes]
GO

ALTER TABLE [Common].[DocTypePrintTrailMaps]  WITH CHECK ADD  CONSTRAINT [FK_DocTypePrintTrailMaps_PrintTrails] FOREIGN KEY([PrintTrailId])
REFERENCES [Common].[PrintTrails] ([Id])


CREATE TABLE [PnS].[PrintTrailBatches](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[PrintBatchName] [varchar](255) NULL,
	[SubmittedOn] [datetime] NULL,
	[MRDFReceivedOn] [datetime] NULL,
	[PrintTrailId] [int] NOT NULL,
	[IsSubmitted]  AS (case when [SubmittedOn] IS NULL then (0) else (1) end),
	[IsMRDFReceived]  AS (case when [MRDFReceivedOn] IS NULL then (0) else (1) end),
	[SuspendedOn] [datetime] NULL,
	[IsSuspended]  AS (case when [SuspendedOn] IS NULL then (0) else (1) end), 
	[ValidationErrors] VARCHAR(255) NULL,
 CONSTRAINT [PK_PrintTrailBatches_Id] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

ALTER TABLE [PnS].[PrintTrailBatches]  WITH CHECK ADD  CONSTRAINT [FK_PrintTrailBatches_PrintTrails] FOREIGN KEY([PrintTrailId])
REFERENCES [Common].[PrintTrails] ([Id])
GO

ALTER TABLE [PnS].[PrintTrailBatches] CHECK CONSTRAINT [FK_PrintTrailBatches_PrintTrails]
GO


ALTER TABLE [AM].[MailPieces]
ADD [PrintTrailId] [int] NULL
ALTER TABLE [AM].[MailPieces]  WITH CHECK ADD  CONSTRAINT [FK_MailPieces_PrintTrails] FOREIGN KEY([PrintTrailId])
REFERENCES [Common].[PrintTrails] ([Id])
GO

ALTER TABLE [AM].[MailPieces] CHECK CONSTRAINT [FK_MailPieces_PrintTrails]
GO

ALTER TABLE [AM].[PrintBatches]
ADD [PrintTrailId] [int] NULL, [MailBatchId] [int] NULL
ALTER TABLE [AM].[PrintBatches]  WITH CHECK ADD  CONSTRAINT [FK_PrintBatches_MailBatches] FOREIGN KEY([MailBatchId])
REFERENCES [AM].[MailBatches] ([Id])
GO

ALTER TABLE [AM].[PrintBatches] CHECK CONSTRAINT [FK_PrintBatches_MailBatches]
GO

ALTER TABLE [AM].[PrintBatches]  WITH CHECK ADD  CONSTRAINT [FK_PrintBatches_PrintTrails] FOREIGN KEY([PrintTrailId])
REFERENCES [Common].[PrintTrails] ([Id])
GO

ALTER TABLE [AM].[PrintBatches] CHECK CONSTRAINT [FK_PrintBatches_PrintTrails]
GO

 --/* Insert seed batchsummary ID in PrintTrailBatches*/

declare @seedid int
set @seedid = (select max(printbatch) from [pns].[batchsummary])
if(@seedid is not null)
begin
	dbcc checkident ('PnS.PrintTrailBatches', RESEED,@seedid)
end

ALTER TABLE [PnS].[BatchSummary]
ADD [PrintTrailId] [int] NULL

ALTER TABLE [PnS].[BatchSummary]  WITH CHECK ADD  CONSTRAINT [FK_BatchSummary_PrintTrailBatches] FOREIGN KEY([PrintTrailId])
REFERENCES [Common].[PrintTrails] ([Id])
GO

ALTER TABLE [PnS].[BatchSummary] CHECK CONSTRAINT [FK_BatchSummary_PrintTrailBatches]
GO


ALTER TABLE [PnS].[MRDFRequest] 
ADD [PrintTrailBatchId] [int] NULL

ALTER TABLE [PnS].[MRDFRequest]  WITH CHECK ADD  CONSTRAINT [FK_MRDFRequest_PrintTrailBatches] FOREIGN KEY([PrintTrailBatchId])
REFERENCES [PnS].[PrintTrailBatches] ([Id])
GO

ALTER TABLE [PnS].[MRDFRequest] CHECK CONSTRAINT [FK_MRDFRequest_PrintTrailBatches]
GO

/****** Object:  Index [PK_MRDFResponse]    Script Date: 11/1/2018 6:54:55 PM ******/
ALTER TABLE [PnS].[MRDFResponse] DROP CONSTRAINT [PK_MRDFResponse]
GO
ALTER TABLE [PnS].[MRDFResponse] DROP CONSTRAINT [FK_MRDFResponse_BatchSummary]
GO

/****** Object:  Index [PK_MRDFResponse]    Script Date: 11/1/2018 6:54:55 PM ******/
ALTER TABLE [PnS].[MRDFResponse] ADD  CONSTRAINT [PK_MRDFResponse] PRIMARY KEY CLUSTERED 
(
	[JobId] ASC,
	[PieceId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
GO

ALTER TABLE [PnS].[MRDFResponse]  WITH CHECK ADD  CONSTRAINT [FK_MRDFResponse_PrintTrailBatches] FOREIGN KEY([JobId])
REFERENCES [PnS].[PrintTrailBatches] ([Id])
GO

ALTER TABLE [PnS].[MRDFResponse] CHECK CONSTRAINT [FK_MRDFResponse_PrintTrailBatches]
GO

ALTER TABLE [Common].[Clients] DROP CONSTRAINT [DF__Clients__Simulat__36B12243]
ALTER TABLE [Common].[Clients]
Drop COLUMN SimulatePCI



Go
CREATE UNIQUE NONCLUSTERED INDEX [IX_PrintVendor_TrailName] ON [Common].[PrintTrails]
(
 [TrailName] ASC,
 [PrintVendorId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)

GO
