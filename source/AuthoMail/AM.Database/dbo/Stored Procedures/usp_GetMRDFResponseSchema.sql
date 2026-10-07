
CREATE PROCEDURE [dbo].[usp_GetMRDFResponseSchema]
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
      ,[SampleValue]
      ,[DataType]
      ,[DataFormatTemplate]
  FROM [PnS].[Schema_MRDFResponse]	WHERE IsActive = 1
  ORDER BY Id
END



























