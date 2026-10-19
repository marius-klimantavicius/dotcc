/* The public test header clears the builtin role before including Python.h. */
#undef Py_BUILD_CORE_BUILTIN
#undef Py_BUILD_CORE_MODULE
#include "api.h"
#ifdef Py_BUILD_CORE
#error public C API test must not inherit the interpreter-core role
#endif
int public_role(void) { return 11; }
