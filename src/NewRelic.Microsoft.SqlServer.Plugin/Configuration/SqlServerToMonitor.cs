using System;
using System.Collections.Generic;
using System.Linq;

using NewRelic.Microsoft.SqlServer.Plugin.Core.Extensions;
using NewRelic.Microsoft.SqlServer.Plugin.Properties;
using NewRelic.Platform.Binding.DotNET;

namespace NewRelic.Microsoft.SqlServer.Plugin.Configuration
{
	public interface ISqlServerToMonitor
	{
		string Name { get; }
		string ConnectionString { get; }

		/// <summary>
		///     The number of seconds since the last recorded successful report of metrics.
		/// </summary>
		int Duration { get; }

		string[] IncludedDatabases { get; }
		string[] ExcludedDatabases { get; }

		void MetricReportSuccessful();
		string ToString();
	}

	public class SqlServerToMonitor : ISqlServerToMonitor
	{
		private DateTime _lastSuccessfulReportTime;

		public SqlServerToMonitor(string name, string connectionString, bool includeSystemDatabases)
			: this(name, connectionString, includeSystemDatabases, null, null) {}

		public SqlServerToMonitor(string name, string connectionString, bool includeSystemDatabases, IEnumerable<string> includedDatabaseNames, IEnumerable<string> excludedDatabaseNames)
		{
			Name = name;
			ConnectionString = connectionString;
			_lastSuccessfulReportTime = DateTime.Now;

			var includedDbs = new List<string>();
			var excludedDbs = new List<string>();

			if (!includeSystemDatabases)
			{
				excludedDbs.AddRange(Constants.SystemDatabases);
			}

			if (includedDatabaseNames != null)
			{
				includedDbs.AddRange(includedDatabaseNames.Select(TransformDatabaseName));
			}

			if (excludedDatabaseNames != null)
			{
				excludedDbs.AddRange(excludedDatabaseNames.Select(TransformDatabaseName));
			}

			IncludedDatabases = includedDbs.ToArray();
			ExcludedDatabases = excludedDbs.ToArray();

			QueryHistory = new Dictionary<string, Queue<IQueryContext>>();
		}

		public IDictionary<string, Queue<IQueryContext>> QueryHistory { get; private set; }

		public string Name { get; private set; }
		public string ConnectionString { get; private set; }

		public int Duration
		{
			get { return (int) DateTime.Now.Subtract(_lastSuccessfulReportTime).TotalSeconds; }
		}

		public string[] IncludedDatabases { get; private set; }
		public string[] ExcludedDatabases { get; private set; }

        public void MetricReportSuccessful(DateTime? reportDate = null)
		{
            _lastSuccessfulReportTime = reportDate ?? DateTime.Now;
			QueryHistory.Values.ForEach(histories => histories.ForEach(qc => qc.DataSent = true));
		}

		public override string ToString()
		{
			return FormatProperties(Name, ConnectionString, IncludedDatabases, ExcludedDatabases);
		}

		/// <summary>
		///     Used to transform a the database name string from the configuration file into a sql ready database name
		/// </summary>
		/// <param name="name">Database name from the configuration file</param>
		/// <returns>Formatted and qualified sql ready string representing a database name</returns>
		private static string TransformDatabaseName(string name)
		{
			return name.Replace('*', '%');
		}

		public static string FormatProperties(string name, string connectionString, string[] includedDatabases, string[] excludedDatabases)
		{
			return string.Format("Name: {0}, ConnectionString: {1}, IncludedDatabases: {2}, ExcludedDatabases: {3}",
			                     name,
			                     connectionString,
			                     string.Join(", ", includedDatabases),
			                     string.Join(", ", excludedDatabases));
		}

		public void UpdateHistory(IQueryContext[] queryContexts)
		{
			queryContexts.ForEach(queryContext =>
			                      {
				                      Queue<IQueryContext> queryHistory = QueryHistory.GetOrCreate(queryContext.QueryName);
				                      if (queryHistory.Count >= 2) //Only track up to last run of this query
				                      {
					                      queryHistory.Dequeue();
				                      }
				                      queryHistory.Enqueue(queryContext);
			                      });
		}

		public PlatformData GeneratePlatformData(AgentData agentData)
		{
			var platformData = new PlatformData(agentData);

			ComponentData[] pendingComponentData = QueryHistory.Select(qh => ComponentDataRetriever.GetData(qh.Value.ToArray()))
			                                                   .Where(c => c != null).ToArray();

			pendingComponentData.ForEach(platformData.AddComponent);

			return platformData;
		}
	}
}
