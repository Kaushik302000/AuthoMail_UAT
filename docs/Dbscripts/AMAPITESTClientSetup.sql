USE [AuthoMail] 
GO

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

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
--- Code Starts Here ---
DECLARE @CreatedBy nvarchar(128) = N'66f8d3e7-b66a-48d9-911f-d3914f1b3a11'
DECLARE @ClientId INT, @Environment VARCHAR(20) = (select [Value] from Common.SystemSettings where Id = 'Common_Environment'), @SFTChannelCode VARCHAR(20)='API_AMAPITEST'
DECLARE @NordisSFTChannelPath varchar(255) = N'F:\AM_Testing\AM_SFTP\NORDIS'
DECLARE @DALLASSFTChannelPath varchar(255) = N'F:\AM_Testing\AM_SFTP\CL_DALLAS'
DECLARE @PnSPath varchar(255) = N'F:\AM_Testing\AM_SFTP\PnS'
DECLARE @AMAPITESTPath varchar(255) = N'F:\AM_Testing\AM_SFTP\AMC'


IF NOT EXISTS(SELECT 1 FROM [Common].[Envelopes] WHERE EnvelopeName = 'TEST_#10')
BEGIN
INSERT INTO [Common].[Envelopes]([EnvelopeName],[EnvelopeDesc],[PartNumber],[IsBREEnvelope],[CreatedBy],[CreatedOn],[ModifiedBy],[ModifiedOn])
VALUES
('TEST_#10',NULL,'22222',0,@CreatedBy,GETUTCDATE(),@CreatedBy,GETUTCDATE())
END

IF NOT EXISTS(SELECT 1 FROM [Common].[Envelopes] WHERE EnvelopeName = 'TEST_9X12')
BEGIN
INSERT INTO [Common].[Envelopes]([EnvelopeName],[EnvelopeDesc],[PartNumber],[IsBREEnvelope],[CreatedBy],[CreatedOn],[ModifiedBy],[ModifiedOn])
VALUES
('TEST_9X12',NULL,'22223',0,@CreatedBy,GETUTCDATE(),@CreatedBy,GETUTCDATE())
END

IF NOT EXISTS(SELECT 1 FROM [Common].[Envelopes] WHERE EnvelopeName = 'TEST_TYVEK')
BEGIN
INSERT INTO [Common].[Envelopes]([EnvelopeName],[EnvelopeDesc],[PartNumber],[IsBREEnvelope],[CreatedBy],[CreatedOn],[ModifiedBy],[ModifiedOn])
VALUES
('TEST_TYVEK',NULL,'22224',0,@CreatedBy,GETUTCDATE(),@CreatedBy,GETUTCDATE())
END

IF NOT EXISTS(SELECT 1 FROM [Common].[Envelopes] WHERE EnvelopeName = 'TEST_USPS_PRI')
BEGIN
INSERT INTO [Common].[Envelopes]([EnvelopeName],[EnvelopeDesc],[PartNumber],[IsBREEnvelope],[CreatedBy],[CreatedOn],[ModifiedBy],[ModifiedOn])
VALUES
('TEST_USPS_PRI',NULL,'32224',0,@CreatedBy,GETUTCDATE(),@CreatedBy,GETUTCDATE())
END

IF NOT EXISTS(SELECT 1 FROM [Common].[Envelopes] WHERE EnvelopeName = 'TEST_USPS_PRIBOX')
BEGIN
INSERT INTO [Common].[Envelopes]([EnvelopeName],[EnvelopeDesc],[PartNumber],[IsBREEnvelope],[CreatedBy],[CreatedOn],[ModifiedBy],[ModifiedOn])
VALUES
('TEST_USPS_PRIBOX',NULL,'32225',0,@CreatedBy,GETUTCDATE(),@CreatedBy,GETUTCDATE())
END

IF NOT EXISTS(SELECT 1 FROM [Common].[Envelopes] WHERE EnvelopeName = 'TEST_USPS_PRI_LARGEBOX')
BEGIN
INSERT INTO [Common].[Envelopes]([EnvelopeName],[EnvelopeDesc],[PartNumber],[IsBREEnvelope],[CreatedBy],[CreatedOn],[ModifiedBy],[ModifiedOn])
VALUES
('TEST_USPS_PRI_LARGEBOX',NULL,'32226',0,@CreatedBy,GETUTCDATE(),@CreatedBy,GETUTCDATE())
END

IF NOT EXISTS(SELECT 1 FROM [Common].[Envelopes] WHERE EnvelopeName = 'TEST_USPS_FEDEX')
BEGIN
INSERT INTO [Common].[Envelopes]([EnvelopeName],[EnvelopeDesc],[PartNumber],[IsBREEnvelope],[CreatedBy],[CreatedOn],[ModifiedBy],[ModifiedOn])
VALUES
('TEST_USPS_FEDEX',NULL,'32227',0,@CreatedBy,GETUTCDATE(),@CreatedBy,GETUTCDATE())
END

IF NOT EXISTS(SELECT 1 FROM [Common].[Envelopes] WHERE EnvelopeName = 'TEST_BRE')
BEGIN
INSERT INTO [Common].[Envelopes]([EnvelopeName],[EnvelopeDesc],[PartNumber],[IsBREEnvelope],[CreatedBy],[CreatedOn],[ModifiedBy],[ModifiedOn])
VALUES
('TEST_BRE',NULL,'30000',1,@CreatedBy,GETUTCDATE(),@CreatedBy,GETUTCDATE())
END

IF NOT EXISTS(SELECT 1 FROM [Common].[Envelopes] WHERE EnvelopeName = 'ABC')
BEGIN
INSERT INTO [Common].[Envelopes]([EnvelopeName],[EnvelopeDesc],[PartNumber],[IsBREEnvelope],[CreatedBy],[CreatedOn],[ModifiedBy],[ModifiedOn])
VALUES
('ABC',NULL,'30001',1,@CreatedBy,GETUTCDATE(),@CreatedBy,GETUTCDATE())
END

