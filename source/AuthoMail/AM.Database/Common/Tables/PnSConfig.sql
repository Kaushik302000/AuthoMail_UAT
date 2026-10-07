CREATE TABLE [Common].[PnSConfig] (
    [Id]         INT           IDENTITY (1, 1) NOT NULL,
    [ConfigName] VARCHAR (255) NULL,
    [DocumentCode] VARCHAR(255) NULL, 
    CONSTRAINT [PK_PnSConfig] PRIMARY KEY CLUSTERED ([Id] ASC) WITH (FILLFACTOR = 70)
);

