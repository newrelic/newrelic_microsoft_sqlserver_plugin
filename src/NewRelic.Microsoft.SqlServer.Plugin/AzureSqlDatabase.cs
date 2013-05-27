using System;
using System.Collections.Generic;
using System.Linq;

using NewRelic.Microsoft.SqlServer.Plugin.Core;
using NewRelic.Microsoft.SqlServer.Plugin.Core.Extensions;
using NewRelic.Microsoft.SqlServer.Plugin.Properties;
using NewRelic.Microsoft.SqlServer.Plugin.QueryTypes;

using log4net;

namespace NewRelic.Microsoft.SqlServer.Plugin
{
	public class AzureSqlDatabase : SqlEndpoint
	{
		private readonly ILog _log;

		public AzureSqlDatabase(string name, string connectionString, ILog log)
			: base(name, connectionString)
		{
			_log = log;
			SqlDmlActivityHistory = new Dictionary<string, SqlDmlActivity>();
		}

		private Dictionary<string, SqlDmlActivity> SqlDmlActivityHistory { get; set; }

		protected override string ComponentGuid
		{
			get { return Constants.SqlAzureComponentGuid; }
		}

		protected internal override IEnumerable<SqlQuery> FilterQueries(IEnumerable<SqlQuery> queries)
		{
			return queries.Where(q => q.QueryAttribute is AzureSqlQueryAttribute);
		}

		private object[] CalculateSqlDmlActivityIncrease(object[] inputResults)
		{
			if (inputResults == null || inputResults.Length == 0)
			{
				_log.Error("No values passed to CalculateSqlDmlActivityIncrease");
				return inputResults;
			}

			SqlDmlActivity[] sqlDmlActivities = inputResults.OfType<SqlDmlActivity>().ToArray();

			if (!sqlDmlActivities.Any())
			{
				_log.Error("In trying to Process results for SqlDmlActivity, results were NULL or not of the appropriate type");
				return inputResults;
			}

			Dictionary<string, SqlDmlActivity> currentValues = sqlDmlActivities.ToDictionary(a => BitConverter.ToString(a.SqlHandle) + ":" + BitConverter.ToString(a.QueryHash));

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

		internal override object[] OnQueryExecuted(ISqlQuery query, object[] results)
		{
			base.OnQueryExecuted(query, results);

			return query.QueryType == typeof (SqlDmlActivity) ? CalculateSqlDmlActivityIncrease(results) : results;
		}
	}
}
