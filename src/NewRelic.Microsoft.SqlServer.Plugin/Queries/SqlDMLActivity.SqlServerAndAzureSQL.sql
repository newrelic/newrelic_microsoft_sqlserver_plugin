-- DML Activity
-- SQL and Azure
-- Feeds a graph to show normal data access in SQL by read and write statements


declare @SqlText as table(
	sql_handle varbinary(64),
	query_hash varbinary(max),
	SqlStatement nvarchar(max),
	creation_time DATETIME,
	execution_count INT,
	total_logical_writes INT,
	total_logical_reads INT,
	total_physical_reads INT	
)


INSERT INTO @SqlText		
SELECT 
	queryData.sql_handle,
	queryData.query_hash,
	MAX(queryData.SQLStatement) AS SQLStatement,
	MAX(queryData.creation_time) AS creation_time,
	SUM(queryData.execution_count) AS execution_count,
	SUM(queryData.total_logical_writes) AS total_logical_writes,
	SUM(queryData.total_logical_reads) AS total_logical_reads,
	SUM(queryData.total_physical_reads) AS total_physical_reads
	FROM (
		SELECT 
			qs.sql_handle,
			qs.query_hash,
			SUBSTRING(
						st.TEXT, 
						(qs.statement_start_offset / 2) + 1, 
						((	CASE qs.statement_end_offset
								WHEN -1 THEN DATALENGTH(st.TEXT) 
								ELSE qs.statement_end_offset
							END - qs.statement_start_offset) / 2) + 1
			) AS SQLStatement,
			qs.creation_time,
			qs.execution_count,
			qs.total_logical_writes,
			qs.total_logical_reads,
			qs.total_physical_reads
		FROM sys.dm_exec_query_stats AS qs
		CROSS APPLY sys.dm_exec_sql_text(qs.sql_handle) AS st) AS queryData
GROUP BY queryData.sql_handle, queryData.query_hash
ORDER BY queryData.sql_handle, queryData.query_hash

SELECT 
	sql_handle AS SqlHandle,
	query_hash AS QueryHash,
	SqlStatement,
	creation_time AS CreationTime,
	execution_count AS ExecutionCount,
	--total_logical_writes,
	--total_logical_reads,
	--total_physical_reads,
	case 
		when 
			SqlStatement like '%INSERT%' 
			or SqlStatement like '%UPDATE%'
			or SqlStatement like '%INTO%'
			or SqlStatement like '%DELETE%'
			or SqlStatement like '%MERGE%'
			or SqlStatement like '%WRITE%'
		then 'Writes'
		when 
			SqlStatement like '%SELECT%'
			or SqlStatement like '%READ%' 
		then 'Reads'
		when (total_logical_writes) > 0 then 'Writes'
		when (total_logical_reads + total_physical_reads) > 0 then 'Reads'
		else 'Unknown'
	end as QueryType
FROM @SqlText