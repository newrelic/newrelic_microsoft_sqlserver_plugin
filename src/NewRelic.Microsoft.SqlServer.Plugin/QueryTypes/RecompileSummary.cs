using NewRelic.Microsoft.SqlServer.Plugin.Core;
using NewRelic.Platform.Binding.DotNET;

namespace NewRelic.Microsoft.SqlServer.Plugin.QueryTypes
{
	[SqlMonitorQuery("RecompileSummary.sql", QueryName = "Recompile Summary", Enabled = true)]
	public class RecompileSummary : IQueryResult
	{
		public string DBName { get; set; }
		public int SingleUseObjects { get; set; }
		public int MultipleUseObjects { get; set; }
		public decimal SingleUsePercent { get; set; }

		public void AddMetrics(ComponentData componentData)
		{
			var dbName = string.IsNullOrEmpty(DBName) ? "(none)" : DBName;
			var recompileType = string.Format("Recompile/{0}/", dbName);
			componentData.AddMetric(recompileType + "SingleUseObjects", SingleUseObjects);
			componentData.AddMetric(recompileType + "MultipleUseObjects", MultipleUseObjects);
			componentData.AddMetric(recompileType + "SingleUsePercent", SingleUsePercent);
		}

		public override string ToString()
		{
			var dbName = string.IsNullOrEmpty(DBName) ? "(none)" : DBName;
			return string.Format("{0}\t{1}\t{2}\t{3}", dbName, SingleUseObjects, MultipleUseObjects, SingleUsePercent);
		}
	}
}
