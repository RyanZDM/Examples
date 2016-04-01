

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace LogoffUser
{
	public static class NativeMethods
	{
		/// <summary>
		/// Log off all user session if its state is WTSActive
		/// </summary>
		/// <returns></returns>
		public static int LogoffSession()
		{
			var loggedOff = 0;
			var serverPtr = WTSOpenServer(Environment.MachineName);
			var sessionIds = GetSessionIds();
			sessionIds.Where(s => s.State == WTS_CONNECTSTATE_CLASS.WTSActive).ToList()
						.ForEach(s =>
						{
							if (WTSLogoffSession(serverPtr, s.SessionID, true))
							{
								loggedOff++;
							}
						});

			return loggedOff;
		}

		/// <summary>
		/// Get all sessions between ID 1 and 65534
		/// 0 session:	Is reserved for Windows service
		/// >= 65535:	For some special usage such as RDP-Tcp, the state is WTSListen	
		/// </summary>
		/// <returns></returns>
		private static List<WTS_SESSION_INFO> GetSessionIds()
		{
			var sessionIds = new List<WTS_SESSION_INFO>();
			var buffer = IntPtr.Zero;
			var count = 0;
			var type = typeof(WTS_SESSION_INFO);

			var serverPtr = WTSOpenServer(Environment.MachineName);
			var ret = WTSEnumerateSessions(serverPtr, 0, 1, ref buffer, ref count);
			var dataSize = Marshal.SizeOf(type);
			var current = (Int64) buffer;
			if (ret != 0)
			{
				for (var i = 0; i < count; i++)
				{
					var si = (WTS_SESSION_INFO)Marshal.PtrToStructure((IntPtr)current, type);
					if (si.SessionID > 1 && si.SessionID < 65535)
					{
						sessionIds.Add(si);
					}

					current += dataSize;
				}

				WTSFreeMemory(buffer);
			}

			return sessionIds;
		}

		#region AIP define
		[StructLayout(LayoutKind.Sequential)]
		private struct WTS_SESSION_INFO
		{
			public Int32 SessionID;
			[MarshalAs(UnmanagedType.LPStr)]
			public String pWinStationName;
			public WTS_CONNECTSTATE_CLASS State;
		}

		private enum WTS_CONNECTSTATE_CLASS
		{
			WTSActive,
			WTSConnected,
			WTSConnectQuery,
			WTSShadow,
			WTSDisconnected,
			WTSIdle,
			WTSListen,
			WTSReset,
			WTSDown,
			WTSInit
		}

		private enum WTS_INFO_CLASS
		{
			WTSInitialProgram,
			WTSApplicationName,
			WTSWorkingDirectory,
			WTSOEMId,
			WTSSessionId,
			WTSUserName,
			WTSWinStationName,
			WTSDomainName,
			WTSConnectState,
			WTSClientBuildNumber,
			WTSClientName,
			WTSClientDirectory,
			WTSClientProductId,
			WTSClientHardwareId,
			WTSClientAddress,
			WTSClientDisplay,
			WTSClientProtocolType,
			WTSIdleTime,
			WTSLogonTime,
			WTSIncomingBytes,
			WTSOutgoingBytes,
			WTSIncomingFrames,
			WTSOutgoingFrames,
			WTSClientInfo,
			WTSSessionInfo
		}


		[DllImport("wtsapi32.dll", SetLastError = true)]
		static extern bool WTSLogoffSession(IntPtr hServer, int SessionId, bool bWait);

		[DllImport("Wtsapi32.dll")]
		static extern bool WTSQuerySessionInformation(
			System.IntPtr hServer, int sessionId, WTS_INFO_CLASS wtsInfoClass, out System.IntPtr ppBuffer, out uint pBytesReturned);

		[DllImport("wtsapi32.dll", SetLastError = true)]
		static extern IntPtr WTSOpenServer([MarshalAs(UnmanagedType.LPStr)] String pServerName);

		[DllImport("wtsapi32.dll")]
		static extern void WTSCloseServer(IntPtr hServer);

		[DllImport("wtsapi32.dll", SetLastError = true)]
		static extern Int32 WTSEnumerateSessions(IntPtr hServer, [MarshalAs(UnmanagedType.U4)] Int32 Reserved, [MarshalAs(UnmanagedType.U4)] Int32 Version, ref IntPtr ppSessionInfo, [MarshalAs(UnmanagedType.U4)] ref Int32 pCount);

		[DllImport("wtsapi32.dll")]
		static extern void WTSFreeMemory(IntPtr pMemory);
		#endregion
	}
}
