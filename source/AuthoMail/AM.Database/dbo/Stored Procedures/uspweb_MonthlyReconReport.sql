CREATE PROCEDURE [dbo].[uspweb_MonthlyReconReport]
	@clientId INT,
	@startDate DATETIME,
	@endDate DATETIME,
	@channel VARCHAR(128)
AS
BEGIN

;WITH CTE1 AS
(
	SELECT B.BatchName,P.UniqueId, P.DocumentOrPackageType ,P.BusinessFriendlyIdValue, P.[FileName], P.[FilePattern]
	,B.ReceivedOn,P.PrintedOn ,P.ShippedOn ,P.TotalPages ,P.RcptFirstName,P.RcptLastName ,P.RcptAddressStreet 
	,P.RcptAddressCity,P.RcptAddressState ,P.RcptAddressZIP,P.Id AS [MailPieceId],P.ShippingCost,P.ShippingStatus
	,P.ErrorDesc, PTB.PrintBatchName,PTB.SubmittedOn,P.IsEDeliveryRequested, P.IsESignRequested, S.Code AS Channel, P.PrintLocation 
	,IIF(PCD.ShippingCarrier = 'E-Delivery', PCD.ShippingCarrier, PCD.ShippingCarrier + ' / ' +PCD.ShippingServiceType ) AS [ShippingMethod], P.TrackingNo, P.TotalSheets
	,P.TotalBlankPages, c.AppCode, E.EnvelopeName ,P.Inserts , P.DeliveryStatus, P.DeliveryDate,P.BlankPageNumbers
	FROM AM.MailBatches B WITH (NOLOCK)
	INNER JOIN AM.MailPieces P WITH (NOLOCK) ON P.MailBatchId=B.Id
	INNER JOIN Common.SFTChannels S WITH (NOLOCK) ON B.SFTChannelId=S.Id
	INNER JOIN Common.Clients C WITH (NOLOCK) ON P.ClientId=C.Id
	LEFT OUTER JOIN AM.DocTypes D WITH (NOLOCK) ON D.Id=P.DocTypeId
	LEFT OUTER JOIN Common.PnSConfigDetail PCD WITH (NOLOCK) ON P.PnSConfigDetailId=PCD.Id
	LEFT OUTER JOIN Common.Envelopes E WITH (NOLOCK) ON P.UsedEnvelopeId=E.Id
	LEFT OUTER JOIN PnS.MRDFRequest REQ WITH (NOLOCK) ON P.UniqueId=REQ.UniqueId
	LEFT OUTER JOIN PnS.BatchSummary BS WITH (NOLOCK) ON REQ.JobId=BS.PrintBatch
	LEFT OUTER JOIN PnS.PrintTrailBatches PTB WITH (NOLOCK) ON REQ.PrintTrailBatchId = PTB.Id
	WHERE P.EffectiveReceivedOnEst BETWEEN @startDate AND @endDate
	AND B.ClientId=@clientId
	AND ISNULL(S.Code, '') = ISNULL(@channel, ISNULL(S.Code, ''))
), CTE2 AS
(
	SELECT *
	FROM 
	(
		SELECT MPP.[MailPieceId],MPP.[PageSize],MPP.[PageCount]
		FROM [Common].[MailPiecePageInfo] MPP WITH (NOLOCK) 
		INNER JOIN CTE1 ON MPP.MailPieceId=CTE1.MailPieceId
	) AS S
	PIVOT
	(
		MAX([PageCount])
		FOR [PageSize] IN ([LETTER],[LEGAL])
	) AS pvt2
), CTE3 AS
(
	SELECT *
	FROM 
	(
		SELECT MPP.[MailPieceId],MPP.[PageSize],MPP.[PageNumbers]
		FROM [Common].[MailPiecePageInfo] MPP WITH (NOLOCK) 
		INNER JOIN CTE1 ON MPP.MailPieceId=CTE1.MailPieceId
	) AS S
	PIVOT
	(
		MAX([PageNumbers])
		FOR [PageSize] IN ([LETTER],[LEGAL])
	) AS pvt3
)
SELECT A.AppCode, A.BatchName AS BatchName, A.UniqueId,A.BusinessFriendlyIdValue AS LoanNumber 
,A.RcptFirstName AS [FirstName],A.RcptLastName AS [LastName],A.RcptAddressStreet AS [Address]
,A.RcptAddressCity AS [City],A.RcptAddressState AS [State],A.RcptAddressZIP AS [ZIP],A.Channel
,A.DocumentOrPackageType AS [DocType],A.ReceivedOn, A.ShippedOn, A.PrintLocation,A.ShippingStatus AS [Status]
,A.TrackingNo, A.[FileName],A.TotalPages, A.TotalSheets, A.TotalBlankPages,A.IsEDeliveryRequested,A.IsESignRequested, A.[ShippingMethod]
,ISNULL(REPLACE(REPLACE(A.ErrorDesc, CHAR(13), ''), CHAR(10), ''),'') AS ErrorDesc
,A.Inserts,A.PrintBatchName, A.SubmittedOn, A.DeliveryStatus, A.DeliveryDate, A.ShippingCost, A.EnvelopeName
,ISNULL(CTE2.LETTER,0) AS [LetterPageCount],ISNULL(CTE2.LEGAL,0) AS [LegalPageCount]
,ISNULL(CTE3.LETTER,'') AS [LetterPageNumbers],ISNULL(CTE3.LEGAL,'') AS [LegalPageNumbers]
,ISNULL(BlankPageNumbers,'') AS BlankPageNumbers
FROM CTE1 A
LEFT OUTER JOIN CTE2 ON A.MailPieceId=CTE2.MailPieceId
LEFT OUTER JOIN CTE3 ON A.MailPieceId=CTE3.MailPieceId
ORDER BY A.MailPieceId
END

