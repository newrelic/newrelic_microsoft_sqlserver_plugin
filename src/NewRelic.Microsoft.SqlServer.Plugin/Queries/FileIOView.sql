-- File I/O View. This data will reset on SQL Server Restart.
-- Size in Bytes good for basic pie or stacked bar chart etc.
-- Data collection nature: Both Cumulative. Needs deltas.

SELECT
	d.name						AS DatabaseName,
	SUM(a.num_of_bytes_read)	AS BytesRead,
	SUM(a.num_of_bytes_written)	AS BytesWritten,
	SUM(a.size_on_disk_bytes)	AS SizeInBytes,
	SUM(a.num_of_reads)			AS NumberOfReads,
	SUM(a.num_of_writes)		AS NumberOfWrites
FROM sys.databases d
LEFT JOIN sys.dm_io_virtual_file_stats(NULL, NULL) a ON d.database_id = a.database_id
/*{WHERE}*/
GROUP BY d.name
ORDER BY d.name