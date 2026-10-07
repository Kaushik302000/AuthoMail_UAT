-- =============================================
-- Author:		Akshar
-- Create date: 26-Mar-18
-- Description:	Insert Pullback requests data
-- Execute : usp_InsertPullBackRequestsData 'edd717f1-de00-41d1-92c1-febe34c52f9d' 
-- =============================================
CREATE PROCEDURE [dbo].[usp_InsertPullBackRequestsData]
	-- Add the parameters for the stored procedure here
	@MailPieceUniqueId uniqueidentifier,
	@ClientId int,
	@Status varchar(255),
	@RequestedOn datetime,
	@RequestedBy nvarchar(128),
    @PBReasons nvarchar(2048)
AS
BEGIN

DECLARE @MailBatchId int, @MailPieceId int

SELECT @MailBatchId = AMM.MailBatchId,@MailPieceId = AMM.Id FROM  AM.MailPieces AMM WHERE AMM.[UniqueId] = @MailPieceUniqueId

	INSERT INTO [AM].[PullBackRequests]
           ([MailBatchId]
		   ,[ClientId]
           ,[MailPieceId]
           ,[MailPieceUniqueId]
           ,[Status]
           ,[RequestedOn]
           ,[RequestedBy]
           ,[PBReasons]
		   )
     VALUES
           (@MailBatchId
		   ,@ClientId
           ,@MailPieceId
           ,@MailPieceUniqueId
           ,@Status
           ,@RequestedOn
           ,@RequestedBy
           ,@PBReasons
		   )
END
