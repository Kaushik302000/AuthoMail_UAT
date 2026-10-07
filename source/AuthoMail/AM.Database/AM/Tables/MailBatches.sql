CREATE TABLE [AM].[MailBatches] (
    [Id]                                  INT              IDENTITY (1000, 1) NOT NULL,
    [UniqueId]                            UNIQUEIDENTIFIER NOT NULL,
    [ClientId]                            INT              NULL,
    [SFTChannelId]                        INT              NULL,
    [ClientBatchId]                       VARCHAR (255)    NULL,
    [BatchName]                           VARCHAR (255)    NULL,
    [DocumentOrPackageType]               VARCHAR (255)    NULL,
    [IsReceived]                          AS               (case when [ReceivedOn] IS NULL then (0) else (1) end),
    [ReceivedOn]                          DATETIME         NULL,
    [IsDownloaded]                        AS               (case when [DownloadedOn] IS NULL then (0) else (1) end),
    [DownloadedOn]                        DATETIME         NULL,
    [IsValidated]                         AS               (case when [ValidatedOn] IS NULL then (0) else (1) end),
    [ValidatedOn]                         DATETIME         NULL,
    [IsAcknowledged]                      AS               (case when [AcknowledgedOn] IS NULL then (0) else (1) end),
    [AcknowledgedOn]                      DATETIME         NULL,
    [IsPrepped]                           AS               (case when [PreppedOn] IS NULL then (0) else (1) end),
    [PreppedOn]                           DATETIME         NULL,
    [IsSubmittedToShipping]               AS               (case when [SubmittedToShippingOn] IS NULL then (0) else (1) end),
    [SubmittedToShippingOn]               DATETIME         NULL,
    [IsShippingResponseReceived]          AS               (case when [ShippingResponseReceivedOn] IS NULL then (0) else (1) end),
    [ShippingResponseReceivedOn]          DATETIME         NULL,
    [IsReportPrepped]                     AS               (case when [ReportPreppedOn] IS NULL then (0) else (1) end),
    [ReportPreppedOn]                     DATETIME         NULL,
    [IsReportedBack]                      AS               (case when [ReportedBackOn] IS NULL then (0) else (1) end),
    [ReportedBackOn]                      DATETIME         NULL,
    [IsSuspended]                         AS               (case when [SuspendedOn] IS NULL then (0) else (1) end),
    [SuspendedOn]                         DATETIME         NULL,
    [IsCancelled]                         AS               (case when [CancelledOn] IS NULL then (0) else (1) end),
    [CancelledOn]                         DATETIME         NULL,
    [Priority]                            VARCHAR (255)    NULL,
    [BatchStatus]                         VARCHAR (255)    NULL,
    [ValidationStatus]                    VARCHAR (255)    NULL,
    [ValidationErrors]                    VARCHAR (MAX)    NULL,
    [PipelineStatus]                      VARCHAR (255)    NULL,
    [IsReturnInfoSpecified]               BIT              NULL,
    [RetRcptPrefix]                       VARCHAR (255)    NULL,
    [RetRcptFirstName]                    VARCHAR (255)    NULL,
    [RetRcptMiddleName]                   VARCHAR (255)    NULL,
    [RetRcptLastName]                     VARCHAR (255)    NULL,
    [RetRcptSuffix]                       VARCHAR (255)    NULL,
    [RetRcptPhone]                        VARCHAR (255)    NULL,
    [RetRcptOrganization]                 VARCHAR (255)    NULL,
    [RetRcptAddrOptionalLine1AboveStreet] VARCHAR (255)    NULL,
    [RetRcptAddrOptionalLine2AboveStreet] VARCHAR (255)    NULL,
    [RetRcptAddressStreet]                VARCHAR (255)    NULL,
    [RetRcptAddressAptOrSuiteOrOther]     VARCHAR (255)    NULL,
    [RetRcptAddrOptionalLine1BelowStreet] VARCHAR (255)    NULL,
    [RetRcptAddrOptionalLine2BelowStreet] VARCHAR (255)    NULL,
    [RetRcptAddressCity]                  VARCHAR (255)    NULL,
    [RetRcptAddressState]                 VARCHAR (255)    NULL,
    [RetRcptAddressZIP]                   VARCHAR (255)    NULL,
    [RetRcptAddressCountry]               VARCHAR (255)    NULL,
    [IsReshippedBatch] BIT NULL, 
    [IsHeld]                      AS               (case when [HeldOn] IS NULL then (0) else (1) end),
    [HeldOn] DATETIME NULL, 
    [IsSubmittedToEDelivery]               AS               (case when [SubmittedToEDeliveryOn] IS NULL then (0) else (1) end),
    [SubmittedToEDeliveryOn]               DATETIME         NULL,
    [ServerIP] VARCHAR(50) NULL, 
    CONSTRAINT [PK_Batch] PRIMARY KEY CLUSTERED ([Id] ASC) WITH (FILLFACTOR = 70),
    CONSTRAINT [FK_MailBatches_Clients] FOREIGN KEY ([ClientId]) REFERENCES [Common].[Clients] ([Id]),
    CONSTRAINT [FK_MailBatches_SFTChannel] FOREIGN KEY ([SFTChannelId]) REFERENCES [Common].[SFTChannels] ([Id])
);


