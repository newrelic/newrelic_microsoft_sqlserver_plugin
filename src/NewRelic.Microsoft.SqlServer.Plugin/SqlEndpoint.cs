using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

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

			IncludedDatabases = new Database[0];
			ExcludedDatabaseNames = new string[0];
		}

		protected abstract string ComponentGuid { get; }

		public IDictionary<string, Queue<IQueryContext>> QueryHistory { get; private set; }
		protected Dictionary<string, SqlDmlActivity> SqlDmlActivityHistory { get; set; }

		public Database[] IncludedDatabases { get; protected set; }

		public string[] IncludedDatabaseNames
		{
			get { return IncludedDatabases.Select(d => d.Name).ToArray(); }
		}

		public string[] ExcludedDatabaseNames { get; protected set; }

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

		public virtual IEnumerable<IQueryContext> ExecuteQueries(ILog log)
		{
			return ExecuteQueries(_queries, ConnectionString, log);
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

		protected IEnumerable<IQueryContext> ExecuteQueries(SqlQuery[] queries, string connectionString, ILog log)
		{
			// Remove password from logging
			var safeConnectionString = new SqlConnectionStringBuilder(connectionString);
			if (!string.IsNullOrEmpty(safeConnectionString.Password))
			{
				safeConnectionString.Password = "[redacted]";
			}

			_VerboseSqlOutputLogger.InfoFormat("Connecting with {0}", safeConnectionString);

			using (var conn = new SqlConnection(connectionString))
			{
				foreach (var query in queries)
				{
					object[] results;
					try
					{
						results = query.Query(conn, this).ToArray();

						// This could be slow, so only proceed if it actually gets logged
						if (_VerboseSqlOutputLogger.IsInfoEnabled)
						{
							var verboseLogging = new StringBuilder();
							verboseLogging.AppendFormat("Executed {0}", query.ResourceName).AppendLine();

							foreach (var result in results)
							{
								verboseLogging.AppendLine(result.ToString());
							}

							_VerboseSqlOutputLogger.Info(verboseLogging.ToString());
						}

						results = OnQueryExecuted(query, results, log);
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

		internal QueryContext CreateQueryContext(ISqlQuery query, IEnumerable<object> results)
		{
			return new QueryContext(query) {Results = results, ComponentData = new ComponentData(Name, ComponentGuid, Duration)};
		}

		protected internal abstract IEnumerable<SqlQuery> FilterQueries(IEnumerable<SqlQuery> queries);

		protected virtual object[] OnQueryExecuted(ISqlQuery query, object[] results, ILog log)
		{
			return query.QueryType == typeof (SqlDmlActivity) ? CalculateSqlDmlActivityIncrease(results, log) : results;
		}

		public override string ToString()
		{
			return string.Format("Name: {0}, ConnectionString: {1}", Name, ConnectionString);
		}

		internal object[] CalculateSqlDmlActivityIncrease(object[] inputResults, ILog log)
		{
			if (inputResults == null || inputResults.Length == 0)
			{
				log.Error("No values passed to CalculateSqlDmlActivityIncrease");
				return inputResults;
			}

			var sqlDmlActivities = inputResults.OfType<SqlDmlActivity>().ToArray();

			if (!sqlDmlActivities.Any())
			{
				log.Error("In trying to Process results for SqlDmlActivity, results were NULL or not of the appropriate type");
				return inputResults;
			}

			var currentValues = sqlDmlActivities.ToDictionary(a => string.Format("{0}:{1}:{2}", BitConverter.ToString(a.PlanHandle), a.SQlStatement, a.CreationTime));

			var reads = 0;
			var writes = 0;

			// If this is the first time through, reads and writes are definitely 0
			if (SqlDmlActivityHistory.Count > 0)
			{
				currentValues
					.ForEach(a =>
					         {
						         int increase;

						         // Find a matching previous value for a delta
						         SqlDmlActivity previous;
						         if (!SqlDmlActivityHistory.TryGetValue(a.Key, out previous))
						         {
							         // Nothing previous, the delta is the absolute value here
							         increase = a.Value.ExecutionCount;
						         }
						         else if (a.Value.QueryType == previous.QueryType)
						         {
									 // Calculate the delta
							         increase = a.Value.ExecutionCount - previous.ExecutionCount;

							         // Only record positive deltas, though theoretically impossible here
							         if (increase <= 0) return;
						         }
						         else
						         {
							         return;
						         }

						         switch (a.Value.QueryType)
						         {
							         case "Writes":
								         writes += increase;
								         break;
							         case "Reads":
								         reads += increase;
								         break;
						         }
					         });
			}

			//Current Becomes the new history
			SqlDmlActivityHistory = currentValues;

			if (_VerboseSqlOutputLogger.IsInfoEnabled)
			{
				_VerboseSqlOutputLogger.InfoFormat("SQL DML Activity: Reads={0} Writes={1}", reads, writes);
				_VerboseSqlOutputLogger.Info("");
			}

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
