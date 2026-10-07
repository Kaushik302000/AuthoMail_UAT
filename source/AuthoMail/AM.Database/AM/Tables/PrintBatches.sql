CREATE TABLE [AM].[PrintBatches] (
    [Id]                         INT              IDENTITY (1000, 1) NOT NULL,
    [UniqueId]                   UNIQUEIDENTIFIER NOT NULL,
    [AppCode]                    VARCHAR (255)    NULL,
    [DocTypeId]                  INT              NULL,
    [BatchStatus]                VARCHAR (255)    NULL,
    [StatusDescription]          VARCHAR (1024)   NULL,
    [IsSubmittedToShipping]      AS               (case when [SubmittedToShippingOn] IS NULL then (0) else (1) end),
    [SubmittedToShippingOn]      DATETIME         NULL,
    [IsShippingResponseReceived] AS               (case when [ShippingResponseReceivedOn] IS NULL then (0) else (1) end),
    [ShippingResponseReceivedOn] DATETIME         NULL,
    [IsReportPrepped]            AS               (case when [ReportPreppedOn] IS NULL then (0) else (1) end),
    [ReportPreppedOn]            DATETIME         NULL,
    [IsReportedBack]             AS               (case when [ReportedBackOn] IS NULL then (0) else (1) end),
    [ReportedBackOn]             DATETIME         NULL,
    [PrintTrailId]               INT              NULL,
    [MailBatchId]                INT              NULL,
    [PnSConfigDetailId]          INT              NULL,
    CONSTRAINT [PK_PrintBatches] PRIMARY KEY CLUSTERED ([Id] ASC) WITH (FILLFACTOR = 70),
    CONSTRAINT [FK_PrintBatches_DocTypes] FOREIGN KEY ([DocTypeId]) REFERENCES [AM].[DocTypes] ([Id]),
    CONSTRAINT [FK_PrintBatches_MailBatches] FOREIGN KEY ([MailBatchId]) REFERENCES [AM].[MailBatches] ([Id]),
    CONSTRAINT [FK_PrintBatches_PnSConfigDetail] FOREIGN KEY ([PnSConfigDetailId]) REFERENCES [Common].[PnSConfigDetail] ([Id]),
    CONSTRAINT [FK_PrintBatches_PrintTrails] FOREIGN KEY ([PrintTrailId]) REFERENCES [Common].[PrintTrails] ([Id])
);




GO
CREATE NONCLUSTERED INDEX [IX_PrintBatches_BatchStatus]
    ON [AM].[PrintBatches]([BatchStatus] ASC) WITH (FILLFACTOR = 70);


GO
CREATE NONCLUSTERED INDEX [IX_PrintBatches_DocTypeId]
    ON [AM].[PrintBatches]([DocTypeId] ASC) WITH (FILLFACTOR = 70);


GO
CREATE NONCLUSTERED INDEX [IX_PrintBatches_ShippingResponseReceivedOn]
    ON [AM].[PrintBatches]([ShippingResponseReceivedOn] ASC) WITH (FILLFACTOR = 70);


GO
CREATE NONCLUSTERED INDEX [IX_PrintBatches_SubmittedToShippingOn]
    ON [AM].[PrintBatches]([SubmittedToShippingOn] ASC) WITH (FILLFACTOR = 70);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_PrintBatches_UniqueId]
    ON [AM].[PrintBatches]([UniqueId] ASC) WITH (FILLFACTOR = 70);


GO
CREATE NONCLUSTERED INDEX [IX_PrintBatches_PnSConfigDetailId]
    ON [AM].[PrintBatches]([PnSConfigDetailId] ASC) WITH (FILLFACTOR = 90);

