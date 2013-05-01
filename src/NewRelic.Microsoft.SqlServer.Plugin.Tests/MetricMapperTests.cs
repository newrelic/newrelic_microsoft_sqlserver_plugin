using NUnit.Framework;
using NewRelic.Platform.Binding.DotNET;

namespace NewRelic.Microsoft.SqlServer.Plugin
{
	[TestFixture]
	public class MetricMapperTests
	{
		private class FakeQueryType
		{
			public long Long { get; set; }
			public int Integer { get; set; }
			public short Short { get; set; }
			public byte Byte { get; set; }
			public decimal Decimal { get; set; }
		}

		[Test]
		public void Assert_byte_is_mapped()
		{
			var fake = new FakeQueryType {Byte = 96,};
			var componentData = new ComponentData("", "", 0);
			const string metricName = "Foo";

			var metricMapper = new MetricMapper(fake.GetType().GetProperty("Byte")) {MetricName = metricName,};

			metricMapper.AddMetric(new QueryContext {ComponentData = componentData,}, fake);

			Assert.That(componentData.Metrics.ContainsKey(metricName), "Expected metric with correct name to be added");
			var condition = componentData.Metrics[metricName];
			Assert.That(condition, Is.EqualTo(fake.Byte), "Metric not mapped correctly");
		}

		[Test]
		public void Assert_decimal_is_mapped()
		{
			var fake = new FakeQueryType {Decimal = 12.3m,};
			var componentData = new ComponentData("", "", 0);
			const string metricName = "Foo";

			var metricMapper = new MetricMapper(fake.GetType().GetProperty("Decimal")) {MetricName = metricName,};

			metricMapper.AddMetric(new QueryContext {ComponentData = componentData,}, fake);

			Assert.That(componentData.Metrics.ContainsKey(metricName), "Expected metric with correct name to be added");
			var condition = componentData.Metrics[metricName];
			Assert.That(condition, Is.EqualTo(fake.Decimal), "Metric not mapped correctly");
		}

		[Test]
		public void Assert_int_is_mapped()
		{
			var fake = new FakeQueryType {Integer = 42,};
			var componentData = new ComponentData("", "", 0);
			const string metricName = "Foo";

			var metricMapper = new MetricMapper(fake.GetType().GetProperty("Integer")) {MetricName = metricName,};

			metricMapper.AddMetric(new QueryContext {ComponentData = componentData,}, fake);

			Assert.That(componentData.Metrics.ContainsKey(metricName), "Expected metric with correct name to be added");
			var condition = componentData.Metrics[metricName];
			Assert.That(condition, Is.EqualTo(fake.Integer), "Metric not mapped correctly");
		}

		[Test]
		public void Assert_long_is_mapped()
		{
			var fake = new FakeQueryType {Long = 27,};
			var componentData = new ComponentData("", "", 0);
			const string metricName = "Foo";

			var metricMapper = new MetricMapper(fake.GetType().GetProperty("Long")) {MetricName = metricName,};

			metricMapper.AddMetric(new QueryContext {ComponentData = componentData,}, fake);

			Assert.That(componentData.Metrics.ContainsKey(metricName), "Expected metric with correct name to be added");
			var condition = componentData.Metrics[metricName];
			Assert.That(condition, Is.EqualTo(fake.Long), "Metric not mapped correctly");
		}

		[Test]
		public void Assert_short_is_mapped()
		{
			var fake = new FakeQueryType {Short = 32045,};
			var componentData = new ComponentData("", "", 0);
			const string metricName = "Foo";

			var metricMapper = new MetricMapper(fake.GetType().GetProperty("Short")) {MetricName = metricName,};

			metricMapper.AddMetric(new QueryContext {ComponentData = componentData,}, fake);

			Assert.That(componentData.Metrics.ContainsKey(metricName), "Expected metric with correct name to be added");
			var condition = componentData.Metrics[metricName];
			Assert.That(condition, Is.EqualTo(fake.Short), "Metric not mapped correctly");
		}
	}
}