using NUnit.Framework;

namespace NewRelic.Microsoft.SqlServer.Plugin.QueryTypes
{
	[TestFixture]
	public class RecompileSummaryTests : DatabaseMetricQueryTestFixtureBase<RecompileSummary>
	{
		protected override string IncludedDatabaseExpectedSql
		{
			get { return "WHERE (d.name IN ('include'))"; }
		}

		protected override string ExcludedDatabaseExpectedSql
		{
			get { return "WHERE (d.name NOT IN ('exclude'))"; }
		}
	}
}
