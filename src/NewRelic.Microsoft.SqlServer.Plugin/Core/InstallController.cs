using System;
using System.Collections;
using System.Configuration.Install;
using System.Linq;
using System.ServiceProcess;
using NewRelic.Microsoft.SqlServer.Plugin.Properties;

namespace NewRelic.Microsoft.SqlServer.Plugin.Core
{
	internal static class InstallController
	{
		public static void Install()
		{
			Install(false);
		}

		public static void Uninstall()
		{
			Install(true);
		}

		private static void Install(bool uninstall)
		{
			try
			{
				Console.WriteLine(uninstall ? "Uninstalling" : "Installing");
				using (var inst = new AssemblyInstaller(typeof (Program).Assembly, null))
				{
					IDictionary state = new Hashtable();
					inst.UseNewContext = true;

					try
					{
						if (uninstall)
						{
							inst.Uninstall(state);
						}
						else
						{
							inst.Install(state);
							inst.Commit(state);
						}
					}
					catch
					{
						try
						{
							inst.Rollback(state);
						}
						catch (Exception ex)
						{
							Console.Error.WriteLine("Error Rolling back");
							Console.Error.WriteLine(ex.Message);
						}
						throw;
					}
				}
			}
			catch (Exception ex)
			{
				Console.Error.WriteLine(ex.Message);
			}
		}

		public static void StartService()
		{
			AdjustService(false);
		}

		public static void StopService()
		{
			AdjustService(true);
		}

		private static void AdjustService(bool stop)
		{
			var controller = GetServiceController();

			if (controller == null)
			{
				Console.Out.WriteLine("Service not found");
				return;
			}

			Console.Out.WriteLine("Service found");

			if (stop)
			{
				if (controller.Status.Equals(ServiceControllerStatus.Running)
				    && controller.CanStop)
				{
					Console.Out.WriteLine("Service is running. Attempting to stop service");
					controller.Stop();
					Console.Out.WriteLine("Service successfully stopped");
				}
				else
				{
					Console.Out.WriteLine("Service is not running; skipping attempt to stop service");					
				}
			}
			else
			{
				Console.Out.WriteLine("Attempting to start service");
				controller.Start();
				Console.Out.WriteLine("Service successfully started");
			}
		}

		private static ServiceController GetServiceController()
		{
			Console.Out.WriteLine("Checking if service {0} exists", ServiceConstants.ServiceName);
			return ServiceController.GetServices().FirstOrDefault(s => s.ServiceName == ServiceConstants.ServiceName);
		}

		public static void InstallOrStart()
		{
			var serviceController = GetServiceController();
			if (serviceController == null)
			{
				Install();
			}
			else
			{
				StartService();
			}
		}
	}
}
