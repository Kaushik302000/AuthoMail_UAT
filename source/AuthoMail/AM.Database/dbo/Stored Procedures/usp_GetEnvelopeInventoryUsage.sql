Create PROCEDURE [dbo].[usp_GetEnvelopeInventoryUsage]
	@Id int
	AS
	BEGIN
		-- SET NOCOUNT ON added to prevent extra result sets from
		-- interfering with SELECT statements.
		SET NOCOUNT ON;
		
;with cte as
(
select  distinct(cast(SubmittedOn as date)) as Date ,SUM(Quantity) AS UsageStock
	from PnS.EnvelopeUsage EU
	INNER JOIN PnS.PrintTrailBatches PTB ON EU.PrintTrailBatchId=PTB.Id
	WHERE EU.EnvelopeId=@Id
	GROUP BY cast(SubmittedOn as date)  
)
select top 30 * from cte
order by date desc
	END
