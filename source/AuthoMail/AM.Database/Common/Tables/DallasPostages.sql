CREATE TABLE [Common].[DallasPostages]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY, 
    [MinPage] INT NULL, 
    [MaxPage] INT NULL, 
    [Weight] DECIMAL(7, 4) NULL, 
    [PostageCost] DECIMAL(7, 4) NULL, 
    [EnvelopeId] INT NULL, 
    [InsertOneWeight] DECIMAL(7, 4) NULL, 
    [InsertOnePostageCost] DECIMAL(7, 4) NULL, 
    [InsertTwoWeight] DECIMAL(7, 4) NULL, 
    [InsertTwoPostageCost] DECIMAL(7, 4) NULL, 
    [InsertThreeWeight] DECIMAL(7, 4) NULL, 
    [InsertThreePostageCost] DECIMAL(7, 4) NULL
)

GO

CREATE UNIQUE INDEX [IXUNIQUE_DallasPostages] ON [Common].[DallasPostages] ([MinPage], [MaxPage], [EnvelopeId])
