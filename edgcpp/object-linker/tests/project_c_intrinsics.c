/* Native compiler builtins retain their mathematical and storage semantics. */
static volatile unsigned long long volatile_result;
static volatile unsigned long long maximum = 18446744073709551615ULL;
static volatile int maximum_int = 2147483647;
static volatile int minimum_int = (-2147483647 - 1);
static volatile long long minimum_long = (-9223372036854775807LL - 1);
static volatile int negative_three = -3;
static volatile int negative_one = -1;
static volatile int zero = 0;
static volatile int eight = 8;
static volatile long long bit48 = 1LL << 48;

static long hint(long value) { return __builtin_expect(value, 7L); }
static int positive(int value) {
    if (value > 0) return value;
    __builtin_unreachable();
}
static int query(int value) {
    /* The operand is not evaluated, even if the optimization query survives. */
    int result = __builtin_constant_p(value++);
    (void)result;
    return value;
}

int main(void) {
    unsigned value = 0x01234567U;
    unsigned long long wide = 0x0123456789abcdefULL;
    if (__builtin_bswap16((unsigned short)value) != 0x6745U) return 1;
    if (__builtin_bswap32(value) != 0x67452301U) return 2;
    if (__builtin_bswap64(wide) != 0xefcdab8967452301ULL) return 3;
    if (__builtin_clz(value) != 7 || __builtin_ctz(value) != 0) return 4;
    if (__builtin_clzll(wide) != 7 || __builtin_ctzll(wide) != 0) return 5;
    if (__builtin_popcount(value) != 12 || __builtin_popcountll(wide) != 32) return 6;
    if (__builtin_parity(value) != 0 || __builtin_parityll(wide ^ 1ULL) != 1) return 7;
    if (__builtin_ffs(zero) != 0 || __builtin_ffs(eight) != 4) return 8;
    if (__builtin_ffsll(bit48) != 49) return 9;
    if (__builtin_clrsb(negative_one) != 31 || __builtin_clrsbll((long long)negative_one) != 63) return 10;

    int signed_result;
    if (!__builtin_add_overflow(maximum_int, 1, &signed_result)) return 11;
    if (signed_result != (-2147483647 - 1)) return 12;
    if (!__builtin_sub_overflow(minimum_int, 1, &signed_result)) return 13;
    if (signed_result != 2147483647) return 14;
    unsigned long long result;
    if (__builtin_add_overflow(-1, maximum, &result)) return 15;
    if (result != 18446744073709551614ULL) return 16;
    if (!__builtin_sub_overflow(maximum, -1, &result)) return 17;
    if (result != 0) return 18;
    if (!__builtin_mul_overflow(maximum, maximum, &result)) return 19;
    if (result != 1) return 20;
    if (!__builtin_mul_overflow(negative_three, 2U, &result)) return 21;
    if (result != 18446744073709551610ULL) return 22;
    long long signed_wide;
    if (!__builtin_mul_overflow(minimum_long, -1LL, &signed_wide)) return 23;
    if (signed_wide != (-9223372036854775807LL - 1)) return 24;
    if (!__builtin_add_overflow(maximum, 4, &volatile_result)) return 25;
    if (volatile_result != 3) return 26;
    int left = 4, right = 7, index = 0;
    int cells[2] = { 0, 0 };
    if (__builtin_add_overflow(left++, right++, &cells[index++])) return 27;
    if (left != 5 || right != 8 || index != 1 || cells[0] != 11 || cells[1] != 0) return 28;
    if (hint(17) != 17 || positive(9) != 9 || query(13) != 13) return 29;
    return 0;
}
