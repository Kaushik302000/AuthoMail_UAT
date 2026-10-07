-- =============================================
-- Author:		Akshar Vala
-- Create date: 27 Mar 2018
-- Description:	To map data in ClientSFTChannels table
-- =============================================
CREATE PROCEDURE [dbo].[usp_MapClientSFTChannels] 
	-- Add the parameters for the stored procedure here
	@AppCode varchar(128), 
	@SFTChannelCode varchar(128)
AS
BEGIN
	INSERT INTO [Common].[ClientSFTChannels]([ClientId],[SFTChannelId])
    SELECT (SELECT Id FROM Common.Clients WHERE AppCode = @AppCode) ClientId, 
	(SELECT Id FROM Common.SFTChannels WHERE Code = @SFTChannelCode)SFTChannelId 
END










