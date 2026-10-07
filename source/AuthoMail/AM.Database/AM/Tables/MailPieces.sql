CREATE TABLE [AM].[MailPieces] (
    [Id]                                  INT              IDENTITY (1000, 1) NOT NULL,
    [UniqueId]                            UNIQUEIDENTIFIER NULL,
    [ClientId]                            INT              NULL,
    [MailBatchId]                         INT              NOT NULL,
    [PrintBatchId]                        INT              NULL,
    [BusinessFriendlyIdName]              VARCHAR (255)    NULL,
    [BusinessFriendlyIdValue]             VARCHAR (255)    NULL,
    [FileName]                            VARCHAR (255)    NULL,
    [FilePattern]                         VARCHAR (255)    NULL,
    [DocumentOrPackageType]               VARCHAR (255)    NULL,
    [SLADays]                             AS               (datediff(second,isnull([ShippedOn],getutcdate()),[DueDate])),
    [DueDate]                             DATETIME         NULL,
    [ShippingStatus]                      VARCHAR (255)    NULL,
    [IsRcptInfoSpecified]                 BIT              NULL,
    [RcptPrefix]                          VARCHAR (255)    NULL,
    [RcptFirstName]                       VARCHAR (255)    NULL,
    [RcptMiddleName]                      VARCHAR (255)    NULL,
    [RcptLastName]                        VARCHAR (255)    NULL,
    [RcptSuffix]                          VARCHAR (255)    NULL,
    [RcptPhone]                           VARCHAR (255)    NULL,
    [RcptOrganization]                    VARCHAR (255)    NULL,
    [CoRcptPrefix]                        VARCHAR (255)    NULL,
    [CoRcptFirstName]                     VARCHAR (255)    NULL,
    [CoRcptMiddleName]                    VARCHAR (255)    NULL,
    [CoRcptLastName]                      VARCHAR (255)    NULL,
    [CoRcptSuffix]                        VARCHAR (255)    NULL,
    [CoRcptPhone]                         VARCHAR (255)    NULL,
    [CoRcptOrganization]                  VARCHAR (255)    NULL,
    [RcptAddrOptionalLine1AboveStreet]    VARCHAR (255)    NULL,
    [RcptAddrOptionalLine2AboveStreet]    VARCHAR (255)    NULL,
    [RcptAddressStreet]                   VARCHAR (255)    NULL,
    [RcptAddressAptOrSuiteOrOther]        VARCHAR (255)    NULL,
    [RcptAddrOptionalLine1BelowStreet]    VARCHAR (255)    NULL,
    [RcptAddrOptionalLine2BelowStreet]    VARCHAR (255)    NULL,
    [RcptAddressCity]                     VARCHAR (255)    NULL,
    [RcptAddressState]                    VARCHAR (255)    NULL,
    [RcptAddressZIP]                      VARCHAR (255)    NULL,
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
    [TotalPages]                          INT              NOT NULL,
    [TotalSheets]                         INT              NOT NULL,
    [ErrorCode]                           VARCHAR (255)    NULL,
    [ErrorDesc]                           VARCHAR (MAX)    NULL,
    [TrackingNo]                          VARCHAR (255)    NULL,
    [IsPullBackRequested]                 AS               (case when [PullBackRequestedOn] IS NULL then (0) else (1) end),
    [PullBackRequestedOn]                 DATETIME         NULL,
    [IsPulledBack]                        AS               (case when [PulledBackOn] IS NULL then (0) else (1) end),
    [PulledBackOn]                        DATETIME         NULL,
    [PulledBackRequestedBy]               NVARCHAR (128)   NULL,
    [IsPrinted]                           AS               (case when [PrintedOn] IS NULL then (0) else (1) end),
    [PrintedOn]                           DATETIME         NULL,
    [IsShipped]                           AS               (case when [ShippedOn] IS NULL then (0) else (1) end),
    [ShippedOn]                           DATETIME         NULL,
    [ShippingCost]                        DECIMAL (7, 4)   NULL,
    [Weight]                              DECIMAL (7, 4)   NULL,
    [IsQCRequested]                       BIT              CONSTRAINT [DF_MailPieces_IsQCRequested] DEFAULT ((0)) NULL,
    [IsQCPerformed]                       BIT              CONSTRAINT [DF_MailPieces_IsQCPerformed] DEFAULT ((0)) NULL,
    [DocTypeId]                           INT              NULL,
    [PrintLocation]                       VARCHAR (255)    NULL,
    [PickedUpOn]                          DATETIME         NULL,
    [IsPickedUp]                          AS               (case when [PickedUpOn] IS NULL then (0) else (1) end),
    [EffectiveReceivedOnEst]              DATETIME         NULL,
    [RcptAddressTypes]                    VARCHAR (255)    NULL,
    [StdSubmissionTime]                   DATETIME         NULL,
    [MaxSubmissionTime]                   DATETIME         NULL,
    [MissedSLAReason]                     VARCHAR (15)     NULL,
    [PrintTrailId]                        INT              NULL,
    [RcptAddressCountry]                  VARCHAR (255)    NULL,
    [RetRcptAddressCountry]               VARCHAR (255)    NULL,
    [PDFFileSize]                         BIGINT           NULL,
    [IsEDeliveryRequested]                BIT              NULL,
    [IsESignRequested]                    BIT              NULL,
    [IsFallBackRequested]                 BIT              NULL,
    [RcptEmailId]                         VARCHAR (255)    NULL,
    [ExpiryDate]                          DATETIME         NULL,
    [PnSConfigDetailId]                   INT              NULL,
    [Inserts]                             VARCHAR (255)    NULL,
    [TotalBlankPages]                     INT              NULL,
    [BlankPageNumbers]                    VARCHAR (MAX)    NULL,
    [ShipmentStatus] VARCHAR(512) NULL, 
    [ShipmentStatusUpdatedOn] DATETIME NULL, 
    [ShipperId] INT NULL, 
    [DeliveryStatus] VARCHAR(255) NULL, 
    [DeliveryDate] DATETIME NULL, 
    [ReturnTrackingNo] VARCHAR(255) NULL, 
    [ReturnDeliveryStatus] VARCHAR(255) NULL, 
    [ReturnDeliveryDate] DATETIME NULL, 
    [UsedEnvelopeId] INT NULL, 
    CONSTRAINT [PK_MailPieces] PRIMARY KEY CLUSTERED ([Id] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_MailPiece_DocTypes] FOREIGN KEY ([DocTypeId]) REFERENCES [AM].[DocTypes] ([Id]),
    CONSTRAINT [FK_MailPieces_Clients] FOREIGN KEY ([ClientId]) REFERENCES [Common].[Clients] ([Id]),
    CONSTRAINT [FK_MailPieces_MailBatches] FOREIGN KEY ([MailBatchId]) REFERENCES [AM].[MailBatches] ([Id]),
    CONSTRAINT [FK_MailPieces_PnSConfigDetail] FOREIGN KEY ([PnSConfigDetailId]) REFERENCES [Common].[PnSConfigDetail] ([Id]),
    CONSTRAINT [FK_MailPieces_PrintBatches] FOREIGN KEY ([PrintBatchId]) REFERENCES [AM].[PrintBatches] ([Id]),
    CONSTRAINT [FK_MailPieces_PrintTrails] FOREIGN KEY ([PrintTrailId]) REFERENCES [Common].[PrintTrails] ([Id]),
    CONSTRAINT [FK_MailPieces_Users] FOREIGN KEY ([PulledBackRequestedBy]) REFERENCES [Profile].[Users] ([Id]),
    CONSTRAINT [FK_MailPieces_ShipperId] FOREIGN KEY ([ShipperId]) REFERENCES [Common].[ShipperInformation] ([Id])
);




