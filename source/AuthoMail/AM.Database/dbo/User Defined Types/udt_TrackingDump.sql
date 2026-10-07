CREATE TYPE [dbo].[udt_TrackingDump] AS TABLE
(
	[TrackingNo] NVARCHAR(50) NULL, 
    [IsReturnTracking] BIT NULL, 
    [ShippingCarrier] NVARCHAR(50) NULL, 
    [ShipmentStatus] NVARCHAR(255) NULL, 
    [ShipmentStatusUpdatedOn] DATETIME NULL, 
    [DeliveryStatus] NVARCHAR(50) NULL, 
    [DeliveryDate] DATETIME NULL
)
