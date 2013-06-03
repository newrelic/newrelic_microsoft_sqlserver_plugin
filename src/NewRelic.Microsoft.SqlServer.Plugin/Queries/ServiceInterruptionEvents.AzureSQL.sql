-- Service Interruption Events
-- Master DB Azure
-- SQL Azure Only
-- Returns Certain Events that will interrupt service (including Throttling)

DECLARE @Types AS TABLE (
			EventType varchar(150),
			EventSubType varchar(150),
			Description varchar(150)
		)

DECLARE @DBAndTypes AS TABLE (
			DatabaseName varchar(150),
			EventType varchar(150),
			EventSubType varchar(150),
			Description varchar(150)
		)

INSERT INTO @Types
	VALUES ('Connectivity', 'idle_connection_timeout', 'Idle Connection Timeout')
INSERT INTO @Types
	VALUES ('Connectivity', 'failed_to_open_db', 'Failed To Open DB')
INSERT INTO @Types
	VALUES ('Connectivity', 'blocked_by_firewall', 'Blocked By Firewall')
INSERT INTO @Types
	VALUES ('Connectivity', 'login_failed_for_user', 'Login Failed')
INSERT INTO @Types
	VALUES ('Throttling', 'long_transaction', 'Long Transaction')
INSERT INTO @Types
	VALUES ('Throttling', 'excessive_lock_usage', 'Excessive Lock Usage')
INSERT INTO @Types
	VALUES ('Throttling', 'excessive_tempdb_usage', 'Excessive TempDB Usage')
INSERT INTO @Types
	VALUES ('Throttling', 'excessive_log_space_usage', 'Excessive Log Space Usage')
INSERT INTO @Types
	VALUES ('Throttling', 'excessive_memory_usage', 'Excessive Memory Usage')
INSERT INTO @Types
	VALUES ('Engine', 'deadlock', 'Deadlock')
INSERT INTO @Types
	VALUES ('Throttling', 'reason_code', 'Other Throttling Code')

INSERT INTO @DBAndTypes
	SELECT
		name	AS DatabaseName,
		EventType,
		EventSubType,
		Description
	FROM	(SELECT
					name
				FROM sys.databases
				UNION
				SELECT
					'')
			AS DBs,
			@Types

-- Since the log updates about 2-4 minutes after the end time,
-- exclude items more than 9 minutes old
-- Join to @Types to limit scope of max end_time to reduce noise from non aggregated items
DECLARE @latestEventWindow datetime = (SELECT
			MAX(e.end_time)
		FROM sys.event_log e
			JOIN @Types t
				ON e.event_subtype_desc = t.EventSubType
		WHERE DATEADD(MINUTE, 9, e.end_time) >= GETUTCDATE())


SELECT
	CASE
		WHEN t.DatabaseName = '' THEN 'N/A' ELSE t.DatabaseName
	END						AS DatabaseName,
	t.EventType,
	t.Description,
	ISNULL(event_count, 0)	AS EventCount
FROM @DBAndTypes t
LEFT JOIN sys.event_log e ON t.DatabaseName = e.database_name
	AND t.EventSubType = e.event_subtype_desc
	AND @latestEventWindow = e.end_time
/*{WHERE}*/
-- Beware of including items from sys.event_log in the WHERE. It will cause the LEFT JOIN to act like a JOIN.
ORDER BY t.DatabaseName,
	t.EventType,
	t.Description