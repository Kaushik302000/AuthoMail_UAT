-- =============================================
-- Author:		Milan Patel
-- Create date: 03-Mar-18
-- Description:	Get MailPiece by batchid.
-- =============================================
CREATE PROCEDURE [dbo].[uspweb_GetMailPieceByBatchId]
	-- Add the parameters for the stored procedure here
	@batchId INT,
	@clientId INT = NULL
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets FROM
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	SELECT *
	FROM [AM].[MailBatches] b,[Common].[Clients] c,[AM].[MailPieces] m
	LEFT OUTER JOIN [AM].[DocTypes] d ON m.DocTypeId=d.Id 
	LEFT OUTER JOIN Common.PnSConfigDetail PCD ON m.PnSConfigDetailId = pcd.Id
	WHERE m.MailBatchId=b.Id AND b.ClientId=ISNULL(@clientId,b.ClientId) AND m.ClientId=c.Id
	AND b.Id=@batchId
END

