CREATE PROCEDURE [dbo].[usp_UpdateTrackingInfo]
AS
BEGIN
SET NOCOUNT ON

	--First Update Inward Tracking
	UPDATE MP
	SET
	MP.DeliveryStatus = TD.DeliveryStatus,
	MP.ShipmentStatus = TD.ShipmentStatus,
	MP.ShipmentStatusUpdatedOn = TD.ShipmentStatusUpdatedOn,
	MP.DeliveryDate = TD.DeliveryDate
	FROM
	AM.MailPieces MP
	JOIN
	AM.[TrackingDump] TD ON TD.MailPieceId = MP.Id
	WHERE TD.IsReturnTracking=0

	--Remove Updated Data
	DELETE FROM AM.[TrackingDump] WHERE IsReturnTracking=0

	--Then Update Return Tracking
	UPDATE MP
	SET
	MP.ReturnDeliveryStatus = TD.DeliveryStatus,
	MP.ReturnDeliveryDate = TD.DeliveryDate
	FROM
	AM.MailPieces MP
	JOIN
	AM.[TrackingDump] TD ON TD.MailPieceId = MP.Id
	WHERE TD.IsReturnTracking=1

	--Remove Updated Data
	DELETE FROM AM.[TrackingDump] WHERE IsReturnTracking=1

END
GO