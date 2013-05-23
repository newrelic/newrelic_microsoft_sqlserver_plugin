-- Object recompile summary by DB
-- Returns count of query plans that are used only once and ones that are used more than once before recompile
-- Returns ratio of the 2
-- Data collection nature: Cumulative. Though the ratio is usable without delta tracking.

WITH RecompileDetails AS (SELECT
			st.dbid	AS DatabaseID,
			cp.UseCounts
		FROM sys.dm_exec_cached_plans cp
		CROSS APPLY sys.dm_exec_sql_text(cp.plan_handle) st
		WHERE cp.cacheobjtype = 'Compiled Plan')
,
	RecompileSums AS (SELECT
			d.Name	AS DatabaseName,
			(SELECT
					COUNT(*)
				FROM RecompileDetails rd2
				WHERE UseCounts = 1 AND rd2.DatabaseID = d.database_id)
			AS SingleUseObjects,
			(SELECT
					COUNT(*)
				FROM RecompileDetails rd2
				WHERE UseCounts > 1 AND rd2.DatabaseID = d.database_id)
			AS MultipleUseObjects
		FROM sys.databases d 
		/*{WHERE}*/)
SELECT
	DatabaseName,
	SingleUseObjects,
	MultipleUseObjects,
	CASE
		WHEN (SingleUseObjects + MultipleUseObjects) = 0 THEN 0 ELSE SingleUseObjects * 100.00 / (SingleUseObjects + MultipleUseObjects)
	END	AS SingleUsePercent
FROM RecompileSums