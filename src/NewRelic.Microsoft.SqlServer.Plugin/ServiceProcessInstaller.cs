using System.ComponentModel;
using System.ServiceProcess;

namespace NewRelic.Microsoft.SqlServer.Plugin
{
	[RunInstaller(true)]
	public sealed class ServiceProcessInstaller : System.ServiceProcess.ServiceProcessInstaller
	{
		public ServiceProcessInstaller()
		{
			//Installs using local Service, this will need to be updated with the proper credentials after install
			Account = ServiceAccount.LocalService;
		}
	}
}