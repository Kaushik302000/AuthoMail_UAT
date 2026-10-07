CREATE  PROCEDURE [dbo].[usp_GetMRDFRequestDataByPrintTrailId] 
@printTrailId INT,
@serverIP VARCHAR(50)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets FROM
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

		SELECT REQ.*,MP.Id AS MailPieceId,MP.EffectiveReceivedOnEst, BS.ClientId,BS.DocTypeId,PCD.ShippingCarrier,
 PCD.ShippingServiceType,C.AppCode,CASE WHEN C.Alias IS NULL OR C.Alias='' THEN C.AppCode ELSE C.Alias END AS Alias,MP.DueDate AS MailPieceDueDate,PCD.EnvelopeId
 ,E.EnvelopeName,E.PartNumber AS EnvelopePartNo,E.EnvelopeName AS EnvelopeDesc
  ,(SELECT [Value] FROM AM.MailPieceCustomInfo MPC WHERE MPC.[Key]='CustomerId' AND MPC.MailPieceId=MP.Id) AS CustomerId
  ,ISNULL((SELECT [Value] FROM AM.MailPieceCustomInfo MPC WHERE MPC.[Key]='CheckPageNumbers' AND MPC.MailPieceId=MP.Id),'') AS ChecksPageNumbers
  ,CAST((CASE WHEN LEN(LTRIM(RTRIM(MP.RcptAddressCountry))) >0 THEN 1 ELSE 0 END )AS BIT) AS IsForeignAddress,MP.ShipperId, MP.Inserts
  ,CAST(CASE WHEN d.IsChecks IS NULL THEN 0 ELSE d.IsChecks END AS BIT) IsChecks,PC.DocumentCode,MP.TotalPages
  ,InsertsPartNos=STUFF
  (  
    (  
      SELECT DISTINCT '|'+ CAST([PartNumber] AS VARCHAR(MAX))  
      FROM Common.Envelopes   
      WHERE UPPER(EnvelopeName) IN (SELECT UPPER(RTRIM(LTRIM(value))) FROM string_split((MP.Inserts),',') WHERE RTRIM(LTRIM(value)) <>'')   
      FOR XMl PATH('')  
    ),1,1,'')
 FROM PnS.MRDFRequest REQ
 INNER JOIN PnS.BatchSummary BS ON REQ.JobId=BS.PrintBatch
 LEFT OUTER JOIN AM.MailPieces MP ON REQ.UniqueId=MP.UniqueId
 INNER JOIN [AM].[DocTypes] d ON mp.DocTypeId=d.Id
 INNER JOIN Common.PnSConfigDetail PCD ON MP.PnSConfigDetailId = pcd.Id
 INNER JOIN Common.PnSConfig PC ON PCD.PnSConfigId=PC.Id
 INNER JOIN Common.Clients C ON C.Id=BS.ClientId
 LEFT OUTER JOIN Common.Envelopes E ON PCD.EnvelopeId=E.Id
 WHERE BS.PipelineStatus='Prepped' AND  IsPrepped = 1 AND IsSubmitted = 0 AND REQ.PrintTrailBatchId IS NULL
 AND BS.PrintTrailId=@printTrailId AND BS.ServerIP = @serverIP
	
END
