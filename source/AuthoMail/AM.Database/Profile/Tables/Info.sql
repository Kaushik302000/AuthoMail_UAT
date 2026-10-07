CREATE TABLE [Profile].[Info] (
    [Id]       INT           IDENTITY (1, 1) NOT NULL,
    [Name]     NVARCHAR (50) NULL,
    [UserType] NVARCHAR (20) NULL,
    CONSTRAINT [PK_Info] PRIMARY KEY CLUSTERED ([Id] ASC)
);

