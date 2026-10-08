#include "float_fixture.h"

int little_endian(void)
{
    unsigned int value = 1;
    return *(unsigned char*)&value == 1;
}
unsigned int float_bits(float value)
{
    unsigned int bits = 0;
    unsigned char* bytes = (unsigned char*)&value;
    for (int i = 0; i < 4; ++i)
        bits = bits | ((unsigned int)bytes[i] << ((little_endian() ? i : 3 - i) * 8));
    return bits;
}
unsigned long long double_bits(double value)
{
    unsigned long long bits = 0;
    unsigned char* bytes = (unsigned char*)&value;
    for (int i = 0; i < 8; ++i)
        bits = bits | ((unsigned long long)bytes[i] << ((little_endian() ? i : 7 - i) * 8));
    return bits;
}
double double_from_bits(unsigned long long bits)
{
    double result = 0.0;
    unsigned char* bytes = (unsigned char*)&result;
    int little = little_endian();
    for (int i = 0; i < 8; ++i)
        bytes[i] = (unsigned char)(bits >> ((little ? i : 7 - i) * 8));
    return result;
}

int floating_constants(void)
{
    if (float_bits(0.1f) != 0x3dcccccdU || double_bits(0.1) != 0x3fb999999999999aULL) return 1;
    if (float_bits(-0.0f) != 0x80000000U || double_bits(-0.0) != 0x8000000000000000ULL) return 2;
    if (float_bits(0x1p-149f) != 1U || double_bits(0x1p-1074) != 1ULL) return 3;
    if (float_bits(0x1.fffffep127f) != 0x7f7fffffU || double_bits(0x1.fffffffffffffp1023) != 0x7fefffffffffffffULL) return 4;
    float values[5] = {1.25f, -0.0f, 0.1f, 0x1p-149f};
    double wide[4] = {-0.0, 0x1p-1074, 0.1};
    if (float_bits(values[1]) != 0x80000000U || float_bits(values[3]) != 1U || float_bits(values[4])) return 5;
    if (double_bits(wide[0]) != 0x8000000000000000ULL || double_bits(wide[1]) != 1ULL || double_bits(wide[3])) return 6;
    if (float_array_sum(values, 1) != 1.25 || sizeof(values) != 20 || sizeof(wide) != 32) return 7;
    float special[4] = {__builtin_inff(), -__builtin_inff(), __builtin_nanf("0x12345"), -0.0f};
    double special_wide[4] = {__builtin_inf(), -__builtin_inf(), __builtin_nan("0x12345"), -0.0};
    if (float_bits(special[0]) != 0x7f800000U || float_bits(special[1]) != 0xff800000U || float_bits(special[2]) != 0x7fc12345U || float_bits(special[3]) != 0x80000000U) return 8;
    if (double_bits(special_wide[0]) != 0x7ff0000000000000ULL || double_bits(special_wide[1]) != 0xfff0000000000000ULL || double_bits(special_wide[2]) != 0x7ff8000000012345ULL || double_bits(special_wide[3]) != 0x8000000000000000ULL) return 9;
    if (float_bits(__builtin_nanf("0x12345")) != 0x7fc12345U || double_bits(__builtin_nan("0x12345")) != 0x7ff8000000012345ULL) return 25;
    if (float_bits(__builtin_inff()) != 0x7f800000U || double_bits(-__builtin_inf()) != 0xfff0000000000000ULL) return 26;
#ifdef __cplusplus
    float zero{};
    double zeroes[2]{};
#else
    float zero = {0};
    double zeroes[2] = {0};
#endif
    if (float_bits(zero) || double_bits(zeroes[0]) || double_bits(zeroes[1])) return 10;
    return 0;
}

int floating_operations(void)
{
    float a = float_add(1.25f, 2.5f);
    double b = double_add(8.5, -1.25);
    if (a != 3.75f || b != 7.25 || -a != -3.75f || +b != 7.25) return 11;
    if (a * 2.0f != 7.5f || b / 2.0 != 3.625 || b - 1.0 != 6.25) return 12;
    if (a++ != 3.75f || a != 4.75f || --a != 3.75f) return 13;
    if (++b != 8.25 || b-- != 8.25 || b != 7.25) return 14;
    double_store(&b, 10.25);
    double* p = &b;
    if ((*p)++ != 10.25 || b != 11.25 || --*p != 10.25) return 15;
    if ((int)double_identity(-3.75) != -3 || (unsigned int)double_identity(4.75) != 4U) return 16;
    if (double_bits((double)float_identity(0.5f)) != 0x3fe0000000000000ULL) return 17;
    if (float_bits((float)double_identity(0.1)) != 0x3dcccccdU) return 18;
    unsigned long long integer = 9007199254740992ULL;
    if ((unsigned long long)(double)integer != integer) return 19;
    if (float_add(float_identity(16777216.0f), 1.0f) != 16777216.0f) return 20;
    if (double_add(double_identity(9007199254740992.0), 1.0) != 9007199254740992.0) return 21;
    float increment_boundary = float_identity(16777216.0f);
    if (++increment_boundary != 16777216.0f || increment_boundary++ != 16777216.0f || increment_boundary != 16777216.0f) return 23;
    if ((long long)double_identity(-9007199254740992.0) != -9007199254740992LL || (unsigned long long)double_identity(9007199254740992.0) != 9007199254740992ULL) return 24;
    float selection = a > 0.0f ? a : -a;
    if (selection != 3.75f || !(a < b) || a >= b || !(b > a) || b <= a) return 22;
    return 0;
}

int floating_specials(void)
{
    double nan = double_from_bits(0x7ff8123456789abcULL);
    double infinity = double_from_bits(0x7ff0000000000000ULL);
    double minus_zero = double_identity(-0.0);
    if (nan == nan || !(nan != nan) || nan < 0.0 || nan >= 0.0 || nan <= 0.0 || nan > 0.0) return 31;
    if (!nan || !infinity || minus_zero || !(!minus_zero)) return 32;
    if (double_bits(double_identity(nan)) != 0x7ff8123456789abcULL) return 33;
    if (!(infinity > 0x1.fffffffffffffp1023) || !(-infinity < -0x1.fffffffffffffp1023)) return 34;
    if (double_bits(-minus_zero) != 0ULL) return 35;
    int effects = 0;
    if (minus_zero && ++effects) return 36;
    if (!(nan || ++effects) || effects) return 37;
#ifdef __cplusplus
    bool truth = (bool)nan;
    bool zero = (bool)minus_zero;
#else
    _Bool truth = (_Bool)nan;
    _Bool zero = (_Bool)minus_zero;
#endif
    if (!truth || zero) return 38;
    return 0;
}

int main(void)
{
    int result = floating_constants();
    if (result) return result;
    result = floating_operations();
    if (result) return result;
    return floating_specials();
}
