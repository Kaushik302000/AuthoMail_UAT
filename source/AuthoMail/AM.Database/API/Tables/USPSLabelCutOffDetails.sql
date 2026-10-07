CREATE TABLE [API].[USPSLabelCutOffDetails]
(
	[Id] INT PRIMARY KEY IDENTITY (1, 1) NOT NULL,
    [USPSUserId] VARCHAR(30) NOT NULL, 
    [BizStartTime] TIME NULL, 
    [BizEndTime] TIME NULL, 
    [SatStartTime] TIME NULL, 
    [SatEndTime] TIME NULL, 
    [PrintVendorLocation] VARCHAR(255) NOT NULL
)

GO

CREATE INDEX [IX_USPSLabelCutOffDetails_UniqueIndex] ON [API].[USPSLabelCutOffDetails] ([USPSUserId], [BizStartTime], [BizEndTime], [PrintVendorLocation])