IF NOT EXISTS(SELECT 1 FROM [Common].[Envelopes] WHERE EnvelopeName = 'XYZ')
BEGIN
INSERT INTO [Common].[Envelopes]([EnvelopeName],[EnvelopeDesc],[PartNumber],[IsBREEnvelope],[CreatedBy],[CreatedOn],[ModifiedBy],[ModifiedOn])
VALUES
('XYZ',NULL,'30002',1,@CreatedBy,GETUTCDATE(),@CreatedBy,GETUTCDATE())
END

IF NOT EXISTS(SELECT 1 FROM [Common].[Envelopes] WHERE EnvelopeName = 'DEF')
BEGIN
INSERT INTO [Common].[Envelopes]([EnvelopeName],[EnvelopeDesc],[PartNumber],[IsBREEnvelope],[CreatedBy],[CreatedOn],[ModifiedBy],[ModifiedOn])
VALUES
('DEF',NULL,'30003',1,@CreatedBy,GETUTCDATE(),@CreatedBy,GETUTCDATE())
END


IF NOT EXISTS(SELECT 1 FROM [Common].[SFTChannels] WHERE Code = 'NORDIS'+'_'+@Environment)
BEGIN
	INSERT [Common].[SFTChannels] ([Code], [Description], [Title], [Protocol], [Host], [UserName], [Password], [ToADEDir], [FromADEDir], [ArchiveDir], [PostDownloadAction], [IsActive]) VALUES ('NORDIS'+'_'+@Environment, NULL, N'Nordis Print Vendor', N'unc', @NordisSFTChannelPath, N'user', N'', N'/Response', N'/Submit', NULL, N'Delete', 1)
END

IF NOT EXISTS(SELECT 1 FROM [Common].[SFTChannels] WHERE Code = 'CL_DALLAS'+'_'+@Environment)
BEGIN
	INSERT [Common].[SFTChannels] ([Code], [Description], [Title], [Protocol], [Host], [UserName], [Password], [ToADEDir], [FromADEDir], [ArchiveDir], [PostDownloadAction], [IsActive]) VALUES ('CL_DALLAS'+'_'+@Environment, NULL, N'CL Dallas Print Vendor', N'unc', @DALLASSFTChannelPath, N'user', N'', N'/Response', N'/Submit', NULL, N'Delete', 1)	
END

IF NOT EXISTS(SELECT 1 FROM [Common].[SFTChannels] WHERE Code = 'PnSService'+'_'+@Environment)
BEGIN
INSERT INTO Common.SFTChannels (Code, Description, Title, Protocol, Host, UserName, Password, ToADEDir, FromADEDir, ArchiveDir, PostDownloadAction, IsActive, APITokenURL, APIResponseURL, APIMaxResponseCount, APITokenHeaders) VALUES ('PnSService'+'_'+@Environment, NULL, 'PnS Service Process', 'unc', @PnSPath, 'user', '', '/', '/', NULL, 'Delete', 1, NULL, NULL, NULL, NULL)
END

IF NOT EXISTS (SELECT 1 FROM Common.PrintVendors WHERE VendorCode = 'NORDIS')
BEGIN
	INSERT INTO [Common].[PrintVendors] ([Name],[SFTChannelId],[BusinessCode],[VendorCode])
	VALUES ('Nordis',(SELECT Id FROM Common.SFTChannels WHERE Code='NORDIS'+'_'+@Environment),'00241','NORDIS')
END
IF NOT EXISTS (SELECT 1 FROM Common.PrintVendors WHERE VendorCode = 'CL_DALLAS')
BEGIN
	INSERT INTO [Common].[PrintVendors] ([Name],[SFTChannelId],[BusinessCode],[VendorCode])
	VALUES ('CoreLogic',(SELECT Id FROM Common.SFTChannels WHERE Code='CL_DALLAS'+'_'+@Environment),'ADE','CL_DALLAS')
END

IF NOT EXISTS (SELECT 1 FROM Common.ShipperInformation WHERE PrintVendorCode = 'CL_DALLAS' AND PrintLocation='Dallas CL')
BEGIN
	INSERT INTO Common.ShipperInformation (PrintVendorId, PrintLocation, PrintVendorName, PrintVendorStreet, PrintVendorCity, PrintVendorState, PrintVendorZip, SFTChannelId, PrintVendorCode) VALUES ((SELECT Id FROM Common.PrintVendors WHERE VendorCode='CL_DALLAS'), 'Dallas CL', 'CoreLogic Dallas Facility', '3001 HACKBERRY RD', 'IRVING', 'TX', '75063', (SELECT Id FROM Common.SFTChannels WHERE Code='CL_DALLAS'+'_'+@Environment), 'CL_DALLAS')
END

IF NOT EXISTS (SELECT 1 FROM Common.ShipperInformation WHERE PrintVendorCode = 'ADE_NORDIS' AND PrintLocation='Coral Springs')
BEGIN
	INSERT INTO Common.ShipperInformation (PrintVendorId, PrintLocation, PrintVendorName, PrintVendorStreet, PrintVendorCity, PrintVendorState, PrintVendorZip, SFTChannelId, PrintVendorCode) VALUES ((SELECT Id FROM Common.PrintVendors WHERE VendorCode='NORDIS'), 'Coral Springs', 'Nordis Technologies', '4401 NW 124TH AVE', 'CORAL SPRINGS', 'FL', '33065', (SELECT Id FROM Common.SFTChannels WHERE Code='NORDIS'+'_'+@Environment), 'ADE_NORDIS')
END

