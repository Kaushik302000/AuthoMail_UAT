
	CREATE FUNCTION [dbo].[ufn_GetDallasBatches]
	(
	@EstDate DATE
	)
	RETURNS @sweep TABLE 
	(
		AppCode VARCHAR(255),
		BatchName varchar(255),
		StartDate datetime,
		EndDate datetime,
		ShippingCarrier varchar(255),
		ShippingServiceType varchar(255)
	)
	AS
	BEGIN
		DECLARE @todayDate DATE
		SELECT @todayDate=@EstDate
		
		INSERT INTO @sweep (AppCode, BatchName,StartDate,EndDate,ShippingCarrier,ShippingServiceType) 
		SELECT AppCode, BatchName,cast(@todayDate AS DATETIME)+cast(StartTime AS DATETIME),cast(@todayDate AS DATETIME)+cast(EndTime AS DATETIME),ShippingCarrier,ShippingServiceType from [dbo].[ufn_GetDallasBatchTimings](@EstDate)	 
		
		
		INSERT INTO @sweep (AppCode, BatchName,StartDate,EndDate,ShippingCarrier,ShippingServiceType) 
		VALUES ('USAA','Backlog',DateAdd(day,-7, cast(@todayDate AS DATETIME)),DateAdd(second,-1, cast(@todayDate AS DATETIME)),'FedEx','PRIORITY_OVERNIGHT')
		
		INSERT INTO @sweep (AppCode, BatchName,StartDate,EndDate,ShippingCarrier,ShippingServiceType) 
		VALUES ('USAA','Backlog',DateAdd(day,-7, cast(@todayDate AS DATETIME)),DateAdd(second,-1, cast(@todayDate AS DATETIME)),'USPS','PRIORITY_EXPRESS')

		INSERT INTO @sweep (AppCode, BatchName,StartDate,EndDate,ShippingCarrier,ShippingServiceType) 
		VALUES ('USAA','Backlog',DateAdd(day,-7, cast(@todayDate AS DATETIME)),DateAdd(second,-1, cast(@todayDate AS DATETIME)),'USPS','PRIORITY')

		INSERT INTO @sweep (AppCode, BatchName,StartDate,EndDate,ShippingCarrier,ShippingServiceType) 
		VALUES ('USAA','Backlog',DateAdd(day,-7, cast(@todayDate AS DATETIME)),DateAdd(second,-1, cast(@todayDate AS DATETIME)),'USPS','IMB')

		INSERT INTO @sweep (AppCode, BatchName,StartDate,EndDate,ShippingCarrier,ShippingServiceType) 
		VALUES ('HSBC','Backlog',DateAdd(day,-7, cast(@todayDate AS DATETIME)),DateAdd(second,-1, cast(@todayDate AS DATETIME)),'USPS','IMB')

		INSERT INTO @sweep (AppCode, BatchName,StartDate,EndDate,ShippingCarrier,ShippingServiceType) 
		VALUES ('BECU','Backlog',DateAdd(day,-7, cast(@todayDate AS DATETIME)),DateAdd(second,-1, cast(@todayDate AS DATETIME)),'USPS','IMB')

		--INSERT INTO @sweep (AppCode, BatchName,StartDate,EndDate,ShippingCarrier,ShippingServiceType) 
		--VALUES ('BOTW','Backlog',DateAdd(day,-7, cast(@todayDate AS DATETIME)),DateAdd(second,-1, cast(@todayDate AS DATETIME)),'USPS','PRIORITY_EXPRESS')

		INSERT INTO @sweep (AppCode, BatchName,StartDate,EndDate,ShippingCarrier,ShippingServiceType) 
		VALUES ('BOTW','Backlog',DateAdd(day,-7, cast(@todayDate AS DATETIME)),DateAdd(second,-1, cast(@todayDate AS DATETIME)),'USPS','PRIORITY')

		INSERT INTO @sweep (AppCode, BatchName,StartDate,EndDate,ShippingCarrier,ShippingServiceType) 
		VALUES ('BOTW','Backlog',DateAdd(day,-7, cast(@todayDate AS DATETIME)),DateAdd(second,-1, cast(@todayDate AS DATETIME)),'USPS','IMB')

		INSERT INTO @sweep (AppCode, BatchName,StartDate,EndDate,ShippingCarrier,ShippingServiceType) 
		VALUES ('AMTEST','Backlog',DateAdd(day,-7, cast(@todayDate AS DATETIME)),DateAdd(second,-1, cast(@todayDate AS DATETIME)),'FedEx','PRIORITY_OVERNIGHT')

		INSERT INTO @sweep (AppCode, BatchName,StartDate,EndDate,ShippingCarrier,ShippingServiceType) 
		VALUES ('AMTEST','Backlog',DateAdd(day,-7, cast(@todayDate AS DATETIME)),DateAdd(second,-1, cast(@todayDate AS DATETIME)),'USPS','IMB')

		INSERT INTO @sweep (AppCode, BatchName,StartDate,EndDate,ShippingCarrier,ShippingServiceType) 
		VALUES ('AMTEST','Backlog',DateAdd(day,-7, cast(@todayDate AS DATETIME)),DateAdd(second,-1, cast(@todayDate AS DATETIME)),'USPS','PRIORITY_EXPRESS')

		INSERT INTO @sweep (AppCode, BatchName,StartDate,EndDate,ShippingCarrier,ShippingServiceType) 
		VALUES ('AMTEST','Backlog',DateAdd(day,-7, cast(@todayDate AS DATETIME)),DateAdd(second,-1, cast(@todayDate AS DATETIME)),'USPS','PRIORITY')

		INSERT INTO @sweep (AppCode, BatchName,StartDate,EndDate,ShippingCarrier,ShippingServiceType) 
		VALUES ('S3','Backlog',DateAdd(day,-7, cast(@todayDate AS DATETIME)),DateAdd(second,-1, cast(@todayDate AS DATETIME)),'USPS','IMB')

		INSERT INTO @sweep (AppCode, BatchName,StartDate,EndDate,ShippingCarrier,ShippingServiceType) 
		VALUES ('PENFED','Backlog',DateAdd(day,-7, cast(@todayDate AS DATETIME)),DateAdd(second,-1, cast(@todayDate AS DATETIME)),'USPS','PRIORITY')
		INSERT INTO @sweep (AppCode, BatchName,StartDate,EndDate,ShippingCarrier,ShippingServiceType) 
		VALUES ('PENFED','Backlog',DateAdd(day,-7, cast(@todayDate AS DATETIME)),DateAdd(second,-1, cast(@todayDate AS DATETIME)),'USPS','PRIORITY_EXPRESS')
		INSERT INTO @sweep (AppCode, BatchName,StartDate,EndDate,ShippingCarrier,ShippingServiceType) 
		VALUES ('PENFED','Backlog',DateAdd(day,-7, cast(@todayDate AS DATETIME)),DateAdd(second,-1, cast(@todayDate AS DATETIME)),'FedEx','PRIORITY_OVERNIGHT')

		RETURN
	END
