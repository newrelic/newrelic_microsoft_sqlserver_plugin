using System;
using System.Reflection;
using NUnit.Framework;

namespace NewRelic.Microsoft.SqlServer.Plugin.Core
{
	[TestFixture]
	public class ExtensionsForAssemblyTests
	{
		[Test]
		public void Assert_partial_names_match_against_words()
		{
			var assembly = Assembly.GetExecutingAssembly();

			var sql = assembly.SearchForStringResources("AnotherQuery.sql");
			Assert.That(sql, Is.EqualTo("SELECT [More] FROM [Anything]"));

			sql = assembly.SearchForStringResources("YetAnotherQuery.sql");
			Assert.That(sql, Is.EqualTo("SELECT [YetAnotherColumn] FROM [YetAnotherTable]"));
		}

		[Test]
		[TestCase("AnotherQuery.sql")]
		[TestCase("Core.AnotherQuery.sql")]
		[TestCase("Plugin.Core.AnotherQuery.sql")]
		[TestCase("SqlServer.Plugin.Core.AnotherQuery.sql")]
		[TestCase("Microsoft.SqlServer.Plugin.Core.AnotherQuery.sql")]
		[TestCase("NewRelic.Microsoft.SqlServer.Plugin.Core.AnotherQuery.sql")]
		public void Assert_partial_resource_name_can_be_searched_for(string partialResourceName)
		{
			var assembly = Assembly.GetExecutingAssembly();
			var sql = assembly.SearchForStringResources(partialResourceName);
			Assert.That(sql, Is.EqualTo("SELECT [More] FROM [Anything]"));
		}

		[Test]
		public void Assert_that_fully_named_embedded_resource_is_returned()
		{
			var assembly = Assembly.GetExecutingAssembly();
			var sql = assembly.GetStringResource(GetType().Namespace + ".ExampleEmbeddedFile.sql");
			Assert.That(sql, Is.EqualTo("SELECT * FROM [Everything]"));
		}

		[Test]
		public void Should_throw_searching_for_bad_resource_name()
		{
			var assembly = Assembly.GetExecutingAssembly();
			Assert.Throws<Exception>(() => assembly.SearchForStringResources("Foo.sql"));
		}

		[Test]
		public void Should_throw_searching_with_ambiguous_match()
		{
			var assembly = Assembly.GetExecutingAssembly();
			var exception = Assert.Throws<Exception>(() => assembly.SearchForStringResources("ExampleEmbeddedFile.sql"));
			Assert.That(exception.Message.ToLower(), Is.StringContaining("ambiguous"));
		}

		[Test]
		public void Should_throw_with_bad_resource_name()
		{
			var assembly = Assembly.GetExecutingAssembly();
			Assert.Throws<Exception>(() => assembly.GetStringResource(GetType().Namespace + ".Foo.sql"));
		}
	}
}
