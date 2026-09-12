#define _POSIX_C_SOURCE 200809L
#include <string.h>
#include <stdint.h>
#include <stdio.h>

static size_t measure(const char* value, size_t bound)
{
    return strnlen(value, bound);
}

int main(void)
{
    char unterminated[3] = {'x', 'y', 'z'};
    char embedded[5] = {'a', 0, 'b', 'c', 'd'};
    printf("bounded %zu %zu %zu\n", measure(unterminated, 0), measure(unterminated, 2), measure(unterminated, 3));
    printf("terminated %zu %zu %zu\n", measure(embedded, 5), measure("", 1), measure("hello", 6));
    printf("wide %zu %zu\n", measure(embedded, (size_t)UINT32_MAX + 1), sizeof(strnlen(embedded, 1)));
    printf("unchanged %d %d\n", unterminated[2], embedded[4]);
    return 0;
}
