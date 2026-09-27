#include <stdio.h>

static int actual_calls, hint_calls;
static long actual(void) { actual_calls++; return 4294967298L; }
static long hint(void) { hint_calls++; return 0; }

int main(void) {
    long value = __builtin_expect(actual(), hint());
    if (value != 4294967298L || actual_calls != 1 || hint_calls != 1) return 1;
    if (sizeof(__builtin_expect(1, 0)) != sizeof(long)) return 2;
    if (__builtin_expect(-7, 1) != -7) return 3;
    if (__builtin_expect(0xffffffffU, 1) != 4294967295L) return 4;
    if (!__builtin_expect(2 > 1, 0)) return 5;
    if (__builtin_expect(1 > 2, 1)) return 6;
    printf("expect %ld actual %d hint %d width %zu\n", value, actual_calls, hint_calls,
           sizeof(__builtin_expect(1, 0)));
    return 0;
}
