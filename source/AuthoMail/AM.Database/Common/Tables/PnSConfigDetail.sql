CREATE TABLE [Common].[PnSConfigDetail] (
    [Id]                     INT           IDENTITY (1, 1) NOT NULL,
    [PnSConfigId]            INT           NOT NULL,
    [PrintSize]              VARCHAR (255) NULL,
    [ContentScalePercent]    FLOAT (53)    CONSTRAINT [DF_PnSConfigDetail_ContentScalePercent] DEFAULT ((100)) NULL,
    [ShippingCarrier]        VARCHAR (255) NULL,
    [ShippingServiceType]    VARCHAR (255) NULL,
    [ShippingAccountCode]    VARCHAR (255) NULL,
    [ShippingSpecialService] VARCHAR (255) NULL,
    [MinPage]                INT           NULL,
    [MaxPage]                INT           NULL,
    [EnvelopeId]             INT           NULL,
    [IsFallBack]             BIT           CONSTRAINT [DF_PnSConfigDetail_IsFallBack] DEFAULT ((0)) NULL,
    [BizdayCutOff]           VARCHAR (255) NULL,
    [BizdaySLAType]          VARCHAR (255) NULL,
    [BizdayTimeSpan]         VARCHAR (255) NULL,
    [SatCutOff]              VARCHAR (255) NULL,
    [SatSLAType]             VARCHAR (255) NULL,
    [SatTimeSpan]            VARCHAR (255) NULL,
    [SunCutOff]              VARCHAR (255) NULL,
    [SunSLAType]             VARCHAR (255) CONSTRAINT [DF_PnSConfigDetail_SunSLAType] DEFAULT ('NA') NULL,
    [SunTimeSpan]            VARCHAR (255) NULL,
    [PrintTrailId]           INT           NULL,
    [BizDayStartTime]        TIME (7)      NULL,
    [BizDayEndTime]          TIME (7)      NULL,
    [SatStartTime]           TIME (7)      NULL,
    [SatEndTime]             TIME (7)      NULL,
    [SunStartTime]           TIME (7)      NULL,
    [SunEndTime]             TIME (7)      NULL,
    [IsPrintReady] BIT NULL, 
    CONSTRAINT [PK_PnSConfigDetail] PRIMARY KEY CLUSTERED ([Id] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_PnSConfigDetail_Envelopes] FOREIGN KEY ([EnvelopeId]) REFERENCES [Common].[Envelopes] ([Id]),
    CONSTRAINT [FK_PnSConfigDetail_PnSConfig] FOREIGN KEY ([PnSConfigId]) REFERENCES [Common].[PnSConfig] ([Id])
);








GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_PnSConfigDetail_UniqueInfo]
    ON [Common].[PnSConfigDetail]([PnSConfigId] ASC, [ShippingCarrier] ASC, [ShippingServiceType] ASC, [ShippingAccountCode] ASC, [ShippingSpecialService] ASC, [MinPage] ASC, [MaxPage] ASC, [PrintTrailId] ASC, [BizDayStartTime] ASC, [BizDayEndTime] ASC, [SatStartTime] ASC, [SatEndTime] ASC, [SunStartTime] ASC, [SunEndTime] ASC) WITH (FILLFACTOR = 90);


GO
CREATE NONCLUSTERED INDEX [IX_PnSConfigDetail_SunStartTime]
    ON [Common].[PnSConfigDetail]([SunStartTime] ASC) WITH (FILLFACTOR = 90);


GO
CREATE NONCLUSTERED INDEX [IX_PnSConfigDetail_SunEndTime]
    ON [Common].[PnSConfigDetail]([SunEndTime] ASC) WITH (FILLFACTOR = 90);


GO
CREATE NONCLUSTERED INDEX [IX_PnSConfigDetail_SatStartTime]
    ON [Common].[PnSConfigDetail]([SatStartTime] ASC) WITH (FILLFACTOR = 90);


GO
CREATE NONCLUSTERED INDEX [IX_PnSConfigDetail_SatEndTime]
    ON [Common].[PnSConfigDetail]([SatEndTime] ASC) WITH (FILLFACTOR = 90);


GO
CREATE NONCLUSTERED INDEX [IX_PnSConfigDetail_PrintTrailId]
    ON [Common].[PnSConfigDetail]([PrintTrailId] ASC) WITH (FILLFACTOR = 90);


GO
CREATE NONCLUSTERED INDEX [IX_PnSConfigDetail_PnSConfigId]
    ON [Common].[PnSConfigDetail]([PnSConfigId] ASC) WITH (FILLFACTOR = 90);


GO
CREATE NONCLUSTERED INDEX [IX_PnSConfigDetail_MinPage]
    ON [Common].[PnSConfigDetail]([MinPage] ASC) WITH (FILLFACTOR = 90);


GO
CREATE NONCLUSTERED INDEX [IX_PnSConfigDetail_MaxPage]
    ON [Common].[PnSConfigDetail]([MaxPage] ASC) WITH (FILLFACTOR = 90);


GO
CREATE NONCLUSTERED INDEX [IX_PnSConfigDetail_EnvelopeId]
    ON [Common].[PnSConfigDetail]([EnvelopeId] ASC) WITH (FILLFACTOR = 90);


GO
CREATE NONCLUSTERED INDEX [IX_PnSConfigDetail_BizDayStartTime]
    ON [Common].[PnSConfigDetail]([BizDayStartTime] ASC) WITH (FILLFACTOR = 90);


GO
CREATE NONCLUSTERED INDEX [IX_PnSConfigDetail_BizDayEndTime]
    ON [Common].[PnSConfigDetail]([BizDayEndTime] ASC) WITH (FILLFACTOR = 90);

