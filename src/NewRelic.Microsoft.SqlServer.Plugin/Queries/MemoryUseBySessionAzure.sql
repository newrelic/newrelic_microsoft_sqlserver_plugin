-- Memory Use By Session Azure
-- For current DB context

--When there are sessions waiting on memory for 20 seconds or more, sessions consuming greater than 16 MB for more than 20 seconds are terminated in the descending order of time the resource has been held, so that the oldest session is terminated first. Termination of sessions stops as soon as the required memory becomes available. 
--Error returned
--        40553 : The session has been terminated because of excessive memory usage. Try modifying your query to process fewer rows.
--                Limit
 

select
session_id,
(memory_usage * (8 * 1024))/1024 as MemoryUsageinKB
from sys.dm_exec_sessions