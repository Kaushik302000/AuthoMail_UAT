-- =============================================
-- Author: Harika Bommareddy
-- Create date:  03-Dec-25
--EXEC CheckDuplicateMailPiece '2025-11-21', '2025-11-27',194,'2_7587770_C1.pdf','2_7587770','KERI ANN MEAGHER',NULL, 'abc','xyz','MS','38655','CSS_VSGCKS'
-- Description:	 Check if the mail piece is duplicate
-- =============================================

CREATE PROCEDURE CheckDuplicateMailPiece
(
	@StartDate DATETIME,
	@EndDate DATETIME,
	@ClientId INT,
	@FileName VARCHAR(255),
	@BusinessFriendlyIdValue VARCHAR(50),
	@RcptFirstName VARCHAR(100),
	@RcptLastName VARCHAR(100),
	@RcptAddressStreet VARCHAR(255),
	@RcptAddressCity VARCHAR(100),
	@RcptAddressState VARCHAR(100),
	@RcptAddressZIP VARCHAR(15),
	@DocumentOrPackageType VARCHAR(100)
)
AS
BEGIN
	SELECT CAST(
	    CASE 
	    WHEN (
	    SELECT COUNT(*) 
	    FROM [AM].[MailPieces]  
	    WHERE EffectiveReceivedOnEst >= @StartDate
	      AND EffectiveReceivedOnEst < @EndDate
	      AND ClientId = @ClientId
	      AND FileName = @FileName
	      AND BusinessFriendlyIdValue = @BusinessFriendlyIdValue
	      AND ShippingStatus NOT IN ('Error','PulledBack')
	      AND ISNULL(RcptFirstName,'') = ISNULL(@RcptFirstName,'')
	      AND ISNULL(RcptLastName,'') = ISNULL(@RcptLastName,'')
	      AND ISNULL(RcptAddressStreet,'') = ISNULL(@RcptAddressStreet,'')
	      AND ISNULL(RcptAddressCity,'') = ISNULL(@RcptAddressCity,'')
	      AND ISNULL(RcptAddressState,'') = ISNULL(@RcptAddressState,'')
	      AND ISNULL(RcptAddressZIP,'') = ISNULL(@RcptAddressZIP,'')
	      AND DocumentOrPackageType = @DocumentOrPackageType
	        ) > 1 
	        THEN 1
	        ELSE 0
	    END AS BIT
	)
END