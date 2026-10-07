CREATE TABLE [PnS].[MRDFResponse] (
    [JobId]                 INT            NOT NULL,
    [PieceId]               INT            NOT NULL,
    [Date]                  DATE           NULL,
    [Feeder2_Fed]           INT            NULL,
    [OperatorId]            VARCHAR (50)   NULL,
    [MachineId]             VARCHAR (50)   NULL,
    [ProcessingTimeStamp]   DATETIME       NULL,
    [DetailedErrorId]       VARCHAR (50)   NULL,
    [Disposition]           VARCHAR (50)   NULL,
    [Input_Fed]             VARCHAR (50)   NULL,
    [Feeder3_Fed]           VARCHAR (50)   NULL,
    [Feeder4_Fed]           VARCHAR (50)   NULL,
    [Feeder5_Fed]           VARCHAR (50)   NULL,
    [ComputedWeight]        DECIMAL (4, 3) NULL,
    [MeterDate]             VARCHAR (50)   NULL,
    [StatusSource]          VARCHAR (50)   NULL,
    [PostageMetered]        DECIMAL (7, 4) NULL,
    [WeightMeasured]        DECIMAL (7, 4) NULL,
    [ExitLocation]          VARCHAR (50)   NULL,
    [TotalSheets]           INT            NULL,
    [PDFFileName]           VARCHAR (255)  NULL,
    [PackageTrackingNumber] VARCHAR (50)   NULL,
    CONSTRAINT [PK_MRDFResponse] PRIMARY KEY CLUSTERED ([JobId] ASC, [PieceId] ASC) WITH (FILLFACTOR = 70),
    CONSTRAINT [FK_MRDFResponse_PrintTrailBatches] FOREIGN KEY ([JobId]) REFERENCES [PnS].[PrintTrailBatches] ([Id])
);




GO
CREATE NONCLUSTERED INDEX [IX_MRDFResponse_Date]
    ON [PnS].[MRDFResponse]([Date] ASC) WITH (FILLFACTOR = 70);


GO
CREATE NONCLUSTERED INDEX [IX_MRDFResponse_Disposition]
    ON [PnS].[MRDFResponse]([Disposition] ASC) WITH (FILLFACTOR = 70);


GO
CREATE NONCLUSTERED INDEX [IX_MRDFResponse_JobId]
    ON [PnS].[MRDFResponse]([JobId] ASC) WITH (FILLFACTOR = 70);


GO
CREATE NONCLUSTERED INDEX [IX_MRDFResponse_ProcessingTimeStamp]
    ON [PnS].[MRDFResponse]([ProcessingTimeStamp] ASC) WITH (FILLFACTOR = 70);

