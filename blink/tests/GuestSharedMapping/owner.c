#define _POSIX_C_SOURCE 200809L
#define _DEFAULT_SOURCE 1
#include "HostMemory.h"
#include <assert.h>
#include <errno.h>
#include <stdlib.h>
#include <sys/mman.h>
#include <string.h>

static void *page;
static unsigned leases;
static uint64_t sequence;
static int available = 1;
static size_t accounted, budget_limit, external_bytes;
static unsigned char image[4100];
static int BindBudget(size_t limit, size_t bytes, uint64_t *token) {
  budget_limit = limit; accounted = bytes; *token = 1; return 0;
}
static int Account(uint64_t token, size_t bytes) {
  assert(token == 1);
  if (bytes > budget_limit - external_bytes) { errno = ENOMEM; return -1; }
  accounted = bytes; return 0;
}
static int ReleaseBudget(uint64_t token) { assert(token == 1); accounted = 0; return 0; }
static int ReadOnly(int fd) {
  if (fd == 7) return 0;
  if (fd == 9) return 1;
  errno = ENOTSUP; return -1;
}
static int Length(int fd, off_t *length) {
  assert(fd == 9); *length = sizeof(image); return 0;
}
static ssize_t ReadAt(int fd, void *address, size_t length, off_t offset) {
  assert(fd == 9 && offset >= 0 && offset + length <= sizeof(image));
  memcpy(address, image + offset, length); return length;
}
static int Acquire(int fd, off_t offset, void **address, uint64_t *token) {
  if (fd != 7 || offset) { errno = ENOTSUP; return -1; }
  if (!page) {
    int error = posix_memalign(&page, 4096, 4096);
    if (error) { errno = error; return -1; }
    memset(page, 0, 4096);
  }
  *address = page; *token = ++sequence; ++leases;
  return 0;
}
static int Release(uint64_t token) { assert(token && leases); --leases; return 0; }
static int Available(uint64_t token) { assert(token); return available; }
int main(void) {
  assert(!BlinkHostMemoryBegin(65536));
  assert(!BlinkHostMemorySetSharedFiles(Acquire, Release, Available, ReadOnly));
  assert(!BlinkHostMemorySetFileReader(ReadAt, Length));
  assert(!BlinkHostMemorySetFileBudget(BindBudget, Account, ReleaseBudget));
  external_bytes = 62000;
  assert(blink_host_mmap(0, 4096, 3, MAP_PRIVATE | MAP_ANONYMOUS, -1, 0) == MAP_FAILED && errno == ENOMEM);
  assert(accounted == 0 && BlinkHostMemoryBytes() == 0);
  external_bytes = 0;
  unsigned char *a = blink_host_mmap(0, 4096, PROT_READ | PROT_WRITE, MAP_SHARED, 7, 0);
  unsigned char *b = blink_host_mmap(0, 4096, PROT_READ, MAP_SHARED, 7, 0);
  assert(a != MAP_FAILED && b == a && leases == 2);
  a[31] = 73; assert(b[31] == 73);
  assert(!blink_host_mprotect(a, 4096, PROT_NONE));
  assert(!BlinkHostMemoryProtection(a));
  available = 0; assert(!BlinkHostMemoryFilePageAvailable(a));
  assert(!blink_host_mprotect(a, 4096, PROT_READ));
  assert(!BlinkHostMemoryFilePageAvailable(a));
  available = 1; assert(BlinkHostMemoryFilePageAvailable(a));
  assert(blink_host_mmap(0, 4096, PROT_READ, MAP_SHARED, 8, 0) == MAP_FAILED && errno == ENOTSUP);
  assert(leases == 2 && BlinkHostMemoryMappings() == 2);
  assert(!blink_host_munmap(a, 4096) && leases == 1 && b[31] == 73);
  assert(!blink_host_munmap(b, 4096) && !leases);
  assert(!BlinkHostMemoryBytes() && !BlinkHostMemoryMappings());
  assert(!accounted);
  image[17] = 99;
  unsigned char *readonly = blink_host_mmap(0, 4096, PROT_READ, MAP_SHARED, 9, 0);
  assert(readonly != MAP_FAILED && readonly[17] == 99 && !BlinkHostMemoryFilePageWritable(readonly));
  assert(blink_host_mprotect(readonly, 4096, PROT_READ | PROT_WRITE) == -1 && errno == EACCES);
  assert(BlinkHostMemoryProtection(readonly) == PROT_READ);
  assert(blink_host_mmap(0, 4096, PROT_READ | PROT_WRITE, MAP_SHARED, 9, 0) == MAP_FAILED && errno == EACCES);
  assert(!blink_host_munmap(readonly, 4096));
  readonly = blink_host_mmap(0, 4096, PROT_READ | PROT_WRITE, MAP_PRIVATE, 9, 0);
  assert(readonly != MAP_FAILED && BlinkHostMemoryFilePageWritable(readonly));
  readonly[17] = 100; assert(image[17] == 99);
  assert(!blink_host_munmap(readonly, 4096));
  assert(blink_host_mmap(0, 4096, PROT_READ, MAP_SHARED, 7, 0) == a);
  BlinkHostMemoryDisposeWorker(); assert(!leases && !BlinkHostMemoryLimit());
#ifdef BLINK_MANAGED_GUEST_THREADS
  uint64_t owner = BlinkHostMemoryCreateShared(65536);
  assert(owner && !BlinkHostMemoryAttach(owner));
  assert(!BlinkHostMemorySetSharedFiles(Acquire, Release, Available, ReadOnly));
  assert(!BlinkHostMemorySetFileBudget(BindBudget, Account, ReleaseBudget));
  assert(blink_host_mmap(0, 4096, PROT_READ, MAP_SHARED, 7, 0) == a);
  assert(!BlinkHostMemoryDetach());
  assert(!BlinkHostMemoryDestroyShared(owner) && !leases && !accounted);
#endif
  free(page);
  return 0;
}
