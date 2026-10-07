CREATE TABLE [AM].[MailBatchCustomInfo] (
    [MailBatchId] INT            NOT NULL,
    [Key]         VARCHAR (128)  NOT NULL,
    [DataType]    VARCHAR (128)  NULL,
    [Value]       VARCHAR (2048) NULL,
    CONSTRAINT [PK_MailBatchCustomInfo] PRIMARY KEY CLUSTERED ([MailBatchId] ASC, [Key] ASC),
    CONSTRAINT [FK_MailBatchCustomInfo_MailBatches] FOREIGN KEY ([MailBatchId]) REFERENCES [AM].[MailBatches] ([Id])
);

