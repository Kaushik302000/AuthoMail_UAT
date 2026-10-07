
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Akshar Vala
-- Create date: 30 Aug 2017
-- Description:	To get all Businessunit names
-- =============================================
CREATE PROCEDURE sp_GetBusinessUnits 
	-- Add the parameters for the stored procedure here
	
AS
BEGIN
	SELECT [Id]
		  ,[BusinessUnit]
	FROM [dbo].[BusinessUnit]
END
GO
