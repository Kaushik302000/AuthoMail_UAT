CREATE PROCEDURE [dbo].[usp_GetPrintBatchDetailByPrintBatchTrailId]
@printTrailBatchId INT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

   SELECT * 
   FROM PnS.PrintTrailBatches
   WHERE Id=@printTrailBatchId
END

