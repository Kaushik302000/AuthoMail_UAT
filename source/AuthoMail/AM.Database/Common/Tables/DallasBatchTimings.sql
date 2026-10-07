CREATE TABLE [Common].[DallasBatchTimings]
(
	[Id] INT NOT NULL  IDENTITY, 
    [AppCode] VARCHAR(255) NOT NULL, 
    [BatchName] VARCHAR(255) NOT NULL, 
    [StartTime] VARCHAR(255) NOT NULL, 
    [EndTime] VARCHAR(255) NOT NULL, 
    [ShippingCarrier] VARCHAR(255) NOT NULL, 
    [ShippingServiceType] VARCHAR(255) NOT NULL, 
    [IsSaturday] BIT NOT NULL, 
    CONSTRAINT [PK_DallasBatchTimings] PRIMARY KEY ([Id])
)

GO

CREATE INDEX [IX_DallasBatchTimings_UniqueBatch] ON [Common].[DallasBatchTimings] ([AppCode], [BatchName], [StartTime], [EndTime], [ShippingCarrier], [ShippingServiceType], [IsSaturday])
