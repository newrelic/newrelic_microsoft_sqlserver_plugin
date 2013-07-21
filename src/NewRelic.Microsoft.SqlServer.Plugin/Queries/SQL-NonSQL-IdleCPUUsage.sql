-- Pass start and end times if desired. 
-- Detail: Returns 1 line for each minute in range. 
-- Summary: Returns Average of range. 
-- Data collection nature: Realtime

-- DECLARE then SET to support SQL 2005
DECLARE @StartTime datetime
SET @StartTime = DATEADD(MINUTE, -2, GETDATE())

-- DECLARE then SET to support SQL 2005
DECLARE @EndTime datetime
SET @EndTime = GETDATE()


DECLARE @ts_now bigint
SELECT
	@ts_now = cpu_ticks / (cpu_ticks / ms_ticks)
FROM sys.dm_os_sys_info;

DECLARE @Results TABLE (
			RecordID bigint NOT NULL,
			EventTime datetime NOT NULL,
			SQLProcessUtilization tinyint NOT NULL,
			OtherProcessUtilization tinyint NOT NULL
		)

INSERT INTO @Results (RecordID
, EventTime
, SQLProcessUtilization
, OtherProcessUtilization)
	SELECT
		RecordID,
		DATEADD(ms, -1 * (@ts_now - [timestamp]), GETDATE())	AS EventTime,
		SQLProcessUtilization,
		-- Some times the values can report impossible values (>100% CPU usage).
		-- Correct for that here by forcing other process to 0 when system idle
		-- and SQL CPU usage are more than 100%.
		CASE
			WHEN SystemIdle + SQLProcessUtilization > 100 THEN 0 ELSE 100 - SystemIdle - SQLProcessUtilization
		END														AS OtherProcessUtilization
	FROM (SELECT
			record.value('(./Record/@id)[1]', 'int')													AS RecordID,
			record.value('(./Record/SchedulerMonitorEvent/SystemHealth/SystemIdle)[1]', 'int')			AS SystemIdle,
			record.value('(./Record/SchedulerMonitorEvent/SystemHealth/ProcessUtilization)[1]', 'int')	AS SQLProcessUtilization,
			[timestamp]
		FROM (SELECT
				[timestamp],
				CONVERT(xml, record)	AS record
			FROM sys.dm_os_ring_buffers
			WHERE ring_buffer_type = N'RING_BUFFER_SCHEDULER_MONITOR'
			AND record LIKE '% %'
			AND DATEADD(ms, -1 * (@ts_now - [timestamp]), GETDATE()) BETWEEN @StartTime AND @EndTime)
		AS x)
	AS y

--Return details
SELECT TOP 1
	RecordID,
	EventTime,
	SQLProcessUtilization,
	OtherProcessUtilization
FROM @Results