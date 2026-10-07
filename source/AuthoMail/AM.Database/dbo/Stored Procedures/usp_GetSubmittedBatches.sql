


CREATE PROCEDURE [dbo].[usp_GetSubmittedBatches]
@serverIP VARCHAR(50),
@printVendorCode VARCHAR(50)
AS
BEGIN

IF(@printVendorCode='')
BEGIN
	SELECT B.Id as PrintTrailBatchId,B.PrintBatchName,PT.PrintVendorCode
	FROM [PnS].PrintTrailBatches B
	INNER JOIN Common.PrintTrails PT ON B.PrintTrailId=PT.Id
	INNER JOIN Common.PrintVendors PV ON PT.PrintVendorId=PV.Id
	WHERE B.IsSubmitted=1 AND B.IsMRDFReceived=0  AND B.IsSuspended=0 AND B.ServerIP=@serverIP
END
ELSE
BEGIN	
	SELECT B.Id as PrintTrailBatchId,B.PrintBatchName,PT.PrintVendorCode
	FROM [PnS].PrintTrailBatches B
	INNER JOIN Common.PrintTrails PT ON B.PrintTrailId=PT.Id
	WHERE B.IsSubmitted=1 AND B.IsMRDFReceived=0  AND PT.PrintVendorCode=@printVendorCode AND B.IsSuspended=0 AND B.ServerIP=@serverIP
END
END

