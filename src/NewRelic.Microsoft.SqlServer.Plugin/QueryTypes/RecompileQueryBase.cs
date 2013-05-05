namespace NewRelic.Microsoft.SqlServer.Plugin.QueryTypes
{
    internal abstract class RecompileQueryBase : DatabaseMetricBase
    {
        protected override string DbNameForWhereClause
        {
            get { return "DB_NAME(st.dbid)"; }
        }

        protected override WhereClauseTokenEnum WhereClauseToken
        {
            get { return WhereClauseTokenEnum.WhereAnd; }
        }
    }
}
