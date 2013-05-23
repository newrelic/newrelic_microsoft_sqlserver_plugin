SELECT
	d.Name							AS DatabaseName,
	COUNT(c.connection_id)			AS NumberOfConnections,
	ISNULL(SUM(c.num_reads), 0)		AS NumberOfReads,
	ISNULL(SUM(c.num_writes), 0)	AS NumberOfWrites
FROM sys.databases d
LEFT JOIN sys.sysprocesses s ON s.dbid = d.database_id
LEFT JOIN sys.dm_exec_connections c ON c.session_id = s.spid
WHERE (s.spid IS NULL OR c.session_id >= 51)
/*{AND_WHERE}*/
GROUP BY d.Name