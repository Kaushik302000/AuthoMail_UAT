-- =============================================
-- Author:		Milan Patel
-- Create date: 03-Mar-18
-- Description:	Get MailPiece by batchid AND status.
-- =============================================
CREATE PROCEDURE [dbo].[uspweb_GetMailPieceByBatchIdAndStatus]
	-- Add the parameters for the stored procedure here
	@batchId INT,
	@status VARCHAR(255),
	@clientId INT = NULL
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets FROM
	-- interfering with SELECT statements.
	SELECT *
	FROM [AM].[MailBatches] b,[Common].[Clients] c,[AM].[MailPieces] m
	LEFT OUTER JOIN [AM].[DocTypes] d ON m.DocTypeId=d.Id 
	LEFT OUTER JOIN Common.PnSConfigDetail PCD ON m.PnSConfigDetailId = pcd.Id
	WHERE m.MailBatchId=b.Id AND b.ClientId=ISNULL(@clientId,b.ClientId)
	AND m.ShippingStatus=@status AND b.Id=@batchId AND m.ClientId=c.Id
	END

