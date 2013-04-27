-- Pass start and end times if desired. 
-- Detail: Returns 1 line for each minute in range. 
-- Summary: Returns Average of range. 

DECLARE 
	@StartTime DATETIME = dateadd(MINUTE,-10,getdate())
	,@EndTime DATETIME = getdate()
	,@ShowDetails BIT = 1 -- 1 = True, 0 = False


DECLARE @ts_now BIGINT
SELECT @ts_now = cpu_ticks / (cpu_ticks / ms_ticks)
FROM sys.dm_os_sys_info;

DECLARE @Results TABLE
	(record_ID BIGINT NOT NULL
	,EventTime datetime NOT NULL
	,SQLProcessUtilization tinyint NOT NULL
	,SystemIdle tinyint NOT NULL
	,OtherProcessUtilization tinyint NOT NULL
  )  

INSERT INTO 
	@Results
(	
	record_ID
	,EventTime
	,SQLProcessUtilization
	,SystemIdle
	,OtherProcessUtilization
)  
SELECT
    record_id
   ,DATEADD(ms, -1 * (@ts_now - [timestamp]), GETDATE()) AS EventTime
   ,SQLProcessUtilization
   ,SystemIdle
   ,100 - SystemIdle - SQLProcessUtilization AS OtherProcessUtilization
FROM
    (
     SELECT
        record.value('(./Record/@id)[1]', 'int') AS record_id
       ,record.value('(./Record/SchedulerMonitorEvent/SystemHealth/SystemIdle)[1]',
                     'int') AS SystemIdle
       ,record.value('(./Record/SchedulerMonitorEvent/SystemHealth/ProcessUtilization)[1]',
                     'int') AS SQLProcessUtilization
       ,TIMESTAMP
     FROM
        (
         SELECT
            TIMESTAMP
           ,CONVERT(XML, record) AS record
         FROM
            sys.dm_os_ring_buffers
         WHERE
            ring_buffer_type = N'RING_BUFFER_SCHEDULER_MONITOR'
            AND record LIKE '% %'
			AND DATEADD(ms, -1 * (@ts_now - [timestamp]), GETDATE()) BETWEEN @StartTime AND @EndTime
        ) AS x
    ) AS y


--Return details
IF @ShowDetails = 1
BEGIN
	SELECT     
		record_ID
	   ,EventTime
	   ,SQLProcessUtilization
	   ,SystemIdle
	   ,OtherProcessUtilization
	FROM @Results
END


--Return average
SELECT
	AVG(SQLProcessUtilization) as AverageSQLCPUUsage,
	AVG(SystemIdle) AS AverageIdle,
	AVG(OtherProcessUtilization) AS AverageOther
	,MIN(EVENTTIME) StartTime
	,MAX(EVENTTIME) EndTime
FROM
	@Results		

