#include <stdio.h>
#include <stddef.h>
#include <limits.h>

static size_t negative_bits = -sizeof(long) * 8;
static unsigned int wrapped32 = UINT_MAX + 1U;
static unsigned long long wrapped64 = ULLONG_MAX * 2ULL;
static unsigned long long widened = UINT_MAX + 2U;

static unsigned long long choose(int condition) {
    return condition ? UINT_MAX + 3U : UINT_MAX * 2U;
}

static int wrapped_case(unsigned int value) {
    switch (value) {
    case UINT_MAX + 2U: return 42;
    default: return 0;
    }
}

int main(void) {
    unsigned int local = 0U - 1U;
    unsigned long long product = (0U - 1U) * 2U;
    printf("%u %u %llu %llu\n", wrapped32, local, wrapped64, widened);
    printf("%llu %llu %llu %d\n", product, choose(1), choose(0), wrapped_case(1));
    printf("%d %d %d\n", negative_bits + sizeof(long) * 8 == 0,
           (size_t)-9 > -sizeof(long) * 8,
           (unsigned short)60000 + (unsigned short)10000);
    printf("%u %llu\n", (UINT_MAX + 2U) * (UINT_MAX + 2U),
           (ULLONG_MAX + 1ULL) - 1ULL);
    return 0;
}
