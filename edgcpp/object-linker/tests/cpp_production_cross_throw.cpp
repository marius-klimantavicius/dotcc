#include "cpp_production_cross.h"
extern "C" void production_cross_throw(int value) { throw ProductionShared(value); }
