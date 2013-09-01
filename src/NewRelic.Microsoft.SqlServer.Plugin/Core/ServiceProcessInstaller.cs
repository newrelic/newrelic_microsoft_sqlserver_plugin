using System.ComponentModel;
using System.ServiceProcess;

namespace NewRelic.Microsoft.SqlServer.Plugin.Core
{
	[RunInstaller(true)]
	public sealed class ServiceProcessInstaller : System.ServiceProcess.ServiceProcessInstaller
	{
		public ServiceProcessInstaller()
		{
			//Installs using Local System. This may need to be updated with the proper credentials after install.
			Account = ServiceAccount.LocalSystem;
		}
	}
}