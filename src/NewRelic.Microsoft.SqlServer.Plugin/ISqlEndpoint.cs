using System.Collections.Generic;

namespace NewRelic.Microsoft.SqlServer.Plugin
{
    public interface ISqlEndpoint
    {
        string Name { get; }
        string ConnectionString { get; }

        string[] IncludedDatabaseNames { get; }
        string[] ExcludedDatabaseNames { get; }

        void SetQueries(IEnumerable<SqlQuery> queries);

        IEnumerable<IQueryContext> ExecuteQueries();

        void ToLog();
    }
}
