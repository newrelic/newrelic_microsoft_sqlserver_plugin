-- Memory View
-- Returns ratio of memory cache hits to non-memory cache hits
-- Returns memory page life expectancy for entire instance for NUMA and non-NUMA processor/memory configs
-- Data collection nature: Cumulative. But this might be just fine for trending. Note: Server resets will zero out the page lifes.
SELECT
	(SELECT
		cntr_value * 100.00
	FROM sys.dm_os_performance_counters
	WHERE counter_name = 'Buffer cache hit ratio')
	/ (SELECT
		cntr_value
	FROM sys.dm_os_performance_counters
	WHERE counter_name = 'Buffer cache hit ratio base')
	AS BufferCacheHitRatio,
	(SELECT
		cntr_value
	FROM sys.dm_os_performance_counters
	WHERE counter_name = 'Page life expectancy'
	-- The OBJECT_NAME is fixed width with many trailing spaces
	AND RTRIM([object_name]) LIKE '%:Buffer Manager')
	AS PageLife