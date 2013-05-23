-- SQL Connections
-- Returns all external user connections to the server by DB
-- Data collection nature: Both Cumulative and Realtime. Count of connections is RT. Number of reads and writes is cumulative for the one connection.

SELECT
	COUNT(c.connection_id)			AS NumberOfConnections,
	ISNULL(SUM(c.num_reads), 0)		AS NumberOfReads,
	ISNULL(SUM(c.num_writes), 0)	AS NumberOfWrites
FROM sys.dm_exec_connections c