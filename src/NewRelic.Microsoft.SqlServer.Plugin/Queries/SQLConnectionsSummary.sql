-- SQL Connections
-- Returns all external user connections to the server by DB
-- Data collection nature: Both Cumulative and Realtime. Count of connections is RT. Number of reads and writes is cumulative for the one connection.

SELECT
	CAST(DB_NAME(s.dbid) AS varchar(150)) AS DatabaseName,
	COUNT(c.connection_id) AS NumberOfConnections
FROM sys.dm_exec_connections c
JOIN sys.sysprocesses s
	ON c.session_id = s.spid
WHERE c.session_id >= 51
/*{AND_WHERE}*/
GROUP BY s.dbid
