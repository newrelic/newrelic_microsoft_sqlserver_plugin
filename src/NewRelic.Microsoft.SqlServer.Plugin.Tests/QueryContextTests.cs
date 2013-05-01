using System;
using NSubstitute;
using NUnit.Framework;
using NewRelic.Microsoft.SqlServer.Plugin.Core;
using NewRelic.Microsoft.SqlServer.Plugin.QueryTypes;

namespace NewRelic.Microsoft.SqlServer.Plugin
{
	[TestFixture]
	public class QueryContextTests
	{
		[Test]
		public void Assert_metric_pattern_substitution_appends_metric_name_if_substitution_wildcard_missing()
		{
			// With trailing slash in the pattern
			var metricName = QueryContext.FormatMetricKey("Custom/Memory/{DatabaseName}/", new Lazy<string>(() => "Tableriffic"), "MyMetric");
			Assert.That(metricName, Is.EqualTo("Custom/Memory/Tableriffic/MyMetric"), "Substitution failed when trailing slash present");

			// Without the trailing slash
			metricName = QueryContext.FormatMetricKey("Custom/Memory/{DatabaseName}", new Lazy<string>(() => "Tableriffic"), "MyMetric");
			Assert.That(metricName, Is.EqualTo("Custom/Memory/Tableriffic/MyMetric"), "Substitution failed when trailing slash is missing");
		}

		[Test]
		public void Assert_metric_pattern_substitution_replaces_database_and_metric_name()
		{
			const string metricPattern = "Custom/Memory/{DatabaseName}/{MetricName}";
			var metricName = QueryContext.FormatMetricKey(metricPattern, new Lazy<string>(() => "ManyTablesDB"), "MyMetric");
			Assert.That(metricName, Is.EqualTo("Custom/Memory/ManyTablesDB/MyMetric"), "Substitution failed");
		}

		[Test]
		public void Assert_metric_pattern_substitution_replaces_database_with_name_from_result()
		{
			const string metricPattern = "Custom/Memory/{DatabaseName}/{MetricName}";
			var context = new QueryContext
			              {
				              Query = new SqlMonitorQuery(GetType(), new QueryAttribute(null, metricPattern), null, ""),
			              };

			var databaseMetric = Substitute.For<IDatabaseMetric>();
			databaseMetric.DatabaseName.Returns("ManyTablesDB");

			var metricName = context.FormatMetricKey(databaseMetric, "MyMetric");
			Assert.That(metricName, Is.EqualTo("Custom/Memory/ManyTablesDB/MyMetric"), "Substitution failed");
		}

		[Test]
		public void Assert_metric_pattern_substitution_sets_database_name_to_none_for_result_without_database_name()
		{
			const string metricPattern = "Custom/Memory/{DatabaseName}/{MetricName}";
			var context = new QueryContext
			              {
				              Query = new SqlMonitorQuery(GetType(), new QueryAttribute(null, metricPattern), null, ""),
			              };

			var metricName = context.FormatMetricKey(new object(), "MyMetric");
			Assert.That(metricName, Is.EqualTo("Custom/Memory/(none)/MyMetric"), "Substitution failed");
		}

		[Test]
		public void Assert_missing_database_name_replaced_with_none()
		{
			const string metricPattern = "Custom/Memory/{DatabaseName}/{MetricName}";
			var metricName = QueryContext.FormatMetricKey(metricPattern, null, "MyMetric");
			Assert.That(metricName, Is.EqualTo("Custom/Memory/(none)/MyMetric"), "Substitution failed");
		}
	}
}
