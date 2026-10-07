CREATE PROCEDURE [dbo].[usp_DeleteMRDFRequestByJobId]
	@jobId int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	IF EXISTS (SELECT 1 FROM PnS.MRDFRequest WHERE JobId=@jobId)
	BEGIN
		DELETE FROM PnS.MRDFRequest WHERE JobId=@jobId
	END
END



























