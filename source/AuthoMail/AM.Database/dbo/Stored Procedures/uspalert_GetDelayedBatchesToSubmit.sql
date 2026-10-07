CREATE PROCEDURE [dbo].[uspalert_GetDelayedBatchesToSubmit] 	
	@cutOffMinutes int
AS
BEGIN
	SELECT mp.Id,mp.BusinessFriendlyIdValue AS LoanNumber,mp.TrackingNo,mb.Id AS MailBatchId,rq.UniqueId,bs.IsSubmitted,c.AppCode,
	mb.BatchName,mb.ReceivedOn,mb.PipelineStatus,mp.DueDate, mp.ShippingStatus,d.DocTypeName AS DocTypeCode,PCD.ShippingCarrier,PCD.ShippingServiceType,mp.ClientId,mp.PrintLocation,pv.VendorCode, rq.MailPieceStatus,mp.IsEDeliveryRequested,mp.IsESignRequested,emp.IsInvitationSent,mp.IsFallBackRequested,pt.TrailName AS PrintTrail  
	FROM AM.MailPieces mp
	INNER JOIN AM.MailBatches mb ON mp.MailBatchId=mb.Id
	LEFT OUTER JOIN AM.EDeliveryMailPieces emp ON mp.UniqueId = emp.UniqueId
	LEFT OUTER JOIN Common.PrintTrails pt ON mp.PrintTrailId = pt.Id
	LEFT OUTER JOIN Common.PrintVendors pv ON pt.PrintVendorId = pv.Id
	LEFT OUTER JOIN Common.Clients c ON mb.ClientId=c.Id
	LEFT OUTER JOIN PnS.MRDFRequest rq ON mp.UniqueId=rq.UniqueId
	LEFT OUTER JOIN PnS.BatchSummary bs ON rq.JobId=bs.PrintBatch
	LEFT OUTER JOIN [AM].[DocTypes] d ON mp.DocTypeId=d.Id 
	LEFT OUTER JOIN Common.PnSConfigDetail PCD ON mp.PnSConfigDetailId = PCD.Id
	WHERE mb.ReceivedOn BETWEEN DATEADD(day,-7,GETUTCDATE()) AND DATEADD(minute,-@cutOffMinutes,GETUTCDATE())
	AND mb.PipelineStatus<>'Held'
	AND 
	(
		mp.ShippingStatus IS NULL 
		OR
		(
			mp.ShippingStatus IN ('InProgress','Shipped') 
			AND 
			(
				rq.UniqueId IS NULL OR ISNULL(bs.IsSubmitted,0)=0
			)
		)
		OR
		(
			mp.ShippingStatus = 'Error' AND mp.ErrorDesc IS NULL
		)
		OR
		(
			mp.ShippingStatus = 'PulledBack' 
			AND
			NOT EXISTS 
			(
				SELECT Id FROM AM.PullBackRequests WHERE MailPieceId = mp.Id
			)
		)
		OR
		(
			mb.ClientId IS NULL
		)
		OR
		(
			(mp.IsEDeliveryRequested = 1 OR mp.IsESignRequested = 1)
			AND emp.IsInvitationSent = 0 
			AND emp.PipelineStatus <> 'ReportedBack'
		)
	)
	OR
	(
		rq.MailPieceStatus='Shipped' 
		AND mp.IsPickedUp <> 1 
		AND mp.DueDate < GETUTCDATE() 
		AND PCD.ShippingCarrier='FedEx' 
		AND mb.ReceivedOn>=DATEADD(DAY,-30,GETUTCDATE())
	)
END
    