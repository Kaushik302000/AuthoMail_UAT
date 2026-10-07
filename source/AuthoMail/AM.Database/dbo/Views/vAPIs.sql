create view vAPIs
as
select
APIName
,APIRequestedOn
,APIResponseOn
,IsAPIError
,IsUSPSError
,Postage
,UserId
,USPSRequestedOn
,USPSResponseOn
from API.USPSeVS
union
select
APIName
,APIRequestedOn
,APIResponseOn
,IsAPIError
,IsUSPSError
,0 as Postage
,UserId
,USPSRequestedOn
,USPSResponseOn
from API.USPS