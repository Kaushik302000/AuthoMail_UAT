CREATE TABLE [API].[Credentials]
(
	[Id]                 INT              IDENTITY (1000, 1) NOT NULL,
    [ClientId]           INT              NOT NULL,
    [SecretKey]          VARCHAR (MAX)    NOT NULL,
    [UserName]           VARCHAR (255)    NOT NULL,
    [Password]           VARCHAR (MAX)    NOT NULL

    CONSTRAINT [PK_API.Credentials] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Credentials_Clients] FOREIGN KEY ([ClientId]) REFERENCES [Common].[Clients] ([Id])
)
