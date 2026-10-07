CREATE PROCEDURE [dbo].[usp_InsertErrorNotificationData]
@mailBatchId int
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @errorMPs INT,@errorMPsInNotify INT,@errorNotifyId INT,@totalMPsCount INT
	
	SET @totalMPsCount=(SELECT COUNT(*) FROM AM.MailPieces WHERE MailBatchId=@mailBatchId)

	SELECT @errorMPs=COUNT(Id) FROM AM.MailPieces WHERE MailBatchId=@mailBatchId AND ShippingStatus='Error'
	IF(@errorMPs>0)
	BEGIN
		IF EXISTS(SELECT Id FROM AM.ErrorNotifications WHERE MailBatchId=@mailBatchId)
		BEGIN
			SET @errorMPsInNotify=(SELECT ErrorMPsCount FROM AM.ErrorNotifications WHERE MailBatchId=@mailBatchId)
			SET @errorNotifyId=(SELECT Id FROM AM.ErrorNotifications WHERE MailBatchId=@mailBatchId)
			IF(@errorMPs > @errorMPsInNotify)
			BEGIN
				DELETE FROM AM.ErrorNotifications WHERE Id=@errorNotifyId

				INSERT INTO AM.ErrorNotifications(ClientId, MailBatchId,TotalMPsCount, ErrorMPsCount, ReceivedOn, CreatedOn)
				SELECT ClientId,Id,@totalMPsCount,@errorMPs,ReceivedOn,GETUTCDATE() FROM AM.MailBatches WHERE Id=@mailBatchId
			END
		END
		ELSE
		BEGIN
			INSERT INTO AM.ErrorNotifications(ClientId, MailBatchId,TotalMPsCount, ErrorMPsCount, ReceivedOn, CreatedOn)
			SELECT ClientId,Id,@totalMPsCount,@errorMPs,ReceivedOn,GETUTCDATE() FROM AM.MailBatches WHERE Id=@mailBatchId
		END
	END
END
