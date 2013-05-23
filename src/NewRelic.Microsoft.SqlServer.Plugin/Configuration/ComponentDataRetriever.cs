using System;
using System.Linq;

using NewRelic.Microsoft.SqlServer.Plugin.Core;
using NewRelic.Microsoft.SqlServer.Plugin.Core.Extensions;
using NewRelic.Microsoft.SqlServer.Plugin.Properties;
using NewRelic.Platform.Binding.DotNET;

using log4net;

namespace NewRelic.Microsoft.SqlServer.Plugin.Configuration
{
	public static class ComponentDataRetriever
	{
		private static readonly ILog _VerboseMetricsLogger = LogManager.GetLogger(Constants.VerboseMetricsLogger);

		/// <summary>
		/// Retrieves or generates the appropriate ComponentData object from the historical and current QueryContexts in <paramref name="queryHistory"/>
		/// </summary>
		/// <param name="queryHistory">A FIFO array of QueryContexts representing the current and historical query data, most recent query is last position</param>
		/// <returns></returns>
		public static ComponentData GetData(IQueryContext[] queryHistory)
		{
			if (!queryHistory.Any())
			{
				return null;
			}

			var queryName = queryHistory.First().QueryName;
			if (queryHistory.Any(qh => qh.QueryName != queryName))
			{
				throw new ArgumentException("GetData can only process history for one query at a time!");
			}

			var mostRecentUnsentQuery = queryHistory.LastOrDefault(q => !q.DataSent);

			//If nothing to send, return null
			if (mostRecentUnsentQuery == null)
			{
				return null;
			}

			//If the query metric should be a Delta (differential, generate ComponentData based on passed in history
			if (mostRecentUnsentQuery.MetricTransformEnum == MetricTransformEnum.Delta)
			{
				//For first encounter, send empty metric
				if (queryHistory.Length == 1)
				{
					_VerboseMetricsLogger.InfoFormat("Not enough historical data to perform delta on data from {0}, generating zeroed out ComponentData", mostRecentUnsentQuery.QueryName);
					return GetZeroedComponentData(mostRecentUnsentQuery.ComponentData);
				}

				//Get Previous Query
				var previousQueryContext = queryHistory.Reverse().Skip(1).First();

				_VerboseMetricsLogger.InfoFormat("Generating delta for data from {0}", mostRecentUnsentQuery.QueryName);
				//Return Delta of current and previous data
				return GetDelta(mostRecentUnsentQuery.ComponentData, previousQueryContext.ComponentData);
			}

			//Otherwise, just return latest unsent
			return mostRecentUnsentQuery.ComponentData;
		}

		private static ComponentData GetZeroedComponentData(ComponentData componentData)
		{
			var zeroedData = new ComponentData
			                 {
				                 Name = componentData.Name,
				                 Guid = componentData.Guid,
				                 Duration = componentData.Duration
			                 };
			componentData.Metrics.ForEach(m =>
			                              {
				                              var val = m.Value;
				                              if (MetricMapper.IsMetricNumeric(val))
				                              {
					                              val = 0;
				                              }

				                              _VerboseMetricsLogger.InfoFormat("Zeroing Component: {0}; Metric: {1}; Value: {2}", zeroedData.Name, m.Key, val);
				                              zeroedData.Metrics.Add(m.Key, val);
			                              });
			return zeroedData;
		}

		private static ComponentData GetDelta(ComponentData currentData, ComponentData previousData)
		{
			var delta = new ComponentData
			            {
				            Name = currentData.Name,
				            Guid = currentData.Guid,
				            Duration = currentData.Duration
			            };

			var commonMetricNames = currentData.Metrics.Select(m => m.Key).Intersect(previousData.Metrics.Select(m => m.Key));

			commonMetricNames.ForEach(mn =>
			                          {
				                          var currentVal = currentData.Metrics[mn];
				                          var previousVal = previousData.Metrics[mn];

				                          if (!MetricMapper.IsMetricNumeric(currentVal))
				                          {
					                          //Simply add the latest value
					                          delta.Metrics.Add(mn, currentVal);
				                          }

				                          //Add metric as Positive Change, if negative add 0 as value
				                          if (currentVal is decimal)
				                          {
					                          var currentValDecimal = (decimal) currentVal;
					                          var previousValDecimal = (decimal) previousVal;

					                          var val = 0.0m;
					                          if (currentValDecimal >= previousValDecimal)
					                          {
						                          val = currentValDecimal - previousValDecimal;
					                          }

					                          _VerboseMetricsLogger.InfoFormat("Generated Delta for Component: {0}; Metric: {1}; Value: {2}", currentData.Name, mn, val);

					                          delta.Metrics.Add(mn, val);
				                          }
				                          else if (currentVal is int)
				                          {
					                          var currentValInt = (int) currentVal;
					                          var previousValInt = (int) previousVal;

					                          var val = 0;
					                          if (currentValInt >= previousValInt)
					                          {
						                          val = currentValInt - previousValInt;
					                          }

					                          _VerboseMetricsLogger.InfoFormat("Generated Delta for Component: {0}; Metric: {1}; Value: {2}", currentData.Name, mn, val);
					                          delta.Metrics.Add(mn, val);
				                          }
			                          });

			return delta;
		}
	}
}
