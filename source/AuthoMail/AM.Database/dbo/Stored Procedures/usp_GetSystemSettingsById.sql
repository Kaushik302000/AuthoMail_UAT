


CREATE PROCEDURE [dbo].[usp_GetSystemSettingsById]
@ID VARCHAR(255)
AS
BEGIN

SELECT Value FROM [Common].[SystemSettings] WHERE Id=@ID

END









































