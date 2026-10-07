CREATE TABLE [Common].[ShipperInformation] (
    [Id]                INT           IDENTITY (1, 1) NOT NULL,
    [PrintVendorId]     INT           NOT NULL,
    [PrintLocation]     VARCHAR (255) NULL,
    [SFTChannelId]      INT           NULL,
    [PrintVendorCode]   VARCHAR (255) NULL,
    [PrintVendorName]   VARCHAR (255) NULL,
    [PrintVendorStreet] VARCHAR (255) NULL,
    [PrintVendorCity]   VARCHAR (255) NULL,
    [PrintVendorState]  VARCHAR (255) NULL,
    [PrintVendorZip]    VARCHAR (255) NULL,
    CONSTRAINT [PK_ShipperInformation_Id] PRIMARY KEY CLUSTERED ([Id] ASC)
);




GO



GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_ShipperPrintVendor_PrintLocation_SFTChannelId]
    ON [Common].[ShipperInformation]([PrintVendorId] ASC, [PrintLocation] ASC, [SFTChannelId] ASC);

