CREATE TABLE [API].[AMRequestLogs]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY, 
    [ClientId] INT NOT NULL,
    [MailPieceId] UNIQUEIDENTIFIER NULL, 
    [IsPrimary] BIT NOT NULL, 
    [Status] VARCHAR(255) NOT NULL, 
    [HTTPStatusCode] INT NOT NULL, 
    [APIResponse] NVARCHAR(2048) NULL,
    [TotalPages] INT NULL,
    [ReceivedOn] DATETIME NOT NULL, 
    [ProcessedOn] DATETIME NULL, 
    [ErrorLog] NVARCHAR(MAX) NULL,
    [RequestSize] DECIMAL(9, 2) NULL,
    [InitialChecks] DECIMAL(7, 2) NULL, 
    [Download] DECIMAL(7, 2) NULL, 
    [Validate] DECIMAL(7, 2) NULL, 
    [Merge] DECIMAL(7, 2) NULL, 
    [PostBatch] DECIMAL(7, 2) NULL, 
    [Cleanup] DECIMAL(7, 2) NULL, 
    [TotalTime] DECIMAL(7, 2) NULL,     
)

GO

CREATE INDEX [IX_AMRequestLogs_MailPieceId] ON [API].[AMRequestLogs] ([MailPieceId])
