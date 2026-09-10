#include <stdlib.h>
typedef int (*DifferentAlias)(int);
int increment(int value) { return value + 1; }
DifferentAlias from_other(void) { return &increment; }
DifferentAlias runtime_other(void) { return abs; }
static int local(int value) { return value + 5; }
DifferentAlias local_other(void) { return local; }
