## New Relic Microsoft SQL Server Plugin

A plugin for monitoring Microsoft SQL Server using the New Relic platform.

## Installation instructions

1. Download the files
2. Unpack them to something like `C:\Program Files\NewRelic\MicrosoftSQLServerPlugin\`. We'll call this `INSTALLDIR`.
3. Configure the plugin.
  1. Open the file `INSTALLDIR\NewRelic.Microsoft.SqlServer.Plugin.exe.config` in a text editor (running as Administrator).
  2. Find the setting `<service licenseKey="YOUR_KEY_HERE"...>` and replace `YOUR_KEY_HERE` with your New Relic license key.
  3. Add a `<sqlServer>` setting for a database you wish to monitor.
     `<sqlServer name="Production Database"` The name of your server is visible on the New Relic dashboard.
     `connectionString="Server=ProductionSqlInstance;Database=master;Trusted_Connection=True;">` Any valid connection string to your database.
4. Verify the settings.
  1. Open a command prompt running as Administrator to `INSTALLDIR`.
  2. Run the plugin in read-only mode: `NewRelic.Microsoft.SqlServer.Plugin.exe --collect-only`
  3. If there are no errors, move on to installing the service.
5. Install the plugin as a Windows service.
  1. Use the command prompt from step #4.1 or open it again.
  2. Execute: `NewRelic.Microsoft.SqlServer.Plugin.exe --install` and ensure you see the message

     `Service NewRelicSQLServerPlugin has been successfully installed.`

6. If you are using `TrustedConnection=True` in your connection string, you may wish to set the user for the Windows service to a user that is permitted to access the SQL Server you are monitoring.

## Uninstalling the plugin

1. Open a command prompt running as Administrator to `INSTALLDIR`.
2. Execute: `NewRelic.Microsoft.SqlServer.Plugin.exe --uninstall` and ensure you see the message

   `Service NewRelicSQLServerPlugin was successfully removed from the system.`

3. Delete the folder that we refer to as `INSTALLDIR` (e.g. `C:\Program Files\NewRelic\MicrosoftSQLServerPlugin\`)