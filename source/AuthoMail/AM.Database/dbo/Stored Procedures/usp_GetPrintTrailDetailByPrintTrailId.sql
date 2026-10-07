CREATE PROCEDURE [dbo].[usp_GetPrintTrailDetailByPrintTrailId] 
@printTrailId INT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	
	SELECT TOP 1 PT.Id as PrintTrailId, PT.PrintVendorId,PT.TrailName, PT.SimulateTrail,
	PV.Name as PrintVendorName,PV.BusinessCode,PV.VendorCode, SI.PrintVendorCode ,SI.SFTChannelId,
	SC.Protocol,SC.Host,SC.UserName,SC.[Password],SC.ToADEDir,SC.FromADEDir, PT.IsSinglePdf, PT.IsManifestRequired, PT.ShipperId
	FROM Common.PrintTrails PT
	INNER JOIN Common.ShipperInformation SI ON PT.ShipperId=SI.Id
	INNER JOIN Common.PrintVendors PV ON PV.Id=PT.PrintVendorId
	INNER JOIN Common.SFTChannels SC ON SI.SFTChannelId=SC.Id
	WHERE PT.Id=@printTrailId
END

