CREATE TABLE [Common].[SystemSettings] (
    [Id]           VARCHAR (128)  NOT NULL,
    [DataType]     VARCHAR (128)  NULL,
    [Value]        VARCHAR (1024) NULL,
    [Description]  VARCHAR (2028) NULL,
    [IsSystem]     BIT            NULL,
    [DefaultValue] VARCHAR (1024) NULL,
    [ModifiedBy]   NVARCHAR (128) NULL,
    [ModifiedOn]   DATETIME       NULL,
    CONSTRAINT [PK_SystemSettings] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_SystemSettings_Users] FOREIGN KEY ([ModifiedBy]) REFERENCES [Profile].[Users] ([Id])
);

