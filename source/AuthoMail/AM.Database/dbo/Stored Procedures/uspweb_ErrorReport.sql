-- =============================================
-- Author:		 Milan Patel
-- Create date:  28-Feb-18
-- Modifiy date: 03-Mar-18
-- Exec [dbo].[uspweb_ErrorReport] 101,'2018-03-19 04:00:00','2018-03-20 03:59:59',NULL,NULL,NULL
-- Description:	 Get Batch by parameters
-- =============================================
CREATE PROCEDURE [dbo].[uspweb_ErrorReport]
	@clientId INT,
	@startDate DATETIME,
	@endDate DATETIME,
	@batchId INT,
	@channel VARCHAR(128),
	@shippingStatus VARCHAR(255)
AS
BEGIN

	SELECT *
	FROM [AM].[MailBatches] b, [Common].[SFTChannels] ch,[Common].[Clients] c,[AM].[MailPieces] m
	LEFT OUTER JOIN [AM].[DocTypes] d ON m.DocTypeId=d.Id 
	LEFT OUTER JOIN Common.PnSConfigDetail PCD ON m.PnSConfigDetailId = pcd.Id
	WHERE m.MailBatchId=b.Id AND c.Id=b.ClientId AND ch.Id=b.SFTChannelId AND m.ClientId=@clientId
	AND ISNULL(b.ReceivedOn,0) BETWEEN ISNULL(@startDate, ISNULL(b.ReceivedOn, 0)) AND ISNULL(@endDate, ISNULL(b.ReceivedOn, 0))
	AND ISNULL(m.MailBatchId, '') = ISNULL(@batchId, ISNULL(m.MailBatchId, ''))
	AND ISNULL(ch.Code, '') = ISNULL(@channel, ISNULL(ch.Code, ''))
	AND ISNULL(m.ShippingStatus, '') = (ISNULL(@shippingStatus, ISNULL(m.ShippingStatus, '')))
	END
	
