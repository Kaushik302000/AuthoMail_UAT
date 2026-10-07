CREATE TABLE [Common].[SFTChannels] (
    [Id]                  INT            IDENTITY (1, 1) NOT NULL,
    [Code]                VARCHAR (128)  NULL,
    [Description]         VARCHAR (512)  NULL,
    [Title]               VARCHAR (255)  NULL,
    [Protocol]            VARCHAR (255)  NULL,
    [Host]                VARCHAR (255)  NULL,
    [UserName]            VARCHAR (255)  NULL,
    [Password]            VARCHAR (255)  NULL,
    [ToADEDir]            VARCHAR (1024) NULL,
    [FromADEDir]          VARCHAR (1024) NULL,
    [ArchiveDir]          VARCHAR (1024) NULL,
    [PostDownloadAction]  VARCHAR (255)  NULL,
    [IsActive]            BIT            CONSTRAINT [DF_SFTChannels_IsActive] DEFAULT ((0)) NULL,
    [APITokenURL]         VARCHAR (255)  NULL,
    [APIResponseURL]      VARCHAR (255)  NULL,
    [APIMaxResponseCount] INT            NULL,
    [APITokenHeaders]     VARCHAR (2048) NULL,
    [ServerIP] VARCHAR(50) NULL, 
    [Port] INT NULL, 
    CONSTRAINT [PK_SFTChannel] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [UQ_SFTChannel] UNIQUE NONCLUSTERED ([Code] ASC)
);




GO
CREATE NONCLUSTERED INDEX [IX_SFTChannels_Code]
    ON [Common].[SFTChannels]([Code] ASC);

