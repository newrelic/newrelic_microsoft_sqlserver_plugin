using System.ServiceProcess;

namespace NewRelic.Microsoft.SqlServer.Plugin.Properties
{
	public static class ServiceConstants
	{
		public const string Description = "Gathers SQL Server metrics and reports them to New Relic";
		public const string DisplayName = "New Relic SQL Server Plugin";
		public const string ServiceName = "NewRelicSQLServerPlugin";
		public const ServiceStartMode StartType = ServiceStartMode.Automatic;
	}
}