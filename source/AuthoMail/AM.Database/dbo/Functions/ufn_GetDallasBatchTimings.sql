
	CREATE FUNCTION [dbo].[ufn_GetDallasBatchTimings]
	(
	@EstDate DATE
	)
	RETURNS @BatchTimings TABLE 
	(
		AppCode VARCHAR(255),
		BatchName VARCHAR(255),
		StartTime VARCHAR(255),
		EndTime VARCHAR(255),
		ShippingCarrier VARCHAR(255),
		ShippingServiceType VARCHAR(255)
	)
	AS
	BEGIN
		
		DECLARE @DayofWeek INT = DATEPART(dw,@EstDate)

		IF(@DayofWeek=7)
		BEGIN
			INSERT INTO @BatchTimings (AppCode, BatchName,StartTime,EndTime,ShippingCarrier,ShippingServiceType) 
			SELECT AppCode,BatchName,StartTime,EndTime,ShippingCarrier,ShippingServiceType FROM Common.DallasBatchTimings WHERE IsSaturday=1
		END
		ELSE
		BEGIN
			INSERT INTO @BatchTimings (AppCode, BatchName,StartTime,EndTime,ShippingCarrier,ShippingServiceType) 
			SELECT AppCode,BatchName,StartTime,EndTime,ShippingCarrier,ShippingServiceType FROM Common.DallasBatchTimings WHERE IsSaturday=0
		END
		
		RETURN
	END
