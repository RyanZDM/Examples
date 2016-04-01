// ////////////////////////////////////////////////////////////////////////////
// Carestream Health RESTRICTED INFORMATION - for internal use only
// ////////////////////////////////////////////////////////////////////////////
// 
// File:	NativeMethods.cs
// Author:	Ryan Zhang
// Date:	2016.03.29
// 
// Copyright 2016, Carestream Health, All Rights Reserved.
// 
// ////////////////////////////////////////////////////////////////////////////
using System;
using System.Runtime.InteropServices;

namespace ServiceTest
{
	public static class NativeMethods
	{
		#region Contants
		public const uint DELETE = 0x00010000;
		public const uint READ_CONTROL = 0x00020000;
		public const uint WRITE_DAC = 0x00040000;
		public const uint WRITE_OWNER = 0x00080000;
		public const uint SYNCHRONIZE = 0x00100000;

		public const uint STANDARD_RIGHTS_REQUIRED = 0x000F0000;

		public const uint STANDARD_RIGHTS_READ = READ_CONTROL;
		public const uint STANDARD_RIGHTS_WRITE = READ_CONTROL;
		public const uint STANDARD_RIGHTS_EXECUTE = READ_CONTROL;

		public const uint STANDARD_RIGHTS_ALL = 0x001F0000;

		public const uint SPECIFIC_RIGHTS_ALL = 0x0000FFFF;


		public const uint TOKEN_ASSIGN_PRIMARY = 0x0001;
		public const uint TOKEN_DUPLICATE = 0x0002;
		public const uint TOKEN_IMPERSONATE = 0x0004;
		public const uint TOKEN_QUERY = 0x0008;
		public const uint TOKEN_QUERY_SOURCE = 0x0010;
		public const uint TOKEN_ADJUST_PRIVILEGES = 0x0020;
		public const uint TOKEN_ADJUST_GROUPS = 0x0040;
		public const uint TOKEN_ADJUST_DEFAULT = 0x0080;
		public const uint TOKEN_ADJUST_SESSIONID = 0x0100;

		public const uint TOKEN_ALL_ACCESS_P = STANDARD_RIGHTS_REQUIRED |
											   TOKEN_ASSIGN_PRIMARY |
											   TOKEN_DUPLICATE |
											   TOKEN_IMPERSONATE |
											   TOKEN_QUERY |
											   TOKEN_QUERY_SOURCE |
											   TOKEN_ADJUST_PRIVILEGES |
											   TOKEN_ADJUST_GROUPS |
											   TOKEN_ADJUST_DEFAULT;

		public const uint TOKEN_ALL_ACCESS = (TOKEN_ALL_ACCESS_P | TOKEN_ADJUST_SESSIONID);
		public const uint TOKEN_READ = (STANDARD_RIGHTS_READ | TOKEN_QUERY);

		public const uint TOKEN_WRITE = STANDARD_RIGHTS_WRITE |
										TOKEN_ADJUST_PRIVILEGES |
										TOKEN_ADJUST_GROUPS |
										TOKEN_ADJUST_DEFAULT;

		public const uint TOKEN_EXECUTE = STANDARD_RIGHTS_EXECUTE;

		public const uint TOKEN_TRUST_CONSTRAINT_MASK = STANDARD_RIGHTS_READ |
														TOKEN_QUERY |
														TOKEN_QUERY_SOURCE;

		public const uint MAXIMUM_ALLOWED = 0x02000000;

		public const uint CREATE_NEW_CONSOLE = 0x00000010;
		public const uint NORMAL_PRIORITY_CLASS = 0x00000020;
		public const uint IDLE_PRIORITY_CLASS = 0x00000040;
		public const uint HIGH_PRIORITY_CLASS = 0x00000080;
		public const uint REALTIME_PRIORITY_CLASS = 0x00000100;
		public const uint BELOW_NORMAL_PRIORITY_CLASS = 0x00004000;
		public const uint ABOVE_NORMAL_PRIORITY_CLASS = 0x00008000;

		public const uint CREATE_BREAKAWAY_FROM_JOB = 0x01000000;
		public const uint CREATE_NEW_PROCESS_GROUP = 0x00000200;
		public const uint CREATE_UNICODE_ENVIRONMENT = 0x00000400;
		public const uint CREATE_SEPARATE_WOW_VDM = 0x00000800;
		public const uint CREATE_SHARED_WOW_VDM = 0x00001000;
		public const uint CREATE_FORCEDOS = 0x00002000;

		#endregion

