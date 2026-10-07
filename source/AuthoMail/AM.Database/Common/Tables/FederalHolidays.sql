CREATE TABLE [Common].[FederalHolidays] (
    [Id]          INT           IDENTITY (1, 1) NOT NULL,
    [HolidayDate] DATE          NULL,
    [Description] VARCHAR (200) NULL,
    CONSTRAINT [PK_FederalHolidays] PRIMARY KEY CLUSTERED ([Id] ASC)
);




GO
CREATE NONCLUSTERED INDEX [IX_FederalHolidays_HolidayDate]
    ON [Common].[FederalHolidays]([HolidayDate] ASC) WITH (FILLFACTOR = 90);

