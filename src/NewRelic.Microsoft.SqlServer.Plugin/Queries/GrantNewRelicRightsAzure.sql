-- SQL Rights Grant for DMVs (Azure Database)

-- Have to be in master. Can't use a USE. Have to have fresh connection to master.
CREATE LOGIN NewRelic WITH password='abcd1234!';
GO

-- Have to be in the DB context for user creation. Can't use a USE. Have to have fresh connection to the user DB. 
CREATE USER NewRelicUser FROM LOGIN NewRelic;
GO
--Can't do this in master...
GRANT VIEW DATABASE STATE TO NewRelicUser
GO
