CREATE TABLE [PnS].[LogBatchTimeLine] (
    [BatchId]           INT      NOT NULL,
    [DownloadStart]     DATETIME NULL,
    [DownloadEnd]       DATETIME NULL,
    [PCIBatchPrepStart] DATETIME NULL,
    [PCIBatchPrepEnd]   DATETIME NULL,
    [IEReportPrepStart] DATETIME NULL,
    [IEReportPrepEnd]   DATETIME NULL,
    [SubmitToPCIStart]  DATETIME NULL,
    [SubmitToPCIEnd]    DATETIME NULL,
    [MRDFDownloadStart] DATETIME NULL,
    [MRDFDownloadEnd]   DATETIME NULL,
    [ReportPrepStart]   DATETIME NULL,
    [ReportPrepEnd]     DATETIME NULL,
    [ReportBackStart]   DATETIME NULL,
    [ReportBackEnd]     DATETIME NULL
);




GO
CREATE NONCLUSTERED INDEX [IX_LogBatchTimeLine_DownloadEnd]
    ON [PnS].[LogBatchTimeLine]([DownloadEnd] ASC) WITH (FILLFACTOR = 70);


GO
CREATE NONCLUSTERED INDEX [IX_LogBatchTimeLine_DownloadStart]
    ON [PnS].[LogBatchTimeLine]([DownloadStart] ASC) WITH (FILLFACTOR = 70);


GO
CREATE NONCLUSTERED INDEX [IX_LogBatchTimeLine_IEReportPrepEnd]
    ON [PnS].[LogBatchTimeLine]([IEReportPrepEnd] ASC) WITH (FILLFACTOR = 70);


GO
CREATE NONCLUSTERED INDEX [IX_LogBatchTimeLine_IEReportPrepStart]
    ON [PnS].[LogBatchTimeLine]([IEReportPrepStart] ASC) WITH (FILLFACTOR = 70);


GO
CREATE NONCLUSTERED INDEX [IX_LogBatchTimeLine_MRDFDownloadEnd]
    ON [PnS].[LogBatchTimeLine]([MRDFDownloadEnd] ASC) WITH (FILLFACTOR = 70);


GO
CREATE NONCLUSTERED INDEX [IX_LogBatchTimeLine_MRDFDownloadStart]
    ON [PnS].[LogBatchTimeLine]([MRDFDownloadStart] ASC) WITH (FILLFACTOR = 70);


GO
CREATE NONCLUSTERED INDEX [IX_LogBatchTimeLine_PCIBatchPrepEnd]
    ON [PnS].[LogBatchTimeLine]([PCIBatchPrepEnd] ASC) WITH (FILLFACTOR = 70);


GO
CREATE NONCLUSTERED INDEX [IX_LogBatchTimeLine_PCIBatchPrepStart]
    ON [PnS].[LogBatchTimeLine]([PCIBatchPrepStart] ASC) WITH (FILLFACTOR = 70);


GO
CREATE NONCLUSTERED INDEX [IX_LogBatchTimeLine_ReportBackEnd]
    ON [PnS].[LogBatchTimeLine]([ReportBackEnd] ASC) WITH (FILLFACTOR = 70);


GO
CREATE NONCLUSTERED INDEX [IX_LogBatchTimeLine_ReportBackStart]
    ON [PnS].[LogBatchTimeLine]([ReportBackStart] ASC) WITH (FILLFACTOR = 70);


GO
CREATE NONCLUSTERED INDEX [IX_LogBatchTimeLine_ReportPrepEnd]
    ON [PnS].[LogBatchTimeLine]([ReportPrepEnd] ASC) WITH (FILLFACTOR = 70);


GO
CREATE NONCLUSTERED INDEX [IX_LogBatchTimeLine_ReportPrepStart]
    ON [PnS].[LogBatchTimeLine]([ReportPrepStart] ASC) WITH (FILLFACTOR = 70);


GO
CREATE NONCLUSTERED INDEX [IX_LogBatchTimeLine_SubmitToPCIEnd]
    ON [PnS].[LogBatchTimeLine]([SubmitToPCIEnd] ASC) WITH (FILLFACTOR = 70);


GO
CREATE NONCLUSTERED INDEX [IX_LogBatchTimeLine_SubmitToPCIStart]
    ON [PnS].[LogBatchTimeLine]([SubmitToPCIStart] ASC) WITH (FILLFACTOR = 70);

GO
CREATE CLUSTERED INDEX [LogBatchTimeLine_BatchId] ON [PnS].[LogBatchTimeLine]
(
	[BatchId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO