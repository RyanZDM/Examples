#pragma once
#include <vector>
#include <pdh.h>
#include "Common.h"
#include "PerformanceCounter.h"

class ProcessHelper
{
public:
	virtual ~ProcessHelper();
	ProcessHelper(int nProcessID);
	ProcessHelper(LPCTSTR szProcessName);
	unsigned int GetInfo();
	BOOL Dump(LPCTSTR fullPath);

	//void SetDumpFileName(LPCTSTR pszFileName) { m_szDumpFileName = pszFileName; }
	//LPCTSTR GetDumpFileName() { return m_szDumpFileName.c_str(); }
	DWORD GetProcessId() { return m_dwProcessID; }
	LPCTSTR GetProcessName() { return m_szProcessName.c_str(); }

private:
	int GetPerformanceCounters(vector<PerformanceCounter*> &counters);
	DWORD GetProcessIdByName(LPCTSTR pszName, BOOL bMatchWholeWord = TRUE);
	LPTSTR GetProcessNameById(DWORD dwProcessId, LPTSTR pBuffer, DWORD dwBufferLen);
	BOOL SetPrivilege(HANDLE hProcess, LPCTSTR pszPrivilege);

private:	
	DWORD m_dwProcessID = 0;		// The process ID
	tstring m_szProcessName;		// The process name
	//tstring m_szDumpFileName;
};

