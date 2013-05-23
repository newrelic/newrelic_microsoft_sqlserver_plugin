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
			StartType = ServiceConstants.StartType;
			Description = ServiceConstants.Description;

			// Use the user's overridden value, if it is available
			var serviceName = Settings.Default != null ? Settings.Default.ServiceName : ServiceConstants.ServiceName;
			ServiceName = serviceName;

			// If the service name was overriden by the user, use this new value as the display name to avoid conflict within the Windows Service database
			DisplayName = serviceName != ServiceConstants.ServiceName ? serviceName : ServiceConstants.DisplayName;
		}
	}
}
