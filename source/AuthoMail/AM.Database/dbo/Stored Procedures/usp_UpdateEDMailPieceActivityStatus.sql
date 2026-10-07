-- =============================================
-- Author:		Akshar vala
-- Create date: 21 oct 2019
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_UpdateEDMailPieceActivityStatus]
	-- Add the parameters for the stored procedure here
	@Id int,
	@IsESignRequested bit,
	@ConsentAcceptedOn datetime,
	@ConsentDeclinedOn datetime,
	@DocReviewedOn datetime,
	@DocEsignedOn datetime,
	@DocDownloadedOn datetime,
	@ExpiryDate Datetime,
	@IpAddress varchar(50)
AS
BEGIN
	DECLARE @IsConsentAccepted bit
	DECLARE @IsConsentDeclined bit
	DECLARE @IsDocDownloaded bit
	DECLARE @IsDocEsigned bit


	UPDATE [AM].[EDeliveryMailPieces] 
	SET [ConsentAcceptedOn] = @ConsentAcceptedOn, [ConsentDeclinedOn] = @ConsentDeclinedOn, [DocReviewedOn] = @DocReviewedOn, [ESignedOn] = @DocEsignedOn, [DocDownloadedOn] = @DocDownloadedOn, DownloadedIPAddress = @IpAddress WHERE Id=@Id

	SELECT @IsConsentAccepted = IsConsentAccepted, @IsConsentDeclined = IsConsentDeclined, @IsDocEsigned = IsESigned, @IsDocDownloaded = [IsDocDownloaded], @ConsentAcceptedOn = ConsentAcceptedOn, @DocEsignedOn = ESignedOn, @DocDownloadedOn = DocDownloadedOn FROM [AM].[EDeliveryMailPieces] WHERE Id = @id

	IF(@IsConsentDeclined=1)
	BEGIN
		UPDATE [AM].[EDeliveryMailPieces] SET PipelineStatus = 'Declined' WHERE Id=@Id
	END

	IF(@IsESignRequested = 0 AND @IsConsentAccepted = 1 AND @IsDocDownloaded = 1 AND @ConsentAcceptedOn <= @ExpiryDate AND @DocDownloadedOn <= @ExpiryDate)
	BEGIN
		UPDATE [AM].[EDeliveryMailPieces] SET PipelineStatus = 'eDelivered', eDeliveredOn = @DocDownloadedOn WHERE Id=@Id
	END

	IF(@IsESignRequested = 1 AND @IsConsentAccepted = 1 AND @IsDocEsigned = 1 AND @ConsentAcceptedOn <= @ExpiryDate AND @DocEsignedOn <= @ExpiryDate)
	BEGIN
		UPDATE [AM].[EDeliveryMailPieces] SET PipelineStatus = 'eSigned' WHERE Id=@Id
	END

	IF(GETUTCDATE() > @ExpiryDate)
	BEGIN
		IF(@IsConsentAccepted = 0 OR @IsDocDownloaded = 0 OR (@IsDocEsigned = 0 AND @IsESignRequested = 1))
		BEGIN
			UPDATE [AM].[EDeliveryMailPieces] SET PipelineStatus = 'Expired', ErrorCode = 'Expired', ErrorDesc = 'SigningRoom is Expired.' WHERE Id=@Id
		END
	END

END
