#include "program.h"

int main() {
    if (byte_values[0] != 0 || byte_values[1] != 255 || byte_values[2] != 17) return 1;
    for (int i = 3; i < 6; ++i) if (byte_values[i] != 0) return 2;
    if (signed_values[0] != -128 || signed_values[1] != -1 || signed_values[2] != 0 || signed_values[3] != 127) return 3;
    if (short_values[0] != -32768 || short_values[1] != -1 || short_values[2] != 0 || short_values[3] != 32767) return 4;
    if (ushort_values[0] != 0 || ushort_values[1] != 65535 || ushort_values[2] != 32768 || ushort_values[3] != 1) return 5;
    if (integer_values[0] != -2147483647 - 1 || integer_values[1] != 2147483647 || integer_values[2] != 3) return 6;
    for (int i = 3; i < 6; ++i) if (integer_values[i] != 0) return 7;
    if (unsigned_values[0] != 0 || unsigned_values[1] != 4294967295u || unsigned_values[2] != 2147483648u || unsigned_values[3] != 1) return 8;
    if (wide_values[0] != -9223372036854775807LL - 1 || wide_values[1] != 9223372036854775807LL || wide_values[2] != -1 || wide_values[3] != 0) return 9;
    if (uwide_values[0] != 0 || uwide_values[1] != 18446744073709551615ULL || uwide_values[2] != 9223372036854775808ULL || uwide_values[3] != 1) return 10;
    for (int i = 0; i < 12; ++i) if (repeated_values[i] != 7) return 11;
    for (int i = 0; i < 5; ++i) if (zero_values[i] != 0) return 12;
    if (integer_address() != integer_values || integer_address() != integer_address()) return 13;
    if (relocations[0] != &forward_value || relocations[1] != &forward_value || *relocations[0] != 23) return 14;
    integer_values[2] = 19;
    if (integer_address()[2] != 19) return 15;
    return 0;
}
