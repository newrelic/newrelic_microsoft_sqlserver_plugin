-- All Databases on instance Azure
-- Must connect to master. If you connect to non master, you get master and that DB only

SELECT
	Name,
	Database_id
FROM sys.databases
