namespace NewRelic.Microsoft.SqlServer.Plugin.Configuration
{
	public class SqlServerToMonitor
	{
		public SqlServerToMonitor(string name, string connectionString)
		{
			Name = name;
			ConnectionString = connectionString;
		}

		public string Name { get; private set; }
		public string ConnectionString { get; private set; }
	}
}
