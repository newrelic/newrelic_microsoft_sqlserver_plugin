using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using NewRelic.Microsoft.SqlServer.Plugin.Core;
using NewRelic.Microsoft.SqlServer.Plugin.Core.Extensions;

namespace NewRelic.Microsoft.SqlServer.Plugin.Queries
{
	[TestFixture]
	public class QueryTypeValidationTests
	{
		public IEnumerable<TestCaseData> QueryTypes
		{
			get
			{
				var assembly = typeof (QueryLocator).Assembly;
				var typesWithAttribute = assembly.GetTypes().Where(t => t.GetCustomAttributes(typeof (QueryAttribute), false).Any());

				return typesWithAttribute.Select(t => new {Type = t, Attribute = t.GetCustomAttributes(typeof (QueryAttribute), false).FirstOrDefault() as QueryAttribute})
				                         .Select(t => new TestCaseData(t.Type, t.Attribute).SetName(t.Type.Name));
			}
		}

		[Test]
		[TestCaseSource("QueryTypes")]
		public void Assert_query_type_has_attribute_with_valid_resource_name(Type queryType, QueryAttribute attribute)
		{
			var sql = queryType.Assembly.SearchForStringResource(attribute.ResourceName);
			Assert.That(sql, Is.Not.Null);
		}
	}
}
