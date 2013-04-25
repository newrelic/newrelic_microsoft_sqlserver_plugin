using System.ComponentModel;
using System.ServiceProcess;

namespace NewRelic.Microsoft.SqlServer.Plugin
{
	[RunInstaller(true)]
	public sealed class SqlMonitorServiceInstaller : ServiceInstaller
	{
		public SqlMonitorServiceInstaller()
		{
			Description = ServiceConstants.Description;
			DisplayName = ServiceConstants.DisplayName;
			ServiceName = ServiceConstants.ServiceName;
			StartType = ServiceConstants.StartType;
		}
	}
}