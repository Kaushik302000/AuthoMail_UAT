-- =============================================
-- Author:		Sagar Golvelkar
-- Create date: 04/20/2018
-- Description:	Get deliverables data with emails data
-- =============================================
Create PROCEDURE [dbo].[usp_GetDeliverableByReportType]
	@ClientId INT,
	@ReportType VARCHAR(255)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	if(@ReportType IS NULL OR @ReportType='')
	BEGIN
		SELECT * FROM [Common].[Deliverables] D 
		LEFT JOIN [Common].[EmailChannels] E ON E.Id=D.EmailChannelId
		WHERE D.ClientId=@ClientId
	END
	ELSE
	BEGIN
		SELECT * FROM [Common].[Deliverables] D 
		LEFT JOIN [Common].[EmailChannels] E ON E.Id=D.EmailChannelId
		WHERE D.ClientId=@ClientId AND D.ReportType=@ReportType
	END	

END