IF NOT EXISTS (SELECT 1 FROM Common.ShipperInformation WHERE PrintVendorCode = 'ADE_NORDIS' AND PrintLocation='Las Vegas')
BEGIN
	INSERT INTO Common.ShipperInformation (PrintVendorId, PrintLocation, PrintVendorName, PrintVendorStreet, PrintVendorCity, PrintVendorState, PrintVendorZip, SFTChannelId, PrintVendorCode) VALUES ((SELECT Id FROM Common.PrintVendors WHERE VendorCode='NORDIS'), 'Las Vegas', 'Nordis Technologies', '1111 GRIER DRIVE, STE A', 'LAS VEGAS', 'NV', '89119', (SELECT Id FROM Common.SFTChannels WHERE Code='NORDIS'+'_'+@Environment), 'ADE_NORDIS')
END	

IF NOT EXISTS(SELECT 1 FROM Common.PrintTrails WHERE TrailName='GENERIC_FIRSTCLASS' AND ShipperId=(SELECT Id FROM Common.ShipperInformation WHERE PrintVendorCode='ADE_NORDIS' AND PrintLocation='Corel Springs'))
BEGIN
	INSERT [Common].[PrintTrails] ([TrailName], [PrintVendorCode], [PrintVendorId], [SimulateTrail], [ShipperId]) VALUES (N'GENERIC_FIRSTCLASS', N'ADE_NORDIS', (select Id from Common.PrintVendors where VendorCode = 'NORDIS'), 1, (SELECT Id FROM Common.ShipperInformation WHERE PrintVendorCode='ADE_NORDIS' AND PrintLocation='Corel Springs'))
END

IF NOT EXISTS(SELECT 1 FROM Common.PrintTrails WHERE TrailName='GENERIC_FEDEX' AND ShipperId=(SELECT Id FROM Common.ShipperInformation WHERE PrintVendorCode='ADE_NORDIS' AND PrintLocation='Las Vegas'))
BEGIN
	INSERT [Common].[PrintTrails] ([TrailName], [PrintVendorCode], [PrintVendorId], [SimulateTrail], [ShipperId]) VALUES (N'GENERIC_FEDEX', N'ADE_NORDIS', (select Id from Common.PrintVendors where VendorCode = 'NORDIS'), 1, (SELECT Id FROM Common.ShipperInformation WHERE PrintVendorCode='ADE_NORDIS' AND PrintLocation='Las Vegas'))
END

IF NOT EXISTS(SELECT 1 FROM Common.PrintTrails WHERE TrailName='GENERIC_PRI' AND ShipperId=(SELECT Id FROM Common.ShipperInformation WHERE PrintVendorCode='ADE_NORDIS' AND PrintLocation='Las Vegas'))
BEGIN
	INSERT [Common].[PrintTrails] ([TrailName], [PrintVendorCode], [PrintVendorId], [SimulateTrail], [ShipperId]) VALUES (N'GENERIC_PRI', N'ADE_NORDIS', (select Id from Common.PrintVendors where VendorCode = 'NORDIS'), 1, (SELECT Id FROM Common.ShipperInformation WHERE PrintVendorCode='ADE_NORDIS' AND PrintLocation='Las Vegas'))
END

IF NOT EXISTS(SELECT 1 FROM Common.PrintTrails WHERE TrailName='GENERIC_PRIEXP' AND ShipperId=(SELECT Id FROM Common.ShipperInformation WHERE PrintVendorCode='ADE_NORDIS' AND PrintLocation='Las Vegas'))
BEGIN
	INSERT [Common].[PrintTrails] ([TrailName], [PrintVendorCode], [PrintVendorId], [SimulateTrail], [ShipperId]) VALUES (N'GENERIC_PRIEXP', N'ADE_NORDIS', (select Id from Common.PrintVendors where VendorCode = 'NORDIS'), 1, (SELECT Id FROM Common.ShipperInformation WHERE PrintVendorCode='ADE_NORDIS' AND PrintLocation='Las Vegas'))
END

--DECLARE @ClientId INT, @Environment VARCHAR(20), @SFTChannelCode VARCHAR(20)='API_AMAPITEST'

INSERT [Common].[Clients] ([Name], [AppCode], [BusinessUnit], [IsAMActive], [IsPnSActive], [TimeZone], [ModifiedOn], [CreatedOn], [CreatedBy], [ModifiedBy], [Address], [Contact1Name], [Contact1Phone], [Contact1Email], [Contact2Name], [Contact2Phone], [Contact2Email], [Contact3Name], [Contact3Phone], [Contact3Email], [ITContactName], [ITContactPhone], [ITContactEmail], [BillingContactName], [BillingContactPhone], [BillingContactEmail], [SigningRoomExpiryDays], [Alias], [IsReqBlankPageValidation], [IsEncrypted]) VALUES ( N'AMAPITEST', N'AMAPITEST', 1, 1, 1, N'Eastern Standard Time', GETUTCDATE(), GETUTCDATE(), @CreatedBy, @CreatedBy, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, NULL, 0, 1)

SET @ClientId = SCOPE_IDENTITY()
SET @Environment= (SELECT [value] FROM Common.SystemSettings WHERE Id='Common_Environment')
IF NOT EXISTS(SELECT 1 FROM Common.Deliverables WHERE ClientId=@ClientId)
BEGIN
INSERT INTO Common.Deliverables (ClientId, ReportName, ReportType, DeliveryType, ReportQuery, Delimeter, FileNameExpression, IsSFTUploadRequired, UploadPath, EmailChannelId, ScheduleId) 
VALUES (@ClientId, 'Error Report', 'Error', 'Batch', '', NULL, 'Error-{Now:yyyyMMdd}.XML', 1, '/Reports', NULL, NULL)

INSERT INTO Common.Deliverables (ClientId, ReportName, ReportType, DeliveryType, ReportQuery, Delimeter, FileNameExpression, IsSFTUploadRequired, UploadPath, EmailChannelId, ScheduleId) 
VALUES (@ClientId, 'Delivery Report', 'Delivery', 'Batch', '', NULL, 'Delivery-{Now:yyyyMMdd}.XML', 1, '/Reports', NULL, NULL)
END

