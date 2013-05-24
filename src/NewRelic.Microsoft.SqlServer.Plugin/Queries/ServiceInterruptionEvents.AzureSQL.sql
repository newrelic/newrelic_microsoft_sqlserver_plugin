-- Service Interruption Events
-- Master DB Azure
-- SQL Azure Only
-- Returns Certain Events that will interrupt service (including Throttling)

declare @Types as Table(
event_type varchar(150),
event_subtype varchar(150),
UI_Text varchar(150)
)

declare @DBAndTypes as Table(
Database_Name varchar(150),
event_type varchar(150),
event_subtype varchar(150),
UI_Text varchar(150)
)

Insert into @Types
values('Connectivity', 'idle_connection_timeout', 'Idle Connection Timeout')
Insert into @Types
values('Connectivity', 'failed_to_open_db', 'Failed To Open DB')
Insert into @Types
values('Connectivity', 'blocked_by_firewall', 'Blocked By Firewall')
Insert into @Types
values('Connectivity', 'login_failed_for_user', 'Login Failed')
Insert into @Types
values('Throttling', 'long_transaction', 'Long Transaction')
Insert into @Types
values('Throttling', 'excessive_lock_usage', 'Excessive Lock Usage')
Insert into @Types
values('Throttling', 'excessive_tempdb_usage', 'Excessive TempDB Usage')
Insert into @Types
values('Throttling', 'excessive_log_space_usage', 'Excessive Log Space Usage')
Insert into @Types
values('Throttling', 'excessive_memory_usage', 'Excessive Memory Usage')
Insert into @Types
values('Engine', 'deadlock', 'Deadlock')
Insert into @Types
values('Throttling', 'reason_code', 'Other Throttling Code')

insert into @DBAndTypes
select 
name as DatabaseName, 
event_type,
event_subtype,
UI_Text
from 
	(
		select name from sys.databases
		union
		select '' 
	) as DBs
, @Types


SELECT 
case 
	when t.Database_Name = '' then 'N/A'
	else t.Database_Name
end as Database_Name,
t.event_type,
t.UI_Text,
sum(isnull(event_count,0)) as  event_count
FROM @DBAndTypes t
	left join sys.event_log e
		on t.Database_Name = e.database_name
		and t.event_subtype = e.event_subtype_desc
group by 
t.Database_Name,
t.event_type,
t.UI_Text
order by 
t.Database_Name,
t.event_type,
t.UI_Text


