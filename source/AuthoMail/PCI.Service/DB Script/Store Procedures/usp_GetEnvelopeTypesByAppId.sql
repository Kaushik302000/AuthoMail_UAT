-- ================================================
-- Template generated from Template Explorer using:
-- Create Procedure (New Menu).SQL
--
-- Use the Specify Values for Template Parameters 
-- command (Ctrl-Shift-M) to fill in the parameter 
-- values below.
--
-- This block of comments will not be included in
-- the definition of the procedure.
-- ================================================
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Sagar Golvelkar
-- Create date: 08/14/2017	
-- Description:	<Description,,>
-- =============================================
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_GetEnvelopeTypesByAppId]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].usp_GetEnvelopeTypesByAppId
GO
CREATE PROCEDURE usp_GetEnvelopeTypesByAppId
	@AppId int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	
	SELECT ET.Id,ET.EnvelopeName,ET.PackageType,ET.CreatedOn,ET.IsActive FROM AppEnvelopeMap AEM INNER JOIN 
	EnvelopeTypes ET ON AEM.EnvelopeId=ET.Id
	WHERE AEM.AppId=@AppId AND ET.IsActive=1
	
END
GO
