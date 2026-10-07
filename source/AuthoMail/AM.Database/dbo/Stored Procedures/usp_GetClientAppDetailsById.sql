
CREATE PROCEDURE [dbo].[usp_GetClientAppDetailsById]
	-- Add the parameters for the stored procedure here
	@appId int
AS
BEGIN
	SELECT C.[Id]
	  ,B.Id AS BUId
	  ,B.BusinessUnit
	  ,B.CustCode
	  ,C.AppCode
      ,CASE WHEN C.Alias IS NULL OR C.Alias='' THEN C.AppCode ELSE C.Alias END AS Alias
      ,[Name]
	  ,[TimeZone]
      ,[IsAMActive]
	  ,[IsPnSActive]
      ,[ModifiedOn]
      ,[ModifiedBy]
      ,[CreatedOn]
      ,[CreatedBy]
  FROM [Common].[Clients] C
  INNER JOIN Common.BusinessUnit B ON B.Id=C.BusinessUnit
  WHERE C.Id = @appId
END


