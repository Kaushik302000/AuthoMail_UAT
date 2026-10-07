CREATE TABLE [API].[USPSeVSCancel] (
    [Id]              INT            IDENTITY (1, 1) NOT NULL,
    [APIName]         NVARCHAR (255) NOT NULL,
    [UserId]          NVARCHAR (255) NULL,
    [IsAPIError]      BIT            NOT NULL,
    [IsUSPSError]     BIT            NOT NULL,
    [APIRequest]      NVARCHAR (MAX) NOT NULL,
    [APIRequestedOn]  DATETIME       NOT NULL,
    [USPSRequest]     NVARCHAR (MAX) NULL,
    [USPSRequestedOn] DATETIME       NULL,
    [USPSResponse]    NVARCHAR (MAX) NULL,
    [USPSResponseOn]  DATETIME       NULL,
    [APIResponse]     NVARCHAR (MAX) NULL,
    [APIResponseOn]   DATETIME       NULL,
    [BarcodeNumber]   NVARCHAR (255) NULL,
    [Status]          NVARCHAR (255) NULL,
    [Reason]          NVARCHAR (MAX) NULL,
    CONSTRAINT [PK_USPSeVSCancel.Id] PRIMARY KEY CLUSTERED ([Id] ASC)
);

