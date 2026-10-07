-- =============================================
-- Author:		 Milan Patel
-- Create date:  07-Mar-18
-- Modifiy date: 07-Mar-18
-- Exec: [dbo].[usp_MailPieceSearch] 101,NULL,NULL,'2018-03-19 04:00:00','2018-03-20 03:59:59',NULL,NULL,NULL
--       [dbo].[usp_MailPieceSearch] 101,NULL,NULL,'2018-03-19 04:00:00',NULL,NULL,NULL,NULL,NULL
-- Description:	 Get Batch by parameters
-- =============================================
CREATE PROCEDURE [dbo].[uspweb_MailPieceSearch]

	@clientId INT,
	@businessFriendlyIdName VARCHAR(255),
	@businessFriendlyIdValue VARCHAR(255),
	@startDate DATETIME,
	@endDate DATETIME,
	@docType VARCHAR(255),
	@shippingStatus VARCHAR(255),
	@channel VARCHAR(128),
	@clientBatchId VARCHAR(255),
	@mailBatchId INT,
	@trackingNo VARCHAR(255)
AS
BEGIN

	 SELECT 
	 m.BusinessFriendlyIdValue AS BusinessIdValue,m.ShippingStatus AS [Status],m.UniqueId,
	 m.RcptFirstName AS FirstName,m.RcptLastName AS LastName,m.RcptAddressCity AS [City],m.RcptAddressZIP AS ZIP,
	 m.RcptAddressState AS [State],m.ShippedOn,m.TrackingNo,m.DueDate,m.ErrorCode,m.ErrorDesc,m.PrintLocation,
	 b.ReceivedOn,
	 d.DocTypeName AS DocType,PCD.ShippingServiceType AS ShippingMethod,PCD.ShippingCarrier,
	 ch.Title AS Channel
	 FROM  [AM].[MailBatches] b,[Common].[SFTChannels] ch,[Common].[Clients] c,[AM].[MailPieces] m
	 LEFT OUTER JOIN [Profile].[Users] u ON m.PulledBackRequestedBy=u.Id	 
	 LEFT OUTER JOIN [AM].[DocTypes] d ON m.DocTypeId=d.Id 
	LEFT OUTER JOIN Common.PnSConfigDetail PCD ON m.PnSConfigDetailId = pcd.Id
	 WHERE m.MailBatchId=b.Id AND b.SFTChannelId=ch.Id AND m.ClientId=c.Id AND m.ClientId=@clientId
	 AND ISNULL(m.BusinessFriendlyIdName, '') = ISNULL(@businessFriendlyIdName, ISNULL(m.BusinessFriendlyIdName, ''))
	 AND ISNULL(m.BusinessFriendlyIdValue, '') = ISNULL(@businessFriendlyIdValue, ISNULL(m.BusinessFriendlyIdValue, ''))
	 AND ISNULL(m.DocTypeId, '') = ISNULL(@docType, ISNULL(m.DocTypeId, ''))
	 AND ISNULL(m.ShippingStatus, '') = ISNULL(@shippingStatus, ISNULL(m.ShippingStatus, ''))
	 AND ISNULL(ch.Code, '') = ISNULL(@channel, ISNULL(ch.Code, ''))
	 AND ISNULL(b.ClientBatchId, '') = ISNULL(@clientBatchId, ISNULL(b.ClientBatchId, ''))
	 AND ISNULL(m.TrackingNo, '') = ISNULL(@trackingNo, ISNULL(m.TrackingNo, ''))
	 AND ISNULL(m.MailBatchId, '') = ISNULL(@mailBatchId, ISNULL(m.MailBatchId, ''))
	 AND b.ReceivedOn BETWEEN @startDate AND @endDate
END

