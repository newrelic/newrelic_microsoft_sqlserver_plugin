-- sys.dm_db_resource_stats exposes 'fine-grained', near real-time resource consumption data,
-- expressed as a percentage of the maximum allowed DTU limits for the service tier/performance
-- level that the db is running (https://msdn.microsoft.com/en-us/library/dn800981.aspx)

SELECT
	AVG(avg_cpu_percent) AS [AvgCpuPercent], 
	MAX(avg_cpu_percent) AS [MaxCpuPercent],
	AVG(avg_data_io_percent) AS [AvgDataIoPercent], 
	MAX(avg_data_io_percent) AS [MaxDataIoPercent],
	AVG(avg_log_write_percent) AS [AvgLogWritePercent], 
	MAX(avg_log_write_percent) AS [MaxLogWritePercent],
	AVG(avg_memory_usage_percent) AS [AvgMemoryUsagePercent], 
	MAX(avg_memory_usage_percent) AS [MaxMemoryUsagePercent]
FROM (SELECT TOP 4 * FROM sys.dm_db_resource_stats) t
-- TOP 4 since plugin works in 1min interval, table is filled in 15sec interval