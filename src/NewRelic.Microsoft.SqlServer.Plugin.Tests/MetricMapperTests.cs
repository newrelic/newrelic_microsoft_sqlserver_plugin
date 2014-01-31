using System;

using NUnit.Framework;

using NewRelic.Microsoft.SqlServer.Plugin.Core;

namespace NewRelic.Microsoft.SqlServer.Plugin
{
    [TestFixture]
    public class MetricMapperTests
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

        protected class MarkedUpFakeQueryType
        {
            public string IgnoredCompletely { get; set; }

            [Metric(Ignore = true)]
            public DateTime DoublyIgnored { get; set; }

            [Metric(Ignore = false)]
            public object IgnoreAnyways { get; set; }

            [Metric(MetricName = "FancyName")]
            public int MetricNameOverridden { get; set; }

            [Metric(MetricName = "SuperImportant", Ignore = true)]
            public int NiceNameButIgnored { get; set; }

            [Metric(Ignore = true)]
            public int SimplyIgnored { get; set; }

            [Metric(MetricValueType = MetricValueType.Value)]
            public long LongValueMetric { get; set; }

            [Metric(MetricValueType = MetricValueType.Value)]
            public int IntValueMetric { get; set; }

            [Metric(MetricValueType = MetricValueType.Value)]
            public byte ByteValueMetric { get; set; }

            [Metric(MetricValueType = MetricValueType.Count)]
            public decimal DecimalCountMetric { get; set; }

            public long ConventionalProperty { get; set; }
        }

        protected class FakeTypeWithUnits
        {
            [Metric(Units = "bytes/sec")]
            public int RateOfSomething { get; set; }
        }

        [Test]
        [TestCase("Comment", TestName = "string")]
        [TestCase("EventTime", TestName = "DateTime")]
        public void Assert_invalid_type_throws(string propertyName)
        {
            var propertyInfo = typeof (FakeQueryType).GetProperty(propertyName);
            Assume.That(propertyInfo, Is.Not.Null, "Expected a property name '{0}' on {1}", propertyName, typeof (FakeQueryType).Name);

            Assert.Throws<ArgumentException>(() => new MetricMapper(propertyInfo));
        }

        [Test]
        public void Assert_metric_units_are_stored_by_mapper()
        {
            var mapper = MetricMapper.TryCreate(typeof(FakeTypeWithUnits).GetProperty("RateOfSomething"));
            Assert.That(mapper, Is.Not.Null, "Mapping of FakeTypeWithUnits failed");
            Assert.That(mapper.MetricUnits, Is.EqualTo("bytes/sec"), "Units from MetricAttribute not mapped correctly");
        }

        [Test(Description = "Returns the MetricName when the map is created. Otherwise, null.")]
        [TestCase("IgnoredCompletely", Result = null, TestName = "Assert non-numeric is still ignored without an attribute")]
        [TestCase("ConventionalProperty", Result = "ConventionalProperty", TestName = "Assert numeric is still discovered without an attribute")]
        [TestCase("DoublyIgnored", Result = null, TestName = "Assert non-numeric is ignored with an attribute where Ignore == true")]
        [TestCase("IgnoreAnyways", Result = null, TestName = "Assert non-numeric is ignored despite an attribute where Ignore == false")]
        [TestCase("MetricNameOverridden", Result = "FancyName", TestName = "Assert numeric with attribute has MetricName overriden")]
        [TestCase("NiceNameButIgnored", Result = null, TestName = "Assert numeric is ignored with attribute where MetricName is set and Ignore == true")]
        [TestCase("SimplyIgnored", Result = null, TestName = "Assert numeric is ignored with attribute where Ignore == true")]
        public string Assert_that_metric_attribute_is_respected(string propertyName)
        {
            var fakeType = typeof (MarkedUpFakeQueryType);
            var metricMapper = MetricMapper.TryCreate(fakeType.GetProperty(propertyName));
            return metricMapper != null ? metricMapper.MetricName : null;
        }

        [Test]
        [TestCase("Comment", TestName = "string")]
        [TestCase("EventTime", TestName = "DateTime")]
        public void Assert_try_create_mapper_gracefully_refuses_invalid_types(string propertyName)
        {
            var propertyInfo = typeof (FakeQueryType).GetProperty(propertyName);
            Assume.That(propertyInfo, Is.Not.Null, "Expected a property name '{0}' on {1}", propertyName, typeof (FakeQueryType).Name);

            var mapper = MetricMapper.TryCreate(propertyInfo);
            Assert.That(mapper, Is.Null, "Should not have mapped {0}", propertyName);
        }
    }
}
