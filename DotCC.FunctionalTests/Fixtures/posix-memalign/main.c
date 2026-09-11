#include <stdlib.h>
#include <stdint.h>
#include <errno.h>
#include <stdio.h>

int main(void)
{
    void *p = NULL;
    size_t alignment = 4096;
    size_t size = 37;
    errno = ERANGE;
    int result = posix_memalign(&p, alignment, size);
    printf("allocated=%d aligned=%d errno=%d\n", result == 0,
           result == 0 && (uintptr_t)p % alignment == 0, errno == ERANGE);
    if (result != 0) return 1;
    unsigned char *bytes = p;
    bytes[0] = 19;
    bytes[36] = 83;
    unsigned char *resized = realloc(p, 73);
    if (resized == NULL) { free(p); return 2; }
    printf("contents=%d\n", resized[0] == 19 && resized[36] == 83);
    free(resized);

    p = (void *)1234;
    result = posix_memalign(&p, 24, 37);
    printf("invalid=%d retained=%d errno=%d\n", result == EINVAL,
           p == (void *)1234, errno == ERANGE);
    /* Alignment and size retain their upper bits through the C ABI. */
    result = posix_memalign(&p, 4294967304UL, 37);
    printf("wide-alignment=%d\n", result == EINVAL && p == (void *)1234);
    result = posix_memalign(&p, 64, (size_t)-1);
    /* glibc may change errno on this malloc failure. The runtime unit tests
       separately require dotcc to preserve it. */
    printf("overflow=%d retained=%d\n", result == ENOMEM, p == (void *)1234);
    result = posix_memalign(&p, 64, 0);
    printf("zero=%d\n", result == 0 && (p == NULL || (uintptr_t)p % 64 == 0));
    if (result == 0) free(p);
    return 0;
}
