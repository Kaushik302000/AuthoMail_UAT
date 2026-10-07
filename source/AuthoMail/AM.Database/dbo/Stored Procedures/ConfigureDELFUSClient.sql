-- =============================================
-- Author:		Sagar Golvelkar
-- Create date: 07/17/2020
-- Description:	Configuring DELFUS External client
-- =============================================
CREATE PROCEDURE [dbo].[ConfigureDELFUSClient]
@CustomerId VARCHAR(255)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	DECLARE @newClientId INT, @ClientCode VARCHAR(255)=N'DELFUS_'+@CustomerId

	DECLARE @AppEnvironment VARCHAR(255), @ChannelCode VARCHAR(255)

	SET @AppEnvironment=(SELECT [Value] FROM Common.SystemSettings WHERE Id='Common_Environment')
	SET @ChannelCode='DELFUS_'+@AppEnvironment

    INSERT [Common].[Clients] ([Name], [AppCode], [BusinessUnit], [IsAMActive], [IsPnSActive], [TimeZone], [ModifiedOn], [CreatedOn], [CreatedBy], [ModifiedBy], [SigningRoomExpiryDays]) 
	VALUES 
	(@ClientCode, @ClientCode, 1, 0, 0, N'Eastern Standard Time', GETUTCDATE(),GETUTCDATE(), (select Id FROM [Profile].[Users] where UserName='system@milestonetechnoconsulting.com'),  (select Id FROM [Profile].[Users] where UserName='system@milestonetechnoconsulting.com'),0)

	SET @newClientId=SCOPE_IDENTITY()

	INSERT INTO Common.Deliverables (ClientId, ReportName, ReportType, DeliveryType, ReportQuery, Delimeter, FileNameExpression, IsSFTUploadRequired, UploadPath, EmailChannelId, ScheduleId) 
	VALUES (@newClientId, 'Error Report', 'Error', 'Batch', '', NULL, 'Error-{Now:yyyyMMdd}.XML', 1, '/Reports', NULL, NULL)
	
	INSERT INTO Common.Deliverables (ClientId, ReportName, ReportType, DeliveryType, ReportQuery, Delimeter, FileNameExpression, IsSFTUploadRequired, UploadPath, EmailChannelId, ScheduleId) 
	VALUES (@newClientId, 'Delivery Report', 'Delivery', 'Batch', '', NULL, 'Delivery-{Now:yyyyMMdd}.XML', 1, '/Reports', NULL, NULL)

	EXEC usp_MapClientSFTChannels @ClientCode,@ChannelCode

	INSERT INTO AM.DocTypes (DocTypeName, IsBRE,  ClientId, QCPercent, IsReturnInfoSpecified, RetRcptAddressStreet, RetRcptAddressCity, RetRcptAddressState, RetRcptAddressZIP, IsActive, ModifiedOn, CreatedOn, CreatedBy, ModifiedBy, NoOfCopies, PnSConfigId)
	SELECT DocTypeName, IsBRE,  @newClientId, QCPercent, IsReturnInfoSpecified, RetRcptAddressStreet, RetRcptAddressCity, RetRcptAddressState, RetRcptAddressZIP, IsActive, ModifiedOn, CreatedOn, CreatedBy, ModifiedBy, NoOfCopies, PnSConfigId FROM AM.DocTypes WHERE ClientId=(SELECT Id FROM Common.Clients WHERE AppCode='DELFUS')

	INSERT INTO AM.DocTypeInserts(DocTypeId,BREEnvelopeId)
	SELECT DocTypeId,BREEnvelopeId FROM AM.DocTypeInserts WHERE DocTypeId IN (SELECT Id FROM AM.DocTypes WHERE ClientId=(SELECT Id FROM Common.Clients WHERE AppCode='DELFUS'))

	INSERT INTO [Common].[PnSSweepTimings]([BundleSeqNo],[AppCode],[VendorCode],[WeekDayType],[SweepType],[SweepTime])
	SELECT [BundleSeqNo],@ClientCode,[VendorCode],[WeekDayType],[SweepType],[SweepTime] FROM [Common].[PnSSweepTimings] WHERE AppCode='DELFUS'
     
	UPDATE [Common].[Clients]
	SET
	[IsAMActive]=1, [IsPnSActive]=1
	WHERE Id= @newClientId

END