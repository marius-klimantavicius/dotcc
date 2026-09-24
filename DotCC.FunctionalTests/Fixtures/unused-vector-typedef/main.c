#include <stdio.h>
#include <stdint.h>

/* Some headers declare SIMD aliases even when their scalar implementation is
 * selected. Keeping this declaration must not change scalar ABI or algorithms. */
typedef unsigned long long lanes __attribute__((vector_size(16)));

static uint64_t scalar_combine(const uint64_t *values, uint64_t mask) {
    uint64_t result = 0;
    while (mask) {
        if (mask & 1) result ^= *values;
        mask >>= 1;
        values++;
    }
    return result;
}

static unsigned int local_shadow(void) {
    typedef unsigned int lanes;
    lanes value = 7;
    return value;
}

int main(void) {
    uint64_t values[3] = {32, 8, 2};
    printf("scalar=%llu shadow=%u\n", (unsigned long long)scalar_combine(values, 7), local_shadow());
    return 0;
}
