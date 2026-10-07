/* Deliberately header-free: exercise semantic facts independently of libc. */
int twice(int value)
{
    return value * 2;
}

int classify(int value)
{
    return (value < 0) + 2 * (value == 0) + 4 * (value > 0);
}

int truth(int left, int right)
{
    int result = !left;
    if (left) result += 2;
    result += 4 * (left && right);
    result += 8 * (left || right);
    return result + (left ? 16 : 32);
}

int loop_sum(int count)
{
    int total = 0;
    for (int i = 0; i < count; ++i) {
        if (i % 3 == 0) continue;
        total += twice(i);
        if (total > 80) break;
    }
    return total;
}

int while_sum(int count)
{
    int total = 0;
    while (count > 0) {
        --count;
        if (count == 2) continue;
        total += count;
    }
    do {
        total += 3;
        --count;
    } while (count > 0);
    return total;
}

unsigned int unsigned_ops(unsigned int value, unsigned int shift)
{
    return ((value >> shift) ^ (value << 1)) + 0xffffffffU;
}

unsigned long long unsigned_wide(unsigned long long value)
{
    return value + 18446744073709551615ULL;
}

long long signed_wide(long long value)
{
    return value + (-9223372036854775807LL - 1);
}

int narrow(int value)
{
    return (unsigned char)value;
}

int signed_ops(int value, int divisor)
{
    return (value / divisor) * 10 + value % divisor;
}

int names(int event)
{
    int result = event;
    {
        int event = 7;
        result += event;
    }
    return result;
}

int short_circuit(int value)
{
    int result = 0;
    if (value && ++result) result += 10;
    if (value || ++result) result += 20;
    return result;
}

int dangling_else(int outer, int inner)
{
    int result = 3;
    if (outer) {
        if (inner) result = 11;
    } else result = 29;
    return result;
}

int nested_loops(int count)
{
    int total = 0;
    for (int i = 0; i < count; ++i) {
        if (i % 2 == 0) continue;
        for (int j = 0; j < count; ++j) {
            if (j == 1) continue;
            if (j > 3) break;
            total += i + j;
        }
        if (total > 80) break;
    }
    return total;
}
