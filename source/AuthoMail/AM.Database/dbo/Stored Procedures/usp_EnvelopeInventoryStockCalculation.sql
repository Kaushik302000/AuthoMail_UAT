CREATE PROCEDURE [dbo].[usp_EnvelopeInventoryStockCalculation]
	@EnvelopeId int=null
AS
BEGIN
	;with allenvelope as
(
	select EnvelopeId,ShipperId,cast(SubmittedOn as date) as SubmittedOn,Quantity,
ROW_NUMBER() OVER (PARTITION BY EnvelopeId order by cast(SubmittedOn as date) desc) as RowNum
	from PnS.EnvelopeUsage EU
	INNER JOIN PnS.PrintTrailBatches PTB ON EU.PrintTrailBatchId=PTB.Id
	where cast(SubmittedOn as date) < cast(GETUTCDATE() as date)
	GROUP BY cast(SubmittedOn as date),EnvelopeId,Quantity,ShipperId
),
monthAvg as
(
	select EnvelopeId,ShipperId,count(distinct(SubmittedOn)) as TotalDays,SUM(Quantity) AS TotalQuantityUsed
	from allenvelope
	where RowNum<=30
	GROUP BY EnvelopeId,ShipperId
),
weekAvg as
(
	select EnvelopeId,ShipperId,count(distinct(cast(SubmittedOn as date))) as TotalDays,SUM(Quantity) AS TotalQuantityUsed
	from allenvelope
	where RowNum<=7
	GROUP BY EnvelopeId,ShipperId
),
mergeAvg as
(
	select m.EnvelopeId,m.ShipperId,case when m.TotalQuantityUsed>isnull(w.TotalQuantityUsed,0) then m.TotalQuantityUsed else w.TotalQuantityUsed end as TotalQuantityUsed,
	case when m.TotalDays>isnull(w.TotalDays,0) then m.TotalDays else w.TotalDays end as TotalDays
	from monthAvg m
	left outer join weekAvg w on m.EnvelopeId=w.EnvelopeId and m.ShipperId=w.ShipperId
)
--select * from mergeAvg

select es.EnvelopeId,es.ShipperId,isnull(c.TotalDays,0) as TotalDays,isnull(c.TotalQuantityUsed,0) as TotalQuantityUsed,e.EnvelopeName,Case When SI.PrintLocation is null Then NULL Else 
CONCAT(SI.PrintLocation, CONCAT(' (', SI.PrintVendorCode, ') ')) End as ShipperName ,es.CurrentQuantity,es.ReOrderDuration,isnull(CEILING(cast(TotalQuantityUsed as float)/TotalDays),0) as AvgRate,isnull(CEILING(cast(es.CurrentQuantity as float)/(TotalQuantityUsed/TotalDays)),0) as EnvDays
from Common.EnvelopeStock  es 
inner join Common.Envelopes e on es.EnvelopeId=e.Id
left outer join mergeAvg c on es.EnvelopeId=c.EnvelopeId and es.ShipperId=c.ShipperId
inner JOIN Common.ShipperInformation SI ON es.ShipperId=SI.Id
Where e.Id= ISNULL(@EnvelopeId,e.Id)

END
