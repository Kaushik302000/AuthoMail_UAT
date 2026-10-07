CREATE TABLE [dbo].[ProductionBridge] (
    [Id]              INT      IDENTITY (1, 1) NOT NULL,
    [LastProcessedOn] DATETIME NOT NULL,
    CONSTRAINT [PK_ProductionBridge] PRIMARY KEY CLUSTERED ([Id] ASC)
);

