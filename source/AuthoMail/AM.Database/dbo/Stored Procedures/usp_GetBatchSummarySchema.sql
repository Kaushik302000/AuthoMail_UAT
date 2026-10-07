
CREATE PROCEDURE [dbo].[usp_GetBatchSummarySchema]
	-- Add the parameters for the stored procedure here
	
AS
BEGIN
	
	SELECT [Id]
      ,[FieldName]
      ,[StartPosition]
      ,[EndPosition]
      ,[Length]
      ,[Type]
      ,[IsActive]
      ,[Comments]
      ,[SampleValue]
      ,[DataType]
      ,[DataFormatTemplate]
      ,[DefaultValue]
  FROM [PnS].[Schema_BatchSummary]	WHERE IsActive = 1
  ORDER BY Id
END


























