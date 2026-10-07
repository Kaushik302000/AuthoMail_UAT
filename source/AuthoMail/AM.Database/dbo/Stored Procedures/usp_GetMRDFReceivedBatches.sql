CREATE PROCEDURE [dbo].[usp_GetMRDFReceivedBatches]
AS
BEGIN
	SELECT B.PrintBatch,D.DocTypeName,C.AppCode,BU.CustCode,CONVERT(VARCHAR(10),B.PrintBatch) AS BatchCode 
	FROM [PnS].BatchSummary B
	INNER JOIN [AM].[DocTypes] d ON B.DocTypeId=d.Id
	INNER JOIN [Common].Clients C ON C.Id=B.ClientId
	INNER JOIN [Common].BusinessUnit BU ON BU.Id=C.BusinessUnit
	WHERE B.IsMRDFReceived=1 AND B.IsPDFsReceived=0 AND B.SubmittedOn > '2018-08-01' AND B.PrintLocation='Fort Mill, SC'
END
