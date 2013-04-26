select 
	connection_id,
	client_net_address,
	num_reads,
	num_writes
from sys.dm_exec_connections
where session_id > 51