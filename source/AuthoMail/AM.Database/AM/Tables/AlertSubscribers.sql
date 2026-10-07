CREATE TABLE [AM].[AlertSubscribers] (
    [Id]        INT            IDENTITY (1, 1) NOT NULL,
    [UserId]    NVARCHAR (128) NOT NULL,
    [ClientId]  INT            NOT NULL,
    [AlertType] VARCHAR (255)  NOT NULL,
    CONSTRAINT [PK_AlertSubscribers] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_AlertSubscribers_Clients] FOREIGN KEY ([ClientId]) REFERENCES [Common].[Clients] ([Id]),
    CONSTRAINT [FK_AlertSubscribers_Users] FOREIGN KEY ([UserId]) REFERENCES [Profile].[Users] ([Id])
);

