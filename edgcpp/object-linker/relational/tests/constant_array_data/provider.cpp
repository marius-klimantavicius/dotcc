#include "program.h"

unsigned char byte_values[6] = {0, 255, 17};
signed char signed_values[4] = {-128, -1, 0, 127};
short short_values[4] = {-32768, -1, 0, 32767};
unsigned short ushort_values[4] = {0, 65535, 32768, 1};
int integer_values[6] = {-2147483647 - 1, 2147483647, 3};
unsigned int unsigned_values[4] = {0, 4294967295u, 2147483648u, 1};
long long wide_values[4] = {-9223372036854775807LL - 1, 9223372036854775807LL, -1, 0};
unsigned long long uwide_values[4] = {0, 18446744073709551615ULL, 9223372036854775808ULL, 1};
int repeated_values[12] = {7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7};
int zero_values[5] = {};

// Relocations retain selected address actions and the ordinary writer.
int *relocations[2] = {&forward_value, &forward_value};
int forward_value = 23;

int *integer_address() { return integer_values; }
