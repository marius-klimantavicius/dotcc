#include "source_types.h"

ItemCount source_shapes(const Count limit, Mask mask, Toggle enabled)
{
    const ItemCount base = 3;
    ItemCount total = base;
    Toggle active = is_enabled(enabled);
    for (Count index = 0; index < limit; ++index) {
        const Count current = index;
        total = total + current;
    }
    if (active && (mask & 1ULL)) return total;
    return 0;
}

Toggle is_enabled(Toggle input)
{
    return input;
}

typedef int Input;
int alternate_parameter(const Input input)
{
    return input + 1;
}

int main(void)
{
    if (source_shapes(4L, 255ULL, (Toggle)1) != 9L) return 1;
    if (source_shapes(4L, 255ULL, (Toggle)0) != 0L) return 2;
    if (alternate_parameter(4) != 5) return 3;
    {
        typedef long Choice;
        Choice first = 4L;
        if (first != 4L) return 4;
    }
    {
        typedef unsigned long long Choice;
        Choice second = 18446744073709551615ULL;
        if (second != 18446744073709551615ULL) return 5;
    }
    return 0;
}
