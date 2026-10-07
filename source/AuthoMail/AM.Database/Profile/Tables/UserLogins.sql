CREATE TABLE [Profile].[UserLogins] (
    [LoginProvider] NVARCHAR (128) NOT NULL,
    [ProviderKey]   NVARCHAR (128) NOT NULL,
    [UserId]        NVARCHAR (128) NOT NULL,
    CONSTRAINT [PK__UserLogin] PRIMARY KEY CLUSTERED ([LoginProvider] ASC, [ProviderKey] ASC, [UserId] ASC),
    CONSTRAINT [FK_UserLogins_Users] FOREIGN KEY ([UserId]) REFERENCES [Profile].[Users] ([Id]) ON DELETE CASCADE
);

