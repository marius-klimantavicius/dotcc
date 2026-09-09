#include <stdio.h>
#define GLOBAL(type,value) value
static int storage = 42;
#define storage GLOBAL(int,storage)
#define ALIAS storage
#define PASS(x) x
static int increment(int value) { return value + 1; }
#define increment(x) increment(x)
static int cycle = 7;
#define cycle other
#define other cycle
static int target = 11, alias = 13;
int main(void) {
    printf("%d %d %d %d %d\n", storage, ALIAS, PASS(storage), increment(8), cycle);
#define alias target
#define target(x) x
    printf("%d ", alias(alias));
#undef target
#define target(x) alias
    printf("%d ", alias(0));
#undef alias
#define alias target(0)
    printf("%d\n", alias);
    return 0;
}
