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


PRINT N'Start execution'
----------------------------------------------------------------------------
INSERT INTO [Common].[Envelopes]([Id],[CurrentQuantity],[IsBREEnvelope],CreatedBy,ModifiedBy,CreatedOn,ModifiedOn)
VALUES('CL_S_9_12',0,0,'41513528-03ec-4fc5-a4bd-d00cf03f9ae4','41513528-03ec-4fc5-a4bd-d00cf03f9ae4',GETUTCDATE(),GETUTCDATE())
INSERT INTO [Common].[Envelopes]([Id],[CurrentQuantity],[IsBREEnvelope],CreatedBy,ModifiedBy,CreatedOn,ModifiedOn)
VALUES('CL_S_6_9',0,0,'41513528-03ec-4fc5-a4bd-d00cf03f9ae4','41513528-03ec-4fc5-a4bd-d00cf03f9ae4',GETUTCDATE(),GETUTCDATE())
INSERT INTO [Common].[Envelopes]([Id],[CurrentQuantity],[IsBREEnvelope],CreatedBy,ModifiedBy,CreatedOn,ModifiedOn)
VALUES('CL_S_10',0,0,'41513528-03ec-4fc5-a4bd-d00cf03f9ae4','41513528-03ec-4fc5-a4bd-d00cf03f9ae4',GETUTCDATE(),GETUTCDATE())
INSERT INTO [Common].[Envelopes]([Id],[CurrentQuantity],[IsBREEnvelope],CreatedBy,ModifiedBy,CreatedOn,ModifiedOn)
VALUES('USAA_S_10',0,0,'41513528-03ec-4fc5-a4bd-d00cf03f9ae4','41513528-03ec-4fc5-a4bd-d00cf03f9ae4',GETUTCDATE(),GETUTCDATE())
INSERT INTO [Common].[Envelopes]([Id],[CurrentQuantity],[IsBREEnvelope],CreatedBy,ModifiedBy,CreatedOn,ModifiedOn)
VALUES('HSBC_B_9_12',0,1,'41513528-03ec-4fc5-a4bd-d00cf03f9ae4','41513528-03ec-4fc5-a4bd-d00cf03f9ae4',GETUTCDATE(),GETUTCDATE())
INSERT INTO [Common].[Envelopes]([Id],[CurrentQuantity],[IsBREEnvelope],CreatedBy,ModifiedBy,CreatedOn,ModifiedOn)
VALUES('BOTW_F_9_12',0,0,'41513528-03ec-4fc5-a4bd-d00cf03f9ae4','41513528-03ec-4fc5-a4bd-d00cf03f9ae4',GETUTCDATE(),GETUTCDATE())
INSERT INTO [Common].[Envelopes]([Id],[CurrentQuantity],[IsBREEnvelope],CreatedBy,ModifiedBy,CreatedOn,ModifiedOn)
VALUES('BOTW_B_10_13',0,1,'41513528-03ec-4fc5-a4bd-d00cf03f9ae4','41513528-03ec-4fc5-a4bd-d00cf03f9ae4',GETUTCDATE(),GETUTCDATE())


--USAA
UPDATE AM.DocTypes
SET
EnvelopeId=(select Id from Common.Envelopes WHERE Id='USAA_S_10')
WHERE ClientId=(SELECT Id FROM Common.Clients WHERE AppCode='USAA') AND ShippingCarrier='USPS' AND ShippingServiceType='IMB'

--CITI, ALASKA, HSBC
UPDATE AM.DocTypes
SET
EnvelopeId=(select Id from Common.Envelopes WHERE Id='CL_S_9_12')
WHERE ClientId IN ((SELECT Id FROM Common.Clients WHERE AppCode='CITIVM'),(SELECT Id FROM Common.Clients WHERE AppCode='ALASKA'),(SELECT Id FROM Common.Clients WHERE AppCode='HSBC'))

--HSBC
UPDATE AM.DocTypes
SET
BREEnvelopeId=CASE WHEN IsBRE=1 THEN (select Id from Common.Envelopes WHERE Id='HSBC_B_9_12') ELSE NULL END
WHERE ClientId IN ((SELECT Id FROM Common.Clients WHERE AppCode='HSBC'))

--RPS
UPDATE AM.DocTypes
SET
EnvelopeId=(select Id from Common.Envelopes WHERE Id='CL_S_6_9')
WHERE ClientId in ((SELECT Id FROM Common.Clients WHERE AppCode='RPS'))

