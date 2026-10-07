SET NUMERIC_ROUNDABORT OFF
GO
SET ANSI_PADDING, ANSI_WARNINGS, CONCAT_NULL_YIELDS_NULL, ARITHABORT, QUOTED_IDENTIFIER, ANSI_NULLS ON
GO
IF EXISTS (SELECT * FROM tempdb..sysobjects WHERE id=OBJECT_ID('tempdb..#tmpErrors')) DROP TABLE #tmpErrors
GO
CREATE TABLE #tmpErrors (Error int)
GO
SET XACT_ABORT ON
GO
SET TRANSACTION ISOLATION LEVEL SERIALIZABLE
GO
BEGIN TRANSACTION
GO


---------------Modifieable thing for you starts here---------

GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

PRINT N'Update ShippingServiceType'

UPDATE AM.DocTypes
SET
ShippingServiceType='IMB'
WHERE ShippingCarrier='USPS' AND ShippingServiceType='USPS'

PRINT N'Inserting master data into Common.PrintVendors'

------Code goes here-----
INSERT INTO Common.PrintVendors([Name],[SFTChannelId],[BusinessCode],[VendorCode]) Values ('Fort Mill, SC',1,'002416','PCI')
INSERT INTO Common.PrintVendors([Name],[SFTChannelId],[BusinessCode],[VendorCode]) Values ('Dallas CL',1,'ADE','CL_DALLAS')

PRINT N'Inserting master data into Common.PrintTrails'

INSERT INTO Common.PrintTrails([TrailName],[PrintVendorId],[SimulateTrail])  Values ('ALASKA-GENERIC',(select Id from [Common].[PrintVendors] where Name ='Fort Mill, SC'),0)
INSERT INTO Common.PrintTrails([TrailName],[PrintVendorId],[SimulateTrail])  Values ('CITIVM-GENERIC',(select Id from [Common].[PrintVendors] where Name ='Fort Mill, SC'),0)
INSERT INTO Common.PrintTrails([TrailName],[PrintVendorId],[SimulateTrail])  Values ('HSBC-GENERIC',(select Id from [Common].[PrintVendors] where Name ='Fort Mill, SC'),0)
INSERT INTO Common.PrintTrails([TrailName],[PrintVendorId],[SimulateTrail])  Values ('DALLAS-90days-CERT',(select Id from [Common].[PrintVendors] where Name ='Fort Mill, SC'),0)
INSERT INTO Common.PrintTrails([TrailName],[PrintVendorId],[SimulateTrail])  Values ('DALLAS-GENERIC',(select Id from [Common].[PrintVendors] where Name ='Fort Mill, SC'),0)
INSERT INTO Common.PrintTrails([TrailName],[PrintVendorId],[SimulateTrail])  Values ('DALLAS-Statements',(select Id from [Common].[PrintVendors] where Name ='Fort Mill, SC'),0)
INSERT INTO Common.PrintTrails([TrailName],[PrintVendorId],[SimulateTrail])  Values ('RPS-GENERIC',(select Id from [Common].[PrintVendors] where Name ='Fort Mill, SC'),0)
INSERT INTO Common.PrintTrails([TrailName],[PrintVendorId],[SimulateTrail])  Values ('USAA-GENERIC',(select Id from [Common].[PrintVendors] where Name ='Fort Mill, SC'),0)
INSERT INTO Common.PrintTrails([TrailName],[PrintVendorId],[SimulateTrail])  Values ('USAA-GENSAME',(select Id from [Common].[PrintVendors] where Name ='Fort Mill, SC'),0)
INSERT INTO Common.PrintTrails([TrailName],[PrintVendorId],[SimulateTrail])  Values ('USAA-GENERIC-FEDEX',(select Id from [Common].[PrintVendors] where Name ='Dallas CL'),0)
INSERT INTO Common.PrintTrails([TrailName],[PrintVendorId],[SimulateTrail])  Values ('USAA-GENERIC-PRI',(select Id from [Common].[PrintVendors] where Name ='Dallas CL'),0)


-- Insert [DocTypePrintTrailMaps] --
PRINT N'Inserting master data into Common.DocTypePrintTrailMaps'

