// GetProcessInfo.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"
#include "ProcessHelper.h"

typedef unsigned long DWORD;

int _tmain(int argc, _TCHAR* argv[])
{
	if (argc < 2)
	{
		tcout << _T("Usage: GetProcessInfo.exe <Process Name>") << endl;
		return -1;
	}

	try
	{
		ProcessHelper helper(argv[1]);
		helper.GetInfo();

		helper.Dump(_T("F:\\Test.dmp"));
		tcout << _T("The dump file of process [") << helper.GetProcessName() << _T("] has been created at F:\\") << endl;
	}
	catch(DWORD dwError)
	{
		tcout << _T("Error occurred, error code=0x") << hex << dwError << endl;
	}

	return 0;
}

