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
            var artifactDir = TestHelper.GetArtifactDir();
            var xml = Assembly.GetExecutingAssembly().SearchForStringResource("Configuration.app.config");
            var appConfigFile = Path.Combine(artifactDir, "ConfigurationTest.app.config");
            File.WriteAllText(appConfigFile, xml);

            var settings = ConfigurationParser.ParseSettings(Substitute.For<ILog>(), appConfigFile);
            Assert.That(settings, Is.Not.Null, "The settings from the config file should not be null");

            Assert.That(settings.LicenseKey, Is.EqualTo("FooGuid"), "LicenseKey not mapped correctly");
            Assert.That(settings.PollIntervalSeconds, Is.EqualTo(45), "PollIntervalSeconds not mapped correctly");

            var expectedSqlInstances = new[]
                {
                    new
                        {
                            Name = "Local",
                            ConnectionString = "Server=.;Database=master;Trusted_Connection=True;",
                            IncludedDatabases = new[]{"Northwind"},
                            ExcludedDatabases = new string[0],
                        },
                    new
                        {
                            Name = "Important Server",
                            ConnectionString = "Server=192.168.10.123,1234;Database=master;User Id=foo;Password=bar;",
                            IncludedDatabases = new string[0],
                            ExcludedDatabases = Constants.SystemDatabases.Concat(new[]{"foo", "bar"}).ToArray(),
                        },
                }.Select(i => SqlEndpoint.FormatProperties(i.Name, i.ConnectionString, i.IncludedDatabases, i.ExcludedDatabases)).ToArray();

            var actualInstances = settings.Endpoints.Select(s => s.ToString()).ToArray();

            Assert.That(actualInstances, Is.EquivalentTo(expectedSqlInstances), "Endpoints Found different from expected");

	        var databaseWithDisplayName = settings.Endpoints.Single(e => e.Name == "Local").IncludedDatabases.Single(d => d.Name == "Northwind");
	        Assert.That(databaseWithDisplayName.DisplayName, Is.EqualTo("Southbreeze"), "Display name cannot be configured");
        }
    }
}