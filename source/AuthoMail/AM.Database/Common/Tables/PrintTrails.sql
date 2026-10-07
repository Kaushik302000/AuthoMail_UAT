CREATE TABLE [Common].[PrintTrails] (
    [Id]              INT           IDENTITY (1, 1) NOT NULL,
    [TrailName]       VARCHAR (255) NULL,
    [PrintVendorCode] VARCHAR (255) NULL,
    [PrintVendorId]   INT           NOT NULL,
    [SimulateTrail]   BIT           NULL,
    [ShipperId]       INT           NULL,
    [IsSinglePdf]     BIT           NULL,
    [IsManifestRequired]      BIT           NULL,
    CONSTRAINT [PK_PrintTrails_Id] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_PrintTrails_PrintVendors] FOREIGN KEY ([PrintVendorId]) REFERENCES [Common].[PrintVendors] ([Id]),
    CONSTRAINT [FK_PrintTrails_ShipperInformation] FOREIGN KEY ([ShipperId]) REFERENCES [Common].[ShipperInformation] ([Id])
);








GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_PrintVendor_TrailName_ShipperId]
    ON [Common].[PrintTrails]([TrailName] ASC, [PrintVendorId] ASC, [ShipperId] ASC);

