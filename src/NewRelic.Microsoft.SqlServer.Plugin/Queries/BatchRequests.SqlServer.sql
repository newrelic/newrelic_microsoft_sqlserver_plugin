-- Batch Requests (needs to be processed for delta)
-- Not for Azure
-- For entire instance

SELECT 
'Total Batch Requests' AS CounterName,
cntr_value AS TotalNumberOfBatches
FROM sys.dm_os_performance_counters
WHERE counter_name = 'Batch Requests/sec'