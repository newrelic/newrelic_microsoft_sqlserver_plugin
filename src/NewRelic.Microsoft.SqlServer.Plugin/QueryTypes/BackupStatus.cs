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

		public override string ToString()
		{
			return string.Format("{0}\t{1}\t{2}\t{3}", DatabaseName, BackupDate, Comment, Flag);
		}
	}
}
