using NUnit.Framework;

namespace NewRelic.Microsoft.SqlServer.Plugin.QueryTypes
{
	[TestFixture]
	public class FileIoViewTests : DatabaseMetricQueryTestFixtureBase<FileIoView>
	{
		protected override string IncludedDatabaseExpectedSql
		{
			get { return "WHERE (DB_NAME(a.database_id) IN ('include')"; }
		}

		protected override string ExcludedDatabaseExpectedSql
		{
			get { return "WHERE (DB_NAME(a.database_id) NOT IN ('exclude')"; }
		}
	}
}
