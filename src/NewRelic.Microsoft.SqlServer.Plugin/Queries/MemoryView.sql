-- Memory View
-- Returns ratio of memory cache hits to non-memory cache hits
-- Returns memory page life expectancy for entire instance for NUMA and non-NUMA processor/memory configs
SELECT
	(SELECT
		cntr_value
	FROM sys.dm_os_performance_counters
	WHERE counter_name = 'Buffer cache hit ratio') / (SELECT
														cntr_value
														FROM sys.dm_os_performance_counters
														WHERE counter_name = 'Buffer cache hit ratio base')
	* 100.00 AS 'BufferCacheHitRatio',
	(SELECT
		cntr_value
	FROM sys.dm_os_performance_counters
	WHERE counter_name = 'Page life expectancy' AND [OBJECT_NAME] = 'SQLServer:Buffer Manager')
	AS 'PageLifeExpectancyInSeconds',
	(SELECT
		cntr_value
	FROM sys.dm_os_performance_counters
	WHERE counter_name = 'Page life expectancy' AND [OBJECT_NAME] = 'SQLServer:Buffer Node')
	AS 'PageLifeExpectancyInSeconds-NUMA'
