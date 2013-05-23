using System;
using System.Linq;

using NewRelic.Microsoft.SqlServer.Plugin.Properties;

namespace NewRelic.Microsoft.SqlServer.Plugin.QueryTypes
{
	public abstract class DatabaseMetricBase : IDatabaseMetric
	{
		protected abstract WhereClauseTokenEnum WhereClauseToken { get; }
		protected abstract string DbNameForWhereClause { get; }
		public string DatabaseName { get; set; }

		public string ParameterizeQuery(string commandText, ISqlEndpoint endpoint)
		{
			var sqlServer = endpoint as SqlServer;
			if (sqlServer == null) return commandText;

			return ParameterizeQuery(commandText, WhereClauseToken, DbNameForWhereClause, sqlServer.IncludedDatabaseNames, sqlServer.ExcludedDatabaseNames);
		}

		internal static string ParameterizeQuery(string commandText, WhereClauseTokenEnum whereClauseToken, string dbNameForWhereClause, string[] includeDBs, string[] excludeDBs)
		{
			string[] dbNames;
			string format;
			if (includeDBs != null && includeDBs.Any())
			{
				dbNames = includeDBs.Select(d => string.Format("'{0}'", d)).ToArray();
				format = "{0} ({1} IN ({2}))";
			}
			else if (excludeDBs != null && excludeDBs.Any())
			{
				dbNames = excludeDBs.Select(d => string.Format("'{0}'", d)).ToArray();
				format = "{0} ({1} NOT IN ({2}))";
			}
			else
			{
				return commandText;
			}

			string whereToken;
			string where;
			switch (whereClauseToken)
			{
				case WhereClauseTokenEnum.Where:
					whereToken = Constants.WhereClauseReplaceToken;
					where = "WHERE";
					break;
				case WhereClauseTokenEnum.WhereAnd:
					whereToken = Constants.WhereClauseAndReplaceToken;
					where = "AND";
					break;
				case WhereClauseTokenEnum.Unknown:
				default:
					throw new ArgumentException(string.Format("WhereClauseToken '{0}' not recognized. ", whereClauseToken));
			}

			if (!commandText.Contains(whereToken))
			{
				throw new Exception(string.Format("SQL is not in the expected format. Missing replacement token '{0}'", whereClauseToken));
			}

			var replacement = string.Format(format, where, dbNameForWhereClause, string.Join(", ", dbNames));
			return commandText.Replace(whereToken, replacement);
		}

	}
}
