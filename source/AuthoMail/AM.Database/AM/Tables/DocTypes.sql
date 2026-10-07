CREATE TABLE [AM].[DocTypes] (
    [Id]                                  INT            IDENTITY (1, 1) NOT NULL,
    [ClientId]                            INT            NOT NULL,
    [QCPercent]                           FLOAT (53)     NULL,
    [IsReturnInfoSpecified]               BIT            NULL,
    [RetRcptPrefix]                       VARCHAR (255)  NULL,
    [RetRcptFirstName]                    VARCHAR (255)  NULL,
    [RetRcptMiddleName]                   VARCHAR (255)  NULL,
    [RetRcptLastName]                     VARCHAR (255)  NULL,
    [RetRcptSuffix]                       VARCHAR (255)  NULL,
    [RetRcptPhone]                        VARCHAR (255)  NULL,
    [RetRcptOrganization]                 VARCHAR (255)  NULL,
    [RetRcptAddrOptionalLine1AboveStreet] VARCHAR (255)  NULL,
    [RetRcptAddrOptionalLine2AboveStreet] VARCHAR (255)  NULL,
    [RetRcptAddressStreet]                VARCHAR (255)  NULL,
    [RetRcptAddressAptOrSuiteOrOther]     VARCHAR (255)  NULL,
    [RetRcptAddrOptionalLine1BelowStreet] VARCHAR (255)  NULL,
    [RetRcptAddrOptionalLine2BelowStreet] VARCHAR (255)  NULL,
    [RetRcptAddressCity]                  VARCHAR (255)  NULL,
    [RetRcptAddressState]                 VARCHAR (255)  NULL,
    [RetRcptAddressZIP]                   VARCHAR (255)  NULL,
    [ClientEmails]                        VARCHAR (1024) NULL,
    [ClientContactNo]                     VARCHAR (255)  NULL,
    [SupportEmails]                       VARCHAR (1024) NULL,
    [SupportContactNo]                    VARCHAR (255)  NULL,
    [IsActive]                            BIT            NOT NULL,
    [ModifiedOn]                          DATETIME       NULL,
    [CreatedOn]                           DATETIME       NULL,
    [CreatedBy]                           NVARCHAR (128) NULL,
    [ModifiedBy]                          NVARCHAR (128) NULL,
    [DocTypeName]                         VARCHAR (512)  DEFAULT ('desc') NOT NULL,
    [AllowInsertsOnly]                       BIT            CONSTRAINT [DF_DocTypes_AllowInsertsOnly] DEFAULT ((0)) NOT NULL,
    [IsBRE]                               BIT            CONSTRAINT [DF_DocTypes_IsBRE] DEFAULT ((0)) NOT NULL,
    [NoOfCopies]                          INT            NOT NULL,
    [RetRcptAddressCountry]               VARCHAR (255)  NULL,
    [PnSConfigId]                         INT            NULL,
    [PrintFormat] VARCHAR(255) NULL, 
    [IsChecks] BIT NULL, 
    [HasReturnLabel] BIT NULL, 
    [ReturnShippingCarrier] VARCHAR(255) NULL, 
    [ReturnShippingServiceType] VARCHAR(255) NULL, 
    [ReturnShippingAccountCode] VARCHAR(255) NULL, 
    [IntlPnSConfigId] INT NULL, 
    [PaperType] VARCHAR(255) NULL, 
    CONSTRAINT [PK_DocTypes] PRIMARY KEY CLUSTERED ([Id] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_CreatedBy_DocTypes_User] FOREIGN KEY ([CreatedBy]) REFERENCES [Profile].[Users] ([Id]),
    CONSTRAINT [FK_DocTypes_Clients] FOREIGN KEY ([ClientId]) REFERENCES [Common].[Clients] ([Id]),
    CONSTRAINT [FK_DocTypes_PnSConfig] FOREIGN KEY ([PnSConfigId]) REFERENCES [Common].[PnSConfig] ([Id]),
    CONSTRAINT [FK_ModifiedBy_DocTypes_User] FOREIGN KEY ([ModifiedBy]) REFERENCES [Profile].[Users] ([Id])
);




GO
CREATE NONCLUSTERED INDEX [IX_DocTypes_ClientId]
    ON [AM].[DocTypes]([ClientId] ASC) WITH (FILLFACTOR = 70);


GO
CREATE NONCLUSTERED INDEX [IX_DocTypes_PnSConfigId]
    ON [AM].[DocTypes]([PnSConfigId] ASC) WITH (FILLFACTOR = 90);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_DocTypes_ClientId_DocTypeName]
    ON [AM].[DocTypes]([ClientId] ASC, [DocTypeName] ASC) WITH (FILLFACTOR = 90);

