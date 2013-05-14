using NewRelic.Microsoft.SqlServer.Plugin.Core;

namespace NewRelic.Microsoft.SqlServer.Plugin.QueryTypes
{
	[Query("SQLConnectionsSummary.sql", "Component/SqlConnectionCount/{DatabaseName}", QueryName = "SQL Connections Summary", Enabled = true)]
	internal class SqlConnectionsSummary : DatabaseMetricBase
	{
		public int NumberOfConnections { get; set; }

		protected override WhereClauseTokenEnum WhereClauseToken
		{
			get { return WhereClauseTokenEnum.WhereAnd; }
		}

		protected override string DbNameForWhereClause
		{
			get { return "DB_NAME(s.dbid)"; }
		}
	}
}