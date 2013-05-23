-- SQL Azure Database Storage Use
-- Must be connected to Database you are reporting on
-- Master will not work


--When the database space allotted to user db is full, the user gets a db full error.  Non-Select DML (Insert, Update, Merge that inserts or updates) are denied.
--Error returned:
--   40544 : The database has reached its size quota. Partition or delete data, drop indexes, or consult the documentation for possible resolutions.  Incident ID: <ID>. Code: <code>.
--            Limit
--150 GB (or less for DBs with smaller quotas)
--Type of requests throttled: Inserts, Updates


SELECT (SUM(reserved_page_count) * 8192) / 1048576 AS DbSizeInMB
FROM    sys.dm_db_partition_stats