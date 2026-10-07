CREATE TABLE [Common].[PnSConfigDetailCoverLetters] (
    [PnSConfigDetailId]       INT            NOT NULL,
    [SeqId]                   INT            NOT NULL,
    [CoverLetterTemplatePath] VARCHAR (1024) NULL,
    CONSTRAINT [PK_PnSConfigDetailCoverLetters] PRIMARY KEY CLUSTERED ([PnSConfigDetailId] ASC, [SeqId] ASC) WITH (FILLFACTOR = 70),
    CONSTRAINT [FK_PnSConfigDetailCoverLetters_PnSConfigDetail] FOREIGN KEY ([PnSConfigDetailId]) REFERENCES [Common].[PnSConfigDetail] ([Id])
);




GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_PnSConfigDetailCoverLetters_UniqueCoverLetter]
    ON [Common].[PnSConfigDetailCoverLetters]([PnSConfigDetailId] ASC, [SeqId] ASC, [CoverLetterTemplatePath] ASC) WITH (FILLFACTOR = 90);

