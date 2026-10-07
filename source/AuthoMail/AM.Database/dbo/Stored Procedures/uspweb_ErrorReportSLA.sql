CREATE PROCEDURE [dbo].[uspweb_ErrorReportSLA]
	@clientId INT,
	@startDateRec DATETIME,
	@endDateRec DATETIME,
	@startDateDue DATETIME,
	@endDateDue DATETIME,
	@batchId INT,
	@docType VARCHAR(128)
AS
BEGIN

	with mailpiece as (
	Select m.*,e.EnvelopeName,ptb.PrintBatchName as PrintFileName from AM.MailPieces m
	LEFT OUTER JOIN Common.PnSConfigDetail PCD ON m.PnSConfigDetailId = pcd.Id
	LEFT OUTER JOIN Common.Envelopes e ON pcd.EnvelopeId=e.Id
	LEFT OUTER JOIN PnS.MRDFRequest req ON m.UniqueId=req.UniqueId
	LEFT OUTER JOIN PnS.PrintTrailBatches ptb on req.PrintTrailBatchId=ptb.Id)
	SELECT *	
	FROM [AM].[MailBatches] b, [Common].[SFTChannels] ch, [Common].[Clients] c,mailpiece m
	LEFT OUTER JOIN [AM].[DocTypes] d ON m.DocTypeId=d.Id 
	LEFT OUTER JOIN Common.PnSConfigDetail PCD ON m.PnSConfigDetailId = pcd.Id
	WHERE m.MailBatchId=b.Id AND c.Id=b.ClientId AND ch.Id=b.SFTChannelId AND m.ClientId=@clientId --AND m.SLADays < 0
	AND ISNULL(b.ReceivedOn,0) BETWEEN ISNULL(@startDateRec, ISNULL(b.ReceivedOn, 0)) AND ISNULL(@endDateRec, ISNULL(b.ReceivedOn, 0))
	AND ISNULL(m.DueDate,0) BETWEEN ISNULL(@startDateDue, ISNULL(m.DueDate, 0)) AND ISNULL(@endDateDue, ISNULL(m.DueDate, 0))
	AND ISNULL(m.MailBatchId, '') = ISNULL(@batchId, ISNULL(m.MailBatchId, ''))
	AND ISNULL(d.Id, '') = ISNULL(@docType, ISNULL(d.Id, ''))
	--AND m.ShippingStatus IN ('InProgress','Shipped')
	--AND ISNULL(m.ShippingStatus, '') IN (ISNULL('''InProgress'',''Shipped''', ISNULL(m.ShippingStatus, '')))
	END

