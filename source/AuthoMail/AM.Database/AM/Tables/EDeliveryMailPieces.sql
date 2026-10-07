CREATE TABLE [AM].[EDeliveryMailPieces] (
    [Id]                   INT              IDENTITY (1000, 1) NOT NULL,
    [UniqueId]             UNIQUEIDENTIFIER NULL,
    [ClientId]             INT              NULL,
    [MailBatchId]          INT              NULL,
    [PipelineStatus]       VARCHAR (255)    NULL,
    [ErrorCode]            VARCHAR (255)    NULL,
    [ErrorDesc]            VARCHAR (MAX)    NULL,
    [SigningRoomId]        INT              NULL,
    [SigningRoomUniqueId]  UNIQUEIDENTIFIER NULL,
    [SigningRoomName]      VARCHAR (255)    NULL,
    [SRDocId]              INT              NULL,
    [SRDocGuId]            UNIQUEIDENTIFIER NULL,
    [ReceivedOn]           DATETIME         NULL,
    [IsReceived]           AS               (case when [ReceivedOn] IS NULL then (0) else (1) end),
    [DownloadedOn]         DATETIME         NULL,
    [IsDownloaded]         AS               (case when [DownloadedOn] IS NULL then (0) else (1) end),
    [ValidatedOn]          DATETIME         NULL,
    [IsValidated]          AS               (case when [ValidatedOn] IS NULL then (0) else (1) end),
    [SigningRoomCreatedOn] DATETIME         NULL,
    [IsSigningRoomCreated] AS               (case when [SigningRoomCreatedOn] IS NULL then (0) else (1) end),
    [InvitationSentOn]     DATETIME         NULL,
    [IsInvitationSent]     AS               (case when [InvitationSentOn] IS NULL then (0) else (1) end),
    [eDeliveredOn]         DATETIME         NULL,
    [IseDelivered]         AS               (case when [eDeliveredOn] IS NULL then (0) else (1) end),
    [ConsentAcceptedOn]    DATETIME         NULL,
    [IsConsentAccepted]    AS               (case when [ConsentAcceptedOn] IS NULL then (0) else (1) end),
    [ConsentDeclinedOn]    DATETIME         NULL,
    [IsConsentDeclined]    AS               (case when [ConsentDeclinedOn] IS NULL then (0) else (1) end),
    [DocReviewedOn]        DATETIME         NULL,
    [IsDocReviewed]        AS               (case when [DocReviewedOn] IS NULL then (0) else (1) end),
    [DocDownloadedOn]      DATETIME         NULL,
    [IsDocDownloaded]      AS               (case when [DocDownloadedOn] IS NULL then (0) else (1) end),
    [DownloadedIPAddress]  VARCHAR (50)     NULL,
    [ESignedOn]            DATETIME         NULL,
    [IsESigned]            AS               (case when [ESignedOn] IS NULL then (0) else (1) end),
    [TotalPages]           INT              NULL,
    [ReportedBackOn]       DATETIME         NULL,
    [IsReportedBack]       AS               (case when [ReportedBackOn] IS NULL then (0) else (1) end),
    [IsESignedDocDownloaded] BIT NULL, 
    CONSTRAINT [PK_EDeliveryMailPieces] PRIMARY KEY CLUSTERED ([Id] ASC) WITH (FILLFACTOR = 70)
);




GO
CREATE NONCLUSTERED INDEX [Idx_EDeliveryMailPieces_ValidatedOn]
    ON [AM].[EDeliveryMailPieces]([ValidatedOn] ASC) WITH (FILLFACTOR = 90);


GO
CREATE NONCLUSTERED INDEX [Idx_EDeliveryMailPieces_SigningRoomCreatedOn]
    ON [AM].[EDeliveryMailPieces]([SigningRoomCreatedOn] ASC) WITH (FILLFACTOR = 90);


GO
CREATE NONCLUSTERED INDEX [Idx_EDeliveryMailPieces_ReportedBackOn]
    ON [AM].[EDeliveryMailPieces]([ReportedBackOn] ASC) WITH (FILLFACTOR = 90);


GO
CREATE NONCLUSTERED INDEX [Idx_EDeliveryMailPieces_ReceivedOn]
    ON [AM].[EDeliveryMailPieces]([ReceivedOn] ASC) WITH (FILLFACTOR = 90);


GO
CREATE NONCLUSTERED INDEX [Idx_EDeliveryMailPieces_InvitationSentOn]
    ON [AM].[EDeliveryMailPieces]([InvitationSentOn] ASC) WITH (FILLFACTOR = 90);


GO
CREATE NONCLUSTERED INDEX [Idx_EDeliveryMailPieces_eDeliveredOn]
    ON [AM].[EDeliveryMailPieces]([eDeliveredOn] ASC) WITH (FILLFACTOR = 90);


GO
CREATE NONCLUSTERED INDEX [Idx_EDeliveryMailPieces_DownloadedOn]
    ON [AM].[EDeliveryMailPieces]([DownloadedOn] ASC) WITH (FILLFACTOR = 90);


GO
CREATE NONCLUSTERED INDEX [Idx_EDeliveryMailPieces_DocReviewedOn]
    ON [AM].[EDeliveryMailPieces]([DocReviewedOn] ASC) WITH (FILLFACTOR = 90);


GO
CREATE NONCLUSTERED INDEX [Idx_EDeliveryMailPieces_DocDownloadedOn]
    ON [AM].[EDeliveryMailPieces]([DocDownloadedOn] ASC) WITH (FILLFACTOR = 90);


GO
CREATE NONCLUSTERED INDEX [Idx_EDeliveryMailPieces_ConsentDeclinedOn]
    ON [AM].[EDeliveryMailPieces]([ConsentDeclinedOn] ASC) WITH (FILLFACTOR = 90);


GO
CREATE NONCLUSTERED INDEX [Idx_EDeliveryMailPieces_ConsentAcceptedOn]
    ON [AM].[EDeliveryMailPieces]([ConsentAcceptedOn] ASC) WITH (FILLFACTOR = 90);

