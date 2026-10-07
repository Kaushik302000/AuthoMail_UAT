CREATE TABLE [PnS].[PrintTrailBatches] (
    [Id]               INT           IDENTITY (1, 1) NOT NULL,
    [PrintBatchName]   VARCHAR (255) NULL,
    [SubmittedOn]      DATETIME      NULL,
    [MRDFReceivedOn]   DATETIME      NULL,
    [PrintTrailId]     INT           NOT NULL,
    [IsSubmitted]      AS            (case when [SubmittedOn] IS NULL then (0) else (1) end),
    [IsMRDFReceived]   AS            (case when [MRDFReceivedOn] IS NULL then (0) else (1) end),
    [SuspendedOn]      DATETIME      NULL,
    [IsSuspended]      AS            (case when [SuspendedOn] IS NULL then (0) else (1) end),
    [ValidationErrors] VARCHAR (MAX) NULL,
    [ZipFileSize]      BIGINT        NULL,
    [ShipperId] INT NULL, 
    [ServerIP] VARCHAR(50) NULL, 
    CONSTRAINT [PK_PrintTrailBatches_Id] PRIMARY KEY CLUSTERED ([Id] ASC) WITH (FILLFACTOR = 70),
    CONSTRAINT [FK_PrintTrailBatches_PrintTrails] FOREIGN KEY ([PrintTrailId]) REFERENCES [Common].[PrintTrails] ([Id]),
    CONSTRAINT [FK_PrintTrailBatches_ShipperId] FOREIGN KEY ([ShipperId]) REFERENCES [Common].[ShipperInformation] ([Id])
);




GO
CREATE NONCLUSTERED INDEX [Idx_PrintTrailBatches_SubmittedOn]
    ON [PnS].[PrintTrailBatches]([SubmittedOn] ASC);

