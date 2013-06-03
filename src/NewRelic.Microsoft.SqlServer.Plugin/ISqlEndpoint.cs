using System;
using System.Collections.Generic;

using NewRelic.Platform.Binding.DotNET;

using log4net;

namespace NewRelic.Microsoft.SqlServer.Plugin
{
	public interface ISqlEndpoint
	{
		string Name { get; }
		string ConnectionString { get; }

		/// <summary>
		///     The number of seconds since the last recorded successful report of metrics.
		/// </summary>
		int Duration { get; }

		string[] IncludedDatabaseNames { get; }
		string[] ExcludedDatabaseNames { get; }

		void SetQueries(IEnumerable<SqlQuery> queries);

		IEnumerable<IQueryContext> ExecuteQueries(ILog log);

		/// <summary>
		/// Inform the server context that a report was sent on its behalf. Used to determine the <see cref="Duration"/>
		/// </summary>
		/// <param name="reportDate"></param>
		void MetricReportSuccessful(DateTime? reportDate = null);

		void UpdateHistory(IQueryContext[] queryContexts);
		PlatformData GeneratePlatformData(AgentData agentData);

		void Trace(ILog log);
	}
}