--BOTW
UPDATE AM.DocTypes
SET 
EnvelopeId=(select Id from Common.Envelopes WHERE Id='BOTW_F_9_12'),
BREEnvelopeId=CASE WHEN IsBRE=1 THEN (select Id from Common.Envelopes WHERE Id='BOTW_B_10_13') ELSE NULL END
WHERE ClientId IN ((SELECT Id FROM Common.Clients WHERE AppCode='BOTW'))



---- UPDATE CUTOFF TIMINGS FOR PRINT READY CLIENTS

UPDATE AM.DocTypes
SET
[BizdayCutOff]='20:00:00',
[BizdaySLAType]='FixedTime',
[BizdayTimeSpan]='00:23:59:59',
[SatCutOff]=NULL,
[SatSLAType]='NA',
[SatTimeSpan]=NULL,
[SunCutOff]=NULL,
[SunSLAType]='NA',
[SunTimeSpan]=NULL
WHERE ClientId in ((SELECT Id FROM Common.Clients WHERE AppCode='CITIVM'),(SELECT Id FROM Common.Clients WHERE AppCode='RPS'))

---- INSERT DELIVERABLES FOR PRINT READY CLIENTS

-- Email Channel Data for CITIVM

INSERT INTO [Common].[EmailChannels]([Title],[To],[Cc],[Bcc],[Subject],[BodyTemplate],[IsBodyHTML])VALUES('CITIVM Download Reporting','sgolvelkar@corelogic.com','avala@corelogic.com,vmungara@corelogic.com',NULL,'Download Summary','HTML',0)

INSERT INTO [Common].[EmailChannels]([Title],[To],[Cc],[Bcc],[Subject],[BodyTemplate],[IsBodyHTML])VALUES('CITIVM Production Reporting','sgolvelkar@corelogic.com','avala@corelogic.com,vmungara@corelogic.com',NULL,'Production Summary','HTML',0)

-- Deliverables Data for CITIVM

INSERT INTO [Common].[Deliverables]([ClientId],[ReportName],[ReportType],[DeliveryType],[ReportQuery],[Delimeter],[FileNameExpression],[IsSFTUploadRequired],[UploadPath],[EmailChannelId],[ScheduleId])VALUES((select Id from Common.Clients WHERE AppCode='CITIVM'),'Error Report','Error','Batch','','\t','Error-{Now:yyyyMMdd}.csv',1,'/Reports',(select Id from Common.EmailChannels WHERE Title='CITIVM Download Reporting'),NULL)

INSERT INTO [Common].[Deliverables]([ClientId],[ReportName],[ReportType],[DeliveryType],[ReportQuery],[Delimeter],[FileNameExpression],[IsSFTUploadRequired],[UploadPath],[EmailChannelId],[ScheduleId])VALUES((select Id from Common.Clients WHERE AppCode='CITIVM'),'Delivery Report','Delivery','Batch','','\t','Delivery-{Now:yyyyMMdd}.csv',1,'/Reports',(select Id from Common.EmailChannels WHERE Title='CITIVM Production Reporting'),NULL)


-- Email Channel Data for RPS

INSERT INTO [Common].[EmailChannels]([Title],[To],[Cc],[Bcc],[Subject],[BodyTemplate],[IsBodyHTML])VALUES('RPS Download Reporting','sgolvelkar@corelogic.com','avala@corelogic.com,vmungara@corelogic.com',NULL,'Download Summary','HTML',0)

INSERT INTO [Common].[EmailChannels]([Title],[To],[Cc],[Bcc],[Subject],[BodyTemplate],[IsBodyHTML])VALUES('RPS Production Reporting','sgolvelkar@corelogic.com','avala@corelogic.com,vmungara@corelogic.com',NULL,'Production Summary','HTML',0)


-- Deliverables Data for RPS

INSERT INTO [Common].[Deliverables]([ClientId],[ReportName],[ReportType],[DeliveryType],[ReportQuery],[Delimeter],[FileNameExpression],[IsSFTUploadRequired],[UploadPath],[EmailChannelId],[ScheduleId])VALUES((select Id from Common.Clients WHERE AppCode='RPS'),'Error Report','Error','Batch','','\t','Error-{Now:yyyyMMdd}.csv',1,'/Reports',(select Id from Common.EmailChannels WHERE Title='RPS Download Reporting'),NULL)

INSERT INTO [Common].[Deliverables]([ClientId],[ReportName],[ReportType],[DeliveryType],[ReportQuery],[Delimeter],[FileNameExpression],[IsSFTUploadRequired],[UploadPath],[EmailChannelId],[ScheduleId])VALUES((select Id from Common.Clients WHERE AppCode='RPS'),'Delivery Report','Delivery','Batch','','\t','Delivery-{Now:yyyyMMdd}.csv',1,'/Reports',(select Id from Common.EmailChannels WHERE Title='RPS Production Reporting'),NULL)