INSERT INTO [Common].[DocTypePrintTrailMaps]([DocTypeId],[PrintTrailId],[BizDayStartTime],[BizDayEndTime],[SatStartTime],[SatEndTime],[SunStartTime],[SunEndTime])
SELECT Id,(SELECT Id FROM Common.PrintTrails WHERE TrailName='USAA-GENERIC-FEDEX'),'00:00:00','23:59:59','00:00:00','23:59:59',NULL,NULL FROM AM.DocTypes WHERE ClientId=(SELECT Id FROM Common.Clients WHERE AppCode='USAA') AND ShippingCarrier='FedEx'
INSERT INTO [Common].[DocTypePrintTrailMaps]([DocTypeId],[PrintTrailId],[BizDayStartTime],[BizDayEndTime],[SatStartTime],[SatEndTime],[SunStartTime],[SunEndTime])
SELECT Id,(SELECT Id FROM Common.PrintTrails WHERE TrailName='USAA-GENERIC-PRI'),'00:00:00','23:59:59','00:00:00','23:59:59',NULL,NULL FROM AM.DocTypes WHERE ClientId=(SELECT Id FROM Common.Clients WHERE AppCode='USAA') AND ShippingCarrier='USPS' AND ShippingServiceType IN ('PRIORITY','PRIORITY_EXPRESS')
INSERT INTO [Common].[DocTypePrintTrailMaps]([DocTypeId],[PrintTrailId],[BizDayStartTime],[BizDayEndTime],[SatStartTime],[SatEndTime],[SunStartTime],[SunEndTime])
SELECT Id,(SELECT Id FROM Common.PrintTrails WHERE TrailName='USAA-GENSAME'),'00:00:00','23:59:59','00:00:00','23:59:59',NULL,NULL FROM AM.DocTypes WHERE ClientId=(SELECT ID FROM Common.Clients WHERE AppCode='USAA') AND DocTypeCode IN ('CreditDisclosures_B1','CreditDisclosures_B2','CreditDisclosures_B3','CreditDisclosures_B4','CreditDisclosures_B5','CreditDisclosures_B6','CreditDisclosures_B7','CreditDisclosures_B8')
INSERT INTO [Common].[DocTypePrintTrailMaps]([DocTypeId],[PrintTrailId],[BizDayStartTime],[BizDayEndTime],[SatStartTime],[SatEndTime],[SunStartTime],[SunEndTime])
SELECT Id,(SELECT Id FROM Common.PrintTrails WHERE TrailName='USAA-GENERIC'),'00:00:00','23:59:59','00:00:00','23:59:59',NULL,NULL FROM AM.DocTypes WHERE ClientId=(SELECT ID FROM Common.Clients WHERE AppCode='USAA') AND ShippingCarrier='USPS' AND ShippingServiceType = 'IMB' AND DocTypeCode NOT IN ('CreditDisclosures_B1','CreditDisclosures_B2','CreditDisclosures_B3','CreditDisclosures_B4','CreditDisclosures_B5','CreditDisclosures_B6','CreditDisclosures_B7','CreditDisclosures_B8')
--->HSBC
INSERT INTO [Common].[DocTypePrintTrailMaps]([DocTypeId],[PrintTrailId],[BizDayStartTime],[BizDayEndTime],[SatStartTime],[SatEndTime],[SunStartTime],[SunEndTime])
SELECT Id,(SELECT Id FROM Common.PrintTrails WHERE TrailName='HSBC-GENERIC'),'00:00:00','23:59:59','00:00:00','23:59:59',NULL,NULL FROM AM.DocTypes WHERE ClientId=(SELECT ID FROM Common.Clients WHERE AppCode='HSBC') AND ShippingCarrier='USPS' AND ShippingServiceType = 'IMB'
--->ALASKA
INSERT INTO [Common].[DocTypePrintTrailMaps]([DocTypeId],[PrintTrailId],[BizDayStartTime],[BizDayEndTime],[SatStartTime],[SatEndTime],[SunStartTime],[SunEndTime])
SELECT Id,(SELECT Id FROM Common.PrintTrails WHERE TrailName='ALASKA-GENERIC'),'00:00:00','23:59:59','00:00:00','23:59:59',NULL,NULL FROM AM.DocTypes WHERE ClientId=(SELECT ID FROM Common.Clients WHERE AppCode='ALASKA') AND ShippingCarrier='USPS' AND ShippingServiceType = 'IMB'
--->CITI
INSERT INTO [Common].[DocTypePrintTrailMaps]([DocTypeId],[PrintTrailId],[BizDayStartTime],[BizDayEndTime],[SatStartTime],[SatEndTime],[SunStartTime],[SunEndTime])
SELECT Id,(SELECT Id FROM Common.PrintTrails WHERE TrailName='CITIVM-GENERIC'),'00:00:00','23:59:59','00:00:00','23:59:59',NULL,NULL FROM AM.DocTypes WHERE ClientId=(SELECT ID FROM Common.Clients WHERE AppCode='CITIVM')
--->RPS
INSERT INTO [Common].[DocTypePrintTrailMaps]([DocTypeId],[PrintTrailId],[BizDayStartTime],[BizDayEndTime],[SatStartTime],[SatEndTime],[SunStartTime],[SunEndTime])
SELECT Id,(SELECT Id FROM Common.PrintTrails WHERE TrailName='RPS-GENERIC'),'00:00:00','23:59:59','00:00:00','23:59:59',NULL,NULL FROM AM.DocTypes WHERE ClientId=(SELECT ID FROM Common.Clients WHERE AppCode='RPS')
--->DALLAS
INSERT INTO [Common].[DocTypePrintTrailMaps]([DocTypeId],[PrintTrailId],[BizDayStartTime],[BizDayEndTime],[SatStartTime],[SatEndTime],[SunStartTime],[SunEndTime])
SELECT Id,(SELECT Id FROM Common.PrintTrails WHERE TrailName='DALLAS-90days-CERT'),'00:00:00','23:59:59','00:00:00','23:59:59',NULL,NULL FROM AM.DocTypes WHERE ClientId=(SELECT ID FROM Common.Clients WHERE AppCode='DALLAS') AND DocTypeCode = '90days'
INSERT INTO [Common].[DocTypePrintTrailMaps]([DocTypeId],[PrintTrailId],[BizDayStartTime],[BizDayEndTime],[SatStartTime],[SatEndTime],[SunStartTime],[SunEndTime])
SELECT Id,(SELECT Id FROM Common.PrintTrails WHERE TrailName='DALLAS-GENERIC'),'00:00:00','23:59:59','00:00:00','23:59:59',NULL,NULL FROM AM.DocTypes WHERE ClientId=(SELECT ID FROM Common.Clients WHERE AppCode='DALLAS') AND DocTypeCode IN ('30days','60days','120days','AnnualPrivacyNotice','Escrow')
INSERT INTO [Common].[DocTypePrintTrailMaps]([DocTypeId],[PrintTrailId],[BizDayStartTime],[BizDayEndTime],[SatStartTime],[SatEndTime],[SunStartTime],[SunEndTime])
SELECT Id,(SELECT Id FROM Common.PrintTrails WHERE TrailName='DALLAS-Statements'),'00:00:00','23:59:59','00:00:00','23:59:59',NULL,NULL FROM AM.DocTypes WHERE ClientId=(SELECT ID FROM Common.Clients WHERE AppCode='DALLAS') AND DocTypeCode = 'Statements'

