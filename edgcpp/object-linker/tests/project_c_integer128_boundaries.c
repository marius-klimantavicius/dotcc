typedef unsigned __int128 U128;
typedef __int128 I128;
#if defined(TEST_SWITCH)
int boundary(U128 value) {
    switch (value) {
        case ((U128)1 << 90): return 1;
        default: return 0;
    }
}
#elif defined(TEST_VOLATILE)
volatile U128 wide;
U128 boundary(void) { return wide; }
#elif defined(TEST_BITFIELD)
struct WideBits { U128 bits : 7; };
U128 boundary(struct WideBits *value) { return value->bits; }
#elif defined(TEST_INTRINSIC_ARGUMENT)
int boundary(U128 left, U128 right, unsigned long *result) {
    return __builtin_add_overflow(left, right, result);
}
#elif defined(TEST_INTRINSIC_DESTINATION)
int boundary(unsigned long left, unsigned long right, U128 *result) {
    return __builtin_add_overflow(left, right, result);
}
#elif defined(TEST_ADDRESS_RELOCATION)
int target;
U128 wide_address = (U128)&target;
U128 boundary(void) { return wide_address; }
#else
#error Select one explicit deferred integer128 boundary
#endif
