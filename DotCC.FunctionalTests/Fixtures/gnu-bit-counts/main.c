#include <stdio.h>

static int calls;
static unsigned long next(void) { calls++; return 0x100000001UL; }

int main(void) {
    for (int bit = 0; bit < 64; ++bit) {
        unsigned long value = 1UL << bit;
        if (__builtin_clzl(value) != 63 - bit || __builtin_ctzl(value) != bit ||
            __builtin_clzll(value) != 63 - bit || __builtin_ctzll(value) != bit ||
            __builtin_popcountl(value) != 1 || __builtin_popcountll(value) != 1) return 1;
        if (bit < 32 && (__builtin_clz((unsigned int)value) != 31 - bit ||
            __builtin_ctz((unsigned int)value) != bit || __builtin_popcount((unsigned int)value) != 1)) return 2;
    }
    /* GCC's fixed unsigned parameter widths truncate before counting. */
    unsigned long wide = 0x100000001UL;
    if (__builtin_clz(wide) != 31 || __builtin_ctz(wide) != 0 || __builtin_popcount(wide) != 1) return 3;
    if (__builtin_popcount(-1) != 32 || __builtin_popcountl(-1) != 64 || __builtin_popcountll(-1) != 64) return 4;
    if (__builtin_popcount(0) != 0 || __builtin_popcountl(0) != 0 || __builtin_popcountll(0) != 0) return 5;
    int count = __builtin_popcountl(next());
    if (count != 2 || calls != 1) return 6;
    printf("widths 32 64 positions 64 evaluation %d\n", calls);
    return 0;
}
