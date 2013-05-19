-- SQL Azure Database Storage Use
-- Must be connected to Database you are reporting on
-- Master will not work


SELECT (SUM(reserved_page_count) * 8192) / 1048576 AS DbSizeInMB
FROM    sys.dm_db_partition_stats