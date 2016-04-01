#include "stdafx.h"
#include "ProcessHelper.h"
#include <pdhmsg.h>
#include <DbgHelp.h>
#include <Psapi.h>
#include <TlHelp32.h>
#include <algorithm>
#include <assert.h>

#pragma comment(lib, "pdh.lib")
#pragma comment(lib, "DbgHelp.lib")

ProcessHelper::~ProcessHelper()
{
}

ProcessHelper::ProcessHelper(int dwProcessID)
	: m_dwProcessID(dwProcessID)
{
	TCHAR szBuffer[MAX_PATH];
	m_szProcessName = GetProcessNameById(dwProcessID, szBuffer, sizeof(szBuffer) / sizeof(szBuffer[0]));
}

ProcessHelper::ProcessHelper(LPCTSTR szProcessName)
	: m_szProcessName(szProcessName)
{
	m_dwProcessID = GetProcessIdByName(szProcessName, FALSE);
}

int ProcessHelper::GetPerformanceCounters(vector<PerformanceCounter*> &counters)
{
	counters.clear();
	counters.push_back(new PerformanceCounter(_T("\\Memory\\Committed Bytes")));
	counters.push_back(new PerformanceCounter(_T("\\Memory\\Available MBytes")));	
	counters.push_back(new PerformanceCounter(_T("\\System\\Processes")));
	counters.push_back(new PerformanceCounter(_T("\\System\\Threads")));
	counters.push_back(new PerformanceCounter(_T("\\Processor(*)\\% Processor Time")));
	//counters.push_back(new PerformanceCounter(_T("\\Process(Everything)\\% Processor Time")));
	counters.push_back(new PerformanceCounter(_T("\\PhysicalDisk(*)\\Current Disk Queue Length")));
	counters.push_back(new PerformanceCounter(_T("\\PhysicalDisk(*)\\% Disk Time")));
	return counters.size();
}

