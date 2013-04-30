using NewRelic.Platform.Binding.DotNET;

namespace NewRelic.Microsoft.SqlServer.Plugin.QueryTypes
{
	public interface IQueryResult
	{
		void AddMetrics(ComponentData componentData);
	}
}
