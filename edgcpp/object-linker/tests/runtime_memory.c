#include <stdlib.h>
#include <string.h>
#include <errno.h>

int main(void) {
    char *p = malloc(32);
    if (!p) return 1;
    memset(p, 0, 32);
    strcpy(p, "shared runtime");
    if (strlen(p) != 14 || strcmp(p, "shared runtime")) return 2;
    char *q = calloc(2, 32);
    if (!q) return 3;
    memcpy(q, p, 15);
    if (memcmp(p, q, 15)) return 4;
    p = realloc(p, 64);
    if (!p || strcmp(p, q)) return 5;
    free(p);
    free(q);
    free(0);
    void *aligned = aligned_alloc(64, 128);
    if (!aligned || ((size_t)aligned & 63)) return 6;
    void *sentinel = aligned;
    if (posix_memalign(&sentinel, 3, 20) != EINVAL || sentinel != aligned) return 7;
    free(aligned);
    void *other = 0;
    if (posix_memalign(&other, 64, 73) || ((size_t)other & 63)) return 8;
    free(other);
    errno = 0;
    volatile size_t too_large = (size_t)-1;
    if (calloc(too_large, 2) || errno != ENOMEM) return 9;
    return 0;
}
