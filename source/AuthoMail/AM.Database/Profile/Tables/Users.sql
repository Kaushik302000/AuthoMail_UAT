CREATE TABLE [Profile].[Users] (
    [Id]                    NVARCHAR (128) NOT NULL,
    [Email]                 NVARCHAR (256) NULL,
    [EmailConfirmed]        BIT            NOT NULL,
    [PasswordHash]          NVARCHAR (MAX) NULL,
    [SecurityStamp]         NVARCHAR (MAX) NULL,
    [PhoneNumber]           NVARCHAR (MAX) NULL,
    [PhoneNumberConfirmed]  BIT            NOT NULL,
    [TwoFactorEnabled]      BIT            NOT NULL,
    [LockoutEndDateUtc]     DATETIME       NULL,
    [LockoutEnabled]        BIT            NOT NULL,
    [AccessFailedCount]     INT            NOT NULL,
    [UserName]              NVARCHAR (256) NOT NULL,
    [ClientId]              INT            NULL,
    [FirstName]             VARCHAR (255)  NULL,
    [LastName]              VARCHAR (255)  NULL,
    [DisplayName]           VARCHAR (255)  NULL,
    [RequestedOn]           DATETIME       NOT NULL,
    [Status]                VARCHAR (35)   NULL,
    [ClosedOn]              DATETIME       NULL,
    [ClosedBy]              NVARCHAR (128) NULL,
    [LastLoginTimeUTC]      DATETIME       NULL,
    [ChangePasswordDateUTC] DATETIME       DEFAULT (getutcdate()) NOT NULL,
    [StatusModifiedBy]      NVARCHAR (128) NULL,
    [StatusModifiedOn]      DATETIME       NULL,
    [SFTChannelId]          INT            NULL,
    CONSTRAINT [PK_Users] PRIMARY KEY CLUSTERED ([Id] ASC) WITH (FILLFACTOR = 70),
    CONSTRAINT [FK_Users_Clients] FOREIGN KEY ([ClientId]) REFERENCES [Common].[Clients] ([Id]),
    CONSTRAINT [FK_Users_StatusModifiedBy] FOREIGN KEY ([StatusModifiedBy]) REFERENCES [Profile].[Users] ([Id]),
    CONSTRAINT [FK_Users_Users] FOREIGN KEY ([ClosedBy]) REFERENCES [Profile].[Users] ([Id])
);


GO
CREATE NONCLUSTERED INDEX [IX_Users_Email]
    ON [Profile].[Users]([Email] ASC) WITH (FILLFACTOR = 70);


GO
CREATE NONCLUSTERED INDEX [IX_Users_Status]
    ON [Profile].[Users]([Status] ASC) WITH (FILLFACTOR = 70);


GO
CREATE NONCLUSTERED INDEX [IX_Users_UserName]
    ON [Profile].[Users]([UserName] ASC) WITH (FILLFACTOR = 70);

