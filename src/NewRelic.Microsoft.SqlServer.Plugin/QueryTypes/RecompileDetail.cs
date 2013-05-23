using NewRelic.Microsoft.SqlServer.Plugin.Core;

namespace NewRelic.Microsoft.SqlServer.Plugin.QueryTypes
{
    [SqlServerQuery("RecompileDetail.sql", "Component/RecompileDetail/{MetricName}/{DatabaseName}", QueryName = "Recompile Detail", Enabled = false)]
	public class RecompileDetail : DatabaseMetricBase
	{
        public int BucketID { get; set; }
        public int UseCounts { get; set; }
        public int SizeInBytes { get; set; }
        public string ObjectType { get; set; }
        public string Text { get; set; }

		protected override string DbNameForWhereClause
		{
			get { return "DB_NAME(st.dbid)"; }
		}

		protected override WhereClauseTokenEnum WhereClauseToken
		{
			get { return WhereClauseTokenEnum.WhereAnd; }
		}
	}
}