IF NOT EXISTS(SELECT 1 FROM [Common].[SFTChannels] WHERE Code=@SFTChannelCode+'_'+@Environment)
BEGIN
	INSERT INTO [Common].[SFTChannels] ([Code], [Description], [Title], [Protocol], [Host], [UserName], [Password], [ToADEDir], [FromADEDir], [ArchiveDir], [PostDownloadAction], [IsActive]) VALUES (@SFTChannelCode+'_'+@Environment, NULL, N'AM API Test', N'unc', @AMAPITESTPath, N'user', N'', N'/', NULL, N'/Archive', N'Archive', 1)
END

IF NOT EXISTS(SELECT 1 FROM [Common].[PnSConfig] WHERE ConfigName='NORDIS-AMAPITEST-FIRSTCLASS')
BEGIN
	INSERT INTO [Common].[PnSConfig]([ConfigName])VALUES('NORDIS-AMAPITEST-FIRSTCLASS')
END

IF NOT EXISTS(SELECT 1 FROM [Common].[PnSConfig] WHERE ConfigName='NORDIS-AMAPITEST-PRIORITY')
BEGIN
	INSERT INTO [Common].[PnSConfig]([ConfigName])VALUES('NORDIS-AMAPITEST-PRIORITY')
END

IF NOT EXISTS(SELECT 1 FROM [Common].[PnSConfig] WHERE ConfigName='NORDIS-AMAPITEST-PRIORITYEXPRESS')
BEGIN
	INSERT INTO [Common].[PnSConfig]([ConfigName])VALUES('NORDIS-AMAPITEST-PRIORITYEXPRESS')
END

IF NOT EXISTS(SELECT 1 FROM [Common].[PnSConfig] WHERE ConfigName='NORDIS-AMAPITEST-FEDEX')
BEGIN
	INSERT INTO [Common].[PnSConfig]([ConfigName])VALUES('NORDIS-AMAPITEST-FEDEX') 
END

IF NOT EXISTS(SELECT 1 FROM [Common].[PnSConfig] WHERE ConfigName='E-Delivery')
BEGIN
	INSERT INTO [Common].[PnSConfig]([ConfigName])VALUES('E-Delivery') 
END

IF NOT EXISTS(SELECT 1 FROM [Common].[PnSConfigDetail] WHERE PnSConfigId = (SELECT Id FROM [Common].[PnSConfig] WHERE ConfigName='E-Delivery'))
BEGIN
	INSERT INTO [Common].[PnSConfigDetail]([PnSConfigId],[PrintSize],[ContentScalePercent],[PrintFormat],[ShippingCarrier],[ShippingServiceType],[ShippingAccountCode],[ShippingSpecialService],[MinPage],[MaxPage],[EnvelopeId],[IsFallBack],[BizdayCutOff],[BizdaySLAType],[BizdayTimeSpan],[SatCutOff],[SatSLAType],[SatTimeSpan],[SunCutOff],[SunSLAType],[SunTimeSpan],[PrintTrailId],[BizDayStartTime],[BizDayEndTime],[SatStartTime],[SatEndTime],[SunStartTime],[SunEndTime])
	VALUES
	((SELECT Id FROM Common.PnSConfig WHERE ConfigName='E-Delivery'),'LETTER-IP',100,'Simplex','E-Delivery','E-Delivery',NULL,NULL,1,9999,NULL,0,'23:59:59','FixedTime','01:00:00:00','23:59:59','FixedTime','01:00:00:00',NULL,'NA',NULL,NULL,'00:00:00','23:59:59','00:00:00','23:59:59',NULL,NULL)
END

