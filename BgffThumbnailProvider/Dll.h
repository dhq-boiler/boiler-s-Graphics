#pragma once

#include <winnt.h>

HRESULT DllRegisterServer(void);
HRESULT DllUnregisterServer(void);

typedef void (*MyFunctionType)();