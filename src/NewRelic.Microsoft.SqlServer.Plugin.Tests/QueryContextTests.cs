using System;

using NSubstitute;

using NUnit.Framework;

using NewRelic.Microsoft.SqlServer.Plugin.QueryTypes;

namespace NewRelic.Microsoft.SqlServer.Plugin
{
	[TestFixture]
	public class QueryContextTests
	{
		private class FakeQueryWithCustomPlaceHolder
		{
			// ReSharper disable UnusedAutoPropertyAccessor.Local
			// ReSharper disable UnusedMember.Local
			public string ThePlaceholder { get; set; }
			public string AnotherPlaceholder { get; set; }
			internal string NotPublicProperty { get; set; }
			public static string NotInstanceProperty { get; set; }
			public string NotPublicGetterProperty { private get; set; }
			public string CaseMattersPeople { get; set; }

			public string PropertyWithoutGetter
			{
				// ReSharper disable ValueParameterNotUsed
				set { }
				// ReSharper restore ValueParameterNotUsed
			}

			public int TheMetric { get; set; }
			// ReSharper restore UnusedMember.Local
			// ReSharper restore UnusedAutoPropertyAccessor.Local
		}

		[Test]
		public void Assert_custom_placeholders_have_non_alphanumerics_replaced_with_underbar()
		{
			const string metricPattern = "Component/Memory/{ThePlaceholder}/{MetricName}";
			var queryResult = new FakeQueryWithCustomPlaceHolder {ThePlaceholder = "I ->have.bad_45+stuff!", TheMetric = 42,};
			var metricName = QueryContext.FormatMetricKey(metricPattern, queryResult, "TheMetric");

			Assert.That(metricName, Is.EqualTo("Component/Memory/I___have_bad_45_stuff_/TheMetric"), "Substitution failed");
		}

		[Test]
		public void Assert_custom_placeholders_have_whitespace_trimmed()
		{
			const string metricPattern = "Component/Memory/{ThePlaceholder}/{MetricName}";
			var queryResult = new FakeQueryWithCustomPlaceHolder {ThePlaceholder = "  space then tab\t", TheMetric = 42,};
			var metricName = QueryContext.FormatMetricKey(metricPattern, queryResult, "TheMetric");

			Assert.That(metricName, Is.EqualTo("Component/Memory/space_then_tab/TheMetric"), "Substitution failed");
		}

		[Test]
		public void Assert_custom_placeholders_in_pattern_are_replaced()
		{
			const string metricPattern = "Component/Memory/{ThePlaceholder}/Machine/{AnotherPlaceholder}/{MetricName}";
			var queryResult = new FakeQueryWithCustomPlaceHolder {ThePlaceholder = "Tada", AnotherPlaceholder = "Flexible", TheMetric = 42,};
			var metricName = QueryContext.FormatMetricKey(metricPattern, queryResult, "TheMetric");

			Assert.That(metricName, Is.EqualTo("Component/Memory/Tada/Machine/Flexible/TheMetric"), "Substitution failed");
		}

		[Test]
		public void Assert_custom_placeholders_with_empty_values_are_replaced_with_empty_string()
		{
			const string metricPattern = "Component/Memory/{ThePlaceholder}/{MetricName}";
			var queryResult = new FakeQueryWithCustomPlaceHolder {ThePlaceholder = "", TheMetric = 42,};
			var metricName = QueryContext.FormatMetricKey(metricPattern, queryResult, "TheMetric");

			Assert.That(metricName, Is.EqualTo("Component/Memory//TheMetric"), "Substitution failed");
		}

		[Test]
		public void Assert_custom_placeholders_with_null_values_are_replaced_with_null_text()
		{
			const string metricPattern = "Component/Memory/{ThePlaceholder}/{MetricName}";
			var queryResult = new FakeQueryWithCustomPlaceHolder {ThePlaceholder = null, TheMetric = 42,};
			var metricName = QueryContext.FormatMetricKey(metricPattern, queryResult, "TheMetric");

			Assert.That(metricName, Is.EqualTo("Component/Memory/null/TheMetric"), "Substitution failed");
		}

		[Test]
		public void Assert_metric_pattern_substitution_appends_metric_name_if_metric_name_wildcard_missing()
		{
			var databaseMetric = Substitute.For<IDatabaseMetric>();
			databaseMetric.DatabaseName.Returns("Tableriffic");

			// With trailing slash in the pattern
			var metricName = QueryContext.FormatMetricKey("Component/Memory/{DatabaseName}/", databaseMetric, "MyMetric");
			Assert.That(metricName, Is.EqualTo("Component/Memory/Tableriffic/MyMetric"), "Substitution failed when trailing slash present");

			// Without the trailing slash
			metricName = QueryContext.FormatMetricKey("Component/Memory/{DatabaseName}", databaseMetric, "MyMetric");
			Assert.That(metricName, Is.EqualTo("Component/Memory/Tableriffic/MyMetric"), "Substitution failed when trailing slash is missing");
		}

