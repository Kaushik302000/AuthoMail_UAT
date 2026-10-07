CREATE TABLE [Common].[Envelopes] (
    [Id]            INT            IDENTITY (1, 1) NOT NULL,
    [EnvelopeName]  VARCHAR (255)  NOT NULL,
    [EnvelopeDesc]  VARCHAR (255)  NULL,
    [PartNumber]    VARCHAR (255)  NOT NULL,
    [IsBREEnvelope] BIT            NOT NULL,
    [IsTrackingRequired] BIT NOT NULL DEFAULT 1, 
    [CreatedBy]     NVARCHAR (128) NOT NULL,
    [CreatedOn]     DATETIME       NOT NULL,
    [ModifiedBy]    NVARCHAR (128) NOT NULL,
    [ModifiedOn]    DATETIME       NOT NULL,
    CONSTRAINT [PK_Envelopes] PRIMARY KEY CLUSTERED ([Id] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_CreatedBy_Envelopes_Users] FOREIGN KEY ([CreatedBy]) REFERENCES [Profile].[Users] ([Id]),
    CONSTRAINT [FK_ModifiedBy_Envelopes_Users] FOREIGN KEY ([ModifiedBy]) REFERENCES [Profile].[Users] ([Id])
);






GO
CREATE UNIQUE NONCLUSTERED INDEX [Envelopes_PartNumber]
    ON [Common].[Envelopes]([PartNumber] ASC);


GO
CREATE UNIQUE NONCLUSTERED INDEX [Envelopes_EnvelopeName]
    ON [Common].[Envelopes]([EnvelopeName] ASC);

