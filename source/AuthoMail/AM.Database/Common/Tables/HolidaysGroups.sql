CREATE TABLE [Common].[HolidaysGroups] (
    [Id]        INT           IDENTITY (1, 1) NOT NULL,
    [GroupName] VARCHAR (255) NULL,
    CONSTRAINT [PK_HolidaysGroup] PRIMARY KEY CLUSTERED ([Id] ASC)
);

