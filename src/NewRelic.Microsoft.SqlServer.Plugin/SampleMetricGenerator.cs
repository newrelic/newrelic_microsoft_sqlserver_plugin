using System;
using System.Threading;
using NewRelic.Microsoft.SqlServer.Plugin.Communication;
using NewRelic.Platform.Binding.DotNET;

namespace NewRelic.Microsoft.SqlServer.Plugin
{
	public static class SampleMetricGenerator
	{
		//Sends Metric of Random int values to Dashboard
		public static void SendSimpleMetricData()
		{
			Console.Out.WriteLine("Sending sample metric data...");
			for (var i = 0; i < 100; i++)
			{
				SendDummyData();
				Thread.Sleep(1000);
			}
		}

		private static void SendDummyData()
		{
			var request = new SqlRequest(Constants.JEM_LicenseKey)
			              {
				              Data = new PlatformData(new AgentData
				                                      {
					                                      Host = "TestMachine",
					                                      Pid = 1,
					                                      Version = "1.0.0"
				                                      })
			              };

			var componentData = new ComponentData("TestComponent", Constants.ComponentGuid, 1);

			var rnd = new Random(DateTime.Now.Millisecond);
			var rando = rnd.Next(0, 3000);

			const string metricName = "Metric1";
			componentData.AddMetric(metricName, rando);
			request.Data.AddComponent(componentData);


			request.SendData();

			Console.Out.WriteLine("Metric ['{0}',{1}] sent to Dashboard", metricName, rando.ToString());
		}
	}
}
