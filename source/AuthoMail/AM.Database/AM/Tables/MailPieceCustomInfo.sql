CREATE TABLE [AM].[MailPieceCustomInfo] (
    [MailPieceId] INT            NOT NULL,
    [Key]         VARCHAR (128)  NOT NULL,
    [DataType]    VARCHAR (128)  NULL,
    [Value]       VARCHAR (2048) NULL,
    CONSTRAINT [FK_MailPieceCustomInfo_MailPieces] FOREIGN KEY ([MailPieceId]) REFERENCES [AM].[MailPieces] ([Id])
);


GO

CREATE INDEX [IX_MailPieceCustomInfo_MailPieceId] ON [AM].[MailPieceCustomInfo] ([MailPieceId])
