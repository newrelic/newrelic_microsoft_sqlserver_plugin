-- Blocking Queries for Azure and SQL

SELECT
	der.session_id,
	der.plan_handle,
	der.sql_handle,
	der.request_id,
	der.start_time,
	der.status,
	der.command,
	der.database_id,
	der.user_id,
	der.wait_type,
	der.wait_time,
	der.last_wait_type,
	der.wait_resource,
	der.total_elapsed_time,
	der.cpu_time,
	der.transaction_isolation_level,
	der.row_count,
	st.text
FROM sys.dm_exec_requests der
CROSS APPLY sys.dm_exec_sql_text(der.sql_handle) AS st
WHERE der.blocking_session_id = 0
AND der.session_id IN (SELECT DISTINCT
								(blocking_session_id)
							FROM sys.dm_exec_requests)
GROUP BY	der.session_id,
			der.plan_handle,
			der.sql_handle,
			der.request_id,
			der.start_time,
			der.status,
			der.command,
			der.database_id,
			der.user_id,
			der.wait_type,
			der.wait_time,
			der.last_wait_type,
			der.wait_resource,
			der.total_elapsed_time,
			der.cpu_time,
			der.transaction_isolation_level,
			der.row_count,
			st.text
ORDER BY der.total_elapsed_time DESC