CREATE VIEW [dbo].[view_USPSData]
AS
SELECT MP.Id, MP.UniqueId, C.AppCode, MB.PipelineStatus, MP.ShippingStatus, D.DocTypeName AS DocTypeCode
, PCD.ShippingCarrier, PCD.ShippingServiceType, MP.ClientId, MP.PrintLocation, MB.BatchName
, MP.BusinessFriendlyIdValue AS LoanNumber, MP.RcptFirstName, MP.RcptLastName, MP.RcptAddressStreet
, MP.RcptAddressAptOrSuiteOrOther,MP.RcptAddressCity, MP.RcptAddressState, MP.RcptAddressZIP, MP.TrackingNo
, MB.ReceivedOn, MP.DocumentOrPackageType, MP.IsPickedUp, MP.PickedUpOn, MP.DueDate, MP.ShippedOn
, MP.EffectiveReceivedOnEst,MP.ShipmentStatus,MP.ShipmentStatusUpdatedOn
, MP.DeliveryDate, MP.DeliveryStatus, MP.ReturnTrackingNo, MP.ReturnDeliveryStatus, MP.ReturnDeliveryDate
FROM AM.MailPieces AS MP 
INNER JOIN AM.MailBatches AS MB ON MB.Id = MP.MailBatchId 
INNER JOIN [AM].[DocTypes] d ON MP.DocTypeId=d.Id 
INNER JOIN Common.PnSConfigDetail PCD ON MP.PnSConfigDetailId = PCD.Id
INNER JOIN Common.Clients AS C ON MB.ClientId = C.Id
WHERE (MP.ShippingStatus IN ('InProgress', 'Shipped')) AND (PCD.ShippingCarrier = 'USPS' AND PCD.ShippingServiceType IN ('PRIORITY','PRIORITY_EXPRESS')) AND (MB.ReceivedOn >= '2020-09-01 05:00:00') AND TrackingNo IS NOT NULL
GO

