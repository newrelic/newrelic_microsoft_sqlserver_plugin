using System.Linq;
using NewRelic.Platform.Binding.DotNET;

namespace NewRelic.Microsoft.SqlServer.Plugin.Communication
{
    public class SqlRequest : Request
    {
        public SqlRequest(string licenseKey) : base(licenseKey)
        {
        }

        public void SendData()
        {
            if (!Data.Components.Any(c => c.Metrics.Any()))
            {
                return;
            }

            Send();
        }
    }
}