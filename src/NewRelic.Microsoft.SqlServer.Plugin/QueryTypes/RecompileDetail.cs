using NewRelic.Microsoft.SqlServer.Plugin.Core;

namespace NewRelic.Microsoft.SqlServer.Plugin.QueryTypes
{
    [SqlServerQuery("RecompileDetail.sql", "Component/RecompileDetail/{MetricName}/{DatabaseName}", QueryName = "Recompile Detail", Enabled = false)]
	public class RecompileDetail : RecompileQueryBase
    {
        public int BucketID { get; set; }
        public int UseCounts { get; set; }
        public int SizeInBytes { get; set; }
        public string ObjectType { get; set; }
        public string Text { get; set; }
    }
}
