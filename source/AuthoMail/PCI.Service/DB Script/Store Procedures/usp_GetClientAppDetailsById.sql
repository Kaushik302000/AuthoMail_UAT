SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Akshar Vala	
-- Create date: 11 Aug 2017
-- Description:	Get Client App Details by AppId
-- EXECUTE : usp_GetClientAppDetailsById 2
-- =============================================
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_GetClientAppDetailsById]') AND type in (N'P', N'PC'))
DROP PROCEDURE dbo.usp_GetClientAppDetailsById
GO
CREATE PROCEDURE usp_GetClientAppDetailsById
	-- Add the parameters for the stored procedure here
	@appId int
AS
BEGIN
	SELECT C.[Id]
	  ,B.Id AS BUId
	  ,B.BusinessUnit
      ,[AppCode]
      ,[AppName]
      ,[SFTSite]
      ,[AppOwnerEmails]
      ,[AppSupportEmails]
      ,[AppOwnerContactNo]
      ,[AppSupportContactNo]
      ,C.[IsActive]
      ,[ModifiedOn]
      ,[ModifiedBy]
      ,[CreatedOn]
      ,[CreatedBy]
  FROM [dbo].[ClientApps] C
  INNER JOIN BusinessUnit B ON B.Id=C.BusinessUnit
  WHERE C.Id = @appId
END
GO
