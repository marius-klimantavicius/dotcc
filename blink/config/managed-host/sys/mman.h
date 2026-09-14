#ifndef BLINK_MANAGED_HOST_SYS_MMAN_H
#define BLINK_MANAGED_HOST_SYS_MMAN_H
#include <stddef.h>
#include <sys/types.h>
/* Reviewed Linux x64 constants; these do not grant host OS mmap capabilities. */
#define PROT_NONE 0
#define PROT_READ 1
#define PROT_WRITE 2
#define PROT_EXEC 4
#define MAP_SHARED 1
#define MAP_PRIVATE 2
#define MAP_FIXED 16
#define MAP_ANONYMOUS 32
#define MAP_FAILED ((void *)-1)
#define MS_ASYNC 1
#define MS_INVALIDATE 2
#define MS_SYNC 4

#define mmap blink_host_mmap
#define munmap blink_host_munmap
#define mprotect blink_host_mprotect
#define msync blink_host_msync
void *mmap(void *, size_t, int, int, int, off_t);
int munmap(void *, size_t);
int mprotect(void *, size_t, int);
int msync(void *, size_t, int);
long BlinkHostMemoryPageSize(void);
int BlinkHostMemoryAddressBits(void);
#endif
