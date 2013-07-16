using System.IO;
using System.Linq;
using System.Reflection;

using NSubstitute;

using NUnit.Framework;

using NewRelic.Microsoft.SqlServer.Plugin.Core.Extensions;
using NewRelic.Microsoft.SqlServer.Plugin.Properties;

using log4net;

namespace NewRelic.Microsoft.SqlServer.Plugin.Configuration
{
	[TestFixture]
	public class ConfigurationParserTests
	{
		[Test]
		public void Assert_external_file_can_be_loaded()
		{
			// Hate to do this, but the config manager will only load from disk
			string artifactDir = TestHelper.GetArtifactDir();
			string xml = Assembly.GetExecutingAssembly().SearchForStringResource("Configuration.app.config");
			string appConfigFile = Path.Combine(artifactDir, "ConfigurationTest.app.config");
			File.WriteAllText(appConfigFile, xml);

			Settings settings = ConfigurationParser.ParseSettings(Substitute.For<ILog>(), appConfigFile);
			Assert.That(settings, Is.Not.Null, "The settings from the config file should not be null");

			Assert.That(settings.LicenseKey, Is.EqualTo("FooGuid"), "LicenseKey not mapped correctly");
			Assert.That(settings.PollIntervalSeconds, Is.EqualTo(45), "PollIntervalSeconds not mapped correctly");
			Assert.That(settings.ServiceName, Is.EqualTo("NewRelicSqlPlugin"), "ServiceName not mapped correctly");

			string[] expectedSqlInstances = new[]
			                                {
				                                new
				                                {
					                                Name = "Local",
					                                ConnectionString = "Server=.;Database=master;Trusted_Connection=True;",
					                                IncludedDatabases = new[] {"Northwind", "tempdb", "master", "model", "msdb"},
					                                ExcludedDatabases = new string[0],
				                                },
				                                new
				                                {
					                                Name = "Important Server",
					                                ConnectionString = "Server=192.168.10.123,1234;Database=master;User Id=foo;Password=bar;",
					                                IncludedDatabases = new string[0],
					                                ExcludedDatabases = new[] {"foo", "bar"}.Concat(Constants.SystemDatabases).ToArray(),
				                                },
			                                }.Select(i => SqlServer.FormatProperties(i.Name, i.ConnectionString, i.IncludedDatabases, i.ExcludedDatabases)).ToArray();

			SqlServer[] sqlServers = settings.Endpoints.OfType<SqlServer>().ToArray();

			string[] actualInstances = sqlServers.Select(s => s.ToString()).ToArray();
			Assert.That(actualInstances, Is.EquivalentTo(expectedSqlInstances), "Endpoints Found different from expected");

			Database databaseWithDisplayName = sqlServers.Single(e => e.Name == "Local").IncludedDatabases.Single(d => d.Name == "Northwind");
			Assert.That(databaseWithDisplayName.DisplayName, Is.EqualTo("Southbreeze"), "Display name cannot be configured");

			string[] azureSqlDatabases = settings.Endpoints.OfType<AzureSqlDatabase>().Select(a => a.ToString()).ToArray();
			var expectedAzureDatabases = new[] {"Name: CloudFtw, ConnectionString: Server=zzz,1433;Database=CloudFtw;User ID=NewRelic;Password=aaa;Trusted_Connection=false;Encrypt=true;Connection Timeout=30;"};
			Assert.That(azureSqlDatabases, Is.EqualTo(expectedAzureDatabases));
		}
	}
}
