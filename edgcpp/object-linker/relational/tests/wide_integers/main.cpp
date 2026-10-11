#include "program.h"
int main() {
    UWide high = (UWide)1 << 100;
    UWide maximum = (UWide)-1;
    Wide minimum = (Wide)((UWide)1 << 127);
    if (wide_values[0] != high || wide_values[1] != maximum || wide_values[2] != ((UWide)1 << 127)) return 1;
    if ((maximum >> 64) != 18446744073709551615ULL) return 2;
    if (minimum / 2 != -((Wide)1 << 126)) return 3;
    Wide negative = -((Wide)1 << 100) - 17;
    if (divide_signed(negative) != negative / 7 + negative % 7) return 4;
    if ((negative >> 98) != -5) return 5;
    UWide transformed = ((high + 3) << 5) ^ ((UWide)1 << 118);
    transformed += 19;
    if (transform(high + 3) != transformed) return 6;
    if (selected<>() != high || selected<((UWide)1 << 127)>() != wide_values[2]) return 7;
    if (variadic_wide(0, high, (Wide)-17) != high - 17) return 8;
    unsigned long low = (unsigned long)(high + 123);
    if (low != 123) return 9;
    int local = 42;
    UWide address = (UWide)(void*)&local;
    int *pointer = (int*)address;
    if (pointer != &local || *pointer != 42) return 10;
    long double extended = (long double)high;
    if ((UWide)extended != high) return 11;
    return 0;
}
