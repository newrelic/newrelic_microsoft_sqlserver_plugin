using NewRelic.Microsoft.SqlServer.Plugin.Core;

namespace NewRelic.Microsoft.SqlServer.Plugin.QueryTypes
{
	[Query("BackupStatus.sql", "Custom/BackupStatus/{DatabaseName}", QueryName = "Backup Status", Enabled = false)]
	[Query("LogBackupStatus.sql", "Custom/LogBackupStatus/{DatabaseName}", QueryName = "Log Backup Status", Enabled = false)]
	internal class BackupStatus : IDatabaseMetric
	{
		public string BackupDate { get; set; }
		public string Comment { get; set; }
		public string Flag { get; set; }
		public string DatabaseName { get; set; }

		public string ParameterizeQuery(string commandText, string[] includeDBs, string[] excludeDBs)
		{
			// TODO
			return commandText;
		}
	}
}
