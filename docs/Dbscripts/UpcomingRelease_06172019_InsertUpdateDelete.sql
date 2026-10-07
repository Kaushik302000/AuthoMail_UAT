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
------------------------------------------------------------------------------
PRINT 'Inserted 3 New Keys in SystemSettings table'
-- UAT Start
INSERT [Common].[SystemSettings] ([Id], [DataType], [Value], [Description], [IsSystem], [DefaultValue], [ModifiedBy], [ModifiedOn]) VALUES (N'Common_ErrorSubscribedEmailSubject', N'String', 'Error Notification For Batch - {0}', 'email subject for error notification', 0, 'Error Notification For Batch - {0}', (select Id from [Profile].Users where UserName = 'system@milestonetechnoconsulting.com'), GETUTCDATE())
INSERT [Common].[SystemSettings] ([Id], [DataType], [Value], [Description], [IsSystem], [DefaultValue], [ModifiedBy], [ModifiedOn]) VALUES (N'Common_ErrorSubscribedEmailCc', N'String', 'avala@corelogic.com,sgolvelkar@corelogic.com', 'cc emails for internal team apart from subscribed email list', 0, 'avala@corelogic.com,sgolvelkar@corelogic.com', (select Id from [Profile].Users where UserName = 'system@milestonetechnoconsulting.com'), GETUTCDATE())
INSERT [Common].[SystemSettings] ([Id], [DataType], [Value], [Description], [IsSystem], [DefaultValue], [ModifiedBy], [ModifiedOn]) VALUES (N'Common_ErrorSubscribedEmailTemplate', N'String', 'HTMLTemplates\ErrorSubScribed.html', 'template for error notification', 0, 'HTMLTemplates\ErrorSubScribed.html', (select Id from [Profile].Users where UserName = 'system@milestonetechnoconsulting.com'), GETUTCDATE())
INSERT [Common].[SystemSettings] ([Id], [DataType], [Value], [Description], [IsSystem], [DefaultValue], [ModifiedBy], [ModifiedOn]) VALUES (N'Common_InventoryBufferStockDays', N'Int32', '6', 'Inventory Buffer Stock Days', 0, '6', (select Id from [Profile].Users where UserName = 'system@milestonetechnoconsulting.com'), GETUTCDATE())
if not exists (select * from Common.SystemSettings where id='Web_Login_PasswordExpirationIsEnable')
begin
INSERT INTO [Common].[SystemSettings] ([Id] ,[DataType] ,[Value] ,[Description] ,[IsSystem] ,[DefaultValue] ,[ModifiedBy] ,[ModifiedOn])     VALUES ('Web_Login_PasswordExpirationIsEnable','Boolean','true','Is password expired policy enable?',0,'true',(select Id from Profile.Users where UserName='system@milestonetechnoconsulting.com'),GETUTCDATE())
print 'insert Web_Login_PasswordExpirationIsEnable'
end
if not exists (select * from Common.SystemSettings where id='Web_Login_PasswordExpirationDays')
begin
INSERT INTO [Common].[SystemSettings] ([Id] ,[DataType] ,[Value] ,[Description] ,[IsSystem] ,[DefaultValue] ,[ModifiedBy] ,[ModifiedOn])     VALUES ('Web_Login_PasswordExpirationDays','Int32',50,'No of password expired days.',0,50,(select Id from Profile.Users where UserName='system@milestonetechnoconsulting.com'),GETUTCDATE())
print 'insert Web_Login_PasswordExpirationDays'
end
-- UAT End
-- UAT Start
PRINT 'Insert Values in AM.ClientDocTypes table based on AM.DocTypes TABLE'
INSERT INTO [AM].[ClientDocTypes] (ClientId,DocTypeName)
SELECT DISTINCT ClientId, Description
FROM AM.DocTypes WHERE Description NOT LIKE '%-ExpressMailing' AND Description NOT LIKE '%-Priority' AND Description NOT LIKE '%-FedEx' AND Description NOT LIKE '%-IMB' AND Description NOT LIKE '%_FedEx'

PRINT 'Update Values in AM.DocTypes table based on AM.ClientDocTypes TABLE'
;with cte as
(
	SELECT DISTINCT Id,ClientId, Description as DocType
	FROM AM.DocTypes WHERE Description NOT LIKE '%-ExpressMailing' AND Description NOT LIKE '%-Priority' AND Description NOT LIKE '%-FedEx' AND Description NOT LIKE '%-IMB' AND Description NOT LIKE '%_FedEx'
), cte2 as
(
	SELECT DISTINCT Id,ClientId, Description as DocType
	FROM AM.DocTypes WHERE Description NOT LIKE '%-ExpressMailing' AND Description NOT LIKE '%-Priority' AND Description NOT LIKE '%-FedEx' AND Description NOT LIKE '%-IMB' AND Description NOT LIKE '%_FedEx'
)
UPDATE D
SET
ClientDocTypeId= CDT.Id
FROM AM.DocTypes D 
INNER JOIN cte ON D.Id=cte.Id 
INNER JOIN AM.ClientDocTypes CDT ON cte.ClientId=CDT.ClientId AND cte.DocType=CDT.DocTypeName
-- UAT End

UPDATE AM.DocTypes
SET
IsFallBack=0

PRINT 'Update Fallback values for USAA packages'
UPDATE AM.DocTypes
SET
IsFallBack=1
WHERE ShippingServiceType IN ('PRIORITY_EXPRESS','PRIORITY') AND ClientId <> (SELECT Id FROM Common.Clients WHERE AppCode='BOTW')


UPDATE AM.DocTypes
SET
EnvelopeId=(SELECT Id FROM Common.Envelopes WHERE EnvelopeName=EnvelopeName1),
BREEnvelopeId=(SELECT Id FROM Common.Envelopes WHERE EnvelopeName=BREEnvelopeName1)
GO

UPDATE [PnS].[EnvelopeUsage]
SET
EnvelopeId=(SELECT Id FROM Common.Envelopes WHERE EnvelopeName=EnvelopeName1)
GO

--REASON TO ADD : We are updating new values based on old values. So after updating those values we don't require below fields
--ALTER TABLE AM.DocTypes DROP COLUMN DocTypeCode,Description,EnvelopeName1,BREEnvelopeName1

--ALTER TABLE [PnS].[EnvelopeUsage] DROP COLUMN EnvelopeName1

--ALTER TABLE [Common].[EnvelopeInventory] DROP COLUMN EnvelopeName1



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