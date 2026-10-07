




CREATE VIEW [dbo].[view_MailPiecesInformation]
AS
SELECT MP.Id,MB.BatchName,C.AppCode, MB.PipelineStatus as MailBatchPipelineStatus, MP.BusinessFriendlyIdValue AS LoanNumber, MP.UniqueId
, MP.ShippingStatus, MP.TrackingNo, MP.DocumentOrPackageType, D.DocTypeName AS DocTypeCode, PCD.ShippingCarrier, PCD.ShippingServiceType,D.IsBRE
, [dbo].[ufn_UTCToEST](MB.ReceivedOn) AS ReceivedOnEst , [dbo].[ufn_UTCToEST](MP.DueDate) AS DueDateEst, [dbo].[ufn_UTCToEST](MP.ShippedOn) AS ShippedOnEst
, MP.EffectiveReceivedOnEst, MP.ClientId
, MP.PrintLocation,  MP.RcptFirstName, MP.RcptLastName, MP.RcptAddressStreet
, MP.RcptAddressAptOrSuiteOrOther, MP.RcptAddressCity, MP.RcptAddressState, MP.RcptAddressZIP
, [dbo].[ufn_UTCToEST](MP.PickedUpOn) AS PickedUpOnEst,MP.SLADays
FROM  AM.MailPieces MP 
INNER JOIN AM.MailBatches AS MB ON MB.Id = MP.MailBatchId 
LEFT OUTER JOIN AM.DocTypes AS D ON D.Id = MP.DocTypeId
LEFT OUTER JOIN Common.PnSConfigDetail AS PCD ON MP.PnSConfigDetailId=PCD.Id
LEFT OUTER JOIN Common.Clients AS C ON MB.ClientId = C.Id