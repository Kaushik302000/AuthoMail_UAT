CREATE TABLE [AM].[ErrorNotifications] (
    [Id]            INT      IDENTITY (1, 1) NOT NULL,
    [ClientId]      INT      NOT NULL,
    [MailBatchId]   INT      NOT NULL,
    [TotalMPsCount] INT      NOT NULL,
    [ErrorMPsCount] INT      NOT NULL,
    [ReceivedOn]    DATETIME NOT NULL,
    [CreatedOn]     DATETIME NOT NULL,
    CONSTRAINT [PK_ErrorNotifications] PRIMARY KEY CLUSTERED ([Id] ASC)
);




GO
CREATE NONCLUSTERED INDEX [Idx_ErrorNotifications_ClientId]
    ON [AM].[ErrorNotifications]([ClientId] ASC);


GO
CREATE UNIQUE NONCLUSTERED INDEX [Idx_ErrorNotifications_MailBatchId]
    ON [AM].[ErrorNotifications]([MailBatchId] ASC);


GO
CREATE NONCLUSTERED INDEX [Idx_ErrorNotifications_ReceivedOn]
    ON [AM].[ErrorNotifications]([ReceivedOn] ASC) WITH (FILLFACTOR = 90);


GO
CREATE NONCLUSTERED INDEX [Idx_ErrorNotifications_CreatedOn]
    ON [AM].[ErrorNotifications]([CreatedOn] ASC) WITH (FILLFACTOR = 90);

