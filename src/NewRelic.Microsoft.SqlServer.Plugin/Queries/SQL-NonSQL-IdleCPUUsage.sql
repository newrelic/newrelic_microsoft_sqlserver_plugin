-- Pass start and end times if desired. 
-- Detail: Returns 1 line for each minute in range. 
-- Summary: Returns Average of range. 

DECLARE	@StartTime datetime = DATEADD(MINUTE, -10, GETDATE()),
		@EndTime datetime = GETDATE()


DECLARE @ts_now bigint
SELECT
	@ts_now = cpu_ticks / (cpu_ticks / ms_ticks)
FROM sys.dm_os_sys_info;

DECLARE @Results TABLE (
	RecordID bigint NOT NULL,
	EventTime datetime NOT NULL,
	SQLProcessUtilization tinyint NOT NULL,
	SystemIdle tinyint NOT NULL,
	OtherProcessUtilization tinyint NOT NULL
)

INSERT INTO @Results (RecordID
, EventTime
, SQLProcessUtilization
, SystemIdle
, OtherProcessUtilization)
	SELECT
		RecordID,
		DATEADD(ms, -1 * (@ts_now - [timestamp]), GETDATE()) AS EventTime,
		SQLProcessUtilization,
		SystemIdle,
		100 - SystemIdle - SQLProcessUtilization AS OtherProcessUtilization
	FROM (SELECT
		record.value('(./Record/@id)[1]', 'int') AS RecordID,
		record.value('(./Record/SchedulerMonitorEvent/SystemHealth/SystemIdle)[1]',
		'int') AS SystemIdle,
		record.value('(./Record/SchedulerMonitorEvent/SystemHealth/ProcessUtilization)[1]',
		'int') AS SQLProcessUtilization,
		TIMESTAMP
	FROM (SELECT
		TIMESTAMP,
		CONVERT(xml, record) AS record
	FROM sys.dm_os_ring_buffers
	WHERE ring_buffer_type = N'RING_BUFFER_SCHEDULER_MONITOR'
	AND record LIKE '% %'
	AND DATEADD(ms, -1 * (@ts_now - [timestamp]), GETDATE()) BETWEEN @StartTime AND @EndTime)
	AS x)
	AS y


--Return details
SELECT
	RecordID,
	EventTime,
	SQLProcessUtilization,
	SystemIdle,
	OtherProcessUtilization
FROM @Results


--Return average
/*
SELECT
	AVG(SQLProcessUtilization) AS AverageSQLCPUUsage,
	AVG(SystemIdle) AS AverageIdle,
	AVG(OtherProcessUtilization) AS AverageOther,
	MIN(EventTime) StartTime,
	MAX(EventTime) EndTime
FROM @Results
*/