#pragma once

#include <wtypes.h>
#include <string>
#include <iostream>
using namespace std;

typedef basic_string<_TCHAR> tstring;
//typedef basic_string<TCHAR, char_traits<TCHAR>, allocator<TCHAR>> tstring;

#if defined(UNICODE) || defined(_UNICODE)
#define tcout std::wcout
#else
#define tcout std::cout
#endif
