-- SQL Connections
-- Returns all external user connections to the server by DB
-- Data collection nature: Both Cumulative and Realtime. Count of connections is RT. Number of reads and writes is cumulative for the one connection.

WITH Databases AS (
		SELECT
			d.database_id	AS dbid,
			d.name
		FROM sys.databases d)
SELECT
	d.Name							AS DatabaseName,
	COUNT(c.connection_id)			AS NumberOfConnections,
	ISNULL(SUM(c.num_reads), 0)		AS NumberOfReads,
	ISNULL(SUM(c.num_writes), 0)	AS NumberOfWrites
FROM Databases d
LEFT JOIN sys.sysprocesses s ON s.dbid = d.dbid
LEFT JOIN sys.dm_exec_connections c ON c.session_id = s.spid
WHERE s.spid IS NULL OR c.session_id >= 51
/*{AND_WHERE}*/
GROUP BY d.Name