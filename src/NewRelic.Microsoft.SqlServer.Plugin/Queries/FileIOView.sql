-- File I/O View. This data will reset on SQL Server Restart.
-- Size in Bytes good for basic pie or stacked bar chart etc.

SELECT
	CAST(DB_NAME(a.database_id) AS varchar(150)) AS DatabaseName,
	SUM(a.num_of_bytes_read) AS BytesRead,
	SUM(a.num_of_bytes_written) AS BytesWritten,
	SUM(a.size_on_disk_bytes) AS SizeInBytes,
	SUM(a.num_of_reads) AS NumberOfReads,
	SUM(a.num_of_writes) AS NumberOfWrites

FROM sys.dm_io_virtual_file_stats(NULL, NULL) a
GROUP BY CAST(DB_NAME(a.database_id) AS varchar(150))
ORDER BY CAST(DB_NAME(a.database_id) AS varchar(150))