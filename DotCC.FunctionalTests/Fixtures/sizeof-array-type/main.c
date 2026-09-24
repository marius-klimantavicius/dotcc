#include <stdint.h>
#include <stdio.h>

#define LIMIT (512ULL * 1024ULL * 1024ULL)
extern char compile_time_assertion[sizeof(char[(LIMIT < (UINT64_MAX - 9U) / 10) ? 1 : -1])];
int main(void) {
    printf("%lu %lu %lu %lu\n", (unsigned long)sizeof(char[1]),
        (unsigned long)sizeof(int[2][3]), (unsigned long)sizeof(void *[4]),
        (unsigned long)sizeof(compile_time_assertion));
    return 0;
}
