CREATE TABLE [AM].[DocTypeInserts] (
    [DocTypeId]                            INT            NOT NULL,
    [BREEnvelopeId]                       INT            NOT NULL, 
    CONSTRAINT [PK_DocTypeInserts] PRIMARY KEY ([DocTypeId], [BREEnvelopeId])
);
GO