unsigned int ProcessHelper::GetInfo()
{
	// for directly adding counter using full counter name
	vector<PerformanceCounter*> counters;
	GetPerformanceCounters(counters);
	// End

	HQUERY hQuery = NULL;	
	__try
	{
		// for adding counter with help of PdhMakeCounterPath()
		PDH_COUNTER_PATH_ELEMENTS counterPE[] = {
			{ NULL, _T("Process"), NULL, NULL, -1, _T("% Processor Time") },
			{ NULL, _T("Process"), NULL, NULL, -1, _T("Thread Count") },
			{ NULL, _T("Process"), NULL, NULL, -1, _T("Private Bytes") },
			{ NULL, _T("Process"), NULL, NULL, -1, _T("Working Set - Private") },
			{ NULL, _T("Process"), NULL, NULL, -1, _T("Working Set") },
			{ NULL, _T("Process"), NULL, NULL, -1, _T("IO Read Operations/sec") },
			{ NULL, _T("Process"), NULL, NULL, -1, _T("IO Write Operations/sec") }
		};

		const int AdditionalCounterCount = sizeof(counterPE) / sizeof(counterPE[0]);
		HCOUNTER hCounters[AdditionalCounterCount];
		// End

		// 1. Open a performance counter query
		PDH_STATUS status = PdhOpenQuery(NULL, NULL, &hQuery);
		if (ERROR_SUCCESS != status) throw status;

		TCHAR szFullPath[MAX_PATH];

		// 2. Add performance counters
		// Add counters without calling PdhMakeCounterPath()
		for (vector<PerformanceCounter*>::const_iterator it = counters.begin(); it != counters.end(); it++)
		{
			PerformanceCounter *p = *it;
			if ((status = PdhAddCounter(hQuery, p->m_szCounterName.c_str(), NULL, &(p->m_hcCounter))) != ERROR_SUCCESS)
				throw status;
		}

		// Add counters with the help of PdhMakeCounterPath()				
		DWORD dwBufferSize;
		for (int i = 0; i < AdditionalCounterCount; i++)
		{
			// Change process name
			counterPE[i].szInstanceName = (LPTSTR)(m_szProcessName.c_str());

			dwBufferSize = sizeof(szFullPath);
			if ((status = PdhMakeCounterPath(&counterPE[i], szFullPath, &dwBufferSize, 0)) != ERROR_SUCCESS)
				throw status;

			if ((status = PdhAddCounter(hQuery, szFullPath, NULL, &hCounters[i])) != ERROR_SUCCESS)
				throw status;
		}

		// 3. Collect performance counter query data
		Sleep(100);
		PDH_FMT_COUNTERVALUE counterValue;
		if ((status = PdhCollectQueryData(hQuery)) != ERROR_SUCCESS) throw status;
		Sleep(100);
		if ((status = PdhCollectQueryData(hQuery)) != ERROR_SUCCESS) throw status;

		// 4. Get value of each performance counter
		for (vector<PerformanceCounter*>::const_iterator it = counters.begin(); it != counters.end(); it++)
		{
			PerformanceCounter *p = *it;
			if ((status = PdhGetFormattedCounterValue(p->m_hcCounter, PDH_FMT_LONG | PDH_FMT_NOCAP100, NULL, &counterValue)) == ERROR_SUCCESS)
			{
				tcout << p->m_szCounterName.c_str() << _T("-->") << counterValue.longValue << endl;
			}
			else
			{
				tcout << p->m_szCounterName.c_str() << _T("--> Failed to get data. Error Code:0x") << hex << status << dec << endl;
			}

			// 5. Remove previously added performance counters
			PdhRemoveCounter(p->m_hcCounter);
		}

		for (int i = 0; i < AdditionalCounterCount; i++)
		{
			if ((status = PdhGetFormattedCounterValue(hCounters[i], PDH_FMT_LONG | PDH_FMT_NOCAP100, NULL, &counterValue)) == ERROR_SUCCESS)
			{
				tcout << counterPE[i].szObjectName << _T("\\") << counterPE[i].szCounterName << _T("\\(") << counterPE[i].szInstanceName << _T(")-->") << counterValue.longValue << endl;
			}
			else
			{
				tcout << counterPE[i].szObjectName << _T("\\") << counterPE[i].szCounterName << _T("\\(") << counterPE[i].szInstanceName << _T(")-->");

				if ((PDH_INVALID_DATA == status) && (PDH_CSTATUS_NO_INSTANCE == counterValue.CStatus))
				{
					tcout << _T("The specified counter instance does not exist.") << endl;
				}
				else
				{
					tcout << _T("--> Failed to get data. Error Code:0x") << hex << status << endl;
				}
			}

			PdhRemoveCounter(hCounters[i]);
		}
	}
	__finally
	{
		if (hQuery)
		{
			// 6. Close the performance counter query
			PdhCloseQuery(hQuery);
		}

		for (vector<PerformanceCounter*>::iterator it = counters.begin(); it != counters.end(); it++)
		{
			PerformanceCounter *p = *it;
			if (p)
				delete p;
		}		
	}

	return 0;
}

BOOL ProcessHelper::Dump(LPCTSTR fullPath)
{
	assert(fullPath);

	HANDLE hProcess = NULL;
	HANDLE hFile = NULL;		
	
	__try
	{
		SetPrivilege(GetCurrentProcess(), SE_DEBUG_NAME);

		hProcess = OpenProcess(PROCESS_QUERY_INFORMATION | PROCESS_VM_READ | PROCESS_DUP_HANDLE, FALSE, m_dwProcessID);
		if (!hProcess) throw GetLastError();

		hFile = CreateFile(fullPath, GENERIC_WRITE, NULL, NULL, CREATE_ALWAYS, FILE_ATTRIBUTE_NORMAL, NULL);
		if (INVALID_HANDLE_VALUE == hFile) throw GetLastError();

		BOOL bRet = MiniDumpWriteDump(hProcess, m_dwProcessID, hFile, MiniDumpNormal, NULL, NULL, NULL);
		if (!bRet) throw GetLastError();

		return bRet;
	}
	__finally
	{
		if (hFile) CloseHandle(hFile);
		if (hProcess) CloseHandle(hProcess);
	}
}