PRINT N'Inserting data into PnS.MRDFResponseHistory from PnS.MRDFResponse'

INSERT INTO [PnS].[MRDFResponseHistory]
  SELECT [JobId] ,[PieceId] ,[Date] ,[Feeder2_Fed] ,[OperatorId] ,[MachineId] ,[ProcessingTimeStamp] ,[DetailedErrorId] ,[Disposition] ,[Input_Fed] ,[Feeder3_Fed] ,[Feeder4_Fed] ,[Feeder5_Fed] ,[ComputedWeight] ,[MeterDate] ,[StatusSource] ,[PostageMetered] ,[WeightMeasured] ,[ExitLocation] ,[TotalSheets] ,[PDFFileName] ,[PackageTrackingNumber] FROM [PnS].[MRDFResponse]


PRINT N'Update data into AM.DocTypes.'
  UPDATE AM.DocTypes
SET
IsBRE=1
WHERE ClientId = (SELECT ID FROM Common.Clients WHERE AppCode='HSBC') AND DocTypeCode IN ('InitialDisclosures','ChangeofCircumstance','CounterOffer','LoanCommitment','Heloc','NOI','RateLock','ThirdPartyAuth','PreApproval','1003','PreApprovalUnderwriting','PreApprovalNotice','FloodAdvisory')



PRINT N'Delete data from PnS.MRDFResponse'
 delete from [PnS].[MRDFResponse]

PRINT N'Delete data from Common.SystemSettings'

 DELETE FROM Common.SystemSettings WHERE Id IN (
'Monitor_DSEmail_Subject',
'Monitor_DS_Email_To',
'Monitor_DS_Email_Cc',
'Monitor_PSEmail_Subject',
'Monitor_PS_Email_To',
'Monitor_PS_Email_Cc',
'Monitor_USAACustIdentifier',
'Monitor_AMCDirPath',
'Monitor_InternalPrintAndShipTiming',
'Common_PrintLocationCL',
'Common_PrintLocationPCI',
'Core_BundleSweepClientChannelIds',
'Monitor_InternalPrintAndShipStartTime',
'Monitor_InternalPrintAndShipEndTime')

GO

PRINT N'Insert data key Web_RegistrationBCCEmails into Common.SystemSettings'
INSERT [Common].[SystemSettings] ([Id], [DataType], [Value], [Description], [IsSystem], [DefaultValue], [ModifiedBy], [ModifiedOn]) VALUES (N'Web_RegistrationBCCEmails', N'String', 'avala@corelogic.com,sgolvelkar@corelogic.com', 'Default BCC emails for getting notification from registration flow', 0, 'avala@corelogic.com,sgolvelkar@corelogic.com', (select Id from [Profile].Users where UserName = 'system@milestonetechnoconsulting.com'), GETUTCDATE())
GO
----------Modifying code ends here--------------------------------
 
IF @@ERROR<>0 AND @@TRANCOUNT>0 ROLLBACK TRANSACTION
GO
IF @@TRANCOUNT=0 BEGIN INSERT INTO #tmpErrors (Error) SELECT 1 BEGIN TRANSACTION END
GO


IF EXISTS (SELECT * FROM #tmpErrors) ROLLBACK TRANSACTION
GO
IF @@TRANCOUNT>0 BEGIN
PRINT 'The database update succeeded'
COMMIT TRANSACTION
END
ELSE PRINT 'The database update failed'
GO
DROP TABLE #tmpErrors
GO