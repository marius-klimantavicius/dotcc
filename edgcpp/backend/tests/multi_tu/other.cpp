#include "shared.h"

static int private_value = 14;
int initialize() { return 9; }
int initialized = initialize();
int* address_other() { return &counter(); }
int call_other() { return twice(counter()) + private_value; }
void throw_other() { throw Derived(11); }
