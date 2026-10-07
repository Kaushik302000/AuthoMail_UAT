CREATE TABLE [AM].[TrackingDump] (
    [Id] INT IDENTITY (1, 1) NOT NULL,
    [TrackingNo] NVARCHAR(50) NULL, 
    [IsReturnTracking] BIT NULL, 
    [ShippingCarrier] NVARCHAR(50) NULL, 
    [ShipmentStatus] NVARCHAR(255) NULL, 
    [ShipmentStatusUpdatedOn] DATETIME NULL, 
    [DeliveryStatus] NVARCHAR(50) NULL, 
    [DeliveryDate] DATETIME NULL, 
    [MailPieceId] INT NULL, 
    CONSTRAINT [PK_TrackingDump] PRIMARY KEY ([Id])
);

