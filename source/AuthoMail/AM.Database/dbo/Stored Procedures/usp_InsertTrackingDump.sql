CREATE PROCEDURE [dbo].[usp_InsertTrackingDump]
	@TrackingDump AS [dbo].udt_TrackingDump READONLY
AS
BEGIN

	SET NOCOUNT ON

	INSERT INTO AM.TrackingDump(IsReturnTracking,TrackingNo,ShippingCarrier,ShipmentStatus,ShipmentStatusUpdatedOn,DeliveryDate,DeliveryStatus,MailPieceId)
	SELECT IsReturnTracking,TrackingNo,ShippingCarrier,ShipmentStatus,ShipmentStatusUpdatedOn,DeliveryDate,DeliveryStatus
	,CASE WHEN IsReturnTracking=0 THEN (SELECT TOP 1 Id FROM [AM].[MailPieces] P1 WITH(NOLOCK) WHERE P1.[TrackingNo] = TD.TrackingNo ORDER BY EffectiveReceivedOnEst DESC) ELSE (SELECT TOP 1 Id FROM [AM].[MailPieces] P2 WITH(NOLOCK) WHERE P2.[ReturnTrackingNo] = TD.TrackingNo ORDER BY EffectiveReceivedOnEst DESC) END
	FROM @TrackingDump TD

	SELECT @@ROWCOUNT

END
GO