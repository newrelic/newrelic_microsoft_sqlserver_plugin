## New Relic Microsoft SQL Server Plugin - .NET

Find the New Relic Microsoft SQL Server plugin in the [New Relic storefront](http://newrelic.com/plugins/new-relic-inc/55)

Find the New Relic Microsoft SQL Server plugin in [Plugin Central](https://rpm.newrelic.com/extensions/com.newrelic.platform.microsoft.sqlserver)

## System Requirements

- A New Relic account. Sign up for a free account [here](http://newrelic.com)
- .NET 3.5
- Windows 7/Server 2008 or later
- SQL Server 2005 or later
- Network access to New Relic

## Installation

This plugin can be installed one of the following ways:

* [Option 1 - New Relic Platform Installer](#option-1--install-with-the-new-relic-platform-installer)
* [Option 2 - Manual Install](#option-2--install-manually)

### Option 1 - Install with the New Relic Platform Installer

The New Relic Platform Installer (NPI) is a simple, lightweight command line tool that helps you easily download, configure and manage New Relic Platform Plugins.  If you're interested in learning more simply go to [our forum category](https://discuss.newrelic.com/category/platform-plugins/platform-installer) and checkout the ['Getting Started' section](https://discuss.newrelic.com/t/getting-started-for-the-platform-installer/842).  If you have any questions, concerns or feedback, please do not hesitate to reach out through the forums as we greatly appreciate your feedback!

Once you've installed the NPI tool, run the following command:

```
	npi install com.newrelic.platform.microsoft.sqlserver
```	

This command will take care of the creation of `newrelic.json` and `plugin.json` configuration files.  See the [configuration information](#configuration-information) section for more information.

### Option 2 - Install Manually (Non-standard)

#### Step 1 - Downloading and Extracting the Plugin

The latest version of the plugin can be downloaded [here](https://github.com/newrelic-platform/newrelic_microsoft_sqlserver_plugin/releases).  Once the plugin is on your box, extract it to a location of your choosing.

#### Step 2 - Configuring the Plugin

Check out the [configuration information](#configuration-information) section for details on configuring your plugin. 

#### Step 3 - Running the Plugin

To run the plugin, execute the following command from a terminal or command window (assuming you are in the directory where the plugin was extracted):

```
	plugin.exe
```
 
#### Step 4 - Keeping the Plugin Running

Step 3 showed you how to run the plugin; however, there are several problems with running the process directly in the foreground (For example, when the machine reboots the process will not be started again).  That said, there are several common ways to keep a plugin running, but they do require more advanced knowledge or additional tooling.  We highly recommend considering using the [New Relic Platform Installer](https://discuss.newrelic.com/t/getting-started-with-the-platform-installer/842) as it will take care of most of the heavy lifting for you.  

If you prefer to be more involved in the maintaince of the process, you can use the following tool to create a Windows service.  

- [WinSW - Third-party Service Wrapper](https://github.com/kohsuke/winsw)

----

## Configuration Information

### Configuration Files

You will need to modify two configuration files in order to set this plugin up to run.  The first (`newrelic.json`) contains configurations used by all Platform plugins (e.g. license key, logging information, proxy settings) and can be shared across your plugins.  The second (`plugin.json`) contains data specific to each plugin such as a list of hosts and port combination for what you are monitoring.  Templates for both of these files should be located in the '`config`' directory in your extracted plugin folder. 

#### Configuring the `plugin.json` file: 

The `plugin.json` file has a provided template in the `config` directory named `plugin.template.json`.  If you are installing manually, make a copy of this template file and rename it to `plugin.json` (the New Relic Platform Installer will automatically handle creation of configuration files for you).  

Below is an example of the `plugin.json` file's contents, you can add multiple objects to the "agents" array to monitor different instances:

```
{
  "agents": [
    {
      "type" : "sqlserver",
      "name" : "Production Database",
      "connectionString" : "Server=hostname\\instanceName;Database=master;Trusted_Connection=True;",
      "includeSystemDatabases" : "false",
      "includes" : [
        {
          "name": "AdventureWorks",
          "displayName": "My AdventureWorks Database"
        }
      ],
      "excludes" : [
        {
          "name": "nameOfDatabaseToExclude"
        }
      ]
    },
    {
      "type" : "azure",
      "name" : "Azure Cloud Database",
      "connectionString" : <Your SQL Azure connection string>
    }
  ]
}
```

**note** - Notice you must escape '\' characters in your connection strings.

**note** - Set the "name" attribute to identify each MS SQL host, e.g. "Production" as this will be used to identify specific instances in the New Relic UI. 

**note** - Each JSON object in the 'agents' array should have a type of either 'sqlserver' or 'azure'.

**note** - Get your SQL Azure connection string from the [Azure Portal](https://manage.windowsazure.com/#Workspaces/SqlAzureExtension/Databases).

#### Configuring the `newrelic.json` file: 

The `newrelic.json` file also has a provided template in the `config` directory named `newrelic.template.json`.  If you are installing manually, make a copy of this template file and rename it to `newrelic.json` (again, the New Relic Platform Installer will automatically handle this for you).  

The `newrelic.json` is a standardized file containing configuration information that applies to any plugin (e.g. license key, logging, proxy settings), so going forward you will be able to copy a single `newrelic.json` file from one plugin to another.  Below is a list of the configuration fields that can be managed through this file:

##### Configuring your New Relic License Key

Your New Relic license key is the only required field in the `newrelic.json` file as it is used to determine what account you are reporting to.  If you do not know what your license key is, you can learn about it [here](https://newrelic.com/docs/subscriptions/license-key).

Example: 

```
{
  "license_key": "YOUR_LICENSE_KEY_HERE"
}
```

##### Logging configuration

By default Platform plugins will have their logging turned on; however, you can manage these settings with the following configurations:

`log_level` - The log level. Valid values: [`debug`, `info`, `warn`, `error`, `fatal`]. Defaults to `info`.

`log_file_name` - The log file name. Defaults to `newrelic_plugin.log`.

`log_file_path` - The log file path. Defaults to `logs`.

`log_limit_in_kbytes` - The log file limit in kilobytes. Defaults to `25600` (25 MB). If limit is set to `0`, the log file size would not be limited.

Example:

```
{
  "license_key": "YOUR_LICENSE_KEY_HERE",
  "log_level": "debug",
  "log_file_path": "C:\\Logs"
}
```

##### Proxy configuration

If you are running your plugin from a machine that runs outbound traffic through a proxy, you can use the following optional configurations in your `newrelic.json` file:

`proxy_host` - The proxy host (e.g. `webcache.example.com`)

`proxy_port` - The proxy port (e.g. `8080`).  Defaults to `80` if a `proxy_host` is set

`proxy_username` - The proxy username

`proxy_password` - The proxy password

Example:

```
{
  "license_key": "YOUR_LICENSE_KEY_HERE",
  "proxy_host": "proxy.mycompany.com",
  "proxy_port": 9000
}
```

### Additional Configuration
 
#### Configure permissions

By default, most services will be installed as Local Service. Use the Services MMC to change the user when using a trusted connection.

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


### Troubleshooting

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

----

To determine SQL SERVER Instance
	  SELECT SERVERPROPERTY('ComputerNamePhysicalNetBIOS') AS ServerName, 
       		@@SERVERNAME AS FullInstanceName, @@SERVICENAME AS InstanceName, 
      		local_net_address AS InstanceIPAddress, local_tcp_port AS InstancePort
  	FROM sys.dm_exec_connections WHERE session_id = @@spid
	
If InstanceName is like MSSQLSERVER then server syntax should be Server={FullInstanceName};Database=master;

## Support

Find a bug? Post it to [http://support.newrelic.com](http://support.newrelic.com).

## Fork me!

The New Relic Platform uses an extensible architecture that allows you to define new metrics beyond the provided defaults. To expose more data about your MS SQL servers, fork this repository, create a new GUID, add the metrics you would like to collect to the code and then build summary metrics and dashboards to expose your newly collected metrics.

## Contributing
 
 You are welcome to send pull requests to us - however, by doing so you agree that you are granting New Relic a non-exclusive, non-revokable, no-cost license to use the code, algorithms, patents, and ideas in that code in our products if we so choose. You also agree the code is provided as-is and you provide no warranties as to its fitness or correctness for any purpose.

## Credits
The New Relic Microsoft SQL Server plugin was originally authored by [Ed Chapel](https://github.com/edchapel), [Jesse Stromwick](https://github.com/jstromwick), and [Mike Merrill](https://github.com/infoone). Subsequent updates and support are provided by [New Relic](http://newrelic.com/platform).
