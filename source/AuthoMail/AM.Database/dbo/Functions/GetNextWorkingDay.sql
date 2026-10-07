
-- =============================================
-- Author:		Sagar Golvelkar
-- Create date: 30-07-2019
-- Description:	Get Next Working Day
-- =============================================
CREATE FUNCTION [dbo].[GetNextWorkingDay]
(
	@DueDate DATETIME,
	@isSatWorkDay BIT,
	@isSunWorkDay BIT
)
RETURNS DATETIME
AS
BEGIN
	
	
	WHILE (dbo.IsBusinessDay(@DueDate,@isSatWorkDay,@isSunWorkDay)<>1)
	BEGIN
		SET @DueDate=DATEADD(DAY,1,@DueDate)
	END

	-- Return the result of the function
	RETURN @DueDate

END