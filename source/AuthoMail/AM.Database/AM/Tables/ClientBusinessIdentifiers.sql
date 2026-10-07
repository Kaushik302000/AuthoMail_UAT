CREATE TABLE [AM].[ClientBusinessIdentifiers] (
    [ClientId]       INT           NOT NULL,
    [BusinessIdName] VARCHAR (255) NOT NULL,
    CONSTRAINT [PK_ClientBusinessIdentifiers] PRIMARY KEY CLUSTERED ([ClientId] ASC, [BusinessIdName] ASC),
    CONSTRAINT [FK_ClientBusinessIdentifiers_Clients] FOREIGN KEY ([ClientId]) REFERENCES [Common].[Clients] ([Id])
);

