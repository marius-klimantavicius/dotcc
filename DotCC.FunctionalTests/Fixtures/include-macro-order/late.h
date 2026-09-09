#ifndef LATE_H
#define LATE_H
#include "early.h"
#define alloc(x) ((x) + 10)
#define CYCLE(x) ((x) + 3)
int during(void) { return CYCLE(4); }
#undef CYCLE
#define CYCLE(x) ((x) + 30)
#define PICK(x) x
static int token = 9;
int prior(void) { return PICK(token); }
#define token 20
#endif
