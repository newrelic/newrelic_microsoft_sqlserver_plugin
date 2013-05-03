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

		public string ParameterizeQuery(string commandText, string[] includeDBs, string[] excludeDBs)
		{
			// TODO 
			return commandText;
		}
	}
}
