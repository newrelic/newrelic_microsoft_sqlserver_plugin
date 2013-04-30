-- Object recompile summary by DB
-- Returns count of query plans that are used only once and ones that are used more than once before recompile
-- Returns ratio of the 2

DECLARE @Details TABLE (
			DBName varchar(200) NULL,
			BucketID bigint NULL,
			UseCounts bigint NULL,
			SizeInBytes bigint NULL,
			Text varchar(MAX) NULL
		)

INSERT INTO @Details
	SELECT
		DB_NAME(st.dbid)	AS DBName,
		cp.BucketID			AS BucketID,
		cp.UseCounts		AS UseCounts,
		cp.size_in_bytes	AS SizeInBytes,
		st.Text				AS Text
	FROM sys.dm_exec_cached_plans cp
	CROSS APPLY sys.dm_exec_sql_text(cp.plan_handle) st
	WHERE cp.cacheobjtype = 'Compiled Plan';

WITH SumsByDatabase AS (
		SELECT
			d.DBName,
			(
				SELECT
					COUNT(*)
				FROM @Details d2
				WHERE d2.UseCounts = 1 AND d2.DBName = d.DBName)
			AS SingleUseObjects,
			(
				SELECT
					COUNT(*)
				FROM @Details d2
				WHERE d2.UseCounts > 1 AND d2.DBName = d.DBName)
			AS MultipleUseObjects
		FROM @Details d
		GROUP BY d.DBName)
SELECT
	DBName,
	SingleUseObjects,
	MultipleUseObjects,
	CASE
		WHEN (SingleUseObjects + MultipleUseObjects) = 0 THEN 0 ELSE CAST(SingleUseObjects AS decimal(18, 2)) / (SingleUseObjects + MultipleUseObjects)
	END	AS SingleUsePercent
FROM SumsByDatabase