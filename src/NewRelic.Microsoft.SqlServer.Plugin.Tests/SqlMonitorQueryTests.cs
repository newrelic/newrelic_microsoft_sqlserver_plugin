using System;
using System.Linq;

using NewRelic.Microsoft.SqlServer.Plugin.Core;
using NewRelic.Microsoft.SqlServer.Plugin.QueryTypes;

using NSubstitute;

using NUnit.Framework;

namespace NewRelic.Microsoft.SqlServer.Plugin
{
    [TestFixture]
    public class SqlMonitorQueryTests
    {
        protected class FakeQueryType
        {
            public long Long { get; set; }
            public int Integer { get; set; }
            public short Short { get; set; }
            public byte Byte { get; set; }
            public decimal Decimal { get; set; }
            public string Comment { get; set; }
            public DateTime EventTime { get; set; }
        }

        [Test]
        public void Assert_only_numerics_are_returned()
        {
            MetricMapper[] metricMappers = MetricQuery.GetMappers(typeof (FakeQueryType));
            Assert.That(metricMappers, Is.Not.Null);

            // Keep these out of order to ensure we don't depend on it
            var expected = new[] {"Long", "Integer", "Short", "Decimal", "Byte"};
            string[] actual = metricMappers.Select(m => m.MetricName).ToArray();
            Assert.That(actual, Is.EquivalentTo(expected), "Properties discovered and mapped wrong");
        }
    }
}
