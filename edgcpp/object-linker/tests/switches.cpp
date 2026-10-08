// Also compiled as C11. Ordinary C fallthrough must retain its order in C#.
int grouped(int n)
{
    int value = 0;
    switch (n) {
    case -2:
    case -1:
        value = 5;
    default:
        value = value + 7;
    case 4:
        value = value + 11;
        break;
    case 9:
        value = 31;
    case 10:
        ;
    }
    return value;
}

int nested(int n)
{
    int total = 0;
    for (int i = 0; i < 5; ++i) {
        switch (i) {
        case 0:
            continue;
        case 1: {
            int value = 20;
            switch (n) {
            case 7: value = value + 2; break;
            default: value = 0; break;
            }
            total = total + value;
            break;
        }
        case 2:
            for (int j = 0; j < 4; ++j) {
                if (j == 1) continue;
                if (j == 3) break;
                total = total + j;
            }
            break;
        default:
            if (i == 3) continue;
            total = total + i;
        }
        total = total + 100;
    }
    return total;
}

int scoped(int n)
{
    switch (n) {
        int value;
    case 1:
        value = 23;
        return value;
    case 2: {
        int value = 47;
        return value;
    }
    default:
        return 9;
    }
}

int wide(unsigned long long n)
{
    switch (n) {
    case 0ULL: return 1;
    case 9223372036854775808ULL: return 2;
    case 18446744073709551615ULL: return 3;
    default: return 4;
    }
}

int signed_wide(long long n)
{
    switch (n) {
    case (-9223372036854775807LL - 1LL): return 11;
    case 9223372036854775807LL: return 12;
    default: return 13;
    }
}

int conditions(int n, int stop)
{
    int result = 0;
    switch (n) {
    case 0:
        if (stop) break;
        result = 3;
    case 1:
        if (stop) return 11;
        else result = result + 7;
    case 2:
        if (stop) return 12;
        else return result + 5;
    default:
        ;
    }
    return result;
}

int selector_once(int n)
{
    int value = 0;
    switch (n++) {
    case 2: value = 10; break;
    case 3: value = 20; break;
    }
    switch (++n) { }
    return value + n;
}

int narrow(unsigned char n)
{
    switch (n) {
    case 0: return 10;
    case 255: return 20;
    }
    return 30;
}

int unsigned_case(unsigned n)
{
    switch (n) {
    case 4294967295U: return 8;
    case 2147483648U: return 9;
    default: return 10;
    }
}

int initialized_tail(int n)
{
    switch (n) {
    case 1:
        ;
        int value = ++n;
        return value;
    }
    return n;
}

#ifndef __cplusplus
int skipped_initializer(int n)
{
    int result = 3;
    switch (n) {
    case 1:
        ;
        int value = ++result;
        result = result + value;
        break;
    case 2:
        value = 21;
        return result + value;
    }
    return result;
}

int converted_case(unsigned n)
{
    switch (n) {
    case -1: return 5;
    default: return 6;
    }
}
#endif

int main(void)
{
    if (grouped(-2) != 23 || grouped(-1) != 23) return 1;
    if (grouped(0) != 18 || grouped(4) != 11) return 2;
    if (grouped(9) != 31 || grouped(10) != 0) return 3;
    if (nested(7) != 328 || nested(0) != 306) return 4;
    if (scoped(1) != 23 || scoped(2) != 47 || scoped(3) != 9) return 5;
    if (wide(0ULL) != 1 || wide(9223372036854775808ULL) != 2) return 6;
    if (wide(18446744073709551615ULL) != 3 || wide(7ULL) != 4) return 7;
    if (signed_wide(-9223372036854775807LL - 1LL) != 11 || signed_wide(9223372036854775807LL) != 12) return 8;
    if (conditions(0, 0) != 15 || conditions(0, 1) != 0) return 9;
    if (conditions(1, 0) != 12 || conditions(1, 1) != 11) return 10;
    if (conditions(2, 0) != 5 || conditions(2, 1) != 12 || conditions(3, 0) != 0) return 11;
    if (selector_once(2) != 14 || selector_once(9) != 11) return 12;
    if (narrow(0) != 10 || narrow(255) != 20 || narrow(19) != 30) return 13;
    if (unsigned_case(4294967295U) != 8 || unsigned_case(2147483648U) != 9 || unsigned_case(0) != 10) return 14;
    if (initialized_tail(1) != 2 || initialized_tail(0) != 0) return 15;
#ifndef __cplusplus
    if (skipped_initializer(1) != 8 || skipped_initializer(2) != 24 || skipped_initializer(0) != 3) return 16;
    if (converted_case(4294967295U) != 5 || converted_case(1) != 6) return 17;
#endif
    return 0;
}
