WITH Connections AS (SELECT
			COUNT(c.connection_id)			AS NumberOfConnections,
			ISNULL(SUM(c.num_reads), 0)		AS NumberOfReads,
			ISNULL(SUM(c.num_writes), 0)	AS NumberOfWrites
		FROM sys.dm_exec_connections c)
,
	MemoryUsage AS (SELECT
			-- Number Of Sessions Azure
			-- For DB Context
			--
			-- Each database has a limit on the number of connections that can be made to it, specified by number of sessions that can be established. When session limit for a database is reached, new connections to the database are denied and user will receive error code 10928. Existing sessions are not terminated.
			-- Error returned
			--     10928 : Resource ID: %d. The %s limit for the database is %d and has been reached. See http://go.microsoft.com/fwlink/?LinkId=267637  for assistance.
			-- Resource ID in error message indicates the resource for which limit has been reached. For sessions, Resource ID = 2.
			COUNT(s.session_id)		AS NumberOfSessions,
			-- Memory Use By Session Azure
			-- For current DB context
			--
			-- When there are sessions waiting on memory for 20 seconds or more, sessions consuming greater than 16 MB for more than 20 seconds are terminated in the descending order of time the resource has been held, so that the oldest session is terminated first. Termination of sessions stops as soon as the required memory becomes available. 
			-- Error returned
			--     40553 : The session has been terminated because of excessive memory usage. Try modifying your query to process fewer rows.
			--             Limit
			SUM(s.memory_usage) * 8	AS SumSessionMemoryUsageInKB,
			MIN(s.memory_usage) * 8	AS MinSessionMemoryUsageInKB,
			MAX(s.memory_usage) * 8	AS MaxSessionMemoryUsageInKB,
			AVG(s.memory_usage) * 8	AS AvgSessionMemoryUsageInKB
		FROM sys.dm_exec_sessions s)
SELECT --
	-- SQL Azure Database Storage Use
	-- Must be connected to Database you are reporting on
	-- Master will not work
	--
	-- When the database space allotted to user db is full, the user gets a db full error.  Non-Select DML (Insert, Update, Merge that inserts or updates) are denied.
	-- Error returned:
	--     40544 : The database has reached its size quota. Partition or delete data, drop indexes, or consult the documentation for possible resolutions.  Incident ID: <ID>. Code: <code>.
	-- Limit:
	--     150 GB (or less for DBs with smaller quotas)
	-- Type of requests throttled: Inserts, Updates
	(SELECT
			(SUM(reserved_page_count) * 8192) / 1048576
		FROM sys.dm_db_partition_stats)
	AS DbSizeInMB, --
	-- Total Current Requests Azure
	-- Returns total for current DB context
	--
	-- If number of concurrent requests made to a database exceed 400, all transactions that have been running for 1 minute or more are terminated.
	-- Error returned
	--     40549 : Session is terminated because you have a long-running transaction. Try shortening your transaction.
	--             Limit
	(SELECT
			COUNT(request_id)
		FROM sys.dm_exec_requests)
	AS TotalCurrentRequests,
	c.*,
	mu.*
FROM MemoryUsage mu
JOIN Connections c ON 1=1
