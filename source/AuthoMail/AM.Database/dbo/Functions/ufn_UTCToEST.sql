CREATE FUNCTION [dbo].[ufn_UTCToEST]
(
	@UTCDate DATETIME
)
RETURNS DATETIME
AS
BEGIN
	

	-- Return the result of the function
	--RETURN DATEADD(HOUR,-5,@UTCDate)
	RETURN CAST((CAST(@UTCDate AS datetime2) AT TIME ZONE 'UTC') AT TIME ZONE 'Eastern Standard Time' AS datetime)

END


