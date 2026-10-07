CREATE TABLE [Common].[Deliverables] (
    [Id]                  INT            IDENTITY (1, 1) NOT NULL,
    [ClientId]            INT            NULL,
    [ReportName]          VARCHAR (255)  NULL,
    [ReportType]          VARCHAR (255)  NULL,
    [DeliveryType]        VARCHAR (255)  NULL,
    [ReportQuery]         VARCHAR (MAX)  NULL,
    [Delimeter]           VARCHAR (255)  NULL,
    [FileNameExpression]  VARCHAR (255)  NULL,
    [IsSFTUploadRequired] BIT            NULL,
    [UploadPath]          VARCHAR (1024) NULL,
    [EmailChannelId]      INT            NULL,
    [ScheduleId]          INT            NULL,
    CONSTRAINT [PK_Deliverables] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Deliverables_Clients] FOREIGN KEY ([ClientId]) REFERENCES [Common].[Clients] ([Id]),
    CONSTRAINT [FK_Deliverables_EmailChannel] FOREIGN KEY ([EmailChannelId]) REFERENCES [Common].[EmailChannels] ([Id])
);


GO
CREATE NONCLUSTERED INDEX [IX_Deliverables_ClientId]
    ON [Common].[Deliverables]([ClientId] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_Deliverables_ReportType]
    ON [Common].[Deliverables]([ReportType] ASC);