GO
CREATE NONCLUSTERED INDEX [IX_MailBatches_AcknowledgedOn]
    ON [AM].[MailBatches]([AcknowledgedOn] ASC) WITH (FILLFACTOR = 70);


GO
CREATE NONCLUSTERED INDEX [IX_MailBatches_BatchStatus]
    ON [AM].[MailBatches]([BatchStatus] ASC) WITH (FILLFACTOR = 70);


GO
CREATE NONCLUSTERED INDEX [IX_MailBatches_CancelledOn]
    ON [AM].[MailBatches]([CancelledOn] ASC) WITH (FILLFACTOR = 70);


GO
CREATE NONCLUSTERED INDEX [IX_MailBatches_ClientBatchId]
    ON [AM].[MailBatches]([ClientBatchId] ASC) WITH (FILLFACTOR = 70);


GO
CREATE NONCLUSTERED INDEX [IX_MailBatches_ClientId]
    ON [AM].[MailBatches]([ClientId] ASC) WITH (FILLFACTOR = 70);


GO
CREATE NONCLUSTERED INDEX [IX_MailBatches_DownloadedOn]
    ON [AM].[MailBatches]([DownloadedOn] ASC) WITH (FILLFACTOR = 70);


GO
CREATE NONCLUSTERED INDEX [IX_MailBatches_PipelineStatus]
    ON [AM].[MailBatches]([PipelineStatus] ASC) WITH (FILLFACTOR = 70);


GO
CREATE NONCLUSTERED INDEX [IX_MailBatches_PreppedOn]
    ON [AM].[MailBatches]([PreppedOn] ASC) WITH (FILLFACTOR = 70);


GO
CREATE NONCLUSTERED INDEX [IX_MailBatches_ReceivedOn]
    ON [AM].[MailBatches]([ReceivedOn] ASC) WITH (FILLFACTOR = 70);


GO
CREATE NONCLUSTERED INDEX [IX_MailBatches_ReportedBackOn]
    ON [AM].[MailBatches]([ReportedBackOn] ASC) WITH (FILLFACTOR = 70);


GO
CREATE NONCLUSTERED INDEX [IX_MailBatches_ReportPreppedOn]
    ON [AM].[MailBatches]([ReportPreppedOn] ASC) WITH (FILLFACTOR = 70);


GO
CREATE NONCLUSTERED INDEX [IX_MailBatches_SFTChannelId]
    ON [AM].[MailBatches]([SFTChannelId] ASC) WITH (FILLFACTOR = 70);


GO
CREATE NONCLUSTERED INDEX [IX_MailBatches_ShippingResponseReceivedOn]
    ON [AM].[MailBatches]([ShippingResponseReceivedOn] ASC) WITH (FILLFACTOR = 70);


GO
CREATE NONCLUSTERED INDEX [IX_MailBatches_SubmittedToShippingOn]
    ON [AM].[MailBatches]([SubmittedToShippingOn] ASC) WITH (FILLFACTOR = 70);


GO
CREATE NONCLUSTERED INDEX [IX_MailBatches_SuspendedOn]
    ON [AM].[MailBatches]([SuspendedOn] ASC) WITH (FILLFACTOR = 70);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_MailBatches_UniqueId]
    ON [AM].[MailBatches]([UniqueId] ASC) WITH (FILLFACTOR = 70);


GO
CREATE NONCLUSTERED INDEX [IX_MailBatches_ValidatedOn]
    ON [AM].[MailBatches]([ValidatedOn] ASC) WITH (FILLFACTOR = 70);
GO

CREATE NONCLUSTERED INDEX [IX_MailBatches_PipelineSFTChannelIdDownloadedOn]
    ON [AM].[MailBatches]([PipelineStatus] ASC, [SFTChannelId] ASC, [IsDownloaded] ASC, [IsValidated] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_MailBatches_PipelineSFTChannelIdValidatedOn]
    ON [AM].[MailBatches]([PipelineStatus] ASC, [SFTChannelId] ASC, [IsValidated] ASC, [IsAcknowledged] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_MailBatches_HeldOn]
    ON [AM].[MailBatches]([HeldOn] ASC) WITH (FILLFACTOR = 70);
GO
