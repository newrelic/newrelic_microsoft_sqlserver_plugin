using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using NewRelic.Microsoft.SqlServer.Plugin.Configuration;
using NewRelic.Microsoft.SqlServer.Plugin.Core.Extensions;
using NewRelic.Microsoft.SqlServer.Plugin.Properties;
using NewRelic.Microsoft.SqlServer.Plugin.QueryTypes;
using NewRelic.Platform.Binding.DotNET;

using log4net;

namespace NewRelic.Microsoft.SqlServer.Plugin
{
	public abstract class SqlEndpointBase : ISqlEndpoint
	{
		private static readonly ILog _ErrorDetailOutputLogger = LogManager.GetLogger(Constants.ErrorDetailLogger);
		private static readonly ILog _VerboseSqlOutputLogger = LogManager.GetLogger(Constants.VerboseSqlLogger);

		private DateTime _lastSuccessfulReportTime;
		private SqlQuery[] _queries;

		protected SqlEndpointBase(string name, string connectionString)
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

		public virtual void ToLog(ILog log)
		{
			// Remove password from logging
			var safeConnectionString = new SqlConnectionStringBuilder(ConnectionString);
			if (!string.IsNullOrEmpty(safeConnectionString.Password))
			{
				safeConnectionString.Password = "[redacted]";
			}

			log.InfoFormat("      {0}: {1}", Name, safeConnectionString);

			// Validate that connection string do not provide both Trusted Security AND user/password
			var hasUserCreds = !string.IsNullOrEmpty(safeConnectionString.UserID) || !string.IsNullOrEmpty(safeConnectionString.Password);
			if (safeConnectionString.IntegratedSecurity == hasUserCreds)
			{
				log.Error("==================================================");
				log.ErrorFormat("Connection string for '{0}' may not contain both Integrated Security and User ID/Password credentials. " +
				                "Review the readme.md and update the config file.", safeConnectionString.DataSource);
				log.Error("==================================================");
			}
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
						_ErrorDetailOutputLogger.Error(string.Format("Error with query '{0}' at endpoint '{1}'", query.QueryName, safeConnectionString), e);
						LogErrorSummary(log, e, query);
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

			var currentValues = sqlDmlActivities
				.GroupBy(a => string.Format("{0}:{1}:{2}:{3}", BitConverter.ToString(a.PlanHandle), BitConverter.ToString(a.SqlStatementHash), a.CreationTime.Ticks, a.QueryType))
				.Select(a => new
				             {
					             a.Key,
					             //If we ever gets dupes, sum Excution Count
					             Activity = new SqlDmlActivity
					                        {
						                        CreationTime = a.First().CreationTime,
						                        SqlStatementHash = a.First().SqlStatementHash,
						                        PlanHandle = a.First().PlanHandle,
						                        QueryType = a.First().QueryType,
						                        ExecutionCount = a.Sum(dml => dml.ExecutionCount),
					                        }
				             })
				.ToDictionary(a => a.Key, a => a.Activity);

			long reads = 0;
			long writes = 0;

			// If this is the first time through, reads and writes are definitely 0
			if (SqlDmlActivityHistory.Count > 0)
			{
				currentValues
					.ForEach(a =>
					         {
						         long increase;

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

		private void LogErrorSummary(ILog log, Exception e, ISqlQuery query)
		{
			var sqlException = e.InnerException as SqlException;
			if (sqlException == null) return;

			var connectionString = new SqlConnectionStringBuilder(ConnectionString);

			switch (sqlException.Number)
			{
				case 297: // User cannot log on via Windows Auth
				case 18456: // User cannot login via SQL Auth
					if (connectionString.IntegratedSecurity)
					{
						// System.Data.SqlClient.SqlException: Login failed. The login is from an untrusted domain and cannot be used with Windows authentication.
						log.ErrorFormat("The Windows service is running as user '{0}', however, the user cannot access the server '{1}'. " +
						                "Consider changing the connection string in the configuration file " +
						                "or adding permissions to your SQL Server (see readme.md).",
						                Environment.UserName, connectionString.DataSource);
					}
					else
					{
						// System.Data.SqlClient.SqlException: Login failed for user '<user id>'.
						log.ErrorFormat("User '{0}' cannot access the server '{1}'. " +
						                "Consider changing the connection string in the configuration file " +
						                "or adding permissions to your SQL Server (see readme.md).",
						                connectionString.UserID, connectionString.DataSource);
					}
					break;

				case 4060: // Missing database user
					// System.Data.SqlClient.SqlException: Cannot open database "Junk" requested by the login. The login failed.
					if (connectionString.IntegratedSecurity)
					{
						log.ErrorFormat("The Windows service is running as user '{0}', however, the user cannot access the database '{1}'. " +
						                "Ensure the login has a user in the database (see readme.md).",
						                Environment.UserName, connectionString.InitialCatalog);
					}
					else
					{
						log.ErrorFormat("User '{0}' cannot access the database '{1}'. " +
						                "Ensure the login has a user in the database (see readme.md).",
						                connectionString.UserID, connectionString.InitialCatalog);
					}
					break;

				case 10060:
				case 10061:
				case 11001:
				case 40615:
					if (sqlException.Message.Contains("sp_set_firewall_rule"))
					{
						var relevantErrorMessage = Regex.Replace(sqlException.Message, @"change to take effect\.(.*)$", string.Empty, RegexOptions.Singleline);
						log.Error("Azure SQL Error: " + relevantErrorMessage);
					}
					else
					{
						log.ErrorFormat("Timeout connecting to server at '{0}'. Verify that the connection string is correct and the server is reachable.",
						                connectionString.DataSource);
					}
					break;

				default:
					// System.Data.SqlClient.SqlException: Arithmetic overflow error for data type tinyint, value = -34.
					// System.Data.SqlClient.SqlException: Arithmetic overflow error converting expression to data type int.
					log.ErrorFormat("Error collecting metric '{0}'. Contact New Relic support at https://support.newrelic.com/home. Details: C{1}, M{2}, S{3}",
					                query.QueryName, sqlException.Class, sqlException.Number, sqlException.State);
					break;
			}
		}
	}
}
