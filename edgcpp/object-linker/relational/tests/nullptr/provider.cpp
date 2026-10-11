#include "program.h"
Null make_null() { return nullptr; }
int accept_null(Null value) { return value == nullptr ? 3 : 8; }
int *as_pointer(Null value) { return value; }
int select(int *value) { return value ? *value : 4; }
int select(Null value) { return accept_null(value) + 2; }
Null with_effect(int *counter) { ++*counter; return nullptr; }
