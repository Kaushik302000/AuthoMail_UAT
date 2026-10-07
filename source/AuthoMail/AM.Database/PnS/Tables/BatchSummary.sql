CREATE TABLE [PnS].[BatchSummary] (
    [PrintBatch]         INT              IDENTITY (1000, 1) NOT NULL,
    [BatchIdentifier]    UNIQUEIDENTIFIER NULL,
    [ClientId]           INT              NULL,
    [JobName]            VARCHAR (512)    NULL,
    [PrintFileName]      VARCHAR (255)    NULL,
    [DueDate]            DATE             NULL,
    [SLADays]            INT              NULL,
    [DocTypeId]          INT              NULL,
    [PipelineStatus]     VARCHAR (512)    NULL,
    [ValidationStatus]   VARCHAR (512)    NULL,
    [OnValidationErrors] VARCHAR (512)    NULL,
    [ValidationErrors]   VARCHAR (MAX)    NULL,
    [BatchStatus]        VARCHAR (512)    NULL,
    [IsReceived]         AS               (case when [ReceivedOn] IS NULL then (0) else (1) end),
    [ReceivedOn]         DATETIME         NULL,
    [IsDownloaded]       AS               (case when [DownloadedOn] IS NULL then (0) else (1) end),
    [DownloadedOn]       DATETIME         NULL,
    [IsPrepped]          AS               (case when [PreppedOn] IS NULL then (0) else (1) end),
    [PreppedOn]          DATETIME         NULL,
    [IsSubmitted]        AS               (case when [SubmittedOn] IS NULL then (0) else (1) end),
    [SubmittedOn]        DATETIME         NULL,
    [IsMRDFReceived]     AS               (case when [MRDFReceivedOn] IS NULL then (0) else (1) end),
    [MRDFReceivedOn]     DATETIME         NULL,
    [IsReportPrepped]    AS               (case when [ReportPreppedOn] IS NULL then (0) else (1) end),
    [ReportPreppedOn]    DATETIME         NULL,
    [IsReportedBack]     AS               (case when [ReportedBackOn] IS NULL then (0) else (1) end),
    [ReportedBackOn]     DATETIME         NULL,
    [IsSuspended]        AS               (case when [SuspendedOn] IS NULL then (0) else (1) end),
    [SuspendedOn]        DATETIME         NULL,
    [IsCancelled]        AS               (case when [CancelledOn] IS NULL then (0) else (1) end),
    [CancelledOn]        DATETIME         NULL,
    [JulianDate]         INT              NULL,
    [Images]             INT              NULL,
    [IsPDFsReceived]     AS               (case when [PDFsReceivedOn] IS NULL then (0) else (1) end),
    [PDFsReceivedOn]     DATETIME         NULL,
    [PrintLocation]      VARCHAR (255)    NULL,
    [PrintTrailId]       INT              NULL,
    [PnSConfigDetailId]  INT              NULL,
    [ServerIP] VARCHAR(50) NULL, 
    CONSTRAINT [PK_BatchSummary] PRIMARY KEY CLUSTERED ([PrintBatch] ASC) WITH (FILLFACTOR = 70),
    CONSTRAINT [FK_BatchSummary_Clients] FOREIGN KEY ([ClientId]) REFERENCES [Common].[Clients] ([Id]),
    CONSTRAINT [FK_BatchSummary_DocTypes] FOREIGN KEY ([DocTypeId]) REFERENCES [AM].[DocTypes] ([Id]),
    CONSTRAINT [FK_BatchSummary_PnSConfigDetail] FOREIGN KEY ([PnSConfigDetailId]) REFERENCES [Common].[PnSConfigDetail] ([Id]),
    CONSTRAINT [FK_BatchSummary_PrintTrailBatches] FOREIGN KEY ([PrintTrailId]) REFERENCES [Common].[PrintTrails] ([Id])
);




GO
CREATE NONCLUSTERED INDEX [IX_BatchSummary_CancelledOn]
    ON [PnS].[BatchSummary]([CancelledOn] ASC) WITH (FILLFACTOR = 70);


GO
CREATE NONCLUSTERED INDEX [IX_BatchSummary_ClientId]
    ON [PnS].[BatchSummary]([ClientId] ASC) WITH (FILLFACTOR = 70);


GO
CREATE NONCLUSTERED INDEX [IX_BatchSummary_DownloadedOn]
    ON [PnS].[BatchSummary]([DownloadedOn] ASC) WITH (FILLFACTOR = 70);


GO
CREATE NONCLUSTERED INDEX [IX_BatchSummary_DueDate]
    ON [PnS].[BatchSummary]([DueDate] ASC) WITH (FILLFACTOR = 70);


GO
CREATE NONCLUSTERED INDEX [IX_BatchSummary_MRDFReceivedOn]
    ON [PnS].[BatchSummary]([MRDFReceivedOn] ASC) WITH (FILLFACTOR = 70);


GO
CREATE NONCLUSTERED INDEX [IX_BatchSummary_PipelineStatus]
    ON [PnS].[BatchSummary]([PipelineStatus] ASC) WITH (FILLFACTOR = 70);


GO
CREATE NONCLUSTERED INDEX [IX_BatchSummary_PreppedOn]
    ON [PnS].[BatchSummary]([PreppedOn] ASC) WITH (FILLFACTOR = 70);


GO
CREATE NONCLUSTERED INDEX [IX_BatchSummary_PrintLocation]
    ON [PnS].[BatchSummary]([PrintLocation] ASC) WITH (FILLFACTOR = 70);


GO
CREATE NONCLUSTERED INDEX [IX_BatchSummary_ReceivedOn]
    ON [PnS].[BatchSummary]([ReceivedOn] ASC) WITH (FILLFACTOR = 70);


GO
CREATE NONCLUSTERED INDEX [IX_BatchSummary_ReportedBackOn]
    ON [PnS].[BatchSummary]([ReportedBackOn] ASC) WITH (FILLFACTOR = 70);


GO
CREATE NONCLUSTERED INDEX [IX_BatchSummary_ReportPreppedOn]
    ON [PnS].[BatchSummary]([ReportPreppedOn] ASC) WITH (FILLFACTOR = 70);


GO
CREATE NONCLUSTERED INDEX [IX_BatchSummary_SubmittedOn]
    ON [PnS].[BatchSummary]([SubmittedOn] ASC) WITH (FILLFACTOR = 70);


GO
CREATE NONCLUSTERED INDEX [IX_BatchSummary_SuspendedOn]
    ON [PnS].[BatchSummary]([SuspendedOn] ASC) WITH (FILLFACTOR = 70);


GO
CREATE NONCLUSTERED INDEX [IX_Deliverables_DocTypeId]
    ON [PnS].[BatchSummary]([DocTypeId] ASC) WITH (FILLFACTOR = 70);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_BatchSummary_BatchIdentifier]
    ON [PnS].[BatchSummary]([BatchIdentifier] ASC) WHERE ([BatchIdentifier] IS NOT NULL) WITH (FILLFACTOR = 90);