IF NOT EXISTS(SELECT 1 FROM [Common].[PnSConfigDetail] WHERE PnSConfigId = (SELECT Id FROM [Common].[PnSConfig] WHERE ConfigName='NORDIS-AMAPITEST-FIRSTCLASS'))
BEGIN
	INSERT INTO [Common].[PnSConfigDetail]([PnSConfigId],[PrintSize],[ContentScalePercent],[PrintFormat],[ShippingCarrier],[ShippingServiceType],[ShippingAccountCode],[ShippingSpecialService],[MinPage],[MaxPage],[EnvelopeId],[IsFallBack],[BizdayCutOff],[BizdaySLAType],[BizdayTimeSpan],[SatCutOff],[SatSLAType],[SatTimeSpan],[SunCutOff],[SunSLAType],[SunTimeSpan],[PrintTrailId],[BizDayStartTime],[BizDayEndTime],[SatStartTime],[SatEndTime],[SunStartTime],[SunEndTime])
	VALUES
	((SELECT Id FROM Common.PnSConfig WHERE ConfigName='NORDIS-AMAPITEST-FIRSTCLASS'),'LETTER-IP',100,'Simplex','USPS','IMB',NULL,NULL,1,50,(SELECT Id FROM Common.Envelopes WHERE EnvelopeName='TEST_TYVEK'),0,'20:00:00','FixedTime','00:23:59:59','20:00:00','FixedTime','00:23:59:59',NULL,'NA',NULL,(SELECT Id FROM Common.PrintTrails WHERE TrailName='GENERIC_FIRSTCLASS' AND PrintVendorCode='ADE_NORDIS'),'00:00:00','20:00:00','00:00:00','20:00:00',NULL,NULL)

	INSERT INTO [Common].[PnSConfigDetail]([PnSConfigId],[PrintSize],[ContentScalePercent],[PrintFormat],[ShippingCarrier],[ShippingServiceType],[ShippingAccountCode],[ShippingSpecialService],[MinPage],[MaxPage],[EnvelopeId],[IsFallBack],[BizdayCutOff],[BizdaySLAType],[BizdayTimeSpan],[SatCutOff],[SatSLAType],[SatTimeSpan],[SunCutOff],[SunSLAType],[SunTimeSpan],[PrintTrailId],[BizDayStartTime],[BizDayEndTime],[SatStartTime],[SatEndTime],[SunStartTime],[SunEndTime])
	VALUES
	((SELECT Id FROM Common.PnSConfig WHERE ConfigName='NORDIS-AMAPITEST-FIRSTCLASS'),'LETTER-IP',100,'Simplex','USPS','PRIORITY',NULL,NULL,51,140,(SELECT Id FROM Common.Envelopes WHERE EnvelopeName='TEST_USPS_PRI'),0,'17:30:00','FixedTime','00:23:59:59','14:00:00','FixedTime','00:23:59:59',NULL,'NA',NULL,(SELECT Id FROM Common.PrintTrails WHERE TrailName='GENERIC_PRI' AND PrintVendorCode='ADE_NORDIS'),'00:00:00','17:30:00','00:00:00','14:00:00',NULL,NULL)

	INSERT INTO [Common].[PnSConfigDetail]([PnSConfigId],[PrintSize],[ContentScalePercent],[PrintFormat],[ShippingCarrier],[ShippingServiceType],[ShippingAccountCode],[ShippingSpecialService],[MinPage],[MaxPage],[EnvelopeId],[IsFallBack],[BizdayCutOff],[BizdaySLAType],[BizdayTimeSpan],[SatCutOff],[SatSLAType],[SatTimeSpan],[SunCutOff],[SunSLAType],[SunTimeSpan],[PrintTrailId],[BizDayStartTime],[BizDayEndTime],[SatStartTime],[SatEndTime],[SunStartTime],[SunEndTime])
	VALUES
	((SELECT Id FROM Common.PnSConfig WHERE ConfigName='NORDIS-AMAPITEST-FIRSTCLASS'),'LETTER-IP',100,'Simplex','USPS','PRIORITY',NULL,NULL,141,700,(SELECT Id FROM Common.Envelopes WHERE EnvelopeName='TEST_USPS_PRIBOX'),0,'17:30:00','FixedTime','00:23:59:59','14:00:00','FixedTime','00:23:59:59',NULL,'NA',NULL,(SELECT Id FROM Common.PrintTrails WHERE TrailName='GENERIC_PRI' AND PrintVendorCode='ADE_NORDIS'),'00:00:00','17:30:00','00:00:00','14:00:00',NULL,NULL)

	INSERT INTO [Common].[PnSConfigDetail]([PnSConfigId],[PrintSize],[ContentScalePercent],[PrintFormat],[ShippingCarrier],[ShippingServiceType],[ShippingAccountCode],[ShippingSpecialService],[MinPage],[MaxPage],[EnvelopeId],[IsFallBack],[BizdayCutOff],[BizdaySLAType],[BizdayTimeSpan],[SatCutOff],[SatSLAType],[SatTimeSpan],[SunCutOff],[SunSLAType],[SunTimeSpan],[PrintTrailId],[BizDayStartTime],[BizDayEndTime],[SatStartTime],[SatEndTime],[SunStartTime],[SunEndTime])
	VALUES
	((SELECT Id FROM Common.PnSConfig WHERE ConfigName='NORDIS-AMAPITEST-FIRSTCLASS'),'LETTER-IP',100,'Simplex','USPS','PRIORITY',NULL,NULL,701,9999,(SELECT Id FROM Common.Envelopes WHERE EnvelopeName='TEST_USPS_PRI_LARGEBOX'),0,'17:30:00','FixedTime','00:23:59:59','14:00:00','FixedTime','00:23:59:59',NULL,'NA',NULL,(SELECT Id FROM Common.PrintTrails WHERE TrailName='GENERIC_PRI' AND PrintVendorCode='ADE_NORDIS'),'00:00:00','17:30:00','00:00:00','14:00:00',NULL,NULL)
END

IF NOT EXISTS(SELECT 1 FROM [Common].[PnSConfigDetail] WHERE PnSConfigId = (SELECT Id FROM [Common].[PnSConfig] WHERE ConfigName='NORDIS-AMAPITEST-PRIORITY'))
BEGIN
	INSERT INTO [Common].[PnSConfigDetail]([PnSConfigId],[PrintSize],[ContentScalePercent],[PrintFormat],[ShippingCarrier],[ShippingServiceType],[ShippingAccountCode],[ShippingSpecialService],[MinPage],[MaxPage],[EnvelopeId],[IsFallBack],[BizdayCutOff],[BizdaySLAType],[BizdayTimeSpan],[SatCutOff],[SatSLAType],[SatTimeSpan],[SunCutOff],[SunSLAType],[SunTimeSpan],[PrintTrailId],[BizDayStartTime],[BizDayEndTime],[SatStartTime],[SatEndTime],[SunStartTime],[SunEndTime])
	VALUES
	((SELECT Id FROM Common.PnSConfig WHERE ConfigName='NORDIS-AMAPITEST-PRIORITY'),'LETTER-IP',100,'Simplex','USPS','PRIORITY',NULL,NULL,1,9999,(SELECT Id FROM Common.Envelopes WHERE EnvelopeName='TEST_USPS_PRI'),0,'17:30:00','FixedTime','00:23:59:59','14:00:00','FixedTime','00:23:59:59',NULL,'NA',NULL,(SELECT Id FROM Common.PrintTrails WHERE TrailName='GENERIC_PRI' AND PrintVendorCode='ADE_NORDIS'),'00:00:00','17:30:00','00:00:00','14:00:00',NULL,NULL)
END

