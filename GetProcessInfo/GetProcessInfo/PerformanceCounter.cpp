#include "stdafx.h"
#include "PerformanceCounter.h"


PerformanceCounter::PerformanceCounter(LPCTSTR szCounterName)
	: m_szCounterName(szCounterName), m_hcCounter(NULL)
{
}


PerformanceCounter::~PerformanceCounter()
{
}
