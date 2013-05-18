using System;
using System.Collections.Generic;
using System.Linq;

using NewRelic.Microsoft.SqlServer.Plugin.Core.Extensions;
using NewRelic.Microsoft.SqlServer.Plugin.Properties;
using NewRelic.Platform.Binding.DotNET;

namespace NewRelic.Microsoft.SqlServer.Plugin.Configuration
{
	public class SqlEndpoint : ISqlEndpoint
	{
		private DateTime _lastSuccessfulReportTime;

		public SqlEndpoint(string name, string connectionString, bool includeSystemDatabases)
			: this(name, connectionString, includeSystemDatabases, null, null) {}

		public SqlEndpoint(string name, string connectionString, bool includeSystemDatabases, IEnumerable<Database> includedDbs, IEnumerable<string> excludedDatabaseNames)
		{
			Name = name;
			ConnectionString = connectionString;
			_lastSuccessfulReportTime = DateTime.Now;

			var excludedDbs = new List<string>();

			if (!includeSystemDatabases)
			{
				excludedDbs.AddRange(Constants.SystemDatabases);
			}

			if (excludedDatabaseNames != null)
			{
				excludedDbs.AddRange(excludedDatabaseNames);
			}

			IncludedDatabases = includedDbs != null ? includedDbs.ToArray() : new Database[0];
			ExcludedDatabaseNames = excludedDbs.ToArray();

			QueryHistory = new Dictionary<string, Queue<IQueryContext>>();
		}

		public IDictionary<string, Queue<IQueryContext>> QueryHistory { get; private set; }

		public string Name { get; private set; }
		public string ConnectionString { get; private set; }

		public int Duration
		{
			get { return (int) DateTime.Now.Subtract(_lastSuccessfulReportTime).TotalSeconds; }
		}

		public Database[] IncludedDatabases { get; private set; }

		public string[] IncludedDatabaseNames
		{
			get { return IncludedDatabases.Select(d => d.Name).ToArray(); }
		}

		public string[] ExcludedDatabaseNames { get; private set; }

		public void MetricReportSuccessful(DateTime? reportDate = null)
		{
			_lastSuccessfulReportTime = reportDate ?? DateTime.Now;
			QueryHistory.Values.ForEach(histories => histories.ForEach(qc => qc.DataSent = true));
		}

		public override string ToString()
		{
			return FormatProperties(Name, ConnectionString, IncludedDatabaseNames, ExcludedDatabaseNames);
		}

		internal static string FormatProperties(string name, string connectionString, string[] includedDatabases, string[] excludedDatabases)
		{
			return string.Format("Name: {0}, ConnectionString: {1}, IncludedDatabaseNames: {2}, ExcludedDatabaseNames: {3}",
			                     name,
			                     connectionString,
			                     string.Join(", ", includedDatabases),
			                     string.Join(", ", excludedDatabases));
		}

		public void UpdateHistory(IQueryContext[] queryContexts)
		{
			queryContexts.ForEach(queryContext =>
			                      {
				                      var queryHistory = QueryHistory.GetOrCreate(queryContext.QueryName);
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

			var pendingComponentData = QueryHistory.Select(qh => ComponentDataRetriever.GetData(qh.Value.ToArray()))
			                                       .Where(c => c != null).ToArray();

			pendingComponentData.ForEach(platformData.AddComponent);

			return platformData;
		}
	}
}
