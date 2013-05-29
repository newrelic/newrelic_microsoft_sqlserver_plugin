-- DML Activity per SQL statement
-- SQL and Azure: SQL is for all DBs on instance. Azure is for DB context. (SQL: DBID is NULL for ad hoc and prepared SQL statements)
-- Feeds a graph to show normal data access in SQL by read and write statements
-- Unable to use query hash. It is not availible in SQL Server 2005.
-- creation_time tells you the date and time that the plan that the query is part of was compiled. The stats are all as of the plan and creation_time.
-- So, we need to track SQL statements in plans and add up the stats since compile time.
-- Encrypted SQL statements are ignored
-- We default to a Read when the statement is not categorized. (rare)
-- Use: 
	--plan_handle, SQLStatement and creation_time will give a unique item to calc delta. 
	--These stay in the result set until they un-cache. 
	--Un-cached items count as a 0 delta. 
	--Drop these from delta tracking list.



declare @SqlText as table(
	plan_handle varbinary(64),
	SqlStatement nvarchar(max),
	creation_time DATETIME,
	execution_count INT,
	total_logical_writes INT,
	total_logical_reads INT,
	total_physical_reads INT	
)


INSERT INTO @SqlText		
SELECT 
	queryData.plan_handle,
	queryData.SQLStatement AS SQLStatement,
	queryData.creation_time AS creation_time,
	SUM(queryData.execution_count) AS execution_count,
	SUM(queryData.total_logical_writes) AS total_logical_writes,
	SUM(queryData.total_logical_reads) AS total_logical_reads,
	SUM(queryData.total_physical_reads) AS total_physical_reads
	FROM (
		SELECT 
			qs.plan_handle,
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
		CROSS APPLY sys.dm_exec_sql_text(qs.sql_handle) AS st
		WHERE st.encrypted = 0) AS queryData
GROUP BY queryData.plan_handle, queryData.SQLStatement, queryData.creation_time
ORDER BY queryData.plan_handle, queryData.SQLStatement, queryData.creation_time

SELECT 
	plan_handle AS PlanHandle,
	SqlStatement,
	creation_time AS CreationTime,
	execution_count AS ExecutionCount,
	--total_logical_writes,
	--total_logical_reads,
	--total_physical_reads,
	case 
		WHEN
			( 
			SqlStatement like '%INSERT %' 
			or SqlStatement like '%UPDATE %'
			or SqlStatement like '%INTO %'
			or SqlStatement like '%DELETE %'
			or SqlStatement like '%MERGE %'
			or SqlStatement like '%WRITE %'
			)
			AND SqlStatement NOT LIKE '%Exclude this query%' --Exclude this query from writes
		then 'Writes'
		when 
			SqlStatement like '%SELECT %'
			or SqlStatement like '%READ %' 
		then 'Reads'
		when (total_logical_writes) > 0 then 'Writes'
		when (total_logical_reads + total_physical_reads) > 0 then 'Reads'
		else 'Reads'
	end as QueryType
FROM @SqlText