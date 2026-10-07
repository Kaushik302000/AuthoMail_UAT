CREATE TABLE [AM].[PullBackRequests] (
    [Id]                INT              IDENTITY (1, 1) NOT NULL,
    [ClientId]          INT              NULL,
    [MailBatchId]       INT              NOT NULL,
    [MailPieceId]       INT              NOT NULL,
    [MailPieceUniqueId] UNIQUEIDENTIFIER NOT NULL,
    [Status]            VARCHAR (255)    NOT NULL,
    [RequestedOn]       DATETIME         NOT NULL,
    [RequestedBy]       NVARCHAR (128)   NOT NULL,
    [PBReasons] NVARCHAR(2048) NULL, 
    CONSTRAINT [PK_AM.PullBackRequests] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_AM.PullBackRequests_MailBatches] FOREIGN KEY ([MailBatchId]) REFERENCES [AM].[MailBatches] ([Id]),
    CONSTRAINT [FK_AM.PullBackRequests_MailPieces] FOREIGN KEY ([MailPieceId]) REFERENCES [AM].[MailPieces] ([Id]),
    CONSTRAINT [FK_AM.PullBackRequests_Users] FOREIGN KEY ([RequestedBy]) REFERENCES [Profile].[Users] ([Id]),
    CONSTRAINT [FK_PullBackRequests_Clients] FOREIGN KEY ([ClientId]) REFERENCES [Common].[Clients] ([Id])
);


GO
CREATE NONCLUSTERED INDEX [IX_PullBackRequests_ClientId]
    ON [AM].[PullBackRequests]([ClientId] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_PullBackRequests_MailBatchId]
    ON [AM].[PullBackRequests]([MailBatchId] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_PullBackRequests_MailPieceId]
    ON [AM].[PullBackRequests]([MailPieceId] ASC);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_PullBackRequests_MailPieceUniqueId]
    ON [AM].[PullBackRequests]([MailPieceUniqueId] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_PullBackRequests_RequestedBy]
    ON [AM].[PullBackRequests]([RequestedBy] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_PullBackRequests_RequestedOn]
    ON [AM].[PullBackRequests]([RequestedOn] ASC);

