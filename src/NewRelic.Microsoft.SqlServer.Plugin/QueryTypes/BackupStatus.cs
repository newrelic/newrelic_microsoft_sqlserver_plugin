using System;
using NewRelic.Microsoft.SqlServer.Plugin.Core;
using NewRelic.Platform.Binding.DotNET;

namespace NewRelic.Microsoft.SqlServer.Plugin.QueryTypes
{
	[SqlMonitorQuery("BackupStatus.sql", QueryName = "Backup Status", Enabled = false)]
	[SqlMonitorQuery("LogBackupStatus.sql", QueryName = "Log Backup Status", Enabled = false)]
	internal class BackupStatus : IQueryResult
	{
		public string DatabaseName { get; set; }
		public string BackupDate { get; set; }
		public string Comment { get; set; }
		public string Flag { get; set; }

		public override string ToString()
		{
			return string.Format("{0}\t{1}\t{2}\t{3}", DatabaseName, BackupDate, Comment, Flag);
		}

		public void AddMetrics(ComponentData componentData)
		{
			throw new NotImplementedException();
		}
	}
}
