CREATE TABLE [Common].[ClientSFTChannels] (
    [ClientId]     INT NOT NULL,
    [SFTChannelId] INT NOT NULL,
    CONSTRAINT [PK_ClientSFTChannels] PRIMARY KEY CLUSTERED ([ClientId] ASC, [SFTChannelId] ASC),
    CONSTRAINT [FK_ClientSFTChannels_Clients] FOREIGN KEY ([ClientId]) REFERENCES [Common].[Clients] ([Id]),
    CONSTRAINT [FK_ClientSFTChannels_SFTChannels] FOREIGN KEY ([SFTChannelId]) REFERENCES [Common].[SFTChannels] ([Id])
);

