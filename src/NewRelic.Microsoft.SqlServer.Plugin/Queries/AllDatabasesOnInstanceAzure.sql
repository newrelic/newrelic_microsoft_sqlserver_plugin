-- All Databases on instance Azure
-- Must connect to master. If you connect to non master, you get master and that DB only

select 
Name,
Database_id
from sys.databases

select * from sys.dm_exec_requests