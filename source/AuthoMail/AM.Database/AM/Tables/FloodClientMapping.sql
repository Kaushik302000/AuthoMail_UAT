CREATE TABLE [AM].[FloodClientMapping]
(
	[ClientId] INT NOT NULL , 
    [Name] VARCHAR(255) NOT NULL
)

GO

CREATE UNIQUE INDEX [IX_Unique_ClientId_ClientName] ON [AM].[FloodClientMapping] ([ClientId], [Name])
