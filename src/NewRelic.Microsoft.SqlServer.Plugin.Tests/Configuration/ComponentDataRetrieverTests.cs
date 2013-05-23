using System;
using System.Collections.Generic;
using System.Linq;

using NSubstitute;

using NUnit.Framework;

using NewRelic.Microsoft.SqlServer.Plugin.Core;
using NewRelic.Microsoft.SqlServer.Plugin.Core.Extensions;
using NewRelic.Platform.Binding.DotNET;

namespace NewRelic.Microsoft.SqlServer.Plugin.Configuration
{
	[TestFixture]
	public class ComponentDataRetrieverTests
	{
		public class GenerateComponentDataInput
		{
			public string QueryName { get; set; }
			public bool DataSent { get; set; }
			public MetricTransformEnum MetricTransformEnum { get; set; }
			public Dictionary<string, object> Metrics { get; set; }
		}

		public IEnumerable<TestCaseData> GetComponentDataTestCases
		{
			get
			{
				return new[]
				       {
					       new TestCaseData((object) new[]
					                                 {
						                                 new GenerateComponentDataInput
						                                 {
							                                 QueryName = "RegularQuery",
							                                 DataSent = true,
							                                 MetricTransformEnum = MetricTransformEnum.Simple,
							                                 Metrics = new[]
							                                           {
								                                           new KeyValuePair<string, object>("Component/Metric/Foo", 1),
								                                           new KeyValuePair<string, object>("Component/Metric/Bar", 3),
								                                           new KeyValuePair<string, object>("Component/Metric/Baz", 45),
							                                           }.ToDictionary(k => k.Key, k => k.Value),
						                                 },
						                                 new GenerateComponentDataInput
						                                 {
							                                 QueryName = "RegularQuery",
							                                 DataSent = false,
							                                 MetricTransformEnum = MetricTransformEnum.Simple,
							                                 Metrics = new[]
							                                           {
								                                           new KeyValuePair<string, object>("Component/Metric/Foo", 6),
								                                           new KeyValuePair<string, object>("Component/Metric/Bar", 5),
								                                           new KeyValuePair<string, object>("Component/Metric/Baz", 90),
							                                           }.ToDictionary(k => k.Key, k => k.Value),
						                                 }
					                                 }).Returns(new[]
					                                            {
						                                            "Component/Metric/Foo:6",
						                                            "Component/Metric/Bar:5",
						                                            "Component/Metric/Baz:90",
					                                            }).SetName("Simple non-delta test"),
					       new TestCaseData((object) new[]
					                                 {
						                                 //Oldest
						                                 new GenerateComponentDataInput
						                                 {
							                                 QueryName = "RegularQuery",
							                                 DataSent = true,
							                                 MetricTransformEnum = MetricTransformEnum.Delta,
							                                 Metrics = new[]
							                                           {
								                                           new KeyValuePair<string, object>("Component/Metric/Foo", 25),
								                                           new KeyValuePair<string, object>("Component/Metric/Bar", 100),
								                                           new KeyValuePair<string, object>("Component/Metric/Baz", 50),
							                                           }.ToDictionary(k => k.Key, k => k.Value),
						                                 },
						                                 //Newest
						                                 new GenerateComponentDataInput
						                                 {
							                                 QueryName = "RegularQuery",
							                                 DataSent = false,
							                                 MetricTransformEnum = MetricTransformEnum.Delta,
							                                 Metrics = new[]
							                                           {
								                                           new KeyValuePair<string, object>("Component/Metric/Foo", 30),
								                                           new KeyValuePair<string, object>("Component/Metric/Bar", 140),
								                                           new KeyValuePair<string, object>("Component/Metric/Baz", 60),
							                                           }.ToDictionary(k => k.Key, k => k.Value),
						                                 }
					                                 }).Returns(new[]
					                                            {
						                                            "Component/Metric/Foo:5",
						                                            "Component/Metric/Bar:40",
						                                            "Component/Metric/Baz:10",
					                                            }).SetName("Delta simple test"),
					       new TestCaseData((object) new[]
					                                 {
						                                 new GenerateComponentDataInput
						                                 {
							                                 QueryName = "RegularQuery",
							                                 DataSent = true,
							                                 MetricTransformEnum = MetricTransformEnum.Delta,
							                                 Metrics = new[]
							                                           {
								                                           new KeyValuePair<string, object>("Component/Metric/Foo", 10),
								                                           new KeyValuePair<string, object>("Component/Metric/Bar", 105),
								                                           new KeyValuePair<string, object>("Component/Metric/Baz", 49),
							                                           }.ToDictionary(k => k.Key, k => k.Value),
						                                 },
						                                 new GenerateComponentDataInput
						                                 {
							                                 QueryName = "RegularQuery",
							                                 DataSent = false,
							                                 MetricTransformEnum = MetricTransformEnum.Delta,
							                                 Metrics = new[]
							                                           {
								                                           new KeyValuePair<string, object>("Component/Metric/Foo", 20),
								                                           new KeyValuePair<string, object>("Component/Metric/Bar", 100),
								                                           new KeyValuePair<string, object>("Component/Metric/Baz", 50),
							                                           }.ToDictionary(k => k.Key, k => k.Value),
						                                 }
					                                 }).Returns(new[]
					                                            {
						                                            "Component/Metric/Foo:10",
						                                            "Component/Metric/Bar:0",
						                                            "Component/Metric/Baz:1",
					                                            }).SetName("Delta with drop in int metric value should send zero test"),
					       new TestCaseData((object) new[]
					                                 {
						                                 new GenerateComponentDataInput
						                                 {
							                                 QueryName = "RegularQuery",
							                                 DataSent = true,
							                                 MetricTransformEnum = MetricTransformEnum.Delta,
							                                 Metrics = new[]
							                                           {
								                                           new KeyValuePair<string, object>("Component/Metric/Foo", 10.0m),
								                                           new KeyValuePair<string, object>("Component/Metric/Bar", 105.0m),
								                                           new KeyValuePair<string, object>("Component/Metric/Baz", 49.0m),
							                                           }.ToDictionary(k => k.Key, k => k.Value),
						                                 },
						                                 new GenerateComponentDataInput
						                                 {
							                                 QueryName = "RegularQuery",
							                                 DataSent = false,
							                                 MetricTransformEnum = MetricTransformEnum.Delta,
							                                 Metrics = new[]
							                                           {
								                                           new KeyValuePair<string, object>("Component/Metric/Foo", 20.0m),
								                                           new KeyValuePair<string, object>("Component/Metric/Bar", 100.0m),
								                                           new KeyValuePair<string, object>("Component/Metric/Baz", 50.0m),
							                                           }.ToDictionary(k => k.Key, k => k.Value),
						                                 }
					                                 }).Returns(new[]
					                                            {
						                                            "Component/Metric/Foo:10.0",
						                                            "Component/Metric/Bar:0.0",
						                                            "Component/Metric/Baz:1.0",
					                                            }).SetName("Delta with drop in decimal metric value should send zero test"),
					       new TestCaseData((object) new[]
					                                 {
						                                 new GenerateComponentDataInput
						                                 {
							                                 QueryName = "RegularQuery",
							                                 DataSent = true,
							                                 MetricTransformEnum = MetricTransformEnum.Simple,
							                                 Metrics = new[]
							                                           {
								                                           new KeyValuePair<string, object>("Component/Metric/Foo", 1),
								                                           new KeyValuePair<string, object>("Component/Metric/Bar", 3),
								                                           new KeyValuePair<string, object>("Component/Metric/Baz", 45),
							                                           }.ToDictionary(k => k.Key, k => k.Value),
						                                 },
						                                 new GenerateComponentDataInput
						                                 {
							                                 QueryName = "WackyQuery",
							                                 DataSent = false,
							                                 MetricTransformEnum = MetricTransformEnum.Simple,
							                                 Metrics = new[]
							                                           {
								                                           new KeyValuePair<string, object>("Component/Metric/Foo", 6),
								                                           new KeyValuePair<string, object>("Component/Metric/Bar", 5),
								                                           new KeyValuePair<string, object>("Component/Metric/Baz", 89),
							                                           }.ToDictionary(k => k.Key, k => k.Value),
						                                 }
					                                 }).Throws(typeof (ArgumentException)).SetName("Assert Different Query Names throws Exception Non-Aggregate Test"),
				       };
			}
		}

