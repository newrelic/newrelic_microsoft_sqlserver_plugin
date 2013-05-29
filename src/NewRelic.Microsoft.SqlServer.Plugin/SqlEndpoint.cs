using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

using NewRelic.Microsoft.SqlServer.Plugin.Configuration;
using NewRelic.Microsoft.SqlServer.Plugin.Core.Extensions;
using NewRelic.Microsoft.SqlServer.Plugin.Properties;
using NewRelic.Microsoft.SqlServer.Plugin.QueryTypes;
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
			SqlDmlActivityHistory = new Dictionary<string, SqlDmlActivity>();
		}

		protected abstract string ComponentGuid { get; }

		public IDictionary<string, Queue<IQueryContext>> QueryHistory { get; private set; }
		protected Dictionary<string, SqlDmlActivity> SqlDmlActivityHistory { get; set; }

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

						results = OnQueryExecuted(query, results, log);

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

		internal virtual object[] OnQueryExecuted(ISqlQuery query, object[] results, ILog log)
		{
			return query.QueryType == typeof (SqlDmlActivity) ? CalculateSqlDmlActivityIncrease(results, log) : results;
		}

		public override string ToString()
		{
			return string.Format("Name: {0}, ConnectionString: {1}", Name, ConnectionString);
		}

		private object[] CalculateSqlDmlActivityIncrease(object[] inputResults, ILog log)
		{
			if (inputResults == null || inputResults.Length == 0)
			{
				log.Error("No values passed to CalculateSqlDmlActivityIncrease");
				return inputResults;
			}

			SqlDmlActivity[] sqlDmlActivities = inputResults.OfType<SqlDmlActivity>().ToArray();

			if (!sqlDmlActivities.Any())
			{
				log.Error("In trying to Process results for SqlDmlActivity, results were NULL or not of the appropriate type");
				return inputResults;
			}

			Dictionary<string, SqlDmlActivity> currentValues = sqlDmlActivities.ToDictionary(a => string.Format("{0}:{1}:{2}",BitConverter.ToString(a.PlanHandle), a.SQlStatement, a.CreationTime));

			int reads = 0;
			int writes = 0;
			currentValues.ForEach(a =>
			                      {
				                      if (SqlDmlActivityHistory.ContainsKey(a.Key))
				                      {
					                      SqlDmlActivity previous = SqlDmlActivityHistory[a.Key];
					                      int currentExecutionCount = a.Value.ExecutionCount;
					                      int previousExecutionCount = previous.ExecutionCount;

					                      if (currentExecutionCount > previousExecutionCount && a.Value.QueryType == previous.QueryType)
					                      {
						                      int increase = currentExecutionCount - previousExecutionCount;

						                      switch (a.Value.QueryType)
						                      {
							                      case "Writes":
								                      writes += increase;
								                      break;
							                      case "Reads":
								                      reads += increase;
								                      break;
						                      }
					                      }
				                      }
			                      });

			//Current Becomes the new history
			SqlDmlActivityHistory = currentValues;

			//return the sum of all increases for reads and writes
			//if there is was no history (first time for this db) then reads and writes will be 0
			return new object[]
			       {
				       new SqlDmlActivity
				       {
					       Reads = reads,
					       Writes = writes,
				       },
			       };
		}
	}
}
