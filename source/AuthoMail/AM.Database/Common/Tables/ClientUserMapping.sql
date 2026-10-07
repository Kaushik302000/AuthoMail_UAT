CREATE TABLE [Common].[ClientUserMapping]
(
	[ClientId] INT NOT NULL, 
    [UserId] NVARCHAR(128) NOT NULL, 
    [Status] VARCHAR(50) NOT NULL, 
    [CreatedOn] DATETIME NOT NULL, 
    [ModifiedOn] DATETIME NOT NULL, 
    [ModifiedById] NVARCHAR(128) NOT NULL

    Constraint PK_Composit_client_user primary key clustered ([ClientId],[UserId]),
    CONSTRAINT [FK_ClientUserMapping_Users] FOREIGN KEY ([ModifiedById]) REFERENCES [Profile].[Users] ([Id])
)
