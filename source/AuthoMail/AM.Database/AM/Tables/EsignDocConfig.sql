CREATE TABLE [AM].[EsignDocConfig]
(
	[Id]                 INT              IDENTITY (1000, 1) NOT NULL,
    [ClientId]           INT              NULL,
	[DocTypeId]          INT              NULL,
    [SignHeight]         INT              NOT NULL,
    [SignWidth]          INT              NOT NULL,
    [SignXOffset]        VARCHAR (255)    NOT NULL,
    [SignYOffset]        VARCHAR (255)    NOT NULL,
    [Optional]           BIT              NULL,
    [SignPageNumber]     INT              NOT NULL

    CONSTRAINT [PK_AM.EsignDocConfig] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_EsignDocConfig_Clients] FOREIGN KEY ([ClientId]) REFERENCES [Common].[Clients] ([Id]),
    CONSTRAINT [FK_EsignDocConfig_DocTypes] FOREIGN KEY ([DocTypeId]) REFERENCES [AM].[DocTypes] ([Id])
)
