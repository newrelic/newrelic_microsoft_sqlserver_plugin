using System;
using System.Collections;
using System.Configuration.Install;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.AccessControl;
using System.Security.Principal;
using System.ServiceProcess;
using System.Text.RegularExpressions;

using NewRelic.Microsoft.SqlServer.Plugin.Core.Extensions;
using NewRelic.Microsoft.SqlServer.Plugin.Properties;

using log4net;

namespace NewRelic.Microsoft.SqlServer.Plugin.Core
{
	internal class InstallController
	{
		private readonly ILog _installLog;
		private readonly bool _isProcessElevated;
		private readonly string _serviceName;

		public InstallController(string serviceName, bool isProcessElevated)
		{
			_serviceName = serviceName;
			_isProcessElevated = isProcessElevated;
			_installLog = LogManager.GetLogger(Constants.InstallLogger);
		}

		public void Install()
		{
			Install(true);
		}

		public void Uninstall()
		{
			Install(false);
		}

		private void Install(bool install)
		{
			try
			{
				_installLog.Info(install ? "Installing" : "Uninstalling");
				using (var inst = new AssemblyInstaller(typeof (Program).Assembly, null))
				{
					IDictionary state = new Hashtable();
					inst.UseNewContext = true;

					try
					{
						EnsureEveryoneHasPermissionsToWriteToLogFiles();

						if (install)
						{
							inst.Install(state);
							inst.Commit(state);
						}
						else
						{
							inst.Uninstall(state);
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
							_installLog.Error("Error Rolling back");
							_installLog.Error(ex.Message);
						}
						throw;
					}
				}
			}
			catch (Exception ex)
			{
				ReportError(ex);
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
				_installLog.Info("Service not found");
				return;
			}

			_installLog.Info("Service found");

			try
			{
				EnsureEveryoneHasPermissionsToWriteToLogFiles();

				if (stop)
				{
					if (controller.Status.Equals(ServiceControllerStatus.Running) && controller.CanStop)
					{
						_installLog.Info("Service is running. Attempting to stop service");
						controller.Stop();
						_installLog.Info("Service successfully stopped");
					}
					else
					{
						_installLog.Info("Service is not running; skipping attempt to stop service");
					}
				}
				else
				{
					_installLog.Info("Attempting to start service");
					controller.Start();
					_installLog.Info("Service successfully started");
				}
			}
			catch (InvalidOperationException e)
			{
				ReportError(e);
			}
		}

		private ServiceController GetServiceController()
		{
			_installLog.InfoFormat("Checking if service {0} exists", _serviceName);
			return ServiceController.GetServices().FirstOrDefault(s => s.ServiceName == _serviceName);
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

		private void ReportError(Exception ex)
		{
			if (_isProcessElevated)
			{
				_installLog.Error(ex.Message);
			}
			else
			{
				const string message = "This must be run from an Administrator prompt. Please see the readme.md for details.";
				_installLog.Error(message);
			}
		}

		/// <summary>
		/// The install controller requires that the process is elevated (aka Run as Administrator).
		/// The log files are created by this administrative account.
		/// Any non-admin account will NOT have permissions to write to these log files.
		/// By adding full control to "Everyone", the non-admin service can write to these logs.
		/// </summary>
		private void EnsureEveryoneHasPermissionsToWriteToLogFiles()
		{
			_installLog.Debug("Resetting permissions on all log files");

			const string log4NetConfig = "log4net.config";
			var assemblyPath = Assembly.GetExecutingAssembly().GetLocalPath();
			var configPath = Path.Combine(assemblyPath, log4NetConfig);
			var log4netConfig = File.ReadAllText(configPath);

			var matches = Regex.Matches(log4netConfig, @"<param name=""File"" value=""(?<logPath>[^""]+)""\s*/>", RegexOptions.ExplicitCapture);
			foreach (var logPath in matches.Cast<Match>().Select(m => m.Groups["logPath"].Value))
			{
				if (!File.Exists(logPath))
				{
					_installLog.WarnFormat("Attempting to reset permissions for log file but it is missing at '{0}'", logPath);
					continue;
				}

				try
				{
					var fileSecurity = new FileSecurity(logPath, AccessControlSections.Access);
					var everyoneSid = new SecurityIdentifier(WellKnownSidType.WorldSid, null);
					fileSecurity.SetAccessRule(new FileSystemAccessRule(everyoneSid, FileSystemRights.FullControl, AccessControlType.Allow));
					File.SetAccessControl(logPath, fileSecurity);
					_installLog.DebugFormat("Reset permissions on '{0}'", logPath);
				}
				catch (Exception e)
				{
					_installLog.Error(string.Format("Failed to reset permissions on '{0}'", logPath), e);
				}
			}
		}
	}
}
