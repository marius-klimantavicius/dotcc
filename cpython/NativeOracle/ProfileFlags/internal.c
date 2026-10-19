#include "api.h"
#ifndef Py_BUILD_CORE
#error internal builtin must retain the builtin role implication
#endif
int internal_role(void) { return 33; }
