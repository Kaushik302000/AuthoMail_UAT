CREATE TABLE [Profile].[UserRoles] (
    [UserId] NVARCHAR (128) NOT NULL,
    [RoleId] NVARCHAR (128) NOT NULL,
    CONSTRAINT [PK__UserRole] PRIMARY KEY CLUSTERED ([UserId] ASC, [RoleId] ASC) WITH (FILLFACTOR = 70),
    CONSTRAINT [FK_UserRoles_Roles] FOREIGN KEY ([RoleId]) REFERENCES [Profile].[Roles] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_UsersRoles_Users] FOREIGN KEY ([UserId]) REFERENCES [Profile].[Users] ([Id]) ON DELETE CASCADE
);

