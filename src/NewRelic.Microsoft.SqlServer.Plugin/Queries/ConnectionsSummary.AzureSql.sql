-- SQL Connections
-- Returns all external user connections to the server by DB
-- Data collection nature: Both Cumulative and Realtime. Count of connections is RT. Number of reads and writes is cumulative for the one connection.

SELECT
	DB_NAME() AS DatabaseName,
	COUNT(c.connection_id) AS NumberOfConnections
FROM sys.dm_exec_connections c
