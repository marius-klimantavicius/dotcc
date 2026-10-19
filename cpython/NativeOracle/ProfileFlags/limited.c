#define Py_LIMITED_API 0x03050000
#undef Py_BUILD_CORE_BUILTIN
#undef Py_BUILD_CORE_MODULE
#include "api.h"
#ifdef Py_BUILD_CORE
#error limited C API test must not inherit the interpreter-core role
#endif
int limited_role(void) { return Py_LIMITED_API == 0x03050000 ? 22 : -1; }
