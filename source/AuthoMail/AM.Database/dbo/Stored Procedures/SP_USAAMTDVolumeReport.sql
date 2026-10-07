CREATE PROCEDURE [dbo].[SP_USAAMTDVolumeReport]
	@startDate datetime,
	@endDate datetime
AS
	select m.BusinessFriendlyIdValue as LoanNumber
	,isnull(m.ShippingStatus,'') as [Status]
	,CONCAT(m.RcptLastName, ' ', m.RcptFirstName) as [Borrower Name]
	,isnull(m.RcptAddressCity,'') as City
	,isnull(m.TrackingNo,'') as TrackingNo
	,CAST((CAST(b.ReceivedOn AS datetime2) AT TIME ZONE 'UTC') AT TIME ZONE 'Eastern Standard Time' AS datetime) as [ReceivedOn (EST)]
	,CAST((CAST(m.DueDate AS datetime2) AT TIME ZONE 'UTC') AT TIME ZONE 'Eastern Standard Time' AS datetime) as [Due (EST)]
	,CAST((CAST(m.ShippedOn AS datetime2) AT TIME ZONE 'UTC') AT TIME ZONE 'Eastern Standard Time' AS datetime) as [Shipped (EST)] 
	,isnull(m.DocumentOrPackageType,'') as DocumentType
	,isnull(ch.Title,'') as Channel
	,CONCAT(cd.ShippingCarrier,' / ',cd.ShippingServiceType) as [Shipping Method]

from AM.MailPieces m
inner join AM.MailBatches b on m.MailBatchId=b.Id
inner join Common.SFTChannels ch on b.SFTChannelId=ch.Id
inner join Common.PnSConfigDetail cd on m.PnSConfigDetailId=cd.Id
where m.ClientId = (select Id from Common.Clients where Name='USAA') and m.ShippingStatus='Shipped'
and b.ReceivedOn between @startDate and @endDate
ORDER BY m.Id
