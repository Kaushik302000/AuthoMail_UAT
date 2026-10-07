CREATE TABLE [Common].[EmailChannels] (
    [Id]           INT            IDENTITY (1, 1) NOT NULL,
    [Title]        VARCHAR (255)  NULL,
    [To]           VARCHAR (1024) NULL,
    [Cc]           VARCHAR (1024) NULL,
    [Bcc]          VARCHAR (1024) NULL,
    [Subject]      VARCHAR (1024) NULL,
    [BodyTemplate] VARCHAR (MAX)  NULL,
    [IsBodyHTML]   BIT            NULL,
    CONSTRAINT [PK_EmailChannel] PRIMARY KEY CLUSTERED ([Id] ASC)
);

