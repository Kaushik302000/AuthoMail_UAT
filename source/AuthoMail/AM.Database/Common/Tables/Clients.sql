CREATE TABLE [Common].[Clients] (
    [Id]                    INT            IDENTITY (100, 1) NOT NULL,
    [Name]                  VARCHAR (255)  NULL,
    [AppCode]               VARCHAR (128)  NULL,
    [BusinessUnit]          INT            NULL,
    [IsAMActive]            BIT            NULL,
    [IsPnSActive]           BIT            NULL,
    [TimeZone]              VARCHAR (255)  NULL,
    [ModifiedOn]            DATETIME       NULL,
    [CreatedOn]             DATETIME       NULL,
    [CreatedBy]             NVARCHAR (128) NULL,
    [ModifiedBy]            NVARCHAR (128) NULL,
    [Address]               NVARCHAR (255) NULL,
    [Contact1Name]          VARCHAR (255)  NULL,
    [Contact1Phone]         VARCHAR (255)  NULL,
    [Contact1Email]         VARCHAR (1024)  NULL,
    [Contact2Name]          VARCHAR (255)  NULL,
    [Contact2Phone]         VARCHAR (255)  NULL,
    [Contact2Email]         VARCHAR (255)  NULL,
    [Contact3Name]          VARCHAR (255)  NULL,
    [Contact3Phone]         VARCHAR (255)  NULL,
    [Contact3Email]         VARCHAR (255)  NULL,
    [ITContactName]         VARCHAR (255)  NULL,
    [ITContactPhone]        VARCHAR (255)  NULL,
    [ITContactEmail]        VARCHAR (255)  NULL,
    [BillingContactName]    VARCHAR (255)  NULL,
    [BillingContactPhone]   VARCHAR (255)  NULL,
    [BillingContactEmail]   VARCHAR (255)  NULL,
    [SigningRoomExpiryDays] INT            NULL,
    [Alias]                 VARCHAR (255)  NULL,
    [IsReqBlankPageValidation] BIT NULL, 
    [IsEncrypted] BIT NULL, 
    [RetentionPeriod] INT NULL, 
    [GroupAlias] VARCHAR(255) NULL, 
    [IsResponseRequired] BIT NULL, 
    [IsHardAddrValidationRequired] BIT NULL, 
    [IsRejectDupRecord] BIT NULL DEFAULT 0, 
    [IsEDeliveryEnabled] BIT NULL DEFAULT 0, 
    CONSTRAINT [PK_Clients] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Clients_BusinessUnit] FOREIGN KEY ([BusinessUnit]) REFERENCES [Common].[BusinessUnit] ([Id]),
    CONSTRAINT [FK_CreatedBy_Clients_User] FOREIGN KEY ([CreatedBy]) REFERENCES [Profile].[Users] ([Id]),
    CONSTRAINT [FK_ModifiedBy_Clients_User] FOREIGN KEY ([ModifiedBy]) REFERENCES [Profile].[Users] ([Id]),
    CONSTRAINT [UK_AppCode] UNIQUE NONCLUSTERED ([AppCode] ASC)
);




GO
CREATE NONCLUSTERED INDEX [IX_Clients_AppCode]
    ON [Common].[Clients]([AppCode] ASC);

