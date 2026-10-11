#include "program.h"
int calls = 0;
int mark(int value) { ++calls; if (value < 0) throw 17; return value; }
Pair make_pair(int value) { return Pair{mark(value), mark(value + 1)}; }
View make_view(int *data, int size) { return View{data, size}; }
