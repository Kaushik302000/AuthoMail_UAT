CREATE TABLE [PnS].[Schema_BatchSummary] (
    [Id]                 INT             NOT NULL,
    [FieldName]          NVARCHAR (255)  NULL,
    [StartPosition]      INT             NOT NULL,
    [EndPosition]        INT             NOT NULL,
    [Length]             INT             NOT NULL,
    [Type]               NVARCHAR (255)  NULL,
    [IsActive]           BIT             NOT NULL,
    [Comments]           NVARCHAR (2000) NULL,
    [SampleValue]        NVARCHAR (255)  NULL,
    [DataType]           NVARCHAR (255)  NULL,
    [DataFormatTemplate] NVARCHAR (255)  NULL,
    [DefaultValue]       NVARCHAR (255)  NULL,
    CONSTRAINT [PK_SchemaBatchSummary] PRIMARY KEY CLUSTERED ([Id] ASC) WITH (FILLFACTOR = 70)
);

