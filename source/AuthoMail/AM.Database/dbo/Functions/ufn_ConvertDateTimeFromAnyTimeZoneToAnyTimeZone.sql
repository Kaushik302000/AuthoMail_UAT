

-- =============================================
-- Author:		Sagar
-- Create date: <Create Date, ,>
-- Description:	<Description, ,>
-- =============================================
CREATE FUNCTION [dbo].[ufn_ConvertDateTimeFromAnyTimeZoneToAnyTimeZone]
(
	@FromTimeZone VARCHAR(50),
	@FromDateTime DATETIME,
	@ToTimeZone VARCHAR(50)
)
RETURNS DATETIME
AS
BEGIN
	

	-- Return the result of the function
--	RETURN CONVERT(DATETIME, SWITCHOFFSET(CONVERT(DATETIMEOFFSET, @FromDateTime AT TIME ZONE @FromTimeZone), DATEPART(TZOFFSET, 
--SYSDATETIMEOFFSET() AT TIME ZONE @ToTimeZone)))
 RETURN CAST((CAST(@FromDateTime AS datetime2) AT TIME ZONE @FromTimeZone) AT TIME ZONE @ToTimeZone AS datetime)

END