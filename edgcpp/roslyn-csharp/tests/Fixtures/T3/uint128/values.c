static unsigned long observations[8];
static unsigned long multiply(unsigned long a, unsigned long b, unsigned long *low) {
    __uint128_t result = (__uint128_t)a * b;
    *low = (unsigned long)result;
    return (unsigned long)(result >> 64);
}
static unsigned long shifted(unsigned long a, unsigned long b, unsigned int *low) {
    __uint128_t result = (__uint128_t)a * b;
    result += ((__uint128_t)1 << 96) | ((__uint128_t)7 << 64) | 13;
    *low = (unsigned int)(result & 0xffffffff);
    return (unsigned long)(result >> 32);
}
int fixture(void) {
    unsigned long low;
    unsigned int narrow;
    observations[0] = multiply(0xffffffffffffffffUL, 0xffffffffffffffffUL, &low);
    observations[1] = low;
    observations[2] = multiply(0x123456789abcdef0UL, 0xfedcba9876543210UL, &low);
    observations[3] = low;
    observations[4] = shifted(0xffffffffffffffffUL, 17, &narrow);
    observations[5] = narrow;
    __uint128_t wide = ((__uint128_t)0x123456789abcdef0UL << 64) | 0xfedcba9876543210UL;
    observations[6] = (unsigned long)(wide >> 64);
    observations[7] = (unsigned long)wide;
    return 8;
}
unsigned long uint128_observe(int index) { return observations[index]; }