GO
CREATE NONCLUSTERED INDEX [IX_MailPieces_BusinessFriendlyIdName]
    ON [AM].[MailPieces]([BusinessFriendlyIdName] ASC) WITH (FILLFACTOR = 70);


GO
CREATE NONCLUSTERED INDEX [IX_MailPieces_BusinessFriendlyIdValue]
    ON [AM].[MailPieces]([BusinessFriendlyIdValue] ASC) WITH (FILLFACTOR = 70);


GO
CREATE NONCLUSTERED INDEX [IX_MailPieces_ClientId]
    ON [AM].[MailPieces]([ClientId] ASC) WITH (FILLFACTOR = 70);


GO
CREATE NONCLUSTERED INDEX [IX_MailPieces_DocTypeId]
    ON [AM].[MailPieces]([DocTypeId] ASC) WITH (FILLFACTOR = 70);


GO
CREATE NONCLUSTERED INDEX [IX_MailPieces_DueDate]
    ON [AM].[MailPieces]([DueDate] ASC) WITH (FILLFACTOR = 70);


GO
CREATE NONCLUSTERED INDEX [IX_MailPieces_EffectiveReceivedOnEst]
    ON [AM].[MailPieces]([EffectiveReceivedOnEst] ASC) WITH (FILLFACTOR = 70);


