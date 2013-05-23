-- Number Of Sessions Azure
-- For DB Context


--Each database has a limit on the number of connections that can be made to it, specified by number of sessions that can be established. When session limit for a database is reached, new connections to the database are denied and user will receive error code 10928. Existing sessions are not terminated.
--Error returned
--         10928 : Resource ID: %d. The %s limit for the database is %d and has been reached. See http://go.microsoft.com/fwlink/?LinkId=267637  for assistance.
--Resource ID in error message indicates the resource for which limit has been reached. For sessions, Resource ID = 2.



select
count(session_id) as NumberOfSessions
from sys.dm_exec_sessions