#include <stdio.h>
int main(void)
{
    const char *p = "hello", *const end = p + 5;
    int count = 0;
    while (p != end) { ++p; ++count; }
    int value = 7;
    int *v = &value, *const fixed_v = v, *const *cursor = &fixed_v;
    cursor = &fixed_v;
    **cursor = 9;
    printf("count=%d value=%d\n", count, value);
    return 0;
}