		[Test]
		[TestCaseSource("GetComponentDataTestCases")]
		public string[] AssertThatComponentDataGeneratedCorrectly(GenerateComponentDataInput[] inputdata)
		{
			var queryContextHistory = inputdata.Select(input =>
			                                           {
				                                           var queryContext = Substitute.For<IQueryContext>();
				                                           queryContext.QueryName.Returns(input.QueryName);
				                                           queryContext.DataSent.Returns(input.DataSent);
				                                           queryContext.MetricTransformEnum.Returns(input.MetricTransformEnum);
				                                           queryContext.ComponentData = new ComponentData();
				                                           input.Metrics.ForEach(m =>
				                                                                 {
					                                                                 if (m.Value is decimal)
					                                                                 {
						                                                                 queryContext.ComponentData.AddMetric(m.Key, (decimal) m.Value);
					                                                                 }

					                                                                 if (m.Value is int)
					                                                                 {
						                                                                 queryContext.ComponentData.AddMetric(m.Key, (int) m.Value);
					                                                                 }
				                                                                 });
				                                           return queryContext;
			                                           }).ToArray();

			var componentData = ComponentDataRetriever.GetData(queryContextHistory);

			return componentData.Metrics.Select(m => String.Format("{0}:{1}", m.Key, m.Value)).ToArray();
		}
	}
}
