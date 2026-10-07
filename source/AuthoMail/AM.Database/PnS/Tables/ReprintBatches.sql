CREATE TABLE [PnS].[ReprintBatches] (
    [PrintBatch]       VARCHAR (10)  NOT NULL,
    [ParentPrintBatch] INT           NULL,
    [AppId]            INT           NULL,
    [DocTypeCode]      VARCHAR (29)  NULL,
    [PipelineStatus]   VARCHAR (512) NULL,
    [PrintFileName]    VARCHAR (512) NULL,
    [IsSubmitted]      AS            (case when [SubmittedOn] IS NULL then (0) else (1) end),
    [SubmittedOn]      DATETIME      NULL,
    [IsMRDFReceived]   AS            (case when [MRDFReceivedOn] IS NULL then (0) else (1) end),
    [MRDFReceivedOn]   DATETIME      NULL,
    CONSTRAINT [PK_ReprintBatches] PRIMARY KEY CLUSTERED ([PrintBatch] ASC)
);

