CREATE TABLE [Common].[PnSSweepTimings] (
    [Id]          INT           IDENTITY (1, 1) NOT NULL,
    [BundleSeqNo] VARCHAR (255) NULL,
    [AppCode]     VARCHAR (20)  NULL,
    [VendorCode]  VARCHAR (20)  NULL,
    [WeekDayType] VARCHAR (20)  NULL,
    [SweepType]   VARCHAR (20)  NULL,
    [SweepTime]   TIME (7)      NULL,
    CONSTRAINT [PK_PnSSweepTimings] PRIMARY KEY CLUSTERED ([Id] ASC)
);


GO
CREATE UNIQUE NONCLUSTERED INDEX [UniqueIndex_BundleSeqNo_AppCode_VendorCode_WeekDayType_SweepTime]
    ON [Common].[PnSSweepTimings]([BundleSeqNo] ASC, [AppCode] ASC, [VendorCode] ASC, [WeekDayType] ASC, [SweepTime] ASC);

