#include <stdio.h>
typedef int (Transform)(int value);
typedef int Plain(int);
typedef Transform Alias;
Transform plus_one;
extern Plain times_two;
static Transform minus_one;
Transform *callbacks[] = {plus_one, times_two};
struct Dispatch { Transform *callback; };
int apply(Transform callback, int value) { return callback(value); }
int plus_one(int value) { return value + 1; }
int times_two(int value) { return value * 2; }
static int minus_one(int value) { return value - 1; }
;
int main(void) {
    typedef int (Local)(int);
    Local *local = minus_one;
    Alias *first = plus_one, *second = times_two;
    Transform **slot = &first;
    struct Dispatch dispatch = {plus_one};
    printf("%d %d %d %d %d %d %d\n", apply(first, 4), second(4), (*slot)(4),
        dispatch.callback(4), callbacks[1](4), local(4), (int)sizeof(Transform *));
    return 0;
}
