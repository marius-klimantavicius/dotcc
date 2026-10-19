/* Shared-service control. With DOTCC_BLAKE2_HEADER, exercise the unchanged
 * same-pin upstream inline selected by HAVE_EXPLICIT_BZERO. */
#include <stddef.h>
#include <stdint.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <errno.h>
#ifdef DOTCC_BLAKE2_HEADER
#include "blake2-impl.h"
#define ERASE secure_zero_memory
#else
#define ERASE explicit_bzero
#endif

static int checks;
static unsigned char global_bytes[273];
static void check(int ok) { if (!ok) abort(); ++checks; }
static int all(const unsigned char *p, size_t n, int value)
{
    for (size_t i = 0; i < n; ++i) if (p[i] != value) return 0;
    return 1;
}
static void region(unsigned char *p, size_t capacity, size_t start, size_t n)
{
    memset(p, 0xa7, capacity);
    errno = EDOM;
    ERASE(p + start, n);
    check(errno == EDOM);
    check(all(p, start, 0xa7));
    check(all(p + start, n, 0));
    check(all(p + start + n, capacity - start - n, 0xa7));
}
/* No read of this key follows its erasure. Inspect optimized call sites;
 * the ordinary region checks alone do not prove dead-buffer non-elision. */
int r13_dead_key(int execute)
{
    unsigned char key[129];
    if (execute) {
        memset(key, 0xc5, sizeof key);
        ERASE(key, sizeof key);
    }
    return execute + 1;
}
int main(int argc, char **argv)
{
    (void)argv;
    unsigned char stack[273];
    unsigned char *heap = malloc(4099);
    check(heap != NULL);
    ERASE(NULL, 0);
    region(stack, sizeof stack, 0, 0);
    region(stack, sizeof stack, 1, 1);
    region(stack, sizeof stack, 3, 255);
    region(stack, sizeof stack, 0, sizeof stack);
    region(global_bytes, sizeof global_bytes, 7, 257);
    region(heap, 4099, 1, 4097);
    region(heap, 4099, 4099, 0);
    check(r13_dead_key(0) == 1 && r13_dead_key(1) == 2);
    free(heap);
    if (argc > 1) {
        /* One real span boundary, with a 64-bit size_t and exact full scan. */
        size_t n = (size_t)2147483647 + 65;
        unsigned char *wide = malloc(n + 2);
        check(wide != NULL && sizeof(size_t) == 8);
        region(wide, n + 2, 1, n);
        free(wide);
    }
    printf("secure-zero checks=%d wide=%d\n", checks, argc > 1);
    return 0;
}
