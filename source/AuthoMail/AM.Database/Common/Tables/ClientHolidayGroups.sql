CREATE TABLE [Common].[ClientHolidayGroups] (
    [ClientId]       INT NOT NULL,
    [HolidayGroupId] INT NOT NULL,
    CONSTRAINT [FK_ClientHolidayGroups_Clients] FOREIGN KEY ([ClientId]) REFERENCES [Common].[Clients] ([Id]),
    CONSTRAINT [FK_ClientHolidayGroups_HolidaysGroups] FOREIGN KEY ([HolidayGroupId]) REFERENCES [Common].[HolidaysGroups] ([Id])
);

