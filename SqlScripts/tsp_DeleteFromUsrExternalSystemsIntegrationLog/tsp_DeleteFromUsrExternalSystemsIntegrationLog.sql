IF OBJECT_ID('tsp_DeleteFromUsrExternalSystemsIntegrationLog', 'P') IS NOT NULL
	DROP PROCEDURE [dbo].[tsp_DeleteFromUsrExternalSystemsIntegrationLog]
GO
CREATE PROCEDURE [dbo].[tsp_DeleteFromUsrExternalSystemsIntegrationLog]
AS
	BEGIN
		BEGIN
			DELETE
			FROM [UsrExternalSystemsIntegrationLog]
			WHERE [CreatedOn] < DATEADD(month, -3, getdate())
		END;
	END
GO