GO
CREATE NONCLUSTERED INDEX [IX_MailPieces_MailBatchId]
    ON [AM].[MailPieces]([MailBatchId] ASC) WITH (FILLFACTOR = 70);


GO
CREATE NONCLUSTERED INDEX [IX_MailPieces_PrintBatchId]
    ON [AM].[MailPieces]([PrintBatchId] ASC) WITH (FILLFACTOR = 70);


GO
CREATE NONCLUSTERED INDEX [IX_MailPieces_PrintedOn]
    ON [AM].[MailPieces]([PrintedOn] ASC) WITH (FILLFACTOR = 70);


GO
CREATE NONCLUSTERED INDEX [IX_MailPieces_PrintLocation]
    ON [AM].[MailPieces]([PrintLocation] ASC) WITH (FILLFACTOR = 70);


GO
CREATE NONCLUSTERED INDEX [IX_MailPieces_PullBackRequestedOn]
    ON [AM].[MailPieces]([PullBackRequestedOn] ASC) WITH (FILLFACTOR = 70);


GO
CREATE NONCLUSTERED INDEX [IX_MailPieces_PulledBackOn]
    ON [AM].[MailPieces]([PulledBackOn] ASC) WITH (FILLFACTOR = 70);


GO
CREATE NONCLUSTERED INDEX [IX_MailPieces_PulledBackRequestedBy]
    ON [AM].[MailPieces]([PulledBackRequestedBy] ASC) WITH (FILLFACTOR = 70);


GO
CREATE NONCLUSTERED INDEX [IX_MailPieces_ShippedOn]
    ON [AM].[MailPieces]([ShippedOn] ASC) WITH (FILLFACTOR = 70);


GO
CREATE NONCLUSTERED INDEX [IX_MailPieces_ShippingStatus]
    ON [AM].[MailPieces]([ShippingStatus] ASC) WITH (FILLFACTOR = 70);


GO
CREATE NONCLUSTERED INDEX [IX_MailPieces_TrackingNo]
    ON [AM].[MailPieces]([TrackingNo] ASC) WHERE ([TrackingNo] IS NOT NULL) WITH (FILLFACTOR = 70);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_MailPieces_UniqueId]
    ON [AM].[MailPieces]([UniqueId] ASC) WITH (FILLFACTOR = 70);


GO
CREATE NONCLUSTERED INDEX [IX_MailPieces_PnSConfigDetailId]
    ON [AM].[MailPieces]([PnSConfigDetailId] ASC) WITH (FILLFACTOR = 90);
GO
CREATE NONCLUSTERED INDEX IX_MailPieces_EffectiveReceivedOnEst_ClientId_FileName
ON [AM].[MailPieces] (
    EffectiveReceivedOnEst, ClientId, FileName
)
INCLUDE (
    BusinessFriendlyIdValue, ShippingStatus, RcptFirstName, RcptLastName, RcptAddressStreet,
    RcptAddressCity, RcptAddressState, RcptAddressZIP, DocumentOrPackageType
)
GO
