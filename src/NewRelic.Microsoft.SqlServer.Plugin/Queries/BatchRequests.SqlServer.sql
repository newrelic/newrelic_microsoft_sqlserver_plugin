-- Batch Requests (needs to be processed for delta)
-- Not for Azure
-- For entire instance

select 
'Total Batch Requests' AS CounterName,
cntr_value AS TotalNumberOfBatches
from sys.dm_os_performance_counters
where counter_name = 'Batch Requests/sec'