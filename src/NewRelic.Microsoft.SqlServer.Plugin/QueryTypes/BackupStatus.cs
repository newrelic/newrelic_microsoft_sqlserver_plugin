using NewRelic.Microsoft.SqlServer.Plugin.Core;

namespace NewRelic.Microsoft.SqlServer.Plugin.QueryTypes
{
	[SqlMonitorQuery("BackupStatus.sql")]
	[SqlMonitorQuery("LogBackupStatus.sql")]
	internal class BackupStatus
	{
		public string Database { get; set; }
		public string BackupDate { get; set; }
		public string Comment { get; set; }
		public string Flag { get; set; }
	}
}
