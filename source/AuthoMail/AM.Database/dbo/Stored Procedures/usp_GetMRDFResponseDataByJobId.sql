
			CREATE PROCEDURE [dbo].[usp_GetMRDFResponseDataByJobId]
			@jobId int
			AS
			BEGIN
			 -- SET NOCOUNT ON added to prevent extra result sets from
			 -- interfering with SELECT statements.
			 SET NOCOUNT ON;
			 
			 SELECT JobId, PieceId, Date, Feeder2_Fed, OperatorId, MachineId, ProcessingTimeStamp, DetailedErrorId
			 , Disposition, Input_Fed, Feeder3_Fed, Feeder4_Fed, Feeder5_Fed, ComputedWeight, MeterDate
			 , StatusSource, PostageMetered, WeightMeasured, ExitLocation, TotalSheets, PDFFileName, PackageTrackingNumber 
			 FROM [PnS].[MRDFResponse] MS 
			 WHERE MS.JobId=@jobId
			END
