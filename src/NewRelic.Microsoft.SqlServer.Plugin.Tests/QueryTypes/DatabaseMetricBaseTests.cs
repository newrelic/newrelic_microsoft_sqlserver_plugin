using System;

using NUnit.Framework;

using NewRelic.Microsoft.SqlServer.Plugin.Properties;

namespace NewRelic.Microsoft.SqlServer.Plugin.QueryTypes
{
	[TestFixture]
	public class DatabaseMetricBaseTests
	{
		[Test]
		public void Assert_that_excluded_databases_are_added_to_command_text()
		{
			const string commandText = "foo " + Constants.WhereClauseReplaceToken + " bar";
			var actual = DatabaseMetricBase.ParameterizeQuery(commandText, WhereClauseTokenEnum.Where, "token", null, new[] { "tempdb", "master", "model", "msdb", });
			Assert.That(actual, Is.EqualTo("foo WHERE (token NOT IN ('tempdb', 'master', 'model', 'msdb')) bar"));
		}

		[Test]
		public void Assert_that_included_databases_are_added_to_command_text()
		{
			const string commandText = "foo " + Constants.WhereClauseAndReplaceToken + " bar";
			var actual = DatabaseMetricBase.ParameterizeQuery(commandText, WhereClauseTokenEnum.WhereAnd, "token", new[] { "BigData", "LittleData", }, null);
			Assert.That(actual, Is.EqualTo("foo AND (token IN ('BigData', 'LittleData')) bar"));
		}

		[Test]
		public void Should_throw_when_replacement_token_is_missing_from_sql()
		{
			Assert.Throws<Exception>(() => DatabaseMetricBase.ParameterizeQuery("no token here", WhereClauseTokenEnum.Where, "no match", new[] {"foo"}, null));
			Assert.Throws<Exception>(() => DatabaseMetricBase.ParameterizeQuery("no token here", WhereClauseTokenEnum.WhereAnd, "no match", new[] {"foo"}, null));
		}
	}
}
