CREATE VIEW [dbo].[vDVExp]
AS
SELECT
m.Id AS PkgId
,MailBatchId
,bs.PrintBatch AS PnSBatchSummaryId
,mrq.PrintTrailBatchId
,m.UniqueId AS PkgUniqueId
,m.BusinessFriendlyIdValue AS BFId
,c.AppCode AS ClientCode
,sftc.Code AS ChannelCode
,d.DocTypeName
,m.TrackingNo
,d.PrintFormat
,PCD.PrintSize
,PCD.EnvelopeId AS EnvPartNo
,IsBRE
,'' AS BREEnvPartNo
,PCD.ShippingCarrier
,PCD.ShippingServiceType
,PCD.ShippingSpecialService
,PCD.BizdayCutOff
,PCD.SatCutOff
,QCPercent
,ShippingStatus
,TotalPages
,m.TotalSheets
,mrq.TotalSheets AS PnSTotalSheets
,ErrorCode AS ErrCode
,IsPullBackRequested
,IsPulledBack
,IsQCPerformed
,IsQCRequested
,m.PrintLocation
,TrailName
,RcptAddressCity
,RcptAddressState
,RcptAddressZIP
,RcptAddressTypes
,b.BatchStatus
,dbo.ufn_UTCToEST(b.ReceivedOn) AS ReceivedOnEst
,EffectiveReceivedOnEst
,dbo.ufn_UTCToEST(m.DueDate) AS DueDateEst
,dbo.ufn_UTCToEST(bs.SubmittedOn) AS SubmittedOnEst
,dbo.ufn_UTCToEST(ShippedOn) AS ShippedOnEst
,dbo.ufn_UTCToEST(b.ReportedBackOn) AS ReportedBackOnEst
,m.SLADays AS SLABuffer
,MissedSLAReason
FROM AM.MailPieces m
INNER JOIN AM.MailBatches b ON m.MailBatchId = b.Id
INNER JOIN Common.Clients c ON b.ClientId = c.Id
INNER JOIN Common.SFTChannels sftc ON b.SFTChannelId = sftc.Id
LEFT OUTER JOIN [AM].[DocTypes] d ON m.DocTypeId=d.Id 
LEFT OUTER JOIN Common.PnSConfigDetail PCD ON m.PnSConfigDetailId = PCD.Id
LEFT OUTER JOIN Common.PrintTrails pt ON m.PrintTrailId = pt.Id
LEFT OUTER JOIN PnS.MRDFRequest mrq ON m.UniqueId = mrq.UniqueId
LEFT OUTER JOIN PnS.BatchSummary bs ON mrq.JobId = bs.PrintBatch

