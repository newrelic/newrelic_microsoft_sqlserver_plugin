## New Relic Microsoft SQL Server Plugin

A plugin for monitoring Microsoft SQL Server using the New Relic platform.

## Installation instructions

1. [Download the files](https://s3.amazonaws.com/new_relic_platform_plugin_binary_hosting/ms_sql_plugin/NewRelic.Microsoft.SqlServer.Plugin.zip)
2. Unpack them to something like `C:\Program Files\New Relic\MicrosoftSQLServerPlugin\`. We'll call this `INSTALLDIR`.
3. Configure the plugin.
  1. Run a text editor **as administrator** and open the file `INSTALLDIR\NewRelic.Microsoft.SqlServer.Plugin.exe.config`.
  2. Find the setting `<service licenseKey="YOUR_KEY_HERE"...>` and replace `YOUR_KEY_HERE` with your New Relic license key.
  3. Configure one or more SQL Servers or Azure SQL Databases ([See example](Example-Config))
      * In the `<sqlServers>` section, add a `<sqlServer>` setting for a SQL Server.
          * `name="Production Database"` The name of your server is visible on the New Relic dashboard.
          * `connectionString="Server=prd.domain.com,1433;Database=master;Trusted_Connection=True;"` Any valid connection string to your database.
      * In the `<azure>` section, add a `<database>` setting for _each_ Windows Azure SQL Database.
          * `name="Production Database"` The name of your Azure SQL Database is visible on the New Relic dashboard.
          * Get the connection string from the [Azure Portal](https://manage.windowsazure.com/#Workspaces/SqlAzureExtension/Databases).<br/>
    `connectionString="Server=tcp:zzz.database.windows.net,1433;Database=CustomerDB;User ID=NewRelic@zzz;`
    `Password=foobar;Trusted_Connection=False;Encrypt=True;Connection Timeout=30;`
4. Verify the settings.
  1. Open a command prompt running **as administrator** to `INSTALLDIR`.
  2. Run the plugin in read-only mode: `NewRelic.Microsoft.SqlServer.Plugin.exe --collect-only`
  3. If there are no errors, move on to installing the service.
5. Install the plugin as a Windows service.
  1. Use the command prompt from step #4.1 or open it again.
  2. Execute: `NewRelic.Microsoft.SqlServer.Plugin.exe --install` and ensure you see the message
     `Service NewRelicSQLServerPlugin has been successfully installed.`

## Configure permissions

By default, the service installs itself as Local Service. Use the Services MMC to change the user when using a trusted connection.

**SQL Server - Trusted Connection**

When `Trusted_Connection=True` is used in the connection string, the plugin connects to SQL using the credentials from the Windows/domain user configured to run the Windows Service. In SQL Server, ensure a login is configured for this Windows/domain user. Finally, grant the minimum rights to the user to perform the queries.

For example, if the user were `THE_DOMAIN\NewRelic`, run the following SQL:

    USE [master];
    GO

    GRANT VIEW SERVER STATE TO [THE_DOMAIN\NewRelic];
    GO

Ensure the user has access to each database.

**SQL Server - SQL Login**

When using a SQL Login in the connection string, ensure the login is configured with the correct rights.

For example, add a login named `NewRelic`. Grant the rights it needs to make the queries.

    USE [master];
    GO

    CREATE LOGIN NewRelic WITH PASSWORD=N'AnyPhraseHere',
    DEFAULT_DATABASE=[master], CHECK_EXPIRATION=OFF,
    CHECK_POLICY=ON;
    GO

    GRANT VIEW SERVER STATE TO NewRelic;
    GO

Then create a user for each database to monitor. For example, with the database "TestData":

    USE TestData;
    GO

    CREATE USER NewRelicUser FOR LOGIN NewRelic;
    GO

**Windows Azure SQL Database**

Azure SQL is configured in two separate connections to the database. Use SSMS to access the `master` database.

First, create the login in the `master` database for your Azure SQL Server.

    CREATE LOGIN NewRelic WITH password='AnyPhraseHere';
    GO

In a new connection to each individual Azure SQL Database:

    CREATE USER NewRelicUser FROM LOGIN NewRelic;
    GO
    
    GRANT VIEW DATABASE STATE TO NewRelicUser
    GO

## Logging

By default, the log files are written to `C:\ProgramData\New Relic\MicrosoftSQLServerPlugin\`. To change the logging settings, edit the `INSTALLDIR\log4net.config` file.

## Want more information?

The [wiki](https://github.com/newrelic-platform/newrelic_microsoft_sqlserver_plugin/wiki/) may be what you are looking for.