IF NOT EXISTS(SELECT 1 FROM [Common].[PnSConfigDetail] WHERE PnSConfigId = (SELECT Id FROM [Common].[PnSConfig] WHERE ConfigName='NORDIS-AMAPITEST-PRIORITYEXPRESS'))
BEGIN
	INSERT INTO [Common].[PnSConfigDetail]([PnSConfigId],[PrintSize],[ContentScalePercent],[PrintFormat],[ShippingCarrier],[ShippingServiceType],[ShippingAccountCode],[ShippingSpecialService],[MinPage],[MaxPage],[EnvelopeId],[IsFallBack],[BizdayCutOff],[BizdaySLAType],[BizdayTimeSpan],[SatCutOff],[SatSLAType],[SatTimeSpan],[SunCutOff],[SunSLAType],[SunTimeSpan],[PrintTrailId],[BizDayStartTime],[BizDayEndTime],[SatStartTime],[SatEndTime],[SunStartTime],[SunEndTime])
	VALUES
	((SELECT Id FROM Common.PnSConfig WHERE ConfigName='NORDIS-AMAPITEST-PRIORITYEXPRESS'),'LETTER-IP',100,'Simplex','USPS','PRIORITY_EXPRESS',NULL,NULL,1,9999,(SELECT Id FROM Common.Envelopes WHERE EnvelopeName='TEST_USPS_PRI'),0,'17:30:00','FixedTime','00:23:59:59','14:00:00','FixedTime','00:23:59:59',NULL,'NA',NULL,(SELECT Id FROM Common.PrintTrails WHERE TrailName='GENERIC_PRIEXP' AND PrintVendorCode='ADE_NORDIS'),'00:00:00','17:30:00','00:00:00','14:00:00',NULL,NULL)
END

IF NOT EXISTS(SELECT 1 FROM [Common].[PnSConfigDetail] WHERE PnSConfigId = (SELECT Id FROM [Common].[PnSConfig] WHERE ConfigName='NORDIS-AMAPITEST-FEDEX'))
BEGIN
	INSERT INTO [Common].[PnSConfigDetail]([PnSConfigId],[PrintSize],[ContentScalePercent],[PrintFormat],[ShippingCarrier],[ShippingServiceType],[ShippingAccountCode],[ShippingSpecialService],[MinPage],[MaxPage],[EnvelopeId],[IsFallBack],[BizdayCutOff],[BizdaySLAType],[BizdayTimeSpan],[SatCutOff],[SatSLAType],[SatTimeSpan],[SunCutOff],[SunSLAType],[SunTimeSpan],[PrintTrailId],[BizDayStartTime],[BizDayEndTime],[SatStartTime],[SatEndTime],[SunStartTime],[SunEndTime])
	VALUES
	((SELECT Id FROM Common.PnSConfig WHERE ConfigName='NORDIS-AMAPITEST-FEDEX'),'LETTER-IP',100,'Simplex','FedEx','PRIORITY_OVERNIGHT','CL_STAGING','SATURDAY_DELIVERY',1,9999,(SELECT Id FROM Common.Envelopes WHERE EnvelopeName='TEST_USPS_FEDEX'),0,'20:00:00','FixedTime','00:23:59:59','14:00:00','FixedTime','00:23:59:59',NULL,'NA',NULL,(SELECT Id FROM Common.PrintTrails WHERE TrailName='GENERIC_FEDEX' AND PrintVendorCode='ADE_NORDIS'),'00:00:00','20:00:00','00:00:00','14:00:00',NULL,NULL)

	INSERT INTO [Common].[PnSConfigDetail]([PnSConfigId],[PrintSize],[ContentScalePercent],[PrintFormat],[ShippingCarrier],[ShippingServiceType],[ShippingAccountCode],[ShippingSpecialService],[MinPage],[MaxPage],[EnvelopeId],[IsFallBack],[BizdayCutOff],[BizdaySLAType],[BizdayTimeSpan],[SatCutOff],[SatSLAType],[SatTimeSpan],[SunCutOff],[SunSLAType],[SunTimeSpan],[PrintTrailId],[BizDayStartTime],[BizDayEndTime],[SatStartTime],[SatEndTime],[SunStartTime],[SunEndTime])
	VALUES
	((SELECT Id FROM Common.PnSConfig WHERE ConfigName='NORDIS-AMAPITEST-FEDEX'),'LETTER-IP',100,'Simplex','USPS','PRIORITY_EXPRESS',NULL,NULL,1,9999,(SELECT Id FROM Common.Envelopes WHERE EnvelopeName='TEST_USPS_PRI'),1,'17:30:00','FixedTime','00:23:59:59','14:00:00','FixedTime','00:23:59:59',NULL,'NA',NULL,(SELECT Id FROM Common.PrintTrails WHERE TrailName='GENERIC_PRIEXP' AND PrintVendorCode='ADE_NORDIS'),'00:00:00','17:30:00','00:00:00','14:00:00',NULL,NULL)

	INSERT INTO [Common].[PnSConfigDetail]([PnSConfigId],[PrintSize],[ContentScalePercent],[PrintFormat],[ShippingCarrier],[ShippingServiceType],[ShippingAccountCode],[ShippingSpecialService],[MinPage],[MaxPage],[EnvelopeId],[IsFallBack],[BizdayCutOff],[BizdaySLAType],[BizdayTimeSpan],[SatCutOff],[SatSLAType],[SatTimeSpan],[SunCutOff],[SunSLAType],[SunTimeSpan],[PrintTrailId],[BizDayStartTime],[BizDayEndTime],[SatStartTime],[SatEndTime],[SunStartTime],[SunEndTime])
	VALUES
	((SELECT Id FROM Common.PnSConfig WHERE ConfigName='NORDIS-AMAPITEST-FEDEX'),'LETTER-IP',100,'Simplex','USPS','PRIORITY',NULL,NULL,1,9999,(SELECT Id FROM Common.Envelopes WHERE EnvelopeName='TEST_USPS_PRI'),1,'17:30:00','FixedTime','00:23:59:59','14:00:00','FixedTime','00:23:59:59',NULL,'NA',NULL,(SELECT Id FROM Common.PrintTrails WHERE TrailName='GENERIC_PRI' AND PrintVendorCode='ADE_NORDIS'),'00:00:00','17:30:00','00:00:00','14:00:00',NULL,NULL)
