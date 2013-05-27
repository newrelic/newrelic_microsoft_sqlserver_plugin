using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

using NewRelic.Microsoft.SqlServer.Plugin.Configuration;
using NewRelic.Microsoft.SqlServer.Plugin.Core.Extensions;
using NewRelic.Microsoft.SqlServer.Plugin.Properties;
using NewRelic.Platform.Binding.DotNET;

using log4net;

namespace NewRelic.Microsoft.SqlServer.Plugin
{
	public abstract class SqlEndpoint : ISqlEndpoint
	{
		private static readonly ILog _VerboseSqlOutputLogger = LogManager.GetLogger(Constants.VerboseSqlLogger);

		private DateTime _lastSuccessfulReportTime;
		private SqlQuery[] _queries;

		protected SqlEndpoint(string name, string connectionString)
		{
			Name = name;
			ConnectionString = connectionString;
			_lastSuccessfulReportTime = DateTime.Now;

			QueryHistory = new Dictionary<string, Queue<IQueryContext>>();
		}

		protected abstract string ComponentGuid { get; }

		public IDictionary<string, Queue<IQueryContext>> QueryHistory { get; private set; }

		public string Name { get; private set; }
		public string ConnectionString { get; private set; }

		public int Duration
		{
			get { return (int) DateTime.Now.Subtract(_lastSuccessfulReportTime).TotalSeconds; }
		}

		public void SetQueries(IEnumerable<SqlQuery> queries)
		{
			_queries = FilterQueries(queries).ToArray();
		}

		public IEnumerable<IQueryContext> ExecuteQueries(ILog log)
		{
			// Remove password from logging
			var safeConnectionString = new SqlConnectionStringBuilder(ConnectionString);
			if (!string.IsNullOrEmpty(safeConnectionString.Password))
			{
				safeConnectionString.Password = "[redacted]";
			}

			_VerboseSqlOutputLogger.InfoFormat("Connecting with {0}", safeConnectionString);
			_VerboseSqlOutputLogger.Info("");

			using (var conn = new SqlConnection(ConnectionString))
			{
				foreach (SqlQuery query in _queries)
				{
					object[] results;
					try
					{
						_VerboseSqlOutputLogger.InfoFormat("Executing {0}", query.ResourceName);
						results = query.Query(conn, this).ToArray();

						if (_VerboseSqlOutputLogger.IsInfoEnabled)
						{
							foreach (object result in results)
							{
								// TODO Replace ToString() with something more useful that prints each property in the object
								_VerboseSqlOutputLogger.Info(result.ToString());
							}
							_VerboseSqlOutputLogger.Info("");
						}

						results = OnQueryExecuted(query, results);

						if (_VerboseSqlOutputLogger.IsInfoEnabled)
						{
							_VerboseSqlOutputLogger.Info("After Results Messaged");

							foreach (object result in results)
							{
								// TODO Replace ToString() with something more useful that prints each property in the object
								_VerboseSqlOutputLogger.Info(result.ToString());
							}
							_VerboseSqlOutputLogger.Info("");
						}

					}
					catch (Exception e)
					{
						log.Error(string.Format("Error with query '{0}'", query.QueryName), e);
						continue;
					}
					yield return CreateQueryContext(query, results);
				}
			}
		}

		public void MetricReportSuccessful(DateTime? reportDate = null)
		{
			_lastSuccessfulReportTime = reportDate ?? DateTime.Now;
			QueryHistory.Values.ForEach(histories => histories.ForEach(qc => qc.DataSent = true));
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

		public virtual void Trace(ILog log)
		{
			// Remove password from logging
			var safeConnectionString = new SqlConnectionStringBuilder(ConnectionString);
			if (!string.IsNullOrEmpty(safeConnectionString.Password))
			{
				safeConnectionString.Password = "[redacted]";
			}

			log.DebugFormat("\t\t{0}: {1}", Name, safeConnectionString);
		}

		internal QueryContext CreateQueryContext(ISqlQuery query, IEnumerable<object> results)
		{
			return new QueryContext(query) {Results = results, ComponentData = new ComponentData(Name, ComponentGuid, Duration)};
		}

		protected internal abstract IEnumerable<SqlQuery> FilterQueries(IEnumerable<SqlQuery> queries);

		internal virtual object[] OnQueryExecuted(ISqlQuery query, object[] results)
		{
			return results;
		}

		public override string ToString()
		{
			return string.Format("Name: {0}, ConnectionString: {1}", Name, ConnectionString);
		}
	}
}
