using System;
using System.Reflection;
using NUnit.Framework;

namespace NewRelic.Microsoft.SqlServer.Plugin.Core
{
	[TestFixture]
	public class ExtensionsForAssemblyTests
	{
		[Test]
		public void Assert_that_embedded_resource_is_returned()
		{
			var assembly = Assembly.GetExecutingAssembly();
			var sql = assembly.GetStringFromResources(GetType().Namespace + ".ExampleEmbeddedFile.sql");
			Assert.That(sql, Is.EqualTo("SELECT * FROM [Everything]"));
		}

		[Test]
		public void Should_throw_exception_with_bad_resource_name()
		{
			var assembly = Assembly.GetExecutingAssembly();
			Assert.Throws<Exception>(() => assembly.GetStringFromResources(GetType().Namespace + ".Foo.sql"));
		}
	}
}
