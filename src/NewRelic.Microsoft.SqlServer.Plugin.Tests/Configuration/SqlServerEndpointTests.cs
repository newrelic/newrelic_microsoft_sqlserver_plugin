using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using NSubstitute;

using NUnit.Framework;

using NewRelic.Microsoft.SqlServer.Plugin.Core;
using NewRelic.Microsoft.SqlServer.Plugin.Core.Extensions;
using NewRelic.Microsoft.SqlServer.Plugin.Properties;
using NewRelic.Microsoft.SqlServer.Plugin.QueryTypes;

namespace NewRelic.Microsoft.SqlServer.Plugin.Configuration
{
    [TestFixture]
    public class SqlServerEndpointTests
    {
        [Test]
        public void Assert_complex_include_system_databases_works()
        {
            var includeDbNames = new[] {"FooDb", "BarDb"};
            IEnumerable<Database> includedDbs = includeDbNames.Select(x => new Database {Name = x,});

            var sqlServerToMonitor = new SqlServerEndpoint("FooServer", ".", true, includedDbs, null);
            Assert.That(sqlServerToMonitor.ExcludedDatabaseNames.Length, Is.EqualTo(0));

            IEnumerable<string> expectedIncludes = Constants.SystemDatabases.ToList().Concat(includeDbNames);
            Assert.That(sqlServerToMonitor.IncludedDatabaseNames, Is.EquivalentTo(expectedIncludes));
        }

        [Test]
        public void Assert_include_exclude_lists_built_appropriately()
        {
            IEnumerable<Database> includedDbs = new[] {"FooDb", "BarDb"}.Select(x => new Database {Name = x,});
            var sqlServerToMonitor = new SqlServerEndpoint("FooServer", ".", false, includedDbs, new[] {"Baz"});
            Assert.That(sqlServerToMonitor.IncludedDatabaseNames, Is.EquivalentTo(new[] {"FooDb", "BarDb"}));
            Assert.That(sqlServerToMonitor.ExcludedDatabaseNames, Is.EquivalentTo(Constants.SystemDatabases.Concat(new[] {"Baz"})));
        }

        [Test]
        public void Assert_include_system_databases_works()
        {
            var sqlServerToMonitor = new SqlServerEndpoint("FooServer", ".", false);
            Assert.That(sqlServerToMonitor.IncludedDatabaseNames.Length, Is.EqualTo(0));
            Assert.That(sqlServerToMonitor.ExcludedDatabaseNames, Is.EquivalentTo(Constants.SystemDatabases));

            sqlServerToMonitor = new SqlServerEndpoint("FooServer", ".", true);
            Assert.That(sqlServerToMonitor.IncludedDatabaseNames.Length, Is.EqualTo(0));
            Assert.That(sqlServerToMonitor.ExcludedDatabaseNames.Length, Is.EqualTo(0));
        }

        [Test]
        public void Assert_that_max_recompile_summary_ignores_other_metrics()
        {
            object[] metrics = {new SqlCpuUsage()};
            var results = new SqlServerEndpoint(null, null, false).GetMaxRecompileSummaryMetric(metrics);
            Assert.That(results, Is.Null, "Implementation should have ignored bad data.");
        }

        [Test]
        public void Assert_that_max_recompile_summary_ignores_empty_metric_array()
        {
            object[] metrics = {};
            var results = new SqlServerEndpoint(null, null, false).GetMaxRecompileSummaryMetric(metrics);
            Assert.That(results, Is.Null, "Implementation should have ignored empty data.");
        }

        [Test]
        public void Assert_that_max_recompile_summary_is_reported()
        {
            object[] metrics =
            {
                new RecompileSummary { DatabaseName = "A", SingleUseObjects = 100, SingleUsePercent = 0, MultipleUseObjects = 1, },
                new RecompileSummary { DatabaseName = "B", SingleUseObjects = 1, SingleUsePercent = 80, MultipleUseObjects = 1, },
                new RecompileSummary { DatabaseName = "C", SingleUseObjects = 1, SingleUsePercent = 0, MultipleUseObjects = 50, },
            };
            var queryContext = new SqlServerEndpoint(null, null, false).GetMaxRecompileSummaryMetric(metrics);
            var results = queryContext.Results.ToArray();

            Assert.That(queryContext, Is.Not.SameAs(metrics), "Expected a new Array");

            var max = results.OfType<RecompileMaximums>().SingleOrDefault();
            Assert.That(max, Is.Not.Null, "Expected a new metric in the results");
            Assert.That(max.SingleUseObjects, Is.EqualTo(100), "Wrong SingleUseObjects value");
            Assert.That(max.SingleUsePercent, Is.EqualTo(80m), "Wrong SingleUsePercent value");
            Assert.That(max.MultipleUseObjects, Is.EqualTo(50), "Wrong MultipleUseObjects value");
        }
    }
}
