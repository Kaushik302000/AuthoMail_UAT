

CREATE view [dbo].[vPrintReadyLegacy]
as
select
(mrq.JobId * 10000 + mrq.PieceId) as PkgId
,(bs.PrintBatch * 10000) as MailBatchId
,mrq.UniqueId as PkgUniqueId
,null as BFId
,c.AppCode as ClientCode
,'CL_PRINT_READY' as ChannelCode
,D.DocTypeName AS DocTypeCode
,D.PrintFormat
,PrintSize
,EnvelopeId as EnvPartNo
,IsBRE
,'' as BREEnvPartNo
,ShippingCarrier
,ShippingServiceType
,ShippingSpecialService
,BizdayCutOff
,SatCutOff
,QCPercent
,MailPieceStatus as ShippingStatus
,mrq.TotalSheets as TotalPages
,mrq.TotalSheets
,ErrId as ErrCode
,0 as IsPullBackRequested
,0 as IsPulledBack
,0 as IsQCPerformed
,0 as IsQCRequested
,bs.PrintLocation
,TrailName
,null as RcptAddressCity
,null as RcptAddressState
,null as RcptAddressZIP
,null as RcptAddressTypes
,bs.BatchStatus
,dbo.ufn_UTCToEST(bs.ReceivedOn) as ReceivedOnEst
,dbo.ufn_UTCToEST(bs.ReceivedOn) as EffectiveReceivedOnEst
,bs.DueDate as DueDateEst
,dbo.ufn_UTCToEST(bs.SubmittedOn) as SubmittedOnEst
,dbo.ufn_UTCToEST(mrs.ProcessingTimeStamp) as ShippedOnEst
,dbo.ufn_UTCToEST(bs.ReportedBackOn) as ReportedBackOnEst
,0 as SLABuffer
,'NA' as MissedSLAReason

from PnS.MRDFRequest mrq
inner join PnS.BatchSummary bs on mrq.JobId = bs.PrintBatch
inner join Common.Clients c on bs.ClientId = c.Id
inner join PnS.MRDFResponseHistory mrs on mrq.JobId = mrs.JobId and mrq.PieceId = mrs.PieceId
left outer join AM.DocTypes d on bs.DocTypeId = d.Id
left outer join Common.PnSConfigDetail pcd on bs.PnSConfigDetailId = pcd.Id
left outer join Common.PrintTrails pt on bs.PrintTrailId = pt.Id

where mrq.PDFFileName = mrs.PDFFileName and bs.BatchIdentifier not in (select UniqueId from AM.PrintBatches)

union

select
(mrq.JobId * 10000 + mrq.PieceId) as PkgId
,(bs.PrintBatch * 10000) as MailBatchId
,mrq.UniqueId as PkgUniqueId
,null as BFId
,c.AppCode as ClientCode
,'CL_PRINT_READY' as ChannelCode
,D.DocTypeName AS DocTypeCode
,D.PrintFormat
,PrintSize
,EnvelopeId as EnvPartNo
,IsBRE
,'' as BREEnvPartNo
,ShippingCarrier
,ShippingServiceType
,ShippingSpecialService
,BizdayCutOff
,SatCutOff
,QCPercent
,MailPieceStatus as ShippingStatus
,mrq.TotalSheets as TotalPages
,mrq.TotalSheets
,ErrId as ErrCode
,0 as IsPullBackRequested
,0 as IsPulledBack
,0 as IsQCPerformed
,0 as IsQCRequested
,bs.PrintLocation
,TrailName
,null as RcptAddressCity
,null as RcptAddressState
,null as RcptAddressZIP
,null as RcptAddressTypes
,bs.BatchStatus
,dbo.ufn_UTCToEST(bs.ReceivedOn) as ReceivedOnEst
,dbo.ufn_UTCToEST(bs.ReceivedOn) as EffectiveReceivedOnEst
,bs.DueDate as DueDateEst
,dbo.ufn_UTCToEST(bs.SubmittedOn) as SubmittedOnEst
,dbo.ufn_UTCToEST(mrs.ProcessingTimeStamp) as ShippedOnEst
,dbo.ufn_UTCToEST(bs.ReportedBackOn) as ReportedBackOnEst
,0 as SLABuffer
,'NA' as MissedSLAReason

from PnS.MRDFRequest mrq
inner join PnS.BatchSummary bs on mrq.JobId = bs.PrintBatch
inner join Common.Clients c on bs.ClientId = c.Id
inner join PnS.MRDFResponse mrs on mrq.PrintTrailBatchId = mrs.JobId and mrq.PieceId = mrs.PieceId
left outer join AM.DocTypes d on bs.DocTypeId = d.Id
left outer join Common.PnSConfigDetail pcd on bs.PnSConfigDetailId = pcd.Id
left outer join Common.PrintTrails pt on bs.PrintTrailId = pt.Id

where mrq.PDFFileName = mrs.PDFFileName and bs.BatchIdentifier not in (select UniqueId from AM.PrintBatches)