CREATE TABLE [AM].[PackagesHoldInfo] (
    [Id]                                  INT              IDENTITY (1, 1) NOT NULL,
    [ClientId]                            INT              NOT NULL,
    [DocumentOrPackageType]               VARCHAR (255)    NULL,
    [HoldOn]                          DATETIME         NOT NULL,
    [ProcessOn]                        DATETIME         NOT NULL, 
    CONSTRAINT [PK_PackagesHoldInfo] PRIMARY KEY ([Id])
);

GO


CREATE INDEX [IX_PackagesHoldInfo_HoldOn] ON [AM].[PackagesHoldInfo] ([HoldOn])

GO

CREATE INDEX [IX_PackagesHoldInfo_ProcessOn] ON [AM].[PackagesHoldInfo] ([ProcessOn])
