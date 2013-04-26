SELECT
	connection_id AS ConnectionId,
	client_net_address AS ClientNetAddress,
	num_reads AS NumberOfReads,
	num_writes AS NumberOfWrites
FROM sys.dm_exec_connections
WHERE session_id > 51