USE [PCIPnS]
GO

/****** Object:  View [dbo].[view_MRDFResponseData]    Script Date: 08/22/2017 19:19:50 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[view_MRDFResponseData]') AND type in (N'P', N'PC'))
DROP VIEW [dbo].[view_MRDFResponseData]
GO

CREATE VIEW [dbo].[view_MRDFResponseData] AS
SELECT REQ.JobId, CASE WHEN RES.PostageMetered IS NULL THEN 0.00 ELSE RES.PostageMetered END AS PostageCost 
,CASE WHEN RES.DetailedErrorId IS NULL THEN '0' ELSE RES.DetailedErrorId END AS ErrId 
,RES.ProcessingTimeStamp AS ShippedOn 
,CASE WHEN RES.WeightMeasured IS NULL THEN 0.00 ELSE RES.WeightMeasured END AS WeightMeasured
,CASE WHEN RES.Disposition IS NULL THEN '0' ELSE RES.Disposition END AS Disposition 
,REQ.PieceId,REQ.[UniqueIdentifier]
,REQ.PDFFileName, REQ.ErrDesc, REQ.MailPieceStatus FROM MRDFRequest REQ
LEFT JOIN MRDFResponse RES
ON  REQ.JobId =RES.JobId AND REQ.PieceId =RES.PieceId
GO


