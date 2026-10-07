CREATE TABLE [Common].[MailPiecePageInfo] (
    [MailPieceId] INT           NOT NULL,
    [PageSize]    VARCHAR (255) NOT NULL,
    [PageCount]   INT           NULL,
    [PageNumbers] VARCHAR (MAX) NULL
    CONSTRAINT [PK_MailPiecePageInfo] PRIMARY KEY CLUSTERED ([MailPieceId] ASC, [PageSize] ASC) WITH (FILLFACTOR = 70),
    CONSTRAINT [FK_MailPiecePageInfo_MailPieces] FOREIGN KEY ([MailPieceId]) REFERENCES [AM].[MailPieces] ([Id])
);




GO
CREATE NONCLUSTERED INDEX [IX_MailPiecePageInfo_PageCount]
    ON [Common].[MailPiecePageInfo]([PageCount] ASC) WITH (FILLFACTOR = 90);