---- INSERT SFTCHANNEL DETAILD FOR PRINT READY CLIENTS

INSERT [Common].[SFTChannels] ([Code], [Description], [Title], [Protocol], [Host], [UserName], [Password], [ToADEDir], [FromADEDir], [ArchiveDir], [PostDownloadAction], [IsActive]) VALUES ( N'PrintReady_Prod', NULL, N'Print ready client Process', N'unc', N'\\Slxqn1vdappsr01.infosolco.net\FAGOSFTP\ADE_PnS\PROD\AM_PRINT_READY', N'user', N'', N'/', NULL, N'', N'Delete', 1)

EXEC usp_MapClientSFTChannels 'CITIVM','PrintReady_Prod'
EXEC usp_MapClientSFTChannels 'RPS','PrintReady_Prod'


---- INSERT AND UPDATE PRINT VENDORS PATHS IN SFTCHANNELS

INSERT [Common].[SFTChannels] ([Code], [Description], [Title], [Protocol], [Host], [UserName], [Password], [ToADEDir], [FromADEDir], [ArchiveDir], [PostDownloadAction], [IsActive]) VALUES ( N'PnSService_Prod', NULL, N'PnS Service Process', N'unc', N'\\slxqn1vdappsr01.infosolco.net\FAGOSFTP\ADE_PnS\PROD', N'user', N'', N'/', N'/', N'', N'Delete', 1)


---- UPDATE PRINT VENDORS SFTCHANNEL

UPDATE Common.PrintVendors
SET
SFTChannelId=(SELECT Id FROM Common.SFTChannels WHERE Code='PCI_Prod')
WHERE Id=(SELECT Id FROM Common.PrintVendors WHERE VENDORCode='PCI')
UPDATE Common.PrintVendors
SET
SFTChannelId=(SELECT Id FROM Common.SFTChannels WHERE Code='CL_DocSol_PnS_DALLAS_Prod')
WHERE Id=(SELECT Id FROM Common.PrintVendors WHERE VENDORCode='CL_DALLAS')


---- ADDED NEW SYSTEM SETTINGS
INSERT [Common].[SystemSettings] ([Id], [DataType], [Value], [Description], [IsSystem], [DefaultValue], [ModifiedBy], [ModifiedOn]) VALUES (N'Core_DirectReportBackChannelCodes', N'String', N'AMC,PrintReady', NULL, 0, N'AMC,PrintReady', (select Id from [Profile].Users where UserName = 'system@milestonetechnoconsulting.com'), GETUTCDATE())
DELETE FROM [Common].[SystemSettings] WHERE Id IN ('Core_DirectReportBackChannelIds','Core_FedExEndPageTemplatePath','Core_USPSEndPageTemplatePath')

---- DELETE EXTRA ENTRIES FROM AM.DocTypeCoverLetters
DELETE FROM AM.DocTypeCoverLetters WHERE DocTypeId IN (select Id from AM.DocTypes WHERE ShippingServiceType<>'IMB')

---- UPDATE Cover Letter for Adverse Action Packages
UPDATE AM.DocTypeCoverLetters 
SET
CoverLetterTemplatePath='USAA_USPS_Generic_FrontBarcode_1.pdf'
WHERE DocTypeId IN (Select Id from AM.DocTypes WHERE ClientId=(select Id from Common.Clients WHERE AppCode='USAA') AND DocTypeCode IN ('AdverseActionPackage','AdverseAction_B1', 'AdverseAction_B2', 'AdverseAction_B3', 'AdverseAction_B4', 'AdverseAction_B5', 'AdverseAction_B6', 'AdverseAction_B7', 'AdverseAction_B8'))

---- INSERT END PAGE In AM.DocTypeCoverLetters

-- USAA
INSERT INTO AM.DocTypeCoverLetters(DocTypeId,SeqId,CoverLetterTemplatePath)
SELECT Id,101,'USAA_FedEx_EndPage_1.pdf' from AM.DocTypes WHERE ClientId=(SELECT ID FROM COMMON.CLIENTS WHERE AppCode='USAA') AND Id IN (select DocTypeId FROM Common.DocTypePrintTrailMaps WHERE PrintTrailId=(SELECT Id FROM Common.PrintTrails WHERE TrailName='USAA-GENERIC-FEDEX'))
INSERT INTO AM.DocTypeCoverLetters(DocTypeId,SeqId,CoverLetterTemplatePath)
SELECT Id,101,'USAA_USPS_EndPage_1.pdf' from AM.DocTypes WHERE ClientId=(SELECT ID FROM COMMON.CLIENTS WHERE AppCode='USAA') AND Id IN (select DocTypeId
FROM Common.DocTypePrintTrailMaps WHERE PrintTrailId=(SELECT Id FROM Common.PrintTrails WHERE TrailName='USAA-GENERIC-PRI'))

