using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using NewRelic.Microsoft.SqlServer.Plugin.Core.Extensions;

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

			var settings = ConfigurationParser.ParseSettings(appConfigFile);
			Assert.That(settings, Is.Not.Null, "The settings from the config file should not be null");

			Assert.That(settings.LicenseKey, Is.EqualTo("FooGuid"), "LicenseKey not mapped correctly");
			Assert.That(settings.PollIntervalSeconds, Is.EqualTo(45), "PollIntervalSeconds not mapped correctly");
			Assert.That(settings.UseSsl, Is.EqualTo(true), "UseSsl not mapped correctly");

			var expectedSqlInstances = new[]
			                           {
				                           "Local -> Server=.;Database=master;Trusted_Connection=True;",
				                           "Important Server -> Server=192.168.10.123,1234;Database=master;User Id=foo;Password=bar;",
			                           };

			var actualInstances = settings.SqlServers.Select(s => string.Format("{0} -> {1}", s.Name, s.ConnectionString)).ToArray();
			Assert.That(actualInstances, Is.EquivalentTo(expectedSqlInstances));
		}
	}
}
