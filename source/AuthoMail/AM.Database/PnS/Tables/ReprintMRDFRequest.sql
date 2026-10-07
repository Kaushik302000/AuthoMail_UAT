CREATE TABLE [PnS].[ReprintMRDFRequest] (
    [JobId]              VARCHAR (10) NOT NULL,
    [PieceId]            INT          NOT NULL,
    [IsResponseReceived] BIT          NULL,
    CONSTRAINT [FK_ReprintMRDFRequest_ReprintBatches] FOREIGN KEY ([JobId]) REFERENCES [PnS].[ReprintBatches] ([PrintBatch])
);

