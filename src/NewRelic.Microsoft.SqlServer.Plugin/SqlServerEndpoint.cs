using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

using NewRelic.Microsoft.SqlServer.Plugin.Configuration;
using NewRelic.Microsoft.SqlServer.Plugin.Core;
using NewRelic.Microsoft.SqlServer.Plugin.Properties;
using NewRelic.Microsoft.SqlServer.Plugin.QueryTypes;
using NewRelic.Platform.Sdk.Utils;

namespace NewRelic.Microsoft.SqlServer.Plugin
{
    public class SqlServerEndpoint : SqlEndpointBase
    {
        private static readonly Logger _log = Logger.GetLogger(typeof(SqlServerEndpoint).Name);

        public SqlServerEndpoint(string name, string connectionString, bool includeSystemDatabases)
            : this(name, connectionString, includeSystemDatabases, null, null) {}

        public SqlServerEndpoint(string name, string connectionString, bool includeSystemDatabases, IEnumerable<Database> includedDatabases, IEnumerable<string> excludedDatabaseNames)
            : base(name, connectionString)
        {
            var excludedDbs = new List<string>();
            var includedDbs = new List<Database>();

            if (excludedDatabaseNames != null)
            {
                excludedDbs.AddRange(excludedDatabaseNames);
            }

            if (includedDatabases != null)
            {
                includedDbs.AddRange(includedDatabases);
            }

            if (includeSystemDatabases && includedDbs.Any())
            {
                IEnumerable<Database> systemDbsToAdd = Constants.SystemDatabases
                                                                .Where(dbName => includedDbs.All(db => db.Name != dbName))
                                                                .Select(dbName => new Database {Name = dbName});
                includedDbs.AddRange(systemDbsToAdd);
            }
            else if (!includeSystemDatabases)
            {
                IEnumerable<string> systemDbsToAdd = Constants.SystemDatabases
                                                              .Where(dbName => excludedDbs.All(db => db != dbName));
                excludedDbs.AddRange(systemDbsToAdd);
            }

            IncludedDatabases = includedDbs.ToArray();
            ExcludedDatabaseNames = excludedDbs.ToArray();
        }

        protected override string ComponentGuid
        {
            get { return Constants.SqlServerComponentGuid; }
        }

        public override string ToString()
        {
            return FormatProperties(Name, ConnectionString, IncludedDatabaseNames, ExcludedDatabaseNames);
        }

        internal static string FormatProperties(string name, string connectionString, string[] includedDatabases, string[] excludedDatabases)
        {
            return String.Format("Name: {0}, ConnectionString: {1}, IncludedDatabaseNames: {2}, ExcludedDatabaseNames: {3}",
                                 name,
                                 connectionString,
                                 string.Join(", ", includedDatabases),
                                 string.Join(", ", excludedDatabases));
        }

        /// <summary>
        ///     Replaces the database name on <see cref="IDatabaseMetric" /> results with the <see cref="Database.DisplayName" />
        ///     when possible.
        /// </summary>
        /// <param name="includedDatabases"></param>
        /// <param name="results"></param>
        internal static void ApplyDatabaseDisplayNames(IEnumerable<Database> includedDatabases, object[] results)
        {
            if (includedDatabases == null)
            {
                return;
            }

            Dictionary<string, string> renameMap = includedDatabases.Where(d => !string.IsNullOrEmpty(d.DisplayName)).ToDictionary(d => d.Name.ToLower(), d => d.DisplayName);
            if (!renameMap.Any())
            {
                return;
            }

            IDatabaseMetric[] databaseMetrics = results.OfType<IDatabaseMetric>().Where(d => !string.IsNullOrEmpty(d.DatabaseName)).ToArray();
            if (!databaseMetrics.Any())
            {
                return;
            }

            foreach (IDatabaseMetric databaseMetric in databaseMetrics)
            {
                string displayName;
                if (renameMap.TryGetValue(databaseMetric.DatabaseName.ToLower(), out displayName))
                {
                    databaseMetric.DatabaseName = displayName;
                }
            }
        }

        protected internal override IEnumerable<SqlQuery> FilterQueries(IEnumerable<SqlQuery> queries)
        {
            return queries.Where(q => q.QueryAttribute is SqlServerQueryAttribute);
        }

