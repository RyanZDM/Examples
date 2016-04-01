// XRSDaemon.cpp : Implementation of WinMain


#include "stdafx.h"
#include "resource.h"
#include "XRSDaemon_i.h"


using namespace ATL;

#include <stdio.h>

#include "..\GetProcessInfo\ProcessHelper.h"

class CXRSDaemonModule : public ATL::CAtlServiceModuleT< CXRSDaemonModule, IDS_SERVICENAME >
{
public :
	DECLARE_LIBID(LIBID_XRSDaemonLib)
	DECLARE_REGISTRY_APPID_RESOURCEID(IDR_XRSDAEMON, "{29EDB8B9-3D90-471B-9606-5CD94C63476C}")
		HRESULT InitializeSecurity() throw()
	{
		// TODO : Call CoInitializeSecurity and provide the appropriate security settings for your service
		// Suggested - PKT Level Authentication, 
		// Impersonation Level of RPC_C_IMP_LEVEL_IDENTIFY 
		// and an appropriate Non NULL Security Descriptor.

		return S_OK;
	}
		HRESULT PreMessageLoop(int nShowCmd);
		void RegisterHotKey();
		HRESULT PostMessageLoop();
};

CXRSDaemonModule _AtlModule;



//
extern "C" int WINAPI _tWinMain(HINSTANCE /*hInstance*/, HINSTANCE /*hPrevInstance*/, 
								LPTSTR /*lpCmdLine*/, int nShowCmd)
{
	return _AtlModule.WinMain(nShowCmd);
}



HRESULT CXRSDaemonModule::PreMessageLoop(int nShowCmd)
{	
	RegisterHotKey();

	return CAtlServiceModuleT::PreMessageLoop(nShowCmd);
}


void CXRSDaemonModule::RegisterHotKey()
{
	BOOL ret = ::RegisterHotKey(NULL, 1, MOD_CONTROL | MOD_ALT | MOD_NOREPEAT, VK_F12);
}


HRESULT CXRSDaemonModule::PostMessageLoop()
{
	// TODO: Add your specialized code here and/or call the base class
	BOOL ret = ::UnregisterHotKey(NULL, 1);

	return CAtlServiceModuleT::PostMessageLoop();
}
