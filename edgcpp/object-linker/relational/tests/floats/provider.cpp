#include "program.h"

float reciprocal(float value) { return 1.0f / value; }
double add_double(double value) { return value + 0.1; }
long double add_extended(long double value) { return value + 0x1p-63L; }
__float128 add_quad(__float128 value) { return value + 0x1p-112Q; }
