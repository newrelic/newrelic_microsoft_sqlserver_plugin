using NUnit.Framework;

namespace NewRelic.Microsoft.SqlServer.Plugin.QueryTypes
{
	[TestFixture]
	public class BackupStatusTests : DatabaseMetricQueryTestFixtureBase<BackupStatus>
	{
		protected override string IncludedDatabaseExpectedSql
		{
			get { return "WHERE (s.name IN ('include')"; }
		}

		protected override string ExcludedDatabaseExpectedSql
		{
			get { return "WHERE (s.name NOT IN ('exclude')"; }
		}
	}
}
