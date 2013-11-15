-- Resource statistics
-- Master DB Azure
-- SQL Azure Only
-- Microsoft introduced Windows Azure SQL Database Premium end of 2013. The Premium database
-- comes with reserved resources for CPU, Memory and IO. Use the new resource_stats view
-- to determine how much of your quota you are using.
-- http://msdn.microsoft.com/en-us/library/windowsazure/dn369873.aspx

SELECT TOP(1)
	avg_cpu_cores_used AS AvgCpuCoresUsed,
	avg_physical_read_iops AS AvgPhysicalReadIops,
	avg_physical_write_iops AS AvgPhysicalWriteIops,
	active_memory_used_kb AS ActiveMemoryUsed,
	active_session_count AS ActiveSessionCount,
	active_worker_count AS ActiveWorkerCount
FROM sys.resource_stats
/*{WHERE}*/
ORDER BY end_time desc