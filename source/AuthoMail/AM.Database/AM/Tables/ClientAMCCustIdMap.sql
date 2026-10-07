CREATE TABLE [AM].[ClientAMCCustIdMap] (
    [ClientId]  INT           NOT NULL,
    [AMCCustId] VARCHAR (128) NOT NULL,
    CONSTRAINT [PK_ClientAMCCustIdMap] PRIMARY KEY CLUSTERED ([ClientId] ASC, [AMCCustId] ASC),
    CONSTRAINT [FK_ClientAMCCustIdMap_Clients] FOREIGN KEY ([ClientId]) REFERENCES [Common].[Clients] ([Id])
);

