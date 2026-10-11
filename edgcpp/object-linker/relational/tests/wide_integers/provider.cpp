#include "program.h"
const UWide wide_values[3] = {(UWide)1 << 100, (UWide)-1, (UWide)1 << 127};
UWide transform(UWide value) {
    value <<= 5;
    value ^= (UWide)1 << 118;
    value += 19;
    return value;
}
Wide divide_signed(Wide value) { return value / 7 + value % 7; }
UWide variadic_wide(int marker, ...) {
    __builtin_va_list arguments;
    __builtin_va_start(arguments, marker);
    UWide value = __builtin_va_arg(arguments, UWide);
    Wide delta = __builtin_va_arg(arguments, Wide);
    __builtin_va_end(arguments);
    return value + (UWide)delta;
}
