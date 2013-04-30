-- Aggregated Wait States. 
-- Is reset on SQL Server Reset
-- Expected wait states are omitted from view


WITH [TempWaitStates] AS (
		SELECT
			ROW_NUMBER() OVER (ORDER BY [wait_time_ms] DESC)	AS [RowNum],
			[wait_type],
			[wait_time_ms] / 1000.0								AS [WaitSeconds],
			([wait_time_ms] - [signal_wait_time_ms]) / 1000.0	AS [ResourceSeconds],
			[signal_wait_time_ms] / 1000.0						AS [SignalSeconds],
			[waiting_tasks_count]								AS [WaitCount],
			[wait_time_ms] * 100 / SUM([wait_time_ms]) OVER ()	AS [Percentage]
		FROM sys.dm_os_wait_stats
		WHERE [wait_type] NOT IN (
		'BROKER_EVENTHANDLER',
		'BROKER_TASK_STOP',
		'BROKER_TO_FLUSH',
		'CHECKPOINT_QUEUE',
		'CLR_AUTO_EVENT',
		'CLR_MANUAL_EVENT',
		'CLR_SEMAPHORE',
		'DIRTY_PAGE_POLL',
		'DISPATCHER_QUEUE_SEMAPHORE',
		'FT_IFTS_SCHEDULER_IDLE_WAIT',
		'FT_IFTSHC_MUTEX',
		'HADR_FILESTREAM_IOMGR_IOCOMPLETION',
		'LAZYWRITER_SLEEP',
		'LOGMGR_QUEUE',
		'ONDEMAND_TASK_QUEUE',
		'REQUEST_FOR_DEADLOCK_SEARCH',
		'RESOURCE_QUEUE',
		'SLEEP_SYSTEMTASK',
		'SLEEP_TASK',
		'SP_SERVER_DIAGNOSTICS_SLEEP',
		'SQLTRACE_BUFFER_FLUSH',
		'SQLTRACE_INCREMENTAL_FLUSH_SLEEP',
		'TRACEWRITE',
		'WAITFOR',
		'XE_DISPATCHER_JOIN',
		'XE_DISPATCHER_WAIT',
		'XE_TIMER_EVENT'
		))
SELECT
	[TWS1].[wait_type]														AS [WaitType],
	CAST([TWS1].[WaitSeconds] AS decimal(14, 2))							AS [WaitSeconds],
	CAST([TWS1].[ResourceSeconds] AS decimal(14, 2))						AS [ResourceSeconds],
	CAST([TWS1].[SignalSeconds] AS decimal(14, 2))							AS [SignalSeconds],
	[TWS1].[WaitCount]														AS [WaitCount],
	CAST(CAST([TWS1].[Percentage] AS decimal(4, 2)) / 100 AS decimal(4, 4))	AS [Percentage],
	CASE
		WHEN ISNULL([TWS1].[WaitCount], 0) = 0 THEN 0 ELSE CAST(([TWS1].[WaitSeconds] / [TWS1].[WaitCount]) AS decimal(14, 4))
	END																		AS [AvgWaitSeconds],
	CASE
		WHEN ISNULL([TWS1].[WaitCount], 0) = 0 THEN 0 ELSE CAST(([TWS1].[ResourceSeconds] / [TWS1].[WaitCount]) AS decimal(14, 4))
	END																		AS [AvgResourceSeconds],
	CASE
		WHEN ISNULL([TWS1].[WaitCount], 0) = 0 THEN 0 ELSE CAST(([TWS1].[SignalSeconds] / [TWS1].[WaitCount]) AS decimal(14, 4))
	END																		AS [AvgSignalSeconds]
FROM [TempWaitStates] AS [TWS1]
JOIN [TempWaitStates] AS [TWS2] ON [TWS2].[RowNum] <= [TWS1].[RowNum]
WHERE [TWS1].[Percentage] >= 0.1
GROUP BY	[TWS1].[RowNum],
			[TWS1].[wait_type],
			[TWS1].[WaitSeconds],
			[TWS1].[ResourceSeconds],
			[TWS1].[SignalSeconds],
			[TWS1].[WaitCount],
			[TWS1].[Percentage]
HAVING SUM([TWS2].[Percentage]) < 90 -- percentage of whole threshold
