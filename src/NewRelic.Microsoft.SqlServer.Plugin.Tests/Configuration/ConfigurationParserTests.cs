using System.IO;
using System.Linq;
using System.Reflection;
using NSubstitute;
using NUnit.Framework;
using NewRelic.Microsoft.SqlServer.Plugin.Core.Extensions;
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
            Assert.That(settings.UseSsl, Is.EqualTo(true), "UseSsl not mapped correctly");


            var expectedSqlInstances = new[]
                {
                    new
                        {
                            Name = "Local",
                            ConnectionString = "Server=.;Database=master;Trusted_Connection=True;",
                            IncludedDatabases = new string[0],
                            ExcludedDatabases = new string[0],
                        },
                    new
                        {
                            Name = "Important Server",
                            ConnectionString = "Server=192.168.10.123,1234;Database=master;User Id=foo;Password=bar;",
                            IncludedDatabases = new[]{"Northwind"},
                            ExcludedDatabases = Constants.SystemDatabases.Concat(new[]{"foo%", "bar%"}).ToArray(),
                        },
                }.Select(i => SqlServerToMonitor.FormatProperties(i.Name, i.ConnectionString, i.IncludedDatabases, i.ExcludedDatabases)).ToArray();

            var actualInstances = settings.SqlServers.Select(s => s.ToString()).ToArray();

            Assert.That(actualInstances, Is.EquivalentTo(expectedSqlInstances), "SqlServers Found different from expected");
        }
    }
}