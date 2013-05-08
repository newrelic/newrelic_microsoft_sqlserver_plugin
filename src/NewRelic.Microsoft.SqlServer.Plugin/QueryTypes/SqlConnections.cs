using System;

using NewRelic.Microsoft.SqlServer.Plugin.Core;

namespace NewRelic.Microsoft.SqlServer.Plugin.QueryTypes
{
	[Query("SQLConnections.sql", "Component/SqlConnections/{DatabaseName}", QueryName = "SQL Connections", Enabled = true)]
	internal class SqlConnections : DatabaseMetricBase
	{
		public Guid ConnectionId { get; set; }
		public string ClientNetAddress { get; set; }
		public int NumberOfReads { get; set; }
		public int NumberOfWrites { get; set; }

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
