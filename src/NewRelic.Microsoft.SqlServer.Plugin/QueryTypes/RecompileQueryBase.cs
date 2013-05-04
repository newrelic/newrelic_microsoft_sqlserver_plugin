using System;
using System.Linq;

namespace NewRelic.Microsoft.SqlServer.Plugin.QueryTypes
{
    public abstract class RecompileQueryBase : IDatabaseMetric
    {
        public string DatabaseName { get; set; }

        public string ParameterizeQuery(string commandText, string[] includeDBs, string[] excludeDBs)
        {
            if (!commandText.Contains(Constants.WhereClauseAndReplaceToken))
            {
                throw new Exception(string.Format("SQL is not in the expected format. Missing replacement token '{0}'", Constants.WhereClauseAndReplaceToken));
            }

            if (includeDBs != null && includeDBs.Any())
            {
                var dbNames = includeDBs.Select(d => string.Format("'{0}'", d)).ToArray();
                var replacement = string.Format("AND (DB_NAME(st.dbid) IN ({0}))", string.Join(", ", dbNames));
                return commandText.Replace(Constants.WhereClauseAndReplaceToken, replacement);
            }

            if (excludeDBs != null && excludeDBs.Any())
            {
                var dbNames = excludeDBs.Select(d => string.Format("'{0}'", d)).ToArray();
                var replacement = string.Format("AND (DB_NAME(st.dbid) NOT IN ({0}))", string.Join(", ", dbNames));
                return commandText.Replace(Constants.WhereClauseAndReplaceToken, replacement);
            }

            return commandText;
        }
    }
}