END

IF NOT EXISTS(SELECT 1 FROM [Common].[PnSConfigDetailCoverLetters] WHERE PnSConfigDetailId=(SELECT Id FROM [Common].[PnSConfigDetail] WHERE PnSConfigId = (SELECT Id FROM Common.PnSConfig WHERE ConfigName='NORDIS-AMAPITEST-FIRSTCLASS') AND MinPage = 1 and MaxPage = 50))
BEGIN
	INSERT INTO [Common].[PnSConfigDetailCoverLetters]([PnSConfigDetailId],[SeqId],[CoverLetterTemplatePath])
	VALUES((SELECT Id FROM [Common].[PnSConfigDetail] WHERE PnSConfigId = (SELECT Id FROM Common.PnSConfig WHERE ConfigName='NORDIS-AMAPITEST-FIRSTCLASS') AND MinPage = 1 and MaxPage = 50),1,'USPS_Generic_1.pdf')
END

IF NOT EXISTS(SELECT 1 FROM [AM].[DocTypes] WHERE ClientId=@ClientId AND DocTypeName='Disclosures')
BEGIN
	INSERT [AM].[DocTypes] ([ClientId], [QCPercent], [IsActive], [ModifiedOn], [CreatedOn], [CreatedBy], [ModifiedBy], [IsBRE], [NoOfCopies], [BREEnvelopeId], [IsPrintReady], [PnSConfigId], [DocTypeName],[AllowInsertsOnly]) 
	VALUES (@ClientId, 0, 1, GETUTCDATE(), GETUTCDATE(), @CreatedBy, @CreatedBy,0,1,NULL,0,(SELECT Id FROM Common.PnSConfig WHERE ConfigName='NORDIS-AMAPITEST-FIRSTCLASS'),'Disclosures',1)
END

IF NOT EXISTS(SELECT 1 FROM [AM].[DocTypes] WHERE ClientId=@ClientId AND DocTypeName='Initial Disclosures')
BEGIN
	INSERT [AM].[DocTypes] ([ClientId], [QCPercent], [IsActive], [ModifiedOn], [CreatedOn], [CreatedBy], [ModifiedBy], [IsBRE], [NoOfCopies], [BREEnvelopeId], [IsPrintReady], [PnSConfigId], [DocTypeName],[AllowInsertsOnly]) VALUES (@ClientId, 0, 1, GETUTCDATE(), GETUTCDATE(), @CreatedBy, @CreatedBy,0,1,NULL,0,(SELECT Id FROM Common.PnSConfig WHERE ConfigName='NORDIS-AMAPITEST-PRIORITY'),'Initial Disclosures',1)
END

IF NOT EXISTS(SELECT 1 FROM [AM].[DocTypes] WHERE ClientId=@ClientId AND DocTypeName='Credit Disclosures')
BEGIN
	INSERT [AM].[DocTypes] ([ClientId], [QCPercent], [IsActive], [ModifiedOn], [CreatedOn], [CreatedBy], [ModifiedBy], [IsBRE], [NoOfCopies], [BREEnvelopeId], [IsPrintReady], [PnSConfigId], [DocTypeName],[AllowInsertsOnly]) VALUES (@ClientId, 0, 1, GETUTCDATE(), GETUTCDATE(), @CreatedBy, @CreatedBy,0,1,NULL,0,(SELECT Id FROM Common.PnSConfig WHERE ConfigName='NORDIS-AMAPITEST-PRIORITYEXPRESS'),'Credit Disclosures',1)
END

IF NOT EXISTS(SELECT 1 FROM [AM].[DocTypes] WHERE ClientId=@ClientId AND DocTypeName='Stand Alone - Loan Estimate')
BEGIN
	INSERT [AM].[DocTypes] ([ClientId], [QCPercent], [IsActive], [ModifiedOn], [CreatedOn], [CreatedBy], [ModifiedBy], [IsBRE], [NoOfCopies], [BREEnvelopeId], [IsPrintReady], [PnSConfigId], [DocTypeName],[AllowInsertsOnly]) VALUES (@ClientId, 0, 1, GETUTCDATE(), GETUTCDATE(), @CreatedBy, @CreatedBy,0,1,NULL,0,(SELECT Id FROM Common.PnSConfig WHERE ConfigName='NORDIS-AMAPITEST-FEDEX'),'Stand Alone - Loan Estimate',1)
END

IF NOT EXISTS(SELECT 1 FROM [AM].[DocTypes] WHERE ClientId=@ClientId AND DocTypeName='Stand Alone - Closing Disclosure')
BEGIN
	INSERT [AM].[DocTypes] ([ClientId], [QCPercent], [IsActive], [ModifiedOn], [CreatedOn], [CreatedBy], [ModifiedBy], [IsBRE], [NoOfCopies], [BREEnvelopeId], [IsPrintReady], [PnSConfigId], [DocTypeName],[AllowInsertsOnly]) VALUES (@ClientId, 0, 1, GETUTCDATE(), GETUTCDATE(), @CreatedBy, @CreatedBy,1,1,(SELECT Id FROM Common.Envelopes WHERE EnvelopeName='TEST_BRE'),0,(SELECT Id FROM Common.PnSConfig WHERE ConfigName='NORDIS-AMAPITEST-FIRSTCLASS'),'Stand Alone - Closing Disclosure',1)
END

IF NOT EXISTS(SELECT 1 FROM [AM].[DocTypes] WHERE ClientId=@ClientId AND DocTypeName='E-Delivery')
BEGIN
	INSERT [AM].[DocTypes] ([ClientId], [QCPercent], [IsActive], [ModifiedOn], [CreatedOn], [CreatedBy], [ModifiedBy], [IsBRE], [NoOfCopies], [BREEnvelopeId], [IsPrintReady], [PnSConfigId], [DocTypeName],[AllowInsertsOnly]) VALUES (@ClientId, 0, 1, GETUTCDATE(), GETUTCDATE(), @CreatedBy, @CreatedBy,0,1,NULL,0,(SELECT Id FROM Common.PnSConfig WHERE ConfigName='E-Delivery'),'E-Delivery',1)
