CREATE PROCEDURE [dbo].[usp_GetAllClientAppDetail]
	-- Add the parameters for the stored procedure here
	
AS
BEGIN
	SELECT 
	C.Id, C.Name, C.AppCode, C.BusinessUnit, C.IsAMActive, C.IsPnSActive, C.ModifiedOn, C.ModifiedBy
	, C.CreatedOn, C.CreatedBy, C.TimeZone
	,B.Id As BUId
	,B.CustCode
	,B.[BusinessUnit]
  FROM [Common].[Clients] C INNER JOIN [Common].[BusinessUnit] B ON B.Id = C.BusinessUnit
END
