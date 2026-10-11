#include "program.h"
struct Dependent {
    int seed;
    Resource value = static_cast<Kind>(seed);
    Dependent(int input) : seed(input) {}
};
int unsupported() { Dependent value(1); return value.value.value; }
