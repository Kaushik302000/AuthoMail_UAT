
-- =============================================
-- Author:		Sagar Golvelkar
-- Create date: 30-07-2019
-- Description:	Check Date is Working or not
-- =============================================
CREATE FUNCTION [dbo].[IsBusinessDay] 
(
	@DueDate DATETIME,
	@isSatWorkDay BIT,
	@isSunWorkDay BIT
)
RETURNS BIT
AS
BEGIN
	-- Declare the return variable here
	DECLARE @dayOfWeek VARCHAR(20)
	
	SET @dayOfWeek=DATENAME(dw,@DueDate)
	
	IF(EXISTS(SELECT * FROM Common.Holidays WHERE CAST(HolidayDate AS DATE)=CAST(@DueDate AS DATE)))
	BEGIN
		 RETURN 0
	END
	
	IF(@dayOfWeek='Saturday')
	BEGIN
		RETURN @isSatWorkDay
	END
	
	IF(@dayOfWeek='Sunday')
	BEGIN
		RETURN @isSunWorkDay
	END

	RETURN 1
END