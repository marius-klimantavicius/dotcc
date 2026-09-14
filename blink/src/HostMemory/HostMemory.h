#ifndef BLINK_HOST_MEMORY_H
#define BLINK_HOST_MEMORY_H
#include <stddef.h>
#include <sys/types.h>

/* One active allocation owner per worker. The limit includes rounded payload
 * and ownership record storage. No persistent CLR references live in C memory. */
int BlinkHostMemoryBegin(size_t);
size_t BlinkHostMemoryBytes(void);
size_t BlinkHostMemoryMappings(void);
int BlinkHostMemoryEnd(void);
/* Only after the entire upstream worker state and its slab-cache references
 * are discarded. This is deliberately distinct from FreeSystem(). */
void BlinkHostMemoryDisposeWorker(void);
long BlinkHostMemoryPageSize(void);
int BlinkHostMemoryAddressBits(void);
void *blink_host_mmap(void *, size_t, int, int, int, off_t);
int blink_host_munmap(void *, size_t);
int blink_host_mprotect(void *, size_t, int);
int blink_host_msync(void *, size_t, int);
#endif
