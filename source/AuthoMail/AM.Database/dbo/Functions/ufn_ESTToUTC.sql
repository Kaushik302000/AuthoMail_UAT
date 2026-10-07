



-- =============================================
-- Author:		Sagar
-- Create date: <Create Date, ,>
-- Description:	<Description, ,>
-- =============================================
CREATE FUNCTION [dbo].[ufn_ESTToUTC]
(
	@ESTDate DATETIME
)
RETURNS DATETIME
AS
BEGIN
	

	-- Return the result of the function
	RETURN CONVERT(DATETIME, SWITCHOFFSET(CONVERT(DATETIMEOFFSET, @ESTDate AT TIME ZONE 'Eastern Standard Time'), DATEPART(TZOFFSET, 
SYSDATETIMEOFFSET() AT TIME ZONE 'UTC')))

END