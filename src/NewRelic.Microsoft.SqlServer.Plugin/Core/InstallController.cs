using System;
using System.Collections;
using System.Configuration.Install;
using System.Linq;
using System.ServiceProcess;
using NewRelic.Microsoft.SqlServer.Plugin.Properties;
using log4net;

namespace NewRelic.Microsoft.SqlServer.Plugin.Core
{
	internal class InstallController
	{
	    private readonly ILog _log;

	    public InstallController(ILog log)
        {
            _log = log;
        }

	    public void Install()
		{
			Install(false);
		}

		public void Uninstall()
		{
			Install(true);
		}

		private void Install(bool uninstall)
		{
			try
			{
                _log.Info(uninstall ? "Uninstalling" : "Installing");
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
                            _log.Error("Error Rolling back");
							_log.Error(ex.Message);
						}
						throw;
					}
				}
			}
			catch (Exception ex)
			{
                _log.Error(ex.Message);
			}
		}

		public void StartService()
		{
			AdjustService(false);
		}

		public void StopService()
		{
			AdjustService(true);
		}

		private void AdjustService(bool stop)
		{
			var controller = GetServiceController();

			if (controller == null)
			{
				_log.Info("Service not found");
				return;
			}

            _log.Info("Service found");

			if (stop)
			{
				if (controller.Status.Equals(ServiceControllerStatus.Running)
				    && controller.CanStop)
				{
                    _log.Info("Service is running. Attempting to stop service");
					controller.Stop();
                    _log.Info("Service successfully stopped");
				}
				else
				{
                    _log.Info("Service is not running; skipping attempt to stop service");					
				}
			}
			else
			{
                _log.Info("Attempting to start service");
				controller.Start();
                _log.Info("Service successfully started");
			}
		}

		private ServiceController GetServiceController()
		{
            _log.InfoFormat("Checking if service {0} exists", ServiceConstants.ServiceName);
			return ServiceController.GetServices().FirstOrDefault(s => s.ServiceName == ServiceConstants.ServiceName);
		}

		public void InstallOrStart()
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
