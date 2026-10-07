CREATE TABLE [PnS].[Schema_MRDFRequest] (
    [Id]                 INT             NOT NULL,
    [FieldName]          NVARCHAR (255)  NULL,
    [Type]               NVARCHAR (255)  NULL,
    [StartPosition]      INT             NOT NULL,
    [Length]             INT             NOT NULL,
    [IsActive]           BIT             NOT NULL,
    [Description]        NVARCHAR (2000) NULL,
    [Comments]           NVARCHAR (2000) NULL,
    [SampleValue]        NVARCHAR (255)  NULL,
    [DataType]           NVARCHAR (255)  NULL,
    [DataFormatTemplate] NVARCHAR (255)  NULL,
    [DefaultValue]       NVARCHAR (255)  NULL,
    CONSTRAINT [PK_SchemaMRDFRequest] PRIMARY KEY CLUSTERED ([Id] ASC)
);

