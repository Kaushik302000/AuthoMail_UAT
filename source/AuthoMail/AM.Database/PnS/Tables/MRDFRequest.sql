CREATE TABLE [PnS].[MRDFRequest] (
    [UniqueId]          UNIQUEIDENTIFIER NULL,
    [JobId]             INT              NOT NULL,
    [PieceId]           INT              NOT NULL,
    [Date]              DATE             NOT NULL,
    [TotalSheets]       INT              NULL,
    [InputWeight]       VARCHAR (255)    NULL,
    [Feeder3_Sel]       VARCHAR (255)    NULL,
    [Feeder4_Sel]       VARCHAR (255)    NULL,
    [Feeder5_Sel]       VARCHAR (255)    NULL,
    [LowCap_Sel]        VARCHAR (255)    NULL,
    [NoSeal_Sel]        VARCHAR (255)    NULL,
    [Feeder2_Sel]       VARCHAR (255)    NULL,
    [ZipMark_Sel]       VARCHAR (255)    NULL,
    [Address1]          VARCHAR (255)    NULL,
    [Address2]          VARCHAR (255)    NULL,
    [Zip5]              VARCHAR (255)    NULL,
    [Zip4]              VARCHAR (255)    NULL,
    [Zip2]              VARCHAR (255)    NULL,
    [DivertVS2]         VARCHAR (255)    NULL,
    [DivertVS3]         VARCHAR (255)    NULL,
    [DivertVS4]         VARCHAR (255)    NULL,
    [ReturnLogoBitMap]  VARCHAR (255)    NULL,
    [ReturnLine1]       VARCHAR (255)    NULL,
    [ReturnLine2]       VARCHAR (255)    NULL,
    [ReturnLine3]       VARCHAR (255)    NULL,
    [ReturnLine4]       VARCHAR (255)    NULL,
    [ReturnLine5]       VARCHAR (255)    NULL,
    [ReturnLine6]       VARCHAR (255)    NULL,
    [GroupNumber]       VARCHAR (255)    NULL,
    [SocialSecNumber]   VARCHAR (255)    NULL,
    [CheckAmount]       VARCHAR (255)    NULL,
    [PassThru]          VARCHAR (255)    NULL,
    [Expansion]         VARCHAR (255)    NULL,
    [EndofRecord]       VARCHAR (255)    NULL,
    [PDFFileName]       VARCHAR (255)    NULL,
    [ErrId]             INT              NULL,
    [ErrDesc]           VARCHAR (MAX)    NULL,
    [MailPieceStatus]   VARCHAR (255)    NULL,
    [IsPDFReceived]     BIT              CONSTRAINT [DF_MRDFRequest_IsPDFReceived] DEFAULT ((0)) NULL,
    [PrintTrailBatchId] INT              NULL,
    [FilePath]          VARCHAR (255)    NULL,
    [PDFFileSize]       BIGINT           NULL,
    [BarcodeNumber] VARCHAR(255) NULL, 
    CONSTRAINT [PK_MRDFRequest] PRIMARY KEY CLUSTERED ([JobId] ASC, [PieceId] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_MRDFRequest_BatchSummary] FOREIGN KEY ([JobId]) REFERENCES [PnS].[BatchSummary] ([PrintBatch]),
    CONSTRAINT [FK_MRDFRequest_PrintTrailBatches] FOREIGN KEY ([PrintTrailBatchId]) REFERENCES [PnS].[PrintTrailBatches] ([Id])
);




GO
CREATE NONCLUSTERED INDEX [IX_MRDFRequest_Date]
    ON [PnS].[MRDFRequest]([Date] ASC) WITH (FILLFACTOR = 70);


GO
CREATE NONCLUSTERED INDEX [IX_MRDFRequest_JobId]
    ON [PnS].[MRDFRequest]([JobId] ASC) WITH (FILLFACTOR = 70);


GO
CREATE NONCLUSTERED INDEX [IX_MRDFRequest_MailPieceStatus]
    ON [PnS].[MRDFRequest]([MailPieceStatus] ASC) WITH (FILLFACTOR = 70);


GO
CREATE NONCLUSTERED INDEX [IX_MRDFRequest_PrintTrailBatchId]
    ON [PnS].[MRDFRequest]([PrintTrailBatchId] ASC);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_MRDFRequest_UniqueId]
    ON [PnS].[MRDFRequest]([UniqueId] ASC) WHERE ([UniqueId] IS NOT NULL) WITH (FILLFACTOR = 70);

