using NewRelic.Microsoft.SqlServer.Plugin.Core;

namespace NewRelic.Microsoft.SqlServer.Plugin.QueryTypes
{
	[Query("RecompileDetail.sql", "Custom/RecompileDetail/{DatabaseName}", QueryName = "Recompile Detail", Enabled = false)]
	public class RecompileDetail : IDatabaseMetric
	{
		public int BucketID { get; set; }
		public int UseCounts { get; set; }
		public int SizeInBytes { get; set; }
		public string ObjectType { get; set; }
		public string Text { get; set; }
		public string DatabaseName { get; set; }

		public string ParameterizeQuery(string commandText, string[] includeDBs, string[] excludeDBs)
		{
			// TODO 
			return commandText;
		}
	}
}
