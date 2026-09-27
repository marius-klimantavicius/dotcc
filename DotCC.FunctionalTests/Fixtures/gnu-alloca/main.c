#include <stdio.h>
#include <stdint.h>
void *alloca(unsigned long size);

static int size_calls;
static unsigned long allocation_size(void) { size_calls++; return 37; }
static int check_lifetime(int n) {
    unsigned char *saved[4];
    for (int i = 0; i < n; i++) {
        unsigned char *p = __builtin_alloca(allocation_size());
        if ((uintptr_t)p % 16) return 1;
        for (int j = 0; j < 37; j++) p[j] = i * 40 + j;
        saved[i] = p;
    }
    /* Every allocation remains live after its lexical block and later allocas. */
    unsigned char *later = alloca(200);
    for (int j = 0; j < 200; j++) later[j] = 255;
    for (int i = 0; i < n; i++)
        for (int j = 0; j < 37; j++)
            if (saved[i][j] != i * 40 + j) return 2;
    if (size_calls != n) return 3;
    return 0;
}
int main(void) {
    int result = check_lifetime(4);
    printf("alloca lifetime %d sizes %d\n", result, size_calls);
    return result;
}
