-- Database Log Backup Status View
-- Returns status of every DB on instance except system dbs.
-- Assumes one backup an hour is ok
-- No backup within a day or none ever is an error

SELECT
	SUBSTRING(s.name, 1, 40) AS [Database],
	CAST(b.backup_start_date AS char(11)) AS [BackupDate],
	CASE
		WHEN b.backup_start_date > DATEADD(mi, -60, GETDATE()) THEN 'Log Backup OK to: ' + CAST(b.backup_start_date AS varchar(30)) ELSE 'Log Backup is NOT current within a hour'
	END AS [Comment],
	CASE
		WHEN b.backup_start_date > DATEADD(mi, -60, GETDATE()) THEN 'OK' ELSE 'ERROR'
	END AS [Flag]
FROM master..sysdatabases s
LEFT OUTER JOIN msdb..backupset b
	ON s.name = b.database_name
	AND b.backup_start_date = (SELECT
		MAX(backup_start_date)
	FROM msdb..backupset
	WHERE database_name = b.database_name
	AND type = 'L') -- Logs
WHERE s.name NOT IN ('tempdb', 'master', 'model', 'msdb')