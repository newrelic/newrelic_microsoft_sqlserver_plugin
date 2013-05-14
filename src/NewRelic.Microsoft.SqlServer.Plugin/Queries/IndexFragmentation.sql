-- Index Fragmentation 
-- Returns DB, Related Table, Fragmentation and index name for index objects with fragmentation over @FragPercent
-- This could take 2 - 15 minutes to run on a server with many large DBs. This returns important information, so an execution strategy is needed.
-- Data collection nature: Realtime


DECLARE @FragPercent int
SET @FragPercent = 10

SELECT
	DB_NAME(pis.database_id) AS DBName,
	OBJECT_NAME(pis.object_id) AS TableName,
	si.name AS ObjectName,
	pis.avg_fragmentation_in_percent
FROM sys.dm_db_index_physical_stats(NULL, NULL, NULL, NULL, 'LIMITED') pis
JOIN sysindexes si
	ON pis.object_id = si.id
	AND
	pis.index_id = si.indid
WHERE pis.avg_fragmentation_in_percent > @FragPercent
ORDER BY pis.avg_fragmentation_in_percent DESC