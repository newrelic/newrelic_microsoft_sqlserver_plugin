## New Relic Microsoft SQL Server Plugin

A plugin for monitoring Microsoft SQL Server using the New Relic platform.

## System Requirements

1. .NET 3.5 or later
2. Windows 7/Server 2008 or later
3. SQL Server 2005 or later

## Installation instructions

1. [Download the files](https://rpm.newrelic.com/extensions/com.newrelic.platform.microsoft.sqlserver) from New Relic.
2. Unpack them to something like `C:\Program Files\New Relic\MicrosoftSQLServerPlugin\` (we'll call this `INSTALLDIR`.) on a server that has access to the SQL server(s) you want to monitor. In general, that means the agent could run on the server hosting the SQL server or another locally connected machine which network access to the SQL server. 
3. Configure the plugin.
  1. Run a text editor **as administrator** and open the file `INSTALLDIR\NewRelic.Microsoft.SqlServer.Plugin.exe.config`.
  2. Find the setting `<service licenseKey="YOUR_KEY_HERE"...>` and replace `YOUR_KEY_HERE` with your New Relic license key.
  3. Configure one or more SQL Servers or Azure SQL Databases
      * In the `<sqlServers>` section, add a `<sqlServer>` setting for _each_ SQL Server instance you wish to monitor.
          * `name="Production Database"` The name of your server is visible on the New Relic dashboard.
          * `connectionString="Server=prd.domain.com,1433;Database=master;Trusted_Connection=True;"` Any valid connection string to your database.
      * In the `<azure>` section, add a `<database>` setting for _each_ Windows Azure SQL Database.
          * `name="Production Database"` The name of your Azure SQL Database is visible on the New Relic dashboard.
          * Get the connection string from the [Azure Portal](https://manage.windowsazure.com/#Workspaces/SqlAzureExtension/Databases).<br/>
    `connectionString="Server=tcp:zzz.database.windows.net,1433;Database=CustomerDB;User ID=NewRelic@zzz;`
    `Password=foobar;Trusted_Connection=False;Encrypt=True;Connection Timeout=30;`
4. Verify the settings.
  1. Open a command prompt, **not** running as administrator, to `INSTALLDIR`.
  2. Run the plugin in read-only, test mode: `NewRelic.Microsoft.SqlServer.Plugin.exe --test`
  3. If there are no errors, move on to installing the service.
5. Install the plugin as a Windows service.
  1. Open a new command prompt, **running as administrator**, to `INSTALLDIR`.
  2. Execute: `NewRelic.Microsoft.SqlServer.Plugin.exe --install` and ensure you see the message
     `Service NewRelicSQLServerPlugin has been successfully installed.`
  3. Start the service: `net start NewRelicSQLServerPlugin`
  4. Review the log file at `C:\ProgramData\New Relic\MicrosoftSQLServerPlugin\SqlMonitor.log` to confirm the service is running. Look at the end of the file for a successful startup similar to the output of step 4.2., but with `[Main] INFO  -   Windows Service: Yes`, indicating the Windows service is running.
  5. After about 5 minutes, data is available in your New Relic dashboard in the 'MS SQL' and/or 'Azure SQL' area.

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

First, create the login in the `master` database for your Azure SQL Server and the user to query for service interuption metrics.

    CREATE LOGIN NewRelic WITH password='AnyPhraseHere';
    GO

    CREATE USER NewRelicUser FROM LOGIN NewRelic;
    GO
    
In a new connection to each individual Azure SQL Database:

    CREATE USER NewRelicUser FROM LOGIN NewRelic;
    GO
    
    GRANT VIEW DATABASE STATE TO NewRelicUser
    GO

## Proxy Support ##

For installations behind a proxy, the details are set in the config file.

    <!-- Proxy settings for connecting to the New Relic service. -->
    <!-- If a proxy is used, the host attribute is required.

    Attributes:
    host - The proxy server host name.
    port - The proxy server port (optional - defaults to 8080).
    user - The username used to authenticate with the proxy server (optional).
    password - The password used to authenticate with the proxy server (optional).
    domain - The domain used to authenticate with the proxy server (optional).
    useDefaultCredentials - 'true' or 'false. Uses the credentials of the account running the plugin (optional - defaults to false).
                            If specified, 'user' and 'password' are ignored.
    -->

    <!--
    <proxy host="hostname" />
    -->

If you are upgrading from version 1.0.9 or earlier, you'll need to replace the previous proxy settings with this new snippet in the config.

## Logging

By default, the log files are written to `C:\ProgramData\New Relic\MicrosoftSQLServerPlugin\`. To change the logging settings, edit the `INSTALLDIR\log4net.config` file.


## Troubleshooting

**Permissions Issues in the Database**

For each database to be monitored, execute a sample query to confirm the correct rights have been applied. Do this by launching SQL Server Management Studio (SSMS) as the user configured to host the service.

* When using Windows authentication, launch SSMS as the user. This can be done but holding down Ctrl+Shift when right-clicking on the SSMS shortcut. In the context menu is an entry for 'Run as a * different user' that prompts for credentials. Enter the username and password of the Windows user that will host the service.

* When using SQL authentication or Azure, launch SSMS normally. When prompted, select 'SQL Server Authentication' and enter the credentials supplied in your config file.

In each database, confirm the following query executes without failure:

SQL Server

    SELECT
        d.Name							AS DatabaseName,
        COUNT(c.connection_id)			AS NumberOfConnections,
        ISNULL(SUM(c.num_reads), 0)		AS NumberOfReads,
        ISNULL(SUM(c.num_writes), 0)	AS NumberOfWrites
    FROM sys.databases d
    LEFT JOIN sys.sysprocesses s ON s.dbid = d.database_id
    LEFT JOIN sys.dm_exec_connections c ON c.session_id = s.spid
    WHERE (s.spid IS NULL OR c.session_id >= 51)
    GROUP BY d.Name

Azure SQL

    SELECT
        ROW_NUMBER() OVER (ORDER BY [wait_time_ms] DESC)	AS [RowNum],
        [wait_type],
        [wait_time_ms] / 1000.0								AS [WaitSeconds],
        ([wait_time_ms] - [signal_wait_time_ms]) / 1000.0	AS [ResourceSeconds],
        [signal_wait_time_ms] / 1000.0						AS [SignalSeconds],
        [waiting_tasks_count]								AS [WaitCount],
        [wait_time_ms] * 100 / SUM([wait_time_ms]) OVER ()	AS [Percentage]
    FROM sys.dm_db_wait_stats

Additional plugin support and troubleshooting assistance can be obtained by visiting [support.newrelic.com](https://support.newrelic.com)

## Uninstall instructions

The plugin is installed as a Windows Service which by default is named NewRelicSqlPlugin
The service can be stopped or restarted manually, however if you want to uninstall the plugin perform the following steps

1. Open a command prompt running **as administrator** to `INSTALLDIR`.
2. Execute the following command: `NewRelic.Microsoft.SqlServer.Plugin.exe --uninstall`

This will stop and remove the service. The binaries and config files will still be in the `INSTALLDIR` 
and log files are not deleted. If you wish to remove these, it must be done manually.

## Credits
The New Relic Microsoft SQL Server plugin was originally authored by [Ed Chapel](https://github.com/edchapel), [Jesse Stromwick](https://github.com/jstromwick), and [Mike Merrill](https://github.com/infoone). Subsequent updates and support are provided by [New Relic](http://newrelic.com/platform).
