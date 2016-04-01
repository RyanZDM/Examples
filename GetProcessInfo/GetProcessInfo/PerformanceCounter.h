#pragma once

#include "Common.h"
#include <Pdh.h>

struct PerformanceCounter
{
public:
	PerformanceCounter(LPCTSTR szCounterName = NULL);
	~PerformanceCounter();

	tstring m_szCounterName;
	HCOUNTER m_hcCounter;
};

