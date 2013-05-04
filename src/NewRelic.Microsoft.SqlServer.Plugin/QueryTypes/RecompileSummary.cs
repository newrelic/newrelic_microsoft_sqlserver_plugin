using NewRelic.Microsoft.SqlServer.Plugin.Core;

namespace NewRelic.Microsoft.SqlServer.Plugin.QueryTypes
{
    [Query("RecompileSummary.sql", "Custom/Recompiles/{DatabaseName}", QueryName = "Recompile Summary", Enabled = true)]
    public class RecompileSummary : RecompileQueryBase
    {
        public int SingleUseObjects { get; set; }
        public int MultipleUseObjects { get; set; }
        public decimal SingleUsePercent { get; set; }
    }
}
