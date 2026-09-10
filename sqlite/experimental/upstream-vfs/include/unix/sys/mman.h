#ifndef DOTCC_UPSTREAM_MMAN_H
#define DOTCC_UPSTREAM_MMAN_H
#include <stddef.h>
#include <sys/types.h>
#define PROT_READ 1
#define PROT_WRITE 2
#define MAP_SHARED 1
#define MAP_FAILED ((void*)-1)
void *uvfs_mmap(void*, size_t, int, int, int, off_t);
int uvfs_munmap(void*, size_t);
#define mmap uvfs_mmap
#define munmap uvfs_munmap
#endif