END

IF NOT EXISTS(SELECT 1 FROM [Common].[PnSSweepTimings] WHERE AppCode='AMAPITEST' AND VendorCode='CL_DALLAS')
BEGIN
	INSERT INTO [Common].[PnSSweepTimings] ([BundleSeqNo],[AppCode],[VendorCode],[WeekDayType],[SweepType],[SweepTime])
	VALUES(1,'AMAPITEST','CL_DALLAS','Weekday','Continuous','00:00:00.0000000')

	INSERT INTO [Common].[PnSSweepTimings] ([BundleSeqNo],[AppCode],[VendorCode],[WeekDayType],[SweepType],[SweepTime])
	VALUES(1,'AMAPITEST','CL_DALLAS','Saturday','Continuous','00:00:00.0000000')
END

IF NOT EXISTS(SELECT 1 FROM [Common].[PnSSweepTimings] WHERE AppCode='AMAPITEST' AND VendorCode='NORDIS')
BEGIN
	INSERT INTO [Common].[PnSSweepTimings] ([BundleSeqNo],[AppCode],[VendorCode],[WeekDayType],[SweepType],[SweepTime])
	VALUES(1,'AMAPITEST','NORDIS','Weekday','OnSchedule','07:45:00.0000000')

	INSERT INTO [Common].[PnSSweepTimings] ([BundleSeqNo],[AppCode],[VendorCode],[WeekDayType],[SweepType],[SweepTime])
	VALUES(2,'AMAPITEST','NORDIS','Weekday','OnSchedule','11:45:00.0000000')

	INSERT INTO [Common].[PnSSweepTimings] ([BundleSeqNo],[AppCode],[VendorCode],[WeekDayType],[SweepType],[SweepTime])
	VALUES(3,'AMAPITEST','NORDIS','Weekday','OnSchedule','15:00:00.0000000')

	INSERT INTO [Common].[PnSSweepTimings] ([BundleSeqNo],[AppCode],[VendorCode],[WeekDayType],[SweepType],[SweepTime])
	VALUES(4,'AMAPITEST','NORDIS','Weekday','OnSchedule','17:00:00.0000000')

	INSERT INTO [Common].[PnSSweepTimings] ([BundleSeqNo],[AppCode],[VendorCode],[WeekDayType],[SweepType],[SweepTime])
	VALUES(5,'AMAPITEST','NORDIS','Weekday','OnSchedule','20:00:00.0000000')

	INSERT INTO [Common].[PnSSweepTimings] ([BundleSeqNo],[AppCode],[VendorCode],[WeekDayType],[SweepType],[SweepTime])
	VALUES(1,'AMAPITEST','NORDIS','Saturday','OnSchedule','07:45:00.0000000')

	INSERT INTO [Common].[PnSSweepTimings] ([BundleSeqNo],[AppCode],[VendorCode],[WeekDayType],[SweepType],[SweepTime])
	VALUES(2,'AMAPITEST','NORDIS','Saturday','OnSchedule','11:45:00.0000000')

	INSERT INTO [Common].[PnSSweepTimings] ([BundleSeqNo],[AppCode],[VendorCode],[WeekDayType],[SweepType],[SweepTime])
	VALUES(3,'AMAPITEST','NORDIS','Saturday','OnSchedule','15:00:00.0000000')

	INSERT INTO [Common].[PnSSweepTimings] ([BundleSeqNo],[AppCode],[VendorCode],[WeekDayType],[SweepType],[SweepTime])
	VALUES(4,'AMAPITEST','NORDIS','Saturday','OnSchedule','17:00:00.0000000')

	INSERT INTO [Common].[PnSSweepTimings] ([BundleSeqNo],[AppCode],[VendorCode],[WeekDayType],[SweepType],[SweepTime])
	VALUES(5,'AMAPITEST','NORDIS','Saturday','OnSchedule','20:00:00.0000000')
END

IF NOT EXISTS(SELECT 1 FROM Profile.Users WHERE Email='amtest')
BEGIN
	INSERT INTO Profile.Users (Id, Email, EmailConfirmed, PasswordHash, SecurityStamp, PhoneNumber, PhoneNumberConfirmed, TwoFactorEnabled, LockoutEndDateUtc, LockoutEnabled, AccessFailedCount, UserName, ClientId, FirstName, LastName, DisplayName, RequestedOn, Status, ClosedOn, ClosedBy, LastLoginTimeUTC, ChangePasswordDateUTC, StatusModifiedBy, StatusModifiedOn, SFTChannelId) VALUES ('f2e490f8-15c9-4f4f-9b28-b14a643ccddf', 'amtest', 1, 'AJU6wq6AtBacLbhTU0ohlKNW/lL4rzePCX7qYAJRplxyQG+wWTgEn7gw9FetbGzVng==', '505cb5cd-5e36-434f-8eda-0d9de906ab0c', NULL, 0, 0, NULL, 1, 0, 'amtest', @ClientId, NULL, NULL, NULL, GETUTCDATE(), 'Approved', GETUTCDATE(), NULL, NULL, GETUTCDATE(), NULL, GETUTCDATE(), (SELECT Id FROM Common.SFTChannels WHERE Code=@SFTChannelCode+'_'+@Environment))
	
	INSERT INTO Profile.UserRoles (UserId, RoleId) VALUES ('f2e490f8-15c9-4f4f-9b28-b14a643ccddf', '10a98c20-bc2c-4914-b823-7e25a78ad673')
END
EXEC usp_MapClientSFTChannels 'AMAPITEST','API_AMAPITEST_Stage'

--- Code Ends here --- 

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