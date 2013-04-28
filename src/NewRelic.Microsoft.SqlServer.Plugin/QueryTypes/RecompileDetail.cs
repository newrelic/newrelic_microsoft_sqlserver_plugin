using NewRelic.Microsoft.SqlServer.Plugin.Core;

namespace NewRelic.Microsoft.SqlServer.Plugin.QueryTypes
{
	[SqlMonitorQuery("RecompileDetail.sql", QueryName = "Recompile Detail", Enabled = false)]
	public class RecompileDetail
	{
		public string DBName { get; set; }
		public int BucketID { get; set; }
		public int UseCounts { get; set; }
		public int SizeInBytes { get; set; }
		public string ObjectType { get; set; }
		public string Text { get; set; }
	}
}