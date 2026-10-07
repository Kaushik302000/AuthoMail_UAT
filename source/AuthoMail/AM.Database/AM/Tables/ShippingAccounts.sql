CREATE TABLE [AM].[ShippingAccounts] (
    [AccountCode]               VARCHAR (128) NOT NULL,
    [WebAuthenticationKey]      VARCHAR (255) NULL,
    [WebAuthenticationPassword] VARCHAR (255) NULL,
    [AccountNumber]             VARCHAR (255) NULL,
    [MeterNumber]               VARCHAR (255) NULL,
    [ShippingCarrier] VARCHAR(255) NULL, 
    CONSTRAINT [PK_ShippingAccounts] PRIMARY KEY CLUSTERED ([AccountCode] ASC)
);

