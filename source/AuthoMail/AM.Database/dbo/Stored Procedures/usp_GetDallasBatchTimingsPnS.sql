CREATE PROCEDURE [dbo].[usp_GetDallasBatchTimingsPnS] 
@EstToday DATE
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	Select * from dbo.ufn_GetDallasBatchTimings(@EstToday)
END