DWORD ProcessHelper::GetProcessIdByName(LPCTSTR pszName, BOOL bMatchWholeWord)
{
	DWORD dwProcessIDs[1024];
	DWORD dwNeeded = 0;
	if (!EnumProcesses(dwProcessIDs, sizeof(dwProcessIDs), &dwNeeded)) throw GetLastError();

	BOOL bFound = false;
	TCHAR szBuffer[MAX_PATH];
	TCHAR szLowerName[MAX_PATH];
	DWORD dwProcessCount = dwNeeded / sizeof(DWORD);
	for (int i = 0; i < dwProcessCount; i++)
	{
		try
		{
			tstring szName(GetProcessNameById(dwProcessIDs[i], szBuffer, MAX_PATH));		

			if (bMatchWholeWord)
			{
				bFound = (_tcsicmp(szName.c_str(), pszName) == 0);
			}
			else
			{
				transform(szName.begin(), szName.end(), szName.begin(), ::_totlower);

				int pos = 0;
				while (pszName[pos]) szLowerName[pos++] = _totlower(pszName[pos]);
				szLowerName[pos] = _T('\0');

				bFound = (szName.find(szLowerName) != string::npos);
			}

			if (bFound)
				return dwProcessIDs[i];
		}
		catch (DWORD dwCode)
		{
			//tcout << _T("Error occurred when try to get the name of process [") << dwProcessIDs[i] << _T("], Error Code:0x") << hex << dwCode << dec << endl;
		}
	}

	return 0;
}

LPTSTR ProcessHelper::GetProcessNameById(DWORD dwProcessId, LPTSTR pBuffer, DWORD dwBufferLen)
{
	assert((pBuffer != NULL) && (dwBufferLen > 0));

	HANDLE hProcess = NULL;		
	
	if (0 == dwProcessId)
	{
		_stprintf_s(pBuffer, dwBufferLen, _T("System Idel Process"));
		return pBuffer;
	}

	DWORD dwSize = dwBufferLen;
	__try
	{
		if (!(hProcess = OpenProcess(PROCESS_QUERY_INFORMATION, FALSE, dwProcessId))) throw GetLastError();
				
		if (!QueryFullProcessImageName(hProcess, 0, pBuffer, &dwSize)) throw GetLastError();

		//HMODULE hModule;
		//DWORD dwNeeded = 0;
		//if (!EnumProcessModules(hProcess, &hModule, sizeof(hModule), &dwNeeded)) //throw GetLastError();
		//{
		//	DWORD a = GetLastError();
		//	throw a;
		//}

		//GetModuleBaseName(hProcess, hModule, szName, sizeof(szName) / sizeof(TCHAR));

		return pBuffer;
	}
	__finally
	{
		if (hProcess) CloseHandle(hProcess);
	}
}

BOOL ProcessHelper::SetPrivilege(HANDLE hProcess, LPCTSTR pszPrivilege)
{
	HANDLE hToken = NULL;
	LUID luid;
	__try
	{
		if (!OpenProcessToken(hProcess, TOKEN_ADJUST_PRIVILEGES | TOKEN_QUERY, &hToken)) throw GetLastError();
		
		TOKEN_PRIVILEGES tp;
		tp.PrivilegeCount = 1;
		tp.Privileges[0].Attributes = SE_PRIVILEGE_ENABLED;

		if (!LookupPrivilegeValue(NULL, pszPrivilege, &(tp.Privileges[0].Luid))) throw GetLastError();					

		if (!AdjustTokenPrivileges(hToken, FALSE, &tp, sizeof(TOKEN_PRIVILEGES), NULL, NULL)) throw GetLastError();
	}
	__finally
	{
		if (hToken) CloseHandle(hToken);
	}
}

DWORD TempGetProcessIdByName(LPCTSTR pszName)
{
	HANDLE hProcessSnap = NULL;

	__try
	{
		hProcessSnap = CreateToolhelp32Snapshot(TH32CS_SNAPPROCESS, 0);
		if (INVALID_HANDLE_VALUE == hProcessSnap) throw GetLastError();

		PROCESSENTRY32 pe;
		pe.dwSize = sizeof(PROCESSENTRY32);
		if (!Process32First(hProcessSnap, &pe)) throw GetLastError();

		do
		{
			//pe.
		} while (Process32Next(hProcessSnap, &pe));

		return 0;
	}
	__finally
	{
		if (hProcessSnap) CloseHandle(hProcessSnap);
	}
}
