using System.ComponentModel;
using System.ServiceProcess;

using NewRelic.Microsoft.SqlServer.Plugin.Configuration;
using NewRelic.Microsoft.SqlServer.Plugin.Properties;

namespace NewRelic.Microsoft.SqlServer.Plugin.Core
{
	[RunInstaller(true)]
	public sealed class SqlMonitorServiceInstaller : ServiceInstaller
	{
		public SqlMonitorServiceInstaller()
		{
			Description = ServiceConstants.Description;
			DisplayName = ServiceConstants.DisplayName;
			StartType = ServiceConstants.StartType;

			var serviceName = Settings.Default != null ? Settings.Default.ServiceName : ServiceConstants.ServiceName;
			ServiceName = serviceName;
		}
	}
}