		/// Return Type: BOOL->int
		///ProcessHandle: HANDLE->void*
		///DesiredAccess: DWORD->unsigned int
		///TokenHandle: PHANDLE->HANDLE*
		[DllImport("advapi32.dll", EntryPoint = "OpenProcessToken", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool OpenProcessToken([In] IntPtr processHandle, uint desiredAccess, out IntPtr tokenHandle);

		/// Return Type: BOOL->int
		///hToken: HANDLE->void*
		///lpApplicationName: LPCWSTR->WCHAR*
		///lpCommandLine: LPWSTR->WCHAR*
		///lpProcessAttributes: LPSECURITY_ATTRIBUTES->_SECURITY_ATTRIBUTES*
		///lpThreadAttributes: LPSECURITY_ATTRIBUTES->_SECURITY_ATTRIBUTES*
		///bInheritHandles: BOOL->int
		///dwCreationFlags: DWORD->unsigned int
		///lpEnvironment: LPVOID->void*
		///lpCurrentDirectory: LPCWSTR->WCHAR*
		///lpStartupInfo: LPSTARTUPINFOW->_STARTUPINFOW*
		///lpProcessInformation: LPPROCESS_INFORMATION->_PROCESS_INFORMATION*
		[DllImport("advapi32.dll", EntryPoint = "CreateProcessAsUserW", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool CreateProcessAsUserW([In] IntPtr hToken,
													   [In] [MarshalAs(UnmanagedType.LPWStr)] string lpApplicationName,
													   IntPtr lpCommandLine,
													   [In] IntPtr lpProcessAttributes,
													   [In] IntPtr lpThreadAttributes,
													   [MarshalAs(UnmanagedType.Bool)] bool bInheritHandles,
													   uint dwCreationFlags,
													   [In] IntPtr lpEnvironment,
													   [In] [MarshalAs(UnmanagedType.LPWStr)] string lpCurrentDirectory,
													   [In] ref STARTUPINFOW lpStartupInfo,
													   [Out] out PROCESS_INFORMATION lpProcessInformation);

		/// Return Type: BOOL->int
		///hObject: HANDLE->void*
		[DllImport("kernel32.dll", EntryPoint = "CloseHandle")]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool CloseHandle([In] IntPtr hObject);

		/// Return Type: BOOL->int
		///ExistingTokenHandle: HANDLE->void*
		///ImpersonationLevel: SECURITY_IMPERSONATION_LEVEL->_SECURITY_IMPERSONATION_LEVEL
		///DuplicateTokenHandle: PHANDLE->HANDLE*
		[DllImport("advapi32.dll", EntryPoint = "DuplicateToken")]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool DuplicateToken([In] IntPtr ExistingTokenHandle,
												 SECURITY_IMPERSONATION_LEVEL ImpersonationLevel,
												 out IntPtr DuplicateTokenHandle);

		[DllImport("userenv.dll", EntryPoint = "CreateEnvironmentBlock", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool CreateEnvironmentBlock([Out] out IntPtr lpEnvironment,
														 [In] IntPtr hToken,
														 [In] bool bInherit);

		/// Return Type: BOOL->int
		///hExistingToken: HANDLE->void*
		///dwDesiredAccess: DWORD->unsigned int
		///lpTokenAttributes: LPSECURITY_ATTRIBUTES->_SECURITY_ATTRIBUTES*
		///ImpersonationLevel: SECURITY_IMPERSONATION_LEVEL->_SECURITY_IMPERSONATION_LEVEL
		///TokenType: TOKEN_TYPE->_TOKEN_TYPE
		///phNewToken: PHANDLE->HANDLE*
		[DllImport("advapi32.dll", EntryPoint = "DuplicateTokenEx")]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool DuplicateTokenEx([In] IntPtr hExistingToken,
												   uint dwDesiredAccess, [In] IntPtr lpTokenAttributes,
												   SECURITY_IMPERSONATION_LEVEL ImpersonationLevel,
												   TOKEN_TYPE TokenType,
												   out IntPtr phNewToken);

		/// Return Type: DWORD->unsigned int
		[DllImport("kernel32.dll", EntryPoint = "WTSGetActiveConsoleSessionId")]
		public static extern uint WTSGetActiveConsoleSessionId();
	}
	
	[StructLayoutAttribute(LayoutKind.Sequential)]
	public struct SECURITY_ATTRIBUTES
	{

		/// DWORD->unsigned int
		public uint nLength;

		/// LPVOID->void*
		public IntPtr lpSecurityDescriptor;

		/// BOOL->int
		[MarshalAs(UnmanagedType.Bool)]
		public bool bInheritHandle;
	}

	[StructLayoutAttribute(LayoutKind.Sequential)]
	public struct STARTUPINFOW
	{

		/// DWORD->unsigned int
		public uint cb;

		/// LPWSTR->WCHAR*
		[MarshalAs(UnmanagedType.LPWStr)]
		public string lpReserved;

		/// LPWSTR->WCHAR*
		[MarshalAs(UnmanagedType.LPWStr)]
		public string lpDesktop;

		/// LPWSTR->WCHAR*
		[MarshalAs(UnmanagedType.LPWStr)]
		public string lpTitle;

		/// DWORD->unsigned int
		public uint dwX;

		/// DWORD->unsigned int
		public uint dwY;

		/// DWORD->unsigned int
		public uint dwXSize;

		/// DWORD->unsigned int
		public uint dwYSize;

		/// DWORD->unsigned int
		public uint dwXCountChars;

		/// DWORD->unsigned int
		public uint dwYCountChars;

		/// DWORD->unsigned int
		public uint dwFillAttribute;

		/// DWORD->unsigned int
		public uint dwFlags;

		/// WORD->unsigned short
		public ushort wShowWindow;

		/// WORD->unsigned short
		public ushort cbReserved2;

		/// LPBYTE->BYTE*
		public IntPtr lpReserved2;

		/// HANDLE->void*
		public IntPtr hStdInput;

		/// HANDLE->void*
		public IntPtr hStdOutput;

		/// HANDLE->void*
		public IntPtr hStdError;
	}

	[StructLayoutAttribute(LayoutKind.Sequential)]
	public struct PROCESS_INFORMATION
	{

		/// HANDLE->void*
		public IntPtr hProcess;

		/// HANDLE->void*
		public IntPtr hThread;

		/// DWORD->unsigned int
		public uint dwProcessId;

		/// DWORD->unsigned int
		public uint dwThreadId;
	}

	public enum SECURITY_IMPERSONATION_LEVEL
	{

		SecurityAnonymous,

		SecurityIdentification,

		SecurityImpersonation,

		SecurityDelegation,
	}

	public enum TOKEN_TYPE
	{

		/// TokenPrimary -> 1
		TokenPrimary = 1,

		TokenImpersonation,
	}
	
}