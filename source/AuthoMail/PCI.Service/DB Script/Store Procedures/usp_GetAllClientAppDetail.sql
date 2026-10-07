
/****** Object:  StoredProcedure [dbo].[usp_GetAllClientAppDetail]    Script Date: 08/14/2017 14:26:25 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		Khushboo Patel
-- Create date: 11 Aug 2017
-- Description:	Get All client detail
-- EXECUTE : usp_GetAllClientAppDetail
-- =============================================
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_GetAllClientAppDetail]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[usp_GetAllClientAppDetail]
GO
CREATE PROCEDURE [dbo].[usp_GetAllClientAppDetail]
	-- Add the parameters for the stored procedure here
	
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	SELECT * from ClientApps
END

GO


