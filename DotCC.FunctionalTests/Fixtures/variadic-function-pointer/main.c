#include "api.h"
#include <stdio.h>
struct Holder { Callback callback; };
static struct Holder holder = { consume };
static Callback table[2] = { consume, consume };
static int argument_calls;
static int callee_calls;
static unsigned short once(void) { argument_calls++; return 65530; }
static Callback choose(void) { callee_calls++; return holder.callback; }
static int increment(int x) { return x + 2; }
int main(void) {
    short negative = -5;
    float real = 1.5f;
    int value = 10;
    int empty = table[0](0);
    int mixed = choose()(1, once(), negative, real, &value, increment, 4294967313UL);
    printf("%d %d %d %d\n", empty, mixed, argument_calls, callee_calls);
    return table[0] != table[1];
}
