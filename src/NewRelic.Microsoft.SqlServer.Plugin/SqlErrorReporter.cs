using System;
using System.Data.SqlClient;
using System.Text.RegularExpressions;

using log4net;

namespace NewRelic.Microsoft.SqlServer.Plugin
{
	public static class SqlErrorReporter
	{
		public static void LogSqlException(this ILog log, SqlException sqlException, ISqlQuery query, string connString)
		{
			var connectionString = new SqlConnectionStringBuilder(connString);

			log.Error(string.Empty);

			switch (sqlException.Number)
			{
				case 297: // User cannot log on via Windows Auth
				case 18456: // User cannot login via SQL Auth
					if (connectionString.IntegratedSecurity)
					{
						// System.Data.SqlClient.SqlException: Login failed. The login is from an untrusted domain and cannot be used with Windows authentication.
						log.ErrorFormat("The Windows service is running as user '{0}', however, the user cannot access the server '{1}'. " +
						                "Consider changing the connection string in the configuration file " +
						                "or adding permissions to your SQL Server (see readme.md).",
						                Environment.UserName, connectionString.DataSource);
					}
					else
					{
						// System.Data.SqlClient.SqlException: Login failed for user '<user id>'.
						log.ErrorFormat("User '{0}' cannot access the server '{1}'. " +
						                "Consider changing the connection string in the configuration file " +
						                "or adding permissions to your SQL Server (see readme.md).",
						                connectionString.UserID, connectionString.DataSource);
					}
					break;

				case 4060: // Missing database user
					// System.Data.SqlClient.SqlException: Cannot open database "Junk" requested by the login. The login failed.
					if (connectionString.IntegratedSecurity)
					{
						log.ErrorFormat("The Windows service is running as user '{0}', however, the user cannot access the database '{1}'. " +
						                "Ensure the login has a user in the database (see readme.md).",
						                Environment.UserName, connectionString.InitialCatalog);
					}
					else
					{
						log.ErrorFormat("User '{0}' cannot access the database '{1}'. " +
						                "Ensure the login has a user in the database (see readme.md).",
						                connectionString.UserID, connectionString.InitialCatalog);
					}
					break;

				case 10060:
				case 10061:
				case 11001:
				case 40615:
					if (sqlException.Message.Contains("sp_set_firewall_rule"))
					{
						var relevantErrorMessage = Regex.Replace(sqlException.Message, @"change to take effect\.(.*)$", string.Empty, RegexOptions.Singleline);
						log.Error("Azure SQL Error: " + relevantErrorMessage);
					}
					else
					{
						log.ErrorFormat("Timeout connecting to server at '{0}'. Verify that the connection string is correct and the server is reachable.",
						                connectionString.DataSource);
					}
					break;

				default:
					log.ErrorFormat("Error collecting metric '{0}': {1}", query.QueryName, sqlException.Message);
					log.ErrorFormat("SQL Exception Details: Class {0}, Number {1}, State {2}", sqlException.Class, sqlException.Number, sqlException.State);
					log.Error(@"Check the error log for more details at 'C:\ProgramData\New Relic\MicrosoftSQLServerPlugin\ErrorDetailOutput.log'");
					log.Error("For additional help, contact New Relic support at https://support.newrelic.com/home. Please paste all log messages above into the support request.");
					break;
			}
		}
	}
}
