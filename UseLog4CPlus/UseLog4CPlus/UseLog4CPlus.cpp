// UseLog4CPlus.cpp : Defines the entry point for the console application.
//

// Note: Need to include the path to "log4cplus" in the "VC++ Directories->Include Directories" first

#include "stdafx.h"
#include "log4cplus/logger.h"
#include "log4cplus/loggingmacros.h"
#include "log4cplus/configurator.h"
#include "log4cplus/initializer.h"

#ifdef _DEBUG
	#pragma comment(lib, "log4cplus/log4cplusD.lib")
#else
	#pragma comment(lib, "log4cplus/log4cplus.lib")
#endif // _DEBUG


int main()
{
	log4cplus::Initializer initializer;
	log4cplus::PropertyConfigurator::doConfigure(_T("log4cplus.ini"));	
	/*log4cplus::BasicConfigurator config;
	config.configure();*/

	log4cplus::Logger logger = log4cplus::Logger::getInstance(LOG4CPLUS_TEXT("Main"));
	LOG4CPLUS_INFO_STR(logger, _T("this is the first string."));
	LOG4CPLUS_INFO_FMT(logger, _T("this is the second string %s."), _T("with format"));
	LOG4CPLUS_INFO(logger, _T("output the current thread:") << 2);
	return 0;
}

