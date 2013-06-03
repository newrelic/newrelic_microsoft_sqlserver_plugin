-- Memory View for NUMA
-- Returns memory page life expectancy for each node for NUMA processor/memory configs

SELECT
	instance_name	AS Node,
	cntr_value		AS PageLife
FROM sys.dm_os_performance_counters
WHERE counter_name = 'Page life expectancy'
-- The OBJECT_NAME is fixed width with many trailing spaces
 AND RTRIM([OBJECT_NAME]) LIKE '%:Buffer Node'