        public override void ToLog()
        {
            base.ToLog();

            // Attempt to connect to the server and get basic details about the server and the databases.
            Dictionary<string, DatabaseDetails> databaseDetailsByName;
            try
            {
                var queryLocator = new QueryLocator(new DapperWrapper());
                SqlQuery serverDetailsQuery = queryLocator.PrepareQueries(new[] { typeof(SqlServerDetails), }, false).Single();
                SqlQuery databasesDetailsQuery = queryLocator.PrepareQueries(new[] { typeof(DatabaseDetails), }, false).Single();
                using (var conn = new SqlConnection(ConnectionString))
                {
                    // Log the server details
                    SqlServerDetails serverDetails = serverDetailsQuery.Query<SqlServerDetails>(conn, this).Single();
                    LogVerboseSqlResults(serverDetailsQuery, new[] { serverDetails });
                    _log.Info("        {0} {1} {2} ({3})", serverDetails.SQLTitle, serverDetails.Edition, serverDetails.ProductLevel, serverDetails.ProductVersion);

                    // Sotre these for reporting below
                    DatabaseDetails[] databasesDetails = databasesDetailsQuery.DatabaseMetricQuery<DatabaseDetails>(conn, this).ToArray();
                    LogVerboseSqlResults(databasesDetailsQuery, databasesDetails);
                    databaseDetailsByName = databasesDetails.ToDictionary(d => d.DatabaseName);
                }
            }
            catch (Exception e)
            {
                // Just log some details here. The subsequent queries for metrics yields more error details.
                _log.Error("        Unable to connect: {0}", e.Message);
                databaseDetailsByName = null;
            }

            bool hasExplicitIncludedDatabases = IncludedDatabaseNames.Any();
            if (hasExplicitIncludedDatabases)
            {
                // Show the user the databases we'll be working from
                foreach (Database database in IncludedDatabases)
                {
                    string message = "        Including DB: " + database.Name;

                    // When the details are reachable, show them
                    if (databaseDetailsByName != null)
                    {
                        DatabaseDetails details;
                        if (databaseDetailsByName.TryGetValue(database.Name, out details))
                        {
                            message += string.Format(" [CompatibilityLevel={0};State={1}({2});CreateDate={3:yyyy-MM-dd};UserAccess={4}({5})]",
                                                     details.compatibility_level,
                                                     details.state_desc,
                                                     details.state,
                                                     details.create_date,
                                                     details.user_access_desc,
                                                     details.user_access);
                        }
                        else
                        {
                            // More error details are reported with metric queries
                            message += " [Unable to find database information]";
                        }
                    }

                    _log.Info(message);
                }
            }
            else if (databaseDetailsByName != null)
            {
                // The user didn't specifically include any databases
                // Report details for all of the DBs we expect to gather metrics against
                foreach (DatabaseDetails details in databaseDetailsByName.Values)
                {
                    _log.Info("        Including DB: {0} [CompatibilityLevel={1};State={2}({3});CreateDate={4:yyyy-MM-dd};UserAccess={5}({6})]",
                                   details.DatabaseName,
                                   details.compatibility_level,
                                   details.state_desc,
                                   details.state,
                                   details.create_date,
                                   details.user_access_desc,
                                   details.user_access);
                }
            }

            // If there are included DB's, log the Excluded DB's as DEBUG info.
            foreach (string database in ExcludedDatabaseNames)
            {
                _log.Debug("        Excluding DB: " + database);
            }
        }

        protected override object[] OnQueryExecuted(ISqlQuery query, object[] results)
        {
            results = base.OnQueryExecuted(query, results);
            ApplyDatabaseDisplayNames(IncludedDatabases, results);
            return results;
        }

        public override IEnumerable<IQueryContext> ExecuteQueries()
        {
            IEnumerable<IQueryContext> queries = base.ExecuteQueries();

            foreach (IQueryContext query in queries)
            {
                yield return query;

                if (query.QueryType != typeof (RecompileSummary)) continue;

                // Manually add this summary metric
                IQueryContext max = GetMaxRecompileSummaryMetric(query.Results);
                if (max != null)
                {
                    yield return max;
                }
            }
        }

        /// <summary>
        ///     Reports a maximum set of values for the recompiles to support the Summary Metric on the New Relic dashboard
        /// </summary>
        /// <param name="results"></param>
        /// <returns></returns>
        internal IQueryContext GetMaxRecompileSummaryMetric(IEnumerable<object> results)
        {
            if (results == null) return null;

            RecompileSummary[] recompileSummaries = results.OfType<RecompileSummary>().ToArray();
            if (!recompileSummaries.Any()) return null;

            var max = new RecompileMaximums
                      {
                          SingleUsePercent = recompileSummaries.Max(s => s.SingleUsePercent),
                          SingleUseObjects = recompileSummaries.Max(s => s.SingleUseObjects),
                          MultipleUseObjects = recompileSummaries.Max(s => s.MultipleUseObjects),
                      };

            var metricQuery = new MetricQuery(typeof (RecompileMaximums), typeof (RecompileMaximums).Name, typeof (RecompileMaximums).Name);
            return CreateQueryContext(metricQuery, new[] {max});
        }
    }
}
