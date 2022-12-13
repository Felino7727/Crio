IF OBJECT_ID('tsp_DeleteFromSmrLoggingRecord', 'P') IS NOT NULL
	DROP PROCEDURE [dbo].[tsp_DeleteFromSmrLoggingRecord]
	
GO
CREATE PROCEDURE [dbo].[tsp_DeleteFromSmrLoggingRecord]
AS
	BEGIN
		BEGIN
			DELETE
			FROM [SmrLoggingRecord]
			WHERE [CreatedOn] < DATEADD(year, -2, getdate())
		END;
	END
GO