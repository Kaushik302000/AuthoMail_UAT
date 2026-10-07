CREATE TABLE [Common].[EnvelopeStock]
(
	[EnvelopeId] INT NOT NULL, 
    [ShipperId] INT NOT NULL, 
    [CurrentQuantity] INT NOT NULL, 
    [ReOrderDuration] INT NOT NULL, 
    CONSTRAINT [FK_EnvelopeStock_Envelope] FOREIGN KEY ([EnvelopeId]) REFERENCES [Common].[Envelopes]([Id]), 
    CONSTRAINT [FK_EnvelopeStock_ShipperInformation] FOREIGN KEY ([ShipperId]) REFERENCES [Common].[ShipperInformation]([Id])
)

GO

CREATE CLUSTERED INDEX [IX_EnvelopeStock_EnvelopeId]
    ON [Common].[EnvelopeStock]([EnvelopeId] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_EnvelopeStock_ShipperId]
    ON [Common].[EnvelopeStock]([ShipperId] ASC);
GO

