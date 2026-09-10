#include <stdint.h>
#include <stdio.h>
#if INT8_C(127) == INT8_MAX && UINT8_C(255) == UINT8_MAX && INT16_C(32767) == INT16_MAX && UINT16_C(65535) == UINT16_MAX && INT32_C(2147483647) == INT32_MAX && UINT32_C(4294967295) == UINT32_MAX && INT64_C(9223372036854775807) == INT64_MAX && UINT64_C(18446744073709551615) == UINT64_MAX && INTMAX_C(9223372036854775807) == INTMAX_MAX && UINTMAX_C(18446744073709551615) == UINTMAX_MAX
#define MATCHING_LIMITS 1
#else
#define MATCHING_LIMITS 0
#endif
static uint64_t table[] = { UINT64_C(0x0123456789abcdef), UINT64_C(0xffffffffffffffff) };
int main(void) {
    int_least8_t i8 = INT8_C(123);
    uint_least8_t u8 = UINT8_C(123);
    int_least16_t i16 = INT16_C(123);
    uint_least16_t u16 = UINT16_C(123);
    int_least32_t i32 = INT32_C(123);
    uint_least32_t u32 = UINT32_C(123);
    int_least64_t i64 = INT64_C(123);
    uint_least64_t u64 = UINT64_C(123);
    intmax_t im = INTMAX_C(123);
    uintmax_t um = UINTMAX_C(123);
    printf("%d %d %d %d %d %d %d %d %d %d\n",
        (int)sizeof(int_least8_t), (int)sizeof(uint_least8_t),
        (int)sizeof(int_least16_t), (int)sizeof(uint_least16_t),
        (int)sizeof(int_least32_t), (int)sizeof(uint_least32_t),
        (int)sizeof(int_least64_t), (int)sizeof(uint_least64_t),
        (int)sizeof(intmax_t), (int)sizeof(uintmax_t));
    printf("%d\n", i8 == 123 && u8 == 123 && i16 == 123 && u16 == 123 &&
        i32 == 123 && u32 == 123 && i64 == 123 && u64 == 123 && im == 123 && um == 123);
    printf("%d %d %d %d\n", MATCHING_LIMITS,
        UINT8_C(0) - 1 == -1, UINT16_C(0) - 1 == -1, ~UINT32_C(0) == UINT32_MAX);
    printf("%d %d %d %d\n", table[0] == UINT64_C(81985529216486895),
        table[1] == UINT64_MAX, -INT64_C(42) == -42, ~UINTMAX_C(0) == UINTMAX_MAX);
    switch (INT64_C(42)) { case INTMAX_C(42): puts("case 42"); break; default: return 1; }
    return 0;
}
