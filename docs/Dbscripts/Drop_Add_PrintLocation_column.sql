ALTER TABLE [AM].[MailPieces] ADD PrintLocation VARCHAR(255) NULL
GO
 
CREATE NONCLUSTERED INDEX IX_MailPieces_PrintLocation ON [AM].[MailPieces] (PrintLocation)
GO
 
ALTER TABLE [PnS].[BatchSummary] ADD PrintLocation VARCHAR(255) NULL
GO

CREATE NONCLUSTERED INDEX IX_BatchSummary_PrintLocation ON [PnS].[BatchSummary] (PrintLocation)
GO

UPDATE [AM].[MailPieces] SET PrintLocation='Fort Mill, SC' WHERE ShippingStatus IN ('InProgress','Shipped','PulledBack')
GO

UPDATE [PnS].[BatchSummary] SET PrintLocation='Fort Mill, SC' WHERE ValidationStatus IN ('Valid','PartiallyValid')
GO

ALTER TABLE [AM].[MailBatches] DROP Column PrintLocation
GO

CREATE UNIQUE NONCLUSTERED INDEX IX_MailPieces_TrackingNo ON [AM].[MailPieces] (TrackingNo) WHERE TrackingNo IS NOT NULL
GO