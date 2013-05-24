-- DML Activity
-- SQL and Azure
-- Feeds a graph to show normal data access in SQL by read and write statements


declare @SqlText as table(
sql_handle varbinary(64),
SqlStatement nvarchar(max))

INSERT INTO @SqlText
SELECT 
    sql_handle,
	SUBSTRING(
				st.TEXT, 
				(qs.statement_start_offset / 2) + 1, 
				((	CASE qs.statement_end_offset
						WHEN -1 THEN DATALENGTH(st.TEXT) 
						ELSE qs.statement_end_offset
					END - qs.statement_start_offset) / 2) + 1
	) AS SQLStatement
FROM sys.dm_exec_query_stats AS qs
CROSS APPLY sys.dm_exec_sql_text(qs.sql_handle) AS st
 
SELECT 
    qs.sql_handle,
	creation_time,
	execution_count AS ExecutionCount,
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
	--,SqlStatement
FROM sys.dm_exec_query_stats qs
	join @SqlText st
		on qs.[sql_handle] = st.[sql_handle]
 
