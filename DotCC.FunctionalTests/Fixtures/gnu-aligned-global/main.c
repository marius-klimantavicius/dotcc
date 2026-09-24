#include <stdio.h>
#include <stdint.h>
#include <stdatomic.h>

static __attribute__((aligned(64))) _Atomic unsigned long counters[9];
static _Atomic unsigned long *active = &counters[7];
static __attribute__((aligned(32))) int scalar = 5;

int main(void) {
    active[0] += 13;
    active[1] += 7;
    printf("%lu %lu %lu %d %d\n", (unsigned long)sizeof(counters),
        counters[7], counters[8], (int)((uintptr_t)counters % 64),
        (int)((uintptr_t)&scalar % 32));
    return scalar - 5;
}