		[Test]
		public void Assert_metric_pattern_substitution_appends_units_text_to_the_end()
		{
			const string metricPattern = "Component/Memory/{MetricName}/Foo";

			var metricName = QueryContext.FormatMetricKey(metricPattern, new object(), "MyMetric", "[bytes/min]");
			Assert.That(metricName, Is.EqualTo("Component/Memory/MyMetric/Foo[bytes/min]"), "Substitution failed");
		}

		[Test]
		public void Assert_metric_pattern_substitution_replaces_database_with_name_from_result()
		{
			const string metricPattern = "Component/Memory/{MetricName}/{DatabaseName}/Foo";

			var databaseMetric = Substitute.For<IDatabaseMetric>();
			databaseMetric.DatabaseName.Returns("ManyTablesDB");

			var metricName = QueryContext.FormatMetricKey(metricPattern, databaseMetric, "MyMetric");
			Assert.That(metricName, Is.EqualTo("Component/Memory/MyMetric/ManyTablesDB/Foo"), "Substitution failed");
		}

		[Test]
		public void Assert_missing_database_name_replaced_with_none()
		{
			const string metricPattern = "Component/Memory/{DatabaseName}/{MetricName}";
			var queryResult = new object();
			var metricName = QueryContext.FormatMetricKey(metricPattern, queryResult, "MyMetric");
			Assert.That(metricName, Is.EqualTo("Component/Memory/(none)/MyMetric"), "Substitution failed");
		}

		[Test]
		public void Should_throw_when_placeholder_case_is_not_exact()
		{
			const string metricPattern = "Component/Memory/{CASEMATTERSPEOPLE}/{MetricName}";
			var queryResult = new FakeQueryWithCustomPlaceHolder {CaseMattersPeople = "Why are you yelling?",};
			var exception = Assert.Throws<Exception>(() => QueryContext.FormatMetricKey(metricPattern, queryResult, "TheMetric"));
			Assert.That(exception.Message.ToLower(), Is.StringMatching("case-sensitive"), "Expected a helpful error message");
		}

		[Test]
		public void Should_throw_with_custom_placeholders_in_pattern_without_getter_property()
		{
			const string metricPattern = "Component/Memory/{PropertyWithoutGetter}/{MetricName}";
			var queryResult = new FakeQueryWithCustomPlaceHolder();

			Assert.Throws<Exception>(() => QueryContext.FormatMetricKey(metricPattern, queryResult, "TheMetric"));
		}

		[Test]
		public void Should_throw_with_custom_placeholders_in_pattern_without_matching_property()
		{
			const string metricPattern = "Component/Memory/{ThisMatchesNothing}/{MetricName}";
			var queryResult = new FakeQueryWithCustomPlaceHolder {ThePlaceholder = "BooHiss"};

			Assert.Throws<Exception>(() => QueryContext.FormatMetricKey(metricPattern, queryResult, "TheMetric"));
		}

		[Test]
		public void Should_throw_with_custom_placeholders_in_pattern_without_public_getter_property()
		{
			const string metricPattern = "Component/Memory/{NotPublicGetterProperty}/{MetricName}";
			var queryResult = new FakeQueryWithCustomPlaceHolder {NotPublicGetterProperty = "BooHiss"};

			Assert.Throws<Exception>(() => QueryContext.FormatMetricKey(metricPattern, queryResult, "TheMetric"));
		}

		[Test]
		public void Should_throw_with_custom_placeholders_in_pattern_without_public_instance_property()
		{
			const string metricPattern = "Component/Memory/{NotInstanceProperty}/{MetricName}";
			var queryResult = new FakeQueryWithCustomPlaceHolder();

			Assert.Throws<Exception>(() => QueryContext.FormatMetricKey(metricPattern, queryResult, "TheMetric"));
		}

		[Test]
		public void Should_throw_with_custom_placeholders_in_pattern_without_public_property()
		{
			const string metricPattern = "Component/Memory/{NotPublicProperty}/{MetricName}";
			var queryResult = new FakeQueryWithCustomPlaceHolder {NotPublicProperty = "BooHiss",};

			Assert.Throws<Exception>(() => QueryContext.FormatMetricKey(metricPattern, queryResult, "TheMetric"));
		}
	}
}
