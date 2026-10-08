unsigned long long mix(unsigned long long value, unsigned int count)
{
    return (value << count) ^ (value >> (64U - count));
}

int widths(void)
{
    signed char small = -128;
    unsigned char byte = 255;
    short half = -32768;
    unsigned short word = 65535;
    unsigned int bits = 4294967295U;
    long long low = -9223372036854775807LL - 1LL;
    unsigned long long high = 18446744073709551615ULL;
    if (small != -128 || byte != 255 || half != -32768 || word != 65535) return 1;
    if (bits != 4294967295U || high != 18446744073709551615ULL) return 2;
    if (low != -9223372036854775807LL - 1LL) return 3;
    if ((unsigned int)high != bits || (unsigned short)bits != word) return 4;
    if ((unsigned long long)low != 9223372036854775808ULL) return 5;
    if ((long long)small != -128LL || (long long)half != -32768LL) return 6;
    if ((unsigned char)word != byte || (short)word != -1) return 7;
    unsigned long long before = high++;
    if (before != 18446744073709551615ULL || high != 0ULL) return 8;
    before = --high;
    if (before != 18446744073709551615ULL || high != before) return 9;
    unsigned int prior = bits++;
    if (prior != 4294967295U || bits != 0U) return 10;
    unsigned short old = word++;
    if (old != 65535 || word != 0) return 11;
    small++;
    half++;
    if (small != -127 || half != -32767) return 12;
    long native_long = -2147483649L;
    unsigned long native_ulong = 4294967296UL;
    if (native_long != -2147483649LL || native_ulong != 4294967296ULL) return 13;
    if (high + 1ULL != 0ULL || bits - 1U != 4294967295U) return 14;
    unsigned int one = 1U;
    unsigned long long wide_one = 1ULL;
    if (-one != 4294967295U || -wide_one != 18446744073709551615ULL) return 15;
    if ((one << 31) != 2147483648U || (high / 3ULL) != 6148914691236517205ULL) return 16;
    if ((high % 3ULL) != 0ULL || (high * 2ULL) != 18446744073709551614ULL) return 17;
    return 0;
}

int bit_operations(void)
{
    unsigned int bits = 0xF0F0U;
    unsigned int mask = 0xFF00U;
    if ((bits & mask) != 0xF000U) return 20;
    if ((bits | mask) != 0xFFF0U) return 21;
    if ((bits ^ mask) != 0x0FF0U) return 22;
    if (~bits != 0xFFFF0F0FU) return 23;
    int negative = -8;
    int shift = 2;
    if ((negative >> shift) != -2 || (12 << shift) != 48) return 24;
    unsigned long long wide = 0x8000000000000000ULL;
    long long count = 63LL;
    if ((wide >> count) != 1ULL) return 25;
    if ((1ULL << count) != wide) return 26;
    if (mix(0x0123456789ABCDEFULL, 8U) != 0x23456789ABCDEF01ULL) return 27;
    if ((~wide & wide) != 0ULL) return 28;
    unsigned char tiny = 0xA5;
    if ((~tiny & 255) != 0x5A) return 29;
    return 0;
}

int conditionals(int choose)
{
    int left = 10;
    int right = 20;
    int picked = choose ? left++ : right++;
    if (choose) {
        if (picked != 10 || left != 11 || right != 20) return 30;
    } else {
        if (picked != 20 || left != 10 || right != 21) return 31;
    }
    unsigned long long selected = choose ? 18446744073709551615ULL : 0ULL;
    if (choose && selected != 18446744073709551615ULL) return 32;
    if (!choose && selected != 0ULL) return 33;
    unsigned char a = 250;
    unsigned char b = 42;
    unsigned char narrow = choose ? a : b;
    if (choose && narrow != 250) return 34;
    if (!choose && narrow != 42) return 35;
    int nested = choose ? (left > 0 ? 7 : 8) : (right > 0 ? 9 : 10);
    if (choose && nested != 7) return 36;
    if (!choose && nested != 9) return 37;
    return 0;
}

int main(void)
{
    int result = widths();
    if (result) return result;
    result = bit_operations();
    if (result) return result;
    result = conditionals(0);
    if (result) return result;
    return conditionals(1);
}
