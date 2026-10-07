/* Backend-only promotion views must match native and EDG's direct pipeline. */
enum Wide { WideZero = 0, WideMax = 0xffffffffU };
struct PromotionBits {
    unsigned small:3;
    signed negative:4;
    unsigned full:32;
    enum Wide enum_bits:3;
    volatile unsigned watched:5;
};
#ifdef __cplusplus
enum Narrow : unsigned char { NarrowMax = 255 };
enum class Scoped : unsigned { ScopedMax = 0xffffffffU };
#endif

static int promotions(void) {
    struct PromotionBits bits = {6, -3, 0xffffffffU, WideZero, 3};
    bits.small /= -1;
    bits.negative %= 2;
    if (bits.small != 2 || bits.negative != -1) return 1;
    if ((bits.small += 1) < -1 || (bits.small++, bits.small) < -1) return 2;
    if (bits.small++ < -1 || ++bits.small < -1) return 3;
    bits.watched /= -1;
    if (bits.watched != 29 || bits.full + 1ULL != 4294967296ULL) return 4;
    unsigned char byte = 255;
    signed char negative = -7;
    enum Wide wide = WideMax;
    if (byte + negative != 248 || wide + -1LL != 4294967294LL) return 5;
    if (bits.enum_bits + -1 != -1 || +bits.enum_bits != 0) return 12;
#ifdef __cplusplus
    Narrow narrow = NarrowMax;
    if (narrow + -256 != -1 || static_cast<unsigned>(Scoped::ScopedMax) != 0xffffffffU) return 6;
#endif
    return 0;
}

static int atomics(void) {
    unsigned value = 5, expected = 5;
    unsigned char flag = 0;
    if (!__atomic_compare_exchange_n(&value, &expected, 9, 0, __ATOMIC_SEQ_CST, __ATOMIC_SEQ_CST)) return 7;
    if (value != 9 || __atomic_test_and_set(&flag, __ATOMIC_SEQ_CST)) return 8;
    if (!__atomic_test_and_set(&flag, __ATOMIC_SEQ_CST)) return 9;
    __atomic_clear(&flag, __ATOMIC_SEQ_CST);
    __atomic_thread_fence(__ATOMIC_SEQ_CST);
    if (flag != 0) return 10;
#ifndef __cplusplus
    _Atomic(unsigned char) small = 254;
    _Atomic(double) real = 0.5;
    ++small; small++; ++real;
    if (small != 0 || real != 1.5) return 11;
#endif
    return 0;
}

int main(void) {
    int result = promotions();
    return result ? result : atomics();
}
