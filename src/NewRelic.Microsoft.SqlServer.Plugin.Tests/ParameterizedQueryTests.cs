using System.Linq;
using System.Reflection;

using NUnit.Framework;

using NewRelic.Microsoft.SqlServer.Plugin.QueryTypes;

namespace NewRelic.Microsoft.SqlServer.Plugin
{
	[TestFixture]
    public class ParameterizedQueryTests
    {
        [Test]
        public void Assert_that_query_appropriately_replaces_where_clause()
        {
            var testCases = new[]
                            {
                                new
                                {
                                    QueryMetric = new BackupStatus() as DatabaseMetricBase,
                                    Includes = new[] {"master"},
                                    Excludes = (string[]) null,
                                    ExpectedWhereClause = "WHERE (s.name IN ('master')",
                                    TestName = "BackupStatus Include Test"
                                },
                                new
                                {
                                    QueryMetric = new BackupStatus() as DatabaseMetricBase,
                                    Includes = (string[]) null,
                                    Excludes = new[] {"master"},
                                    ExpectedWhereClause = "WHERE (s.name NOT IN ('master')",
                                    TestName = "BackupStatus Exclude Test"
                                },
                                new
                                {
                                    QueryMetric = new SqlConnections() as DatabaseMetricBase,
                                    Includes = new[] {"master"},
                                    Excludes = (string[]) null,
                                    ExpectedWhereClause = "AND (DB_NAME(s.dbid) IN ('master')",
                                    TestName = "SqlConnections Include Test"
                                },
                                new
                                {
                                    QueryMetric = new SqlConnections() as DatabaseMetricBase,
                                    Includes = (string[]) null,
                                    Excludes = new[] {"master"},
                                    ExpectedWhereClause = "AND (DB_NAME(s.dbid) NOT IN ('master')",
                                    TestName = "SqlConnections Exclude Test"
                                },
								  new
                                {
                                    QueryMetric = new SqlConnectionsSummary() as DatabaseMetricBase,
                                    Includes = new[] {"master"},
                                    Excludes = (string[]) null,
                                    ExpectedWhereClause = "AND (DB_NAME(s.dbid) IN ('master')",
                                    TestName = "SqlConnectionsSummary Include Test"
                                },
                                new
                                {
                                    QueryMetric = new SqlConnectionsSummary() as DatabaseMetricBase,
                                    Includes = (string[]) null,
                                    Excludes = new[] {"master"},
                                    ExpectedWhereClause = "AND (DB_NAME(s.dbid) NOT IN ('master')",
                                    TestName = "SqlConnectionsSummary Exclude Test"
                                },
                                new
                                {
                                    QueryMetric = new FileIoView() as DatabaseMetricBase,
                                    Includes = new[] {"master"},
                                    Excludes = (string[]) null,
                                    ExpectedWhereClause = "WHERE (DB_NAME(a.database_id) IN ('master')",
                                    TestName = "FileIOView Include Test"
                                },
                                new
                                {
                                    QueryMetric = new FileIoView() as DatabaseMetricBase,
                                    Includes = (string[]) null,
                                    Excludes = new[] {"master"},
                                    ExpectedWhereClause = "WHERE (DB_NAME(a.database_id) NOT IN ('master')",
                                    TestName = "FileIOView Exclude Test"
                                },
                                new
                                {
                                    QueryMetric = new RecompileDetail() as DatabaseMetricBase,
                                    Includes = new[] {"master"},
                                    Excludes = (string[]) null,
                                    ExpectedWhereClause = "AND (DB_NAME(st.dbid) IN ('master')",
                                    TestName = "RecompileDetail Include Test"
                                },
                                new
                                {
                                    QueryMetric = new RecompileDetail() as DatabaseMetricBase,
                                    Includes = (string[]) null,
                                    Excludes = new[] {"master"},
                                    ExpectedWhereClause = "AND (DB_NAME(st.dbid) NOT IN ('master')",
                                    TestName = "RecompileDetail Exclude Test"
                                },
                                new
                                {
                                    QueryMetric = new RecompileSummary() as DatabaseMetricBase,
                                    Includes = new[] {"master"},
                                    Excludes = (string[]) null,
                                    ExpectedWhereClause = "AND (DB_NAME(st.dbid) IN ('master')",
                                    TestName = "RecompileSummary Include Test"
                                },
                                new
                                {
                                    QueryMetric = new RecompileSummary() as DatabaseMetricBase,
                                    Includes = (string[]) null,
                                    Excludes = new[] {"master"},
                                    ExpectedWhereClause = "AND (DB_NAME(st.dbid) NOT IN ('master')",
                                    TestName = "RecompileSummary Exclude Test"
                                },
                            };

            foreach (var testCase in testCases)
            {
                var queryLocator = new QueryLocator(null);
                var queries = queryLocator.PrepareQueries(new[] {testCase.QueryMetric.GetType()}, false);

                foreach (var query in queries)
                {
                    var actual = testCase.QueryMetric.ParameterizeQuery(query.CommandText, testCase.Includes, testCase.Excludes);
                    Assert.That(actual, Is.StringContaining(testCase.ExpectedWhereClause), "Expected Where clause not found in testcase '{0}' and query '{1}'", testCase.TestName, query.QueryName);
                }
            }

            var testedParameterizedQueries = testCases.Select(tc => tc.QueryMetric.GetType()).Distinct().ToArray();
            var concreteImplementationsOfDatabaseMetricBase = Assembly.GetAssembly(typeof (DatabaseMetricBase)).GetTypes()
                                                                      .Where(t => t.IsSubclassOf(typeof (DatabaseMetricBase))
                                                                                  && !t.IsInterface
                                                                                  && !t.IsAbstract)
                                                                      .ToArray();

            Assert.That(concreteImplementationsOfDatabaseMetricBase, Is.EquivalentTo(testedParameterizedQueries), "Expected all Implementations of DatabaseMetricBase to be tested");
        }
    }
}
