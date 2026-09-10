#include <stdio.h>
struct Result { int value; };
static struct Result answer = {42};
static const struct Result *find(int unused) { return &answer; }
static const struct Result *(*const finder)(int) = find;
static int twice(int x) { return x * 2; }
static int (*mutable)(int) = twice;
static int (*empty)(int);
int main(void) {
    empty = mutable;
    printf("%d %d %d\n", finder(0)->value, empty(21), mutable == twice);
    return 0;
}
