#include <stdarg.h>
typedef __int128 I128;
typedef unsigned __int128 U128;
#define HIGH ((U128)1 << 127)
static U128 unsigned_values[] = { 0, HIGH, ~(U128)0, ((U128)0x123456789abcdef0ULL << 64) | 0xfedcba9876543210ULL };
static I128 signed_values[] = { (I128)(HIGH - 1), (I128)HIGH, -1234567890123456789LL };
static __thread U128 thread_value = ((U128)1 << 100) + 17;
struct WideRecord { unsigned char tag; U128 values[2]; I128 signed_value; };
static struct WideRecord global_record = { 3, { HIGH, ~(U128)0 }, (I128)HIGH };

__attribute__((noinline)) static U128 combine(U128 left, U128 right) {
    if ((unsigned long)&left % 16 || (unsigned long)&right % 16) return 0;
    return (left << 67) ^ (right >> 3);
}
__attribute__((noinline)) static struct WideRecord copy_record(struct WideRecord value) {
    if ((unsigned long)&value % 16) value.tag = 0;
    return value;
}
static I128 variadic_sum(int ignored, ...) {
    va_list values;
    va_start(values, ignored);
    int prefix = va_arg(values, int);
    U128 positive = va_arg(values, U128);
    double middle = va_arg(values, double);
    I128 negative = va_arg(values, I128);
    va_end(values);
    return (I128)positive + negative + prefix + (int)middle;
}
int main(void) {
    if (sizeof(U128) != 16 || _Alignof(U128) != 16) return 1;
    if (unsigned_values[1] != HIGH || unsigned_values[2] + 1 != 0) return 2;
    if (signed_values[0] != (I128)(HIGH - 1) || signed_values[1] != (I128)HIGH) return 3;
    if ((signed_values[1] >> 126) != -2 || signed_values[2] != -1234567890123456789LL) return 4;
    U128 high = (U128)1 << 100;
    U128 product = (high + 3) * 17;
    if (product / 17 != high + 3 || product % 17) return 5;
    I128 negative = -(I128)(high + 7);
    if (negative / 7 * 7 + negative % 7 != negative) return 6;
    U128 (*callback)(U128,U128) = combine;
    if (callback(3, high) != (((U128)3 << 67) ^ (high >> 3))) return 7;
    struct WideRecord local = copy_record(global_record);
    if ((unsigned long)&local % 16 || (unsigned long)&local.values[0] % 16 ||
        (unsigned long)&global_record % 16 || (unsigned long)&thread_value % 16) return 8;
    if (local.tag != 3 || local.values[1] != ~(U128)0 || local.signed_value != (I128)HIGH) return 9;
    if (thread_value != high + 17 || variadic_sum(0, 5, high + 3, 2.0, -(I128)high) != 10) return 10;
    if ((U128)(double)high != high || (I128)(long double)negative != -(I128)high) return 11;
    void *sentinel = (void*)~0UL;
    if ((U128)sentinel != ~(U128)0 || (I128)sentinel != -1 || (void*)(U128)sentinel != sentinel) return 12;
    int items[4] = { 1, 2, 3, 4 };
    U128 index = 2;
    int *pointer = items + index;
    if (*pointer != 3) return 13;
    pointer += (I128)-1;
    if (*pointer != 2) return 14;
    U128 local_unsigned[] = { HIGH + 9, ~(U128)0, ((U128)0x123456789abcdef0ULL << 64) | 0xfedcba9876543210ULL };
    I128 local_signed[] = { (I128)HIGH, (I128)(HIGH - 1), -1234567890123456789LL };
    U128 *unsigned_address = local_unsigned;
    I128 *signed_address = local_signed;
    if ((unsigned long)unsigned_address % 16 || (unsigned long)signed_address % 16) return 15;
    if (unsigned_address[0] != HIGH + 9 || unsigned_address[1] != unsigned_values[2] ||
        unsigned_address[2] != unsigned_values[3]) return 16;
    if (signed_address[0] != signed_values[1] || signed_address[1] != signed_values[0] ||
        signed_address[2] != signed_values[2]) return 17;
    unsigned_address[1] = 17;
    signed_address[0] = -19;
    if (unsigned_values[2] != ~(U128)0 || signed_values[1] != (I128)HIGH ||
        unsigned_address[1] != 17 || signed_address[0] != -19) return 18;
    if ((void*)(U128)&global_record != (void*)&global_record ||
        (void*)(I128)&global_record != (void*)&global_record) return 19;
    if ((U128 (*)(U128,U128))(U128)&combine != callback) return 20;
    return 0;
}
