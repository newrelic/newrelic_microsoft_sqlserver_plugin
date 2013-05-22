-- Memory Page Life NUMA
-- SQL not Azure
-- This returns page life per CPU/Memory Node. All for multiple records.

select 'Node: ' + cast(instance_name as Varchar(10)) as NUMA_Node, cntr_value
 FROM sys.dm_os_performance_counters
 WHERE counter_name = 'Page life expectancy' AND [OBJECT_NAME] = 'SQLServer:Buffer Node'