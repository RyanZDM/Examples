#define _USE_NLOG_

using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Topshelf.Logging;
#if _USE_NLOG_
using NLog;
#else
using log4net;
#endif

using Topshelf;

namespace ServiceTest
{
	public class ExampleService
	{
		#region Private variables
#if _USE_NLOG_
		//private readonly Logger logger = LogManager.GetCurrentClassLogger();
		private readonly Logger logger;
#else
		private readonly ILog logger;
#endif
		#endregion

		public ExampleService()
		{
			logger = LogManager.GetLogger(GetType().FullName);
#if (!_USE_NLOG_)
			log4net.Config.XmlConfigurator.Configure();
#endif
		}

		public bool Start()
		{
			// TODO runtime change log file path
			//var target = (FileTarget)LogManager.Configuration.FindTargetByName("logfile");
			//target.FileName = "<path to log file>";

			logger.Info("Service started.");

			var log = HostLogger.Current.Get(GetType().FullName);
			log.Info("Service started. - Another way to write log. ");

			// For test only
			Task.Run(() =>
			{
				Thread.Sleep(5000);
				LaunchProgramInSession(2, @"C:\Windows\System32\notepad.exe");
			});
			
			//LaunchProgramInSession(2, @"C:\Windows\System32\notepad.exe");

			return true;
		}

		public bool Stop()
		{
			logger.Info("Service stopped.");
			return true;
		}

		public bool Shutdown()
		{
			logger.Info("Computer shutdown.");
			return true;
		}

		/// <summary>
		/// Session change event sequence:
		/// - Windows Start (Logon):
		///		ConsoleConnect -> SessionLogon
		/// - Windows Logoff/Shutdown/Reboot:
		///		SessionLogoff -> ConsoleDisconnect
		/// - Lock Windows:
		///		SessionLock
		/// - Unlock Windows:
		///		SessionUnlock
		/// </summary>
		/// <param name="args"></param>
		public void OnSessionChanged(SessionChangedArguments args)
		{
#if _USE_NLOG_
			logger.Info("Session changed. Code:[{0}], ID:[{1}].", args.ReasonCode, args.SessionId);
#else
			// logger.Info accepts two arguments only
			logger.InfoFormat("Session changed. Code:[{0}], ID:[{1}].", args.ReasonCode, args.SessionId);
#endif
			// Launch the specified program once user logged on Windows
			if (args.ReasonCode == SessionChangeReasonCode.SessionLogon)
			{
				LaunchProgramInSession(args.SessionId, @"C:\Windows\System32\notepad.exe");
			}
		}

		public void LaunchProgramInSession(int sessionId, string program)
		{
			var processes = Process.GetProcessesByName("explorer");
			var explorer = processes.FirstOrDefault(p => p.SessionId == sessionId);
			if (explorer == null)
			{
				logger.Warn("Not found the explorer.exe process in session {0}", sessionId);
				return;
			}
			
			var token = IntPtr.Zero;
			try
			{
				if (!NativeMethods.OpenProcessToken(processes[0].Handle,
													NativeMethods.TOKEN_READ | NativeMethods.TOKEN_ASSIGN_PRIMARY,
													out token))
				{
					logger.Warn("OpenProcessToken failed.");
					return;
				}

				#region codes for calling API DuplicateTokenEx

				//var duplicatedToken = IntPtr.Zero;
				//if (!NativeMethods.DuplicateTokenEx(token,
				//									NativeMethods.MAXIMUM_ALLOWED,//NativeMethods.TOKEN_ALL_ACCESS,// NativeMethods.TOKEN_READ | NativeMethods.TOKEN_EXECUTE | NativeMethods.TOKEN_DUPLICATE | NativeMethods.TOKEN_IMPERSONATE,
				//									IntPtr.Zero,
				//									SECURITY_IMPERSONATION_LEVEL.SecurityImpersonation,
				//									TOKEN_TYPE.TokenPrimary,
				//									out duplicatedToken))
				//{
				//	// TODO log a warning message
				//	logger.Warn("DuplicateTokenEx failed.");
				//	NativeMethods.CloseHandle(token);
				//	return;
				//}

				#endregion

				var si = new STARTUPINFOW();
				si.cb = (uint) Marshal.SizeOf(si);
				si.lpDesktop = @"winsta0\default";

				PROCESS_INFORMATION pi;
				var created = NativeMethods.CreateProcessAsUserW(token,
																 program,
																 IntPtr.Zero,
																 IntPtr.Zero,
																 IntPtr.Zero,
																 false,
																 NativeMethods.NORMAL_PRIORITY_CLASS,
																 IntPtr.Zero, // TODO if need to create environment of target logged on user
																 null,
																 ref si,
																 out pi);
				if (created && pi.hProcess != IntPtr.Zero)
				{
					NativeMethods.CloseHandle(pi.hProcess);
					NativeMethods.CloseHandle(pi.hThread);
				}
				else
				{
					var errorCode = Marshal.GetLastWin32Error();
					logger.Warn("CreateProcessAsUserW failed with error code:{0}", errorCode);
				}
			}
			finally
			{
				if (token != IntPtr.Zero)
				{
					NativeMethods.CloseHandle(token);
				}
			}
		}
	}
}
