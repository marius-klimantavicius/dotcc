#define _GNU_SOURCE
#include <errno.h>
#include <fcntl.h>
#include <pthread.h>
#include <stdint.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <sys/mman.h>
#include <sys/resource.h>
#include <unistd.h>

/* Ordinary libc stack discovery and mapping lifecycle. No injected faults,
 * malformed images, host /proc forwarding, or fixed-address assumptions. */
static char maps[1 << 20];
static int load_maps(void) {
  int fd = open("/proc/self/maps", O_RDONLY | O_CLOEXEC);
  if (fd < 0) return 1;
  size_t used = 0;
  for (;;) {
    ssize_t n = read(fd, maps + used, sizeof(maps) - 1 - used);
    if (n < 0) { close(fd); return 2; }
    if (!n) break;
    used += (size_t)n;
    if (used == sizeof(maps) - 1) { close(fd); return 3; }
  }
  maps[used] = 0;
  return close(fd) != 0;
}
static int mapping(uintptr_t target, const char *mode, int absent) {
  uintptr_t previous = 0;
  int found = 0;
  for (const char *line = maps; *line;) {
    unsigned long start, end;
    char permissions[5];
    if (sscanf(line, "%lx-%lx %4s", &start, &end, permissions) != 3 ||
        start >= end || start < previous) return 1;
    previous = end;
    if (target >= start && target < end) {
      if (mode && strcmp(mode, permissions)) return 2;
      ++found;
    }
    const char *next = strchr(line, '\n');
    if (!next) return 3;
    line = next + 1;
  }
  return absent ? found != 0 : found != 1;
}
static int query_stack(void) {
  pthread_attr_t attr;
  void *base;
  size_t size;
  int local;
  if (pthread_getattr_np(pthread_self(), &attr)) return 1;
  int status = pthread_attr_getstack(&attr, &base, &size);
  int destroyed = pthread_attr_destroy(&attr);
  if (status || destroyed || size < 131072 || (uintptr_t)&local < (uintptr_t)base ||
      (uintptr_t)&local - (uintptr_t)base >= size) return 2;
  if (load_maps() || mapping((uintptr_t)&local, "rw-p", 0)) return 3;
  return 0;
}
static void *worker(void *unused) {
  (void)unused;
  return (void *)(uintptr_t)query_stack();
}
int main(void) {
  struct rlimit stack;
  if (getrlimit(RLIMIT_STACK, &stack) || stack.rlim_cur < 131072) return 10;
  if (query_stack()) return 11;
  pthread_t thread;
  void *result;
  if (pthread_create(&thread, 0, worker, 0) || pthread_join(thread, &result) || result) return 12;
  unsigned char *area = mmap(0, 12288, PROT_READ | PROT_WRITE,
                            MAP_PRIVATE | MAP_ANONYMOUS, -1, 0);
  if (area == MAP_FAILED) return 13;
  if (load_maps() || mapping((uintptr_t)area, "rw-p", 0) ||
      mapping((uintptr_t)area + 8192, "rw-p", 0)) return 14;
  if (mprotect(area + 4096, 4096, PROT_NONE) || mprotect(area + 8192, 4096, PROT_READ)) return 15;
  if (load_maps() || mapping((uintptr_t)area, "rw-p", 0) ||
      mapping((uintptr_t)area + 4096, "---p", 0) ||
      mapping((uintptr_t)area + 8192, "r--p", 0) ||
      mapping((uintptr_t)main, "r-xp", 0)) return 16;
  if (munmap(area + 4096, 4096)) return 17;
  if (load_maps() || mapping((uintptr_t)area + 4096, 0, 1)) return 18;
  int fd = memfd_create("ordinary-guest-maps", MFD_CLOEXEC);
  if (fd < 0 || ftruncate(fd, 4096)) return 19;
  void *shared = mmap(0, 4096, PROT_NONE, MAP_SHARED, fd, 0);
  if (shared == MAP_FAILED || load_maps() || mapping((uintptr_t)shared, "---s", 0)) return 20;
  if (mprotect(shared, 4096, PROT_READ | PROT_WRITE) || close(fd)) return 21;
  *(volatile unsigned char *)shared = 7;
  if (load_maps() || mapping((uintptr_t)shared, "rw-s", 0)) return 22;
  if (munmap(shared, 4096) || munmap(area, 4096) || munmap(area + 8192, 4096)) return 23;
  puts("guest-process-maps: main/worker stacks, rwx/none, private/shared, unmap: PASS");
  return 0;
}
