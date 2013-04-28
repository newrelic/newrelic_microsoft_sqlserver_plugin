using NewRelic.Microsoft.SqlServer.Plugin.Core;

namespace NewRelic.Microsoft.SqlServer.Plugin.QueryTypes
{
	[SqlMonitorQuery("RecompileSummary.sql", QueryName = "Recompile Summary", Enabled = false)]
	public class RecompileSummary
	{
		public string DBName { get; set; }
		public string ObjectType { get; set; }
		public int SingleUseObjects { get; set; }
		public int MultiUseObjects { get; set; }
		public decimal SingleUsePercent { get; set; }
	}
}