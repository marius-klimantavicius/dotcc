/* Reduced from ptls_buffer_reserve_aligned; native C allocates aligned storage. */
#define _POSIX_C_SOURCE 200112L
#include <stdlib.h>
#include <stdint.h>
int main(void)
{
    void *p = 0;
    if (posix_memalign(&p, 64, 1024) != 0)
        return 1;
    int misaligned = ((uintptr_t)p & 63) != 0;
    free(p);
    return misaligned;
}
