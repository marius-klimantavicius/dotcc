#include <stdio.h>
static int twice(int x) { return x * 2; }
static int increment(int x) { return x + 1; }
static int nine(void) { return 9; }
static int (*const builtins[])(int) = {twice, increment};
int (*mutable[3])(int) = {increment};
static int (*slots[2])(int);
int (*const noargs[])() = {nine};
int main(void) {
    slots[0] = builtins[0];
    mutable[1] = builtins[1];
    printf("%d %d %d %d %d %d %d %d\n", slots[0](21), mutable[1](16),
        mutable[2] == 0, slots[1] == 0, noargs[0](),
        (int)(sizeof(builtins) / sizeof(builtins[0])), 0 == slots[1], 0 != builtins[0]);
    return 0;
}
