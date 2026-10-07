CREATE TABLE [Common].[UserShipAccMapping]
(
	[UserId] NVARCHAR(128) NOT NULL, 
    [AccountCode] NVARCHAR(128) NOT NULL, 
    CONSTRAINT [CPK_UserShipAccMapping] PRIMARY KEY ([UserId], [AccountCode]), 
    CONSTRAINT [FK_UserShipAccMapping_Users] FOREIGN KEY ([UserId]) REFERENCES [Profile].[Users]([Id]), 
)
