using NewRelic.Platform.Binding.DotNET;

namespace NewRelic.Microsoft.SqlServer.Plugin.QueryTypes
{
	public interface IQueryResult
	{
		string DefineComponent(string sqlInstance);
		void AddMetrics(ComponentData componentData);
	}
}
