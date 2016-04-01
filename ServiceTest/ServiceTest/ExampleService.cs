using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Topshelf.Logging;

// TODO: Define or un-define "_USE_NLOG_" in project settings for using NLog or Log4Net
#if _USE_NLOG_
using NLog;
using LogManager = NLog.LogManager;

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
				//LaunchProgramInSession(2, @"C:\Windows\System32\notepad.exe");
				LaunchProgramInLoggedOnSession(@"C:\Windows\System32\notepad.exe");
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
				const string program = @"C:\Windows\System32\notepad.exe";
				try
				{
					LaunchProgramInSession(args.SessionId, program);
				}
				catch (Exception ex)
				{
					logger.Error("Failed to launch program '{0}' in seesion {1}, error='{2}'", program, args.SessionId, ex);
				}
			}
		}

		/// <summary>
		/// Launch program in current active interactive session
		/// </summary>
		/// <param name="program">The program to be launched</param>
		public static void LaunchProgramInLoggedOnSession(string program)
		{
			LaunchProgramInSession((int)NativeMethods.WTSGetActiveConsoleSessionId(), program);
		}

		/// <summary>
		/// Launch the program in specified session
		/// </summary>
		/// <param name="sessionId">The ID of session the program to be launched</param>
		/// <param name="program">The program to be launched</param>
		public static void LaunchProgramInSession(int sessionId, string program)
		{
			var processes = Process.GetProcessesByName("explorer");
			if (processes.Length < 1)
				processes = Process.GetProcesses();		// Get all processes no matter what session they belong to

			var targetProcess = processes.FirstOrDefault(p => p.SessionId == sessionId);
			if (targetProcess == null)
				throw new Exception(string.Format("Not found process in session {0}", sessionId));
			
			LaunchProgramAsUser(targetProcess, program);
		}

		/// <summary>
		/// Launch the program as the same user of target process in the same session target process belong to
		/// </summary>
		/// <param name="targetProcess">The process's user and session will be used for launching program</param>
		/// <param name="program">Program to be launched</param>
		public static void LaunchProgramAsUser(Process targetProcess, string program)
		{
			var token = IntPtr.Zero;
			try
			{
				#region Get logged on user's primary token
				if (!NativeMethods.OpenProcessToken(targetProcess.Handle,
													NativeMethods.TOKEN_READ | NativeMethods.TOKEN_ASSIGN_PRIMARY,
													out token))
					throw new Exception(string.Format("OpenProcessToken failed."));
				#endregion

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

				#region Get environment settings of logged on user
				IntPtr environmentBlock;
				if (!NativeMethods.CreateEnvironmentBlock(out environmentBlock, token, false))
					throw new Exception(string.Format("CreateEnvironmentBlock failed with error code:{0}", Marshal.GetLastWin32Error()));

				/*
				// Convert to string
				var offset = 0;
				var userEnvironments = new Dictionary<string, string>();
				while (true)
				{
					var ptr = new IntPtr(environmentBlock.ToInt64() + offset);
					var envVar = Marshal.PtrToStringUni(ptr);
					offset += Encoding.Unicode.GetByteCount(envVar) + 2;
					if (string.IsNullOrEmpty(envVar))
						break;

					var valuePair = envVar.Split(new string[] {"="}, StringSplitOptions.RemoveEmptyEntries);
					if (valuePair.Length > 1 && !string.IsNullOrWhiteSpace(valuePair[0]))
					{
						userEnvironments[valuePair[0]] = valuePair[1];
					}
				}*/
				#endregion

				#region Launch the program as logged on user in target session
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
																 NativeMethods.NORMAL_PRIORITY_CLASS | NativeMethods.CREATE_UNICODE_ENVIRONMENT,
																 environmentBlock, // */IntPtr.Zero, // Null if not need environment of logged on user
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
					throw new Exception(string.Format("CreateProcessAsUserW failed with error code:{0}", Marshal.GetLastWin32Error()));
				}
				#endregion
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
