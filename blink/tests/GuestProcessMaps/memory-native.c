#define _GNU_SOURCE
#include "HostMemory.h"
#include <assert.h>
#include <errno.h>
#include <stdio.h>
#include <string.h>
#include <sys/mman.h>
#include <stdlib.h>
static unsigned char *shared_page;
static int released;
static int readonly_file(int fd) { return fd == 8; }
static ssize_t read_at(int fd, void *p, size_t n, off_t offset) {
  (void)fd; (void)offset; memset(p, 'x', n); return n;
}
static int read_length(int fd, off_t *size) { (void)fd; *size = 4096; return 0; }
static int acquire(int fd, off_t offset, void **p, uint64_t *lease) {
  assert(fd == 7 && offset == 0); *p = shared_page; *lease = 17; return 0;
}
static int release(uint64_t lease) { assert(lease == 17); ++released; return 0; }
static int available(uint64_t lease) { assert(lease == 17); return 1; }
int main(void) {
  uint64_t owner = BlinkHostMemoryCreateShared(1 << 20);
  assert(owner && !BlinkHostMemoryAttach(owner));
  assert(!posix_memalign((void **)&shared_page, 4096, 4096));
  assert(!BlinkHostMemorySetFileReader(read_at, read_length));
  assert(!BlinkHostMemorySetSharedFiles(acquire, release, available, readonly_file));
  void *private = blink_host_mmap(0, 8192, PROT_NONE, MAP_PRIVATE | MAP_ANONYMOUS, -1, 0);
  void *mutable = blink_host_mmap(0, 4096, PROT_NONE, MAP_SHARED, 7, 0);
  void *immutable = blink_host_mmap(0, 4096, PROT_READ, MAP_SHARED, 8, 0);
  assert(private != MAP_FAILED && mutable != MAP_FAILED && immutable != MAP_FAILED);
  size_t bytes = BlinkHostMemoryBytes(), count = BlinkHostMemoryMappings();
  errno = 123;
  assert(!BlinkHostMemoryFilePageShared(private));
  assert(!BlinkHostMemoryFilePageShared((unsigned char *)private + 4096));
  assert(BlinkHostMemoryFilePageShared(mutable));
  assert(BlinkHostMemoryFilePageShared((unsigned char *)mutable + 4095));
  assert(BlinkHostMemoryFilePageShared(immutable));
  assert(!BlinkHostMemoryFilePageShared(&owner) && !BlinkHostMemoryFilePageShared(0));
  assert(errno == 123 && BlinkHostMemoryBytes() == bytes && BlinkHostMemoryMappings() == count);
  assert(!blink_host_mprotect(mutable, 4096, PROT_READ | PROT_WRITE));
  assert(BlinkHostMemoryFilePageShared(mutable));
  assert(!blink_host_munmap(private, 8192) && !BlinkHostMemoryFilePageShared(private));
  assert(!blink_host_munmap(mutable, 4096) && !blink_host_munmap(immutable, 4096));
  assert(released == 1 && BlinkHostMemoryMappings() == 0);
  assert(!BlinkHostMemoryDetach() && !BlinkHostMemoryDestroyShared(owner));
  free(shared_page);
  puts("process-maps native metadata: private/mutable-shared/immutable-shared, errno, accounting, cleanup: PASS");
}
