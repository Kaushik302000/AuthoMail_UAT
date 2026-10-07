CREATE TABLE [Common].[BusinessUnit] (
    [Id]           INT          IDENTITY (1, 1) NOT NULL,
    [BusinessUnit] VARCHAR (10) NOT NULL,
    [CustCode]     VARCHAR (20) NULL,
    [IsActive]     BIT          NOT NULL,
    CONSTRAINT [PK_BusinessUnit] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [UK_BusinessUnit] UNIQUE NONCLUSTERED ([BusinessUnit] ASC)
);

