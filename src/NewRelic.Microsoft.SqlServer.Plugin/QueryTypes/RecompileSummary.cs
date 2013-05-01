using NewRelic.Microsoft.SqlServer.Plugin.Core;

namespace NewRelic.Microsoft.SqlServer.Plugin.QueryTypes
{
	[Query("RecompileSummary.sql", "Custom/Recompiles/{DatabaseName}", QueryName = "Recompile Summary", Enabled = true)]
	public class RecompileSummary : IDatabaseMetric
	{
		public int SingleUseObjects { get; set; }
		public int MultipleUseObjects { get; set; }
		public decimal SingleUsePercent { get; set; }
		public string DatabaseName { get; set; }

		public override string ToString()
		{
			var dbName = string.IsNullOrEmpty(DatabaseName) ? "(none)" : DatabaseName;
			return string.Format("{0}\t{1}\t{2}\t{3}", dbName, SingleUseObjects, MultipleUseObjects, SingleUsePercent);
		}
	}
}
