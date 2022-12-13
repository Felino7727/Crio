IF OBJECT_ID('tsp_DeteleFromUsrExternalSystemsIntegrationLog', 'P') IS NOT NULL
	DROP PROCEDURE [dbo].[tsp_DeteleFromUsrExternalSystemsIntegrationLog]
GO
CREATE PROCEDURE [dbo].[tsp_DeteleFromUsrExternalSystemsIntegrationLog]
AS
	BEGIN
		BEGIN
			DELETE 
			FROM [UsrExternalSystemsIntegrationLog] 
			WHERE DATEADD(month, 1, [CreatedOn]) < getdate()
		END;
	END
GO