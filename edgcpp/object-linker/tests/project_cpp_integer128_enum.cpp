typedef unsigned __int128 U128;
typedef __int128 I128;
enum class WideUnsigned : U128 {
    high = (U128)1 << 127,
    maximum = ~(U128)0,
};
enum class WideSigned : I128 {
    minimum = (I128)((U128)1 << 127),
    negative = -1234567890123456789LL,
};
static WideUnsigned values[] = { WideUnsigned::high, WideUnsigned::maximum };
static WideSigned negative = WideSigned::minimum;
__attribute__((noinline)) WideUnsigned identity(WideUnsigned value) { return value; }
int main() {
    WideSigned local[] = { WideSigned::minimum, WideSigned::negative };
    if (sizeof(WideUnsigned) != 16 || alignof(WideSigned) != 16) return 1;
    if (identity(values[0]) != WideUnsigned::high || values[1] != WideUnsigned::maximum) return 2;
    if (local[0] != negative || (I128)local[1] != -1234567890123456789LL) return 3;
    return 0;
}