-- HSBC
INSERT INTO AM.DocTypeCoverLetters(DocTypeId,SeqId,CoverLetterTemplatePath)
SELECT Id,101,'HSBC_USPS_EndPage_1.pdf' from AM.DocTypes WHERE ClientId=(SELECT ID FROM COMMON.CLIENTS WHERE AppCode='HSBC') AND Id IN (select DocTypeId
FROM Common.DocTypePrintTrailMaps WHERE PrintTrailId=(SELECT Id FROM Common.PrintTrails WHERE TrailName='HSBC-GENERIC'))

-- BOTW
INSERT INTO AM.DocTypeCoverLetters(DocTypeId,SeqId,CoverLetterTemplatePath)
SELECT Id,101,'BOTW_USPS_EndPage_1.pdf' from AM.DocTypes WHERE ClientId=(SELECT id FROM Common.Clients WHERE AppCode='BOTW') AND Id IN (select DocTypeId
FROM Common.DocTypePrintTrailMaps WHERE PrintTrailId=(SELECT Id FROM Common.PrintTrails WHERE TrailName='BOTW-GENERIC'))

-- USAA Adverse Action Print Trail Maps for Dallas
UPDATE  [Common].[DocTypePrintTrailMaps] 
SET
[BizDayStartTime]='00:00:00',
[BizDayEndTime]='15:00:00',
[SatStartTime]='00:00:00',
[SatEndTime]='15:00:00'
WHERE DocTypeId IN (Select Id from AM.DocTypes WHERE ClientId=(select Id from Common.Clients WHERE AppCode='USAA') AND DocTypeCode IN ('AdverseActionPackage','AdverseAction_B1', 'AdverseAction_B2', 'AdverseAction_B3', 'AdverseAction_B4', 'AdverseAction_B5', 'AdverseAction_B6', 'AdverseAction_B7', 'AdverseAction_B8'))

INSERT INTO [Common].[DocTypePrintTrailMaps]([DocTypeId],[PrintTrailId],[BizDayStartTime],[BizDayEndTime],[SatStartTime],[SatEndTime],[SunStartTime],[SunEndTime])
SELECT Id,(SELECT Id FROM Common.PrintTrails WHERE TrailName='USAA-GENERIC'),'20:00:01','23:59:59','20:00:01','23:59:59',NULL,NULL from AM.DocTypes WHERE ClientId=(select Id from Common.Clients
WHERE AppCode='USAA') AND DocTypeCode IN ('AdverseActionPackage','AdverseAction_B1', 'AdverseAction_B2', 'AdverseAction_B3', 'AdverseAction_B4', 'AdverseAction_B5', 'AdverseAction_B6', 'AdverseAction_B7', 'AdverseAction_B8')

INSERT INTO [Common].[DocTypePrintTrailMaps]([DocTypeId],[PrintTrailId],[BizDayStartTime],[BizDayEndTime],[SatStartTime],[SatEndTime],[SunStartTime],[SunEndTime])
SELECT Id,(SELECT Id FROM Common.PrintTrails WHERE TrailName='USAA-GENSAME'),'15:00:01','20:00:00','15:00:01','20:00:00',NULL,NULL from AM.DocTypes WHERE ClientId=(select Id from Common.Clients
WHERE AppCode='USAA') AND DocTypeCode IN ('AdverseActionPackage','AdverseAction_B1', 'AdverseAction_B2', 'AdverseAction_B3', 'AdverseAction_B4', 'AdverseAction_B5', 'AdverseAction_B6', 'AdverseAction_B7', 'AdverseAction_B8')

UPDATE Common.PrintTrails
SET
PrintVendorId=(SELECT Id FROM Common.PrintVendors WHERE VendorCode='CL_DALLAS')
WHERE TrailName='USAA-GENSAME'

UPDATE Common.Clients SET IsAMActive=1,IsPnSActive=1 WHERE Id in ((SELECT Id FROM Common.Clients WHERE AppCode='CITIVM'),(SELECT Id FROM Common.Clients WHERE AppCode='RPS'))

------------------------------------------------------------------------------
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
ELSE 
begin
PRINT 'The database update failed'
end
GO
DROP TABLE #tmpErrors
GO