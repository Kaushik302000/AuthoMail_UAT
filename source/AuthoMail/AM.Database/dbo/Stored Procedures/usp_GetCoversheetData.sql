

CREATE PROCEDURE [dbo].[usp_GetCoversheetData]
@MailPieceId INT
AS
BEGIN

SELECT Id AS MailPieceId
,ISNULL(RetRcptFirstName,'') AS RetFirstName
,ISNULL(RetRcptLastName,'') AS RetLastName
,ISNULL(RetRcptAddressStreet,'') AS RetAddressLine1
,ISNULL(RetRcptAddrOptionalLine1BelowStreet,'') AS RetAddressLine2
,ISNULL(RetRcptAddressCity,'') AS RetCity
,ISNULL(RetRcptAddressState,'') AS RetState
,ISNULL(RetRcptAddressZIP,'')AS RetZip
,ISNULL(RetRcptAddressCountry,'') AS RetCountry
,(
	SELECT [Value] 
	FROM AM.MailPieceCustomInfo 
	WHERE MailPieceId=@MailPieceId AND [Key]='Base64_Logo'
  ) AS Base64_Logo
,ISNULL(RcptFirstName,'') AS ToFirstName
,ISNULL(RcptLastName,'') AS ToLastName
,ISNULL(RcptAddressStreet,'') AS ToAddressLine1
,ISNULL(RcptAddrOptionalLine1BelowStreet,'') AS ToAddressLine2
,ISNULL(RcptAddressCity,'') AS ToCity
,ISNULL(RcptAddressState,'') AS ToState
,ISNULL(RcptAddressZIP,'') AS ToZip
,ISNULL(RcptAddressCountry,'') AS ToCountry
,CASE WHEN RcptAddressTypes LIKE '%FOREIGN%'THEN 1 ELSE 0 END AS IsForeignAddress
FROM AM.MailPieces WHERE Id=@MailPieceId 
	
END