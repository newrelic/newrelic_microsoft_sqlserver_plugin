-- Memory View
-- Returns ratio of memory cache hits to non-memory cache hits
-- Returns memory page life expectancy for entire instance for NUMA and non-NUMA processor/memory configs
-- Data collection nature: Cumulative. But this might be just fine for trending. Note: Server resets will zero out the page lifes.
SELECT
	(
		SELECT cntr_value
		FROM sys.dm_os_performance_counters
		WHERE counter_name = 'Buffer cache hit ratio'
	) / (
		SELECT cntr_value
		FROM sys.dm_os_performance_counters
		WHERE counter_name = 'Buffer cache hit ratio base') AS BufferCacheHitRatio,
	(
		SELECT cntr_value
		FROM sys.dm_os_performance_counters
		WHERE counter_name = 'Page life expectancy' AND [OBJECT_NAME] = 'SQLServer:Buffer Manager') AS PageLifeExpectancyInSeconds,
	(
		SELECT cntr_value
		FROM sys.dm_os_performance_counters
		WHERE counter_name = 'Page life expectancy' AND [OBJECT_NAME] = 'SQLServer:Buffer Node') AS PageLifeExpectancyInSecondsNuma
