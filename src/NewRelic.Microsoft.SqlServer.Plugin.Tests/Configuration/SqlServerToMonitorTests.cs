using System.Collections.Generic;
using System.Linq;
using System.Threading;

using NSubstitute;

using NUnit.Framework;

using NewRelic.Microsoft.SqlServer.Plugin.Core.Extensions;
using NewRelic.Microsoft.SqlServer.Plugin.Properties;

namespace NewRelic.Microsoft.SqlServer.Plugin.Configuration
{
    [TestFixture]
    public class SqlServerToMonitorTests
    {
        public IEnumerable<TestCaseData> QueryHistoryTestData
        {
            get
            {
                return new[]
                       {
                           new TestCaseData((object) new[]
                                                     {
                                                         new[]
                                                         {
                                                             "QueryOne", "QueryTwo", "QueryThree"
                                                         }
                                                     })
                               .Returns(new[]
                                        {
                                            "QueryOne:1",
                                            "QueryTwo:1",
                                            "QueryThree:1"
                                        }.ToArray()).SetName("Simple History"),
                           new TestCaseData((object) new[]
                                                     {
                                                         new[]
                                                         {
                                                             "QueryOne", "QueryOne", "QueryTwo", "QueryTwo", "QueryTwo", "QueryThree", "QueryThree", "QueryThree", "QueryThree"
                                                         }
                                                     })
                               .Returns(new[]
                                        {
                                            "QueryOne:2",
                                            "QueryTwo:3",
                                            "QueryThree:3",
                                        }.ToArray()).SetName("Limit to 3 single pass"),
                           new TestCaseData((object) new[]
                                                     {
                                                         new[]
                                                         {
                                                             "QueryOne", "QueryTwo", "QueryThree"
                                                         },
                                                         new[]
                                                         {
                                                             "QueryOne", "QueryTwo", "QueryThree"
                                                         },
                                                         new[]
                                                         {
                                                             "QueryOne", "QueryTwo", "QueryThree"
                                                         },
                                                         new[]
                                                         {
                                                             "QueryOne", "QueryTwo", "QueryThree"
                                                         },
                                                         new[]
                                                         {
                                                             "QueryOne", "QueryTwo", "QueryThree"
                                                         },
                                                     })
                               .Returns(new[]
                                        {
                                            "QueryOne:3",
                                            "QueryTwo:3",
                                            "QueryThree:3",
                                        }.ToArray()).SetName("Limit to 3 multi pass"),
                       };
            }
        }

        [TestCaseSource("QueryHistoryTestData")]
        public string[] Assert_that_query_history_updated_appropriately(string[][] queryNames)
        {
            var sqlServerToMonitor = new SqlServerToMonitor("Best_DB_Ever", "", false);

            Assert.That(sqlServerToMonitor.QueryHistory.Count, Is.EqualTo(0), "History Should start off empty");

            queryNames.ForEach(queryNamesPass =>
                               {
                                   var queryContexts = queryNamesPass.Select(queryName =>
                                                                             {
                                                                                 var queryContext = Substitute.For<IQueryContext>();
                                                                                 queryContext.QueryName.Returns(queryName);
                                                                                 return queryContext;
                                                                             }).ToArray();
                                   sqlServerToMonitor.UpdateHistory(queryContexts);
                               });

            var actual = sqlServerToMonitor.QueryHistory.Select(qh => string.Format("{0}:{1}", qh.Key, qh.Value.Count)).ToArray();

            return actual;
        }

        [Test]
        public void AssertIncludeExcludeListsBuiltAppropriately()
        {
            var sqlServerToMonitor = new SqlServerToMonitor("FooServer", ".", false, new[] {"FooDb*", "Bar%db"}, new[] {"Baz"});
            Assert.That(sqlServerToMonitor.IncludedDatabases, Is.EquivalentTo(new[] {"FooDb%", "Bar%db"}));
            Assert.That(sqlServerToMonitor.ExcludedDatabases, Is.EquivalentTo(Constants.SystemDatabases.Concat(new[] {"Baz"})));
        }

        [Test]
        public void AssertIncludeSystemDatabasesWorks()
        {
            var sqlServerToMonitor = new SqlServerToMonitor("FooServer", ".", false);
            Assert.That(sqlServerToMonitor.IncludedDatabases.Length, Is.EqualTo(0));
            Assert.That(sqlServerToMonitor.ExcludedDatabases, Is.EquivalentTo(Constants.SystemDatabases));

            sqlServerToMonitor = new SqlServerToMonitor("FooServer", ".", true);
            Assert.That(sqlServerToMonitor.IncludedDatabases.Length, Is.EqualTo(0));
            Assert.That(sqlServerToMonitor.ExcludedDatabases.Length, Is.EqualTo(0));
        }

        [Test]
        public void Assert_that_duration_is_reported_correctly()
        {
            var sqlServerToMonitor = new SqlServerToMonitor("", "", false);
            Assert.That(sqlServerToMonitor.Duration, Is.EqualTo(0), "Expected 0 second Duration immediately after .ctor called");

            Thread.Sleep(1000);
            Assert.That(sqlServerToMonitor.Duration, Is.EqualTo(1), "Expected 1 second Duration after Thread.Sleep(1000)");
        }
    }
}
