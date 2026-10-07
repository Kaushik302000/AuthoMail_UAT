CREATE TABLE [Common].[PrintVendors] (
    [Id]           INT           IDENTITY (1, 1) NOT NULL,
    [Name]         VARCHAR (255) NULL,
    [SFTChannelId] INT           NULL,
    [BusinessCode] VARCHAR (255) NULL,
    [VendorCode]   VARCHAR (255) NULL,
    [IsActive] BIT NULL, 
    CONSTRAINT [PK_PrintVendors_Id] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_PrintVendors_SFTChannels] FOREIGN KEY ([SFTChannelId]) REFERENCES [Common].[SFTChannels] ([Id])
);



