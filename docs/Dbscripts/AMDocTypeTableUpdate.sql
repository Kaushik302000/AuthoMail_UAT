create table AM.ClientDocTypes
(
	Id int IDENTITY(1,1) NOT NULL,
	ClientId int not null,
	Name nvarchar(255) not null,
	CONSTRAINT [PK_ClientDocTypes_Id] PRIMARY KEY CLUSTERED (Id ASC),
	CONSTRAINT FK_ClientDocTypes_Clients FOREIGN KEY (ClientId) REFERENCES Common.Clients (Id) 
)
GO
CREATE UNIQUE INDEX Idx_ClientDocTypes_Name
ON AM.ClientDocTypes (Name);
GO
alter table AM.DocTypes
add ClientDocTypesId int null
go
ALTER TABLE AM.DocTypes  WITH CHECK ADD  CONSTRAINT [FK_DocTypes_ClientDocTypes] FOREIGN KEY(ClientDocTypesId)
REFERENCES AM.ClientDocTypes ([Id])
go
CREATE INDEX Idx_ClientDocTypes_ClientId
ON AM.ClientDocTypes (ClientId);
GO
CREATE INDEX Idx_DocTypes_ClientDocTypesId
ON AM.DocTypes (ClientDocTypesId);
GO
Drop index IX_DocTypes_ClientId_Description
on AM.DocTypes
Go
Drop index IX_DocTypes_ClientId_DocTypeCode
on AM.DocTypes
go
EXEC sp_rename 'AM.DocTypes.DocTypeCode', 'DocTypeCodeDelete', 'COLUMN';
Go
EXEC sp_rename 'AM.DocTypes.Description', 'DescriptionDelete', 'COLUMN';
Go
ALTER TABLE AM.DocTypes ALTER COLUMN DocTypeCodeDelete varchar(255) NULL
go
ALTER TABLE AM.DocTypes ALTER COLUMN DescriptionDelete varchar(512) NULL
go
