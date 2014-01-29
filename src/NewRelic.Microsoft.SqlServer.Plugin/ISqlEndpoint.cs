using System.Collections.Generic;

using log4net;

namespace NewRelic.Microsoft.SqlServer.Plugin
{
    public interface ISqlEndpoint
    {
        string Name { get; }
        string ConnectionString { get; }

        string[] IncludedDatabaseNames { get; }
        string[] ExcludedDatabaseNames { get; }

        void SetQueries(IEnumerable<SqlQuery> queries);

        IEnumerable<IQueryContext> ExecuteQueries(ILog log);

        void ToLog(ILog log);
    }
}
