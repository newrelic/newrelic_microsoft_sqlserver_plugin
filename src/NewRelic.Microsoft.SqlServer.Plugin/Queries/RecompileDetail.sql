SELECT
	DB_NAME(st.dbid)	AS DBName,
	cp.bucketid			AS Bucketid,
	cp.usecounts		AS UseCounts,
	cp.size_in_bytes	AS SizeInBytes,
	cp.objtype			AS ObjectType,
	st.Text				AS Text
FROM sys.dm_exec_cached_plans cp
CROSS APPLY sys.dm_exec_sql_text(cp.plan_handle) st
WHERE cp.cacheobjtype = 'Compiled Plan'