
CREATE PROCEDURE [dbo].[usp_GetMRDFRequestSchema]
	-- Add the parameters for the stored procedure here
	
AS
BEGIN
	SELECT [Id]
      ,[FieldName]
      ,[Type]
      ,[StartPosition]
      ,[Length]
      ,[IsActive]
      ,[Description]
      ,[Comments]
      ,[SampleValue]
      ,[DataType]
      ,[DataFormatTemplate]
      ,[DefaultValue]
  FROM [PnS].[Schema_MRDFRequest]	WHERE IsActive = 1
  ORDER BY Id
END


























