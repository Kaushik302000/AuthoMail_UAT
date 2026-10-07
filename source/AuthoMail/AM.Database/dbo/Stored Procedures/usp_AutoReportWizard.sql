-- =============================================
-- Author:		 Avi chauhan
-- Create date:  21-Aug-23
-- Modifiy date: 23-Aug-23
-- Exec [dbo].[usp_AutoReportWizard] '2018-03-19 04:00:00','2018-04-20 03:59:59'
-- Description:	 Get Auto Report Wizard
-- =============================================
CREATE PROCEDURE [dbo].[usp_AutoReportWizard]
	@startDate DATETIME,
	@endDate DATETIME
AS
BEGIN

	 SELECT C.AppCode, B.BatchName,B.ReceivedOn,S.Code , P.RcptFirstName,P.RcptLastName,P.DocumentOrPackageType,P.TotalPages, 
     P.TotalSheets,P.ShippedOn, P.ErrorDesc, P.TrackingNo,P.ShippingStatus,PCD.ShippingCarrier+'-'+PCD.ShippingServiceType as ShippingMethod
     FROM AM.MailPieces P WITH (NOLOCK)
     INNER JOIN AM.MailBatches B WITH (NOLOCK) ON P.MailBatchId = B.Id 
     INNER JOIN Common.SFTChannels S WITH (NOLOCK) ON B.SFTChannelId = S.Id 
     LEFT OUTER JOIN Common.Clients C WITH (NOLOCK) ON P.ClientId= C.Id 
     LEFT OUTER JOIN Common.PnSConfigDetail PCD WITH (NOLOCK) ON P.PnSConfigDetailId = PCD.Id 
     WHERE B.ReceivedOn BETWEEN @startDate AND @endDate
     ORDER BY B.ReceivedOn

END
