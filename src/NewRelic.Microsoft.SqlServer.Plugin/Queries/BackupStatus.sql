-- Database Backup Status View
-- Returns status of every DB on instance except tempdb.
-- Assumes one backup a day is ok
-- No backup within a day or none ever is an error


SELECT
	SUBSTRING(s.name, 1, 40)				AS [DatabaseName],
	CAST(b.backup_start_date AS char(11))	AS [BackupDate],
	CASE
		WHEN b.backup_start_date > DATEADD(dd, -1, GETDATE()) THEN 'Backup is current within a day' ELSE 'Backup is NOT current within a day'
	END										AS [Comment],
	CASE
		WHEN b.backup_start_date > DATEADD(dd, -1, GETDATE()) THEN 'OK' ELSE 'ERROR'
	END										AS [Flag]
FROM master..sysdatabases s
LEFT OUTER JOIN msdb..backupset b ON s.name = b.database_name
	AND b.backup_start_date = (
		SELECT
			MAX(backup_start_date)
		FROM msdb..backupset
		WHERE database_name = b.database_name
		AND type = 'D') -- full database backups only, not log backups
WHERE s.name <> 'tempdb'