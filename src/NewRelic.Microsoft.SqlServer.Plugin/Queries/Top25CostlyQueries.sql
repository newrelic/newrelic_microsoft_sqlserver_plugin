-- Top queries  

SELECT TOP 25

	total_worker_time / execution_count AS AverageCPU,
	total_worker_time AS TotalCPU,
	total_elapsed_time / execution_count AS AverageDuration,
	total_elapsed_time AS TotalDuration,
	(total_logical_reads + total_physical_reads) / execution_count AS AverageReads,
	(total_logical_reads + total_physical_reads) AS TotalReads,
	execution_count AS ExecutionCount,
	SUBSTRING(st.TEXT, (qs.statement_start_offset / 2) + 1, ((CASE qs.statement_end_offset
		WHEN -1 THEN DATALENGTH(st.TEXT) ELSE qs.statement_end_offset
	END - qs.statement_start_offset) / 2) + 1) AS SQLStatement
FROM sys.dm_exec_query_stats AS qs
CROSS APPLY sys.dm_exec_sql_text(qs.sql_handle) AS st
CROSS APPLY sys.dm_exec_query_plan(qs.plan_handle) AS qp
ORDER BY total_worker_time / execution_count DESC