using NewRelic.Microsoft.SqlServer.Plugin.Core;
using NewRelic.Platform.Binding.DotNET;

namespace NewRelic.Microsoft.SqlServer.Plugin.QueryTypes
{
	[SqlMonitorQuery("RecompileSummary.sql", QueryName = "Recompile Summary", Enabled = true)]
	public class RecompileSummary : IQueryResult
	{
		public string DBName { get; set; }
		public string ObjectType { get; set; }
		public int SingleUseObjects { get; set; }
		public int MultiUseObjects { get; set; }
		public decimal SingleUsePercent { get; set; }

		public string DefineComponent(string sqlInstance)
		{
			return string.Format(@"{0} - {1}", sqlInstance, DBName);
		}

		public void AddMetrics(ComponentData componentData)
		{
			var recompileType = string.Format("Recompile/{0}/", ObjectType);
			componentData.AddMetric(recompileType + "SingleUseObjects", SingleUseObjects);
			componentData.AddMetric(recompileType + "MultiUseObjects", MultiUseObjects);
			componentData.AddMetric(recompileType + "SingleUsePercent", SingleUsePercent);
		}

		public override string ToString()
		{
			return string.Format("{0}\t{1}\t{2}\t{3}\t{4}", DBName, ObjectType, SingleUseObjects, MultiUseObjects, SingleUsePercent);
		}
	}
}
