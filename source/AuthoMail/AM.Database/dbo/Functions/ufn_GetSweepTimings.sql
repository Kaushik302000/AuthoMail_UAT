CREATE FUNCTION [dbo].[ufn_GetSweepTimings] 
( 
 
)
RETURNS TABLE
AS
RETURN
(
	SELECT BundleSeqNo AS SrNo,AppCode,VendorCode,WeekDayType,SweepType,SweepTime FROM Common.PnSSweepTimings
)