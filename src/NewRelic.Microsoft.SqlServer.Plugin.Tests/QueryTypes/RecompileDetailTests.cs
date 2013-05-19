using NUnit.Framework;

namespace NewRelic.Microsoft.SqlServer.Plugin.QueryTypes
{
	[TestFixture]
	public class RecompileDetailTests : DatabaseMetricQueryTestFixtureBase<RecompileDetail>
	{
		protected override string IncludedDatabaseExpectedSql
		{
			get { return "AND (DB_NAME(st.dbid) IN ('include')"; }
		}

		protected override string ExcludedDatabaseExpectedSql
		{
			get { return "AND (DB_NAME(st.dbid) NOT IN ('exclude')"; }
		}
	}
}
