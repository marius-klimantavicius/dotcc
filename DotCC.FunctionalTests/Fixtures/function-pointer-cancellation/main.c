#include <stdio.h>
typedef int (*Callback)(int);
static int add(int value) { return value + 1; }
static int calls;
static Callback pick(void) { calls++; return add; }
int main(void) {
    Callback p = add;
    Callback copy = &*p;
    Callback empty = 0;
    Callback once = &*pick();
    Callback *slot = &p;
    Callback from_slot = &**slot;
    printf("%d %d %d %d %d\n", copy == add, (&*empty) == 0, copy(41), once == from_slot, calls);
    return 0;
}
