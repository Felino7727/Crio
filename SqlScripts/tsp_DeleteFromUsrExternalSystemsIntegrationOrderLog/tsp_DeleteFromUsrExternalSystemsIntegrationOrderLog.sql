IF OBJECT_ID('tsp_DeleteFromUsrExternalSystemsIntegrationOrderLog', 'P') IS NOT NULL
	DROP PROCEDURE [dbo].[tsp_DeleteFromUsrExternalSystemsIntegrationOrderLog]
	
GO
CREATE PROCEDURE [dbo].[tsp_DeleteFromUsrExternalSystemsIntegrationOrderLog]
AS
	BEGIN
		BEGIN
			DELETE 
			FROM [UsrExternalSystemsIntegrationOrderLog] 
			WHERE [CreatedOn] < DATEADD(month, -3, getdate())
		END;
	END
GO