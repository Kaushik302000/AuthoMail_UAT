CREATE TABLE [Common].[EnvelopeInventory] (
    [Id]               INT            IDENTITY (10000, 1) NOT NULL,
    [EnvelopeId]       INT            NOT NULL,
    [Description]      VARCHAR (2048) NULL,
    [ReceivedOn]       DATETIME       NULL,
    [ReceivedQuantity] INT            NOT NULL,
    [UnitPrice]        DECIMAL (5, 2) NULL,
    [CreatedBy]        NVARCHAR (128) NOT NULL,
    [ModifiedBy]       NVARCHAR (128) NOT NULL,
    [CreatedOn]        DATETIME       NOT NULL,
    [ModifiedOn]       DATETIME       NOT NULL,
    [ShipperId] INT NOT NULL, 
    CONSTRAINT [PK_EnvelopeInventory] PRIMARY KEY CLUSTERED ([Id] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_CreatedBy_EnvelopeInventory_Users] FOREIGN KEY ([CreatedBy]) REFERENCES [Profile].[Users] ([Id]),
    CONSTRAINT [FK_EnvelopeId_EnvelopeInventory] FOREIGN KEY ([EnvelopeId]) REFERENCES [Common].[Envelopes] ([Id]),
    CONSTRAINT [FK_ModifiedBy_EnvelopeInventory_Users] FOREIGN KEY ([ModifiedBy]) REFERENCES [Profile].[Users] ([Id]),
    CONSTRAINT [FK_ShippedId_EnvelopeInventory_ShipperInformation] FOREIGN KEY ([ShipperId]) REFERENCES [Common].[ShipperInformation] ([Id]),
);




GO
CREATE NONCLUSTERED INDEX [IX_EnvelopeInventory_UnitPrice]
    ON [Common].[EnvelopeInventory]([UnitPrice] ASC) WITH (FILLFACTOR = 90);


GO
CREATE NONCLUSTERED INDEX [IX_EnvelopeInventory_ReceivedQuantity]
    ON [Common].[EnvelopeInventory]([ReceivedQuantity] ASC) WITH (FILLFACTOR = 90);


GO
CREATE NONCLUSTERED INDEX [IX_EnvelopeInventory_ReceivedOn]
    ON [Common].[EnvelopeInventory]([ReceivedOn] ASC) WITH (FILLFACTOR = 90);


GO
CREATE NONCLUSTERED INDEX [IX_EnvelopeInventory_ModifiedOn]
    ON [Common].[EnvelopeInventory]([ModifiedOn] ASC) WITH (FILLFACTOR = 90);


GO
CREATE NONCLUSTERED INDEX [IX_EnvelopeInventory_EnvelopeId]
    ON [Common].[EnvelopeInventory]([EnvelopeId] ASC) WITH (FILLFACTOR = 90);


GO
CREATE NONCLUSTERED INDEX [IX_EnvelopeInventory_CreatedOn]
    ON [Common].[EnvelopeInventory]([CreatedOn] ASC) WITH (FILLFACTOR = 90);

