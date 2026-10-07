CREATE PROCEDURE [dbo].[usp_GetDocTypeDetailsByAppId]
  @clientId INT
AS
BEGIN
  -- SET NOCOUNT ON added to prevent extra result sets FROM
  -- interfering with SELECT statements.
  SET NOCOUNT ON;
  
  SELECT D.ClientId,D.[PrintSize],D.[ContentScalePercent],D.[QCPercent],D.[ClientEmails],D.[SupportEmails],D.DocTypeName,D.IsActive,D.ModifiedOn,D.CreatedOn,D.Id AS DocTypeId,D.ShippingCarrier,D.ShippingServiceType,D.IsBRE,D.PrintFormat,D.PnSConfigDetailId,D.PnSConfigId
  FROM v_DocTypes D
  INNER JOIN Common.Clients C ON D.ClientId = C.Id
  WHERE C.Id=@clientId
  UNION ALL
  SELECT D.ClientId,D.[PrintSize],D.[ContentScalePercent],D.[QCPercent],D.[ClientEmails],D.[SupportEmails],D.DocTypeName,D.IsActive,D.ModifiedOn,D.CreatedOn,D.Id AS DocTypeId,D.ShippingCarrier,D.ShippingServiceType,D.IsBRE,D.PrintFormat,D.PnSConfigDetailId,D.PnSConfigId
  FROM v_IntlDocTypes D
  INNER JOIN Common.Clients C ON D.ClientId = C.Id
  WHERE C.Id=@clientId
  
END
