-- SQL Connections
-- Returns all external user connections to the server by DB
-- Data collection nature: Both Cumulative and Realtime. Count of connections is RT. Number of reads and writes is cumulative for the one connection.

SELECT
	c.connection_id AS ConnectionId,
	c.client_net_address AS ClientNetAddress,
	db_name(s.dbid) AS DatabaseName, 
	c.num_reads AS NumberOfReads,
	c.num_writes AS NumberOfWrites
FROM sys.dm_exec_connections c
	JOIN sys.sysprocesses s
		ON c.session_id = s.spid
WHERE c.session_id >= 51
/*{AND_WHERE}*/
