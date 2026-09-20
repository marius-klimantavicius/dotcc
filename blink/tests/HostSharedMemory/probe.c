#define _GNU_SOURCE 1
#include "HostMemory.h"
#include <errno.h>
#include <fcntl.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <sys/mman.h>
#include <sys/stat.h>
#include <sys/uio.h>
#include <unistd.h>
#ifdef BLINK_TEST_MANAGED
#include "host-io.h"
#endif
#ifdef BLINK_MANAGED_GUEST_THREADS
#include <pthread.h>
#endif
#if defined(BLINK_TEST_MANAGED) && !defined(BLINK_HOST_GUEST_THREADS_H)
#error Shared memory fixture must select the reviewed threaded pthread overlay
#endif

static void Require(int condition, int line) {
  if (!condition) { fprintf(stderr, "shared memory assertion line %d errno %d\n", line, errno); abort(); }
}
#define CHECK(x) Require(!!(x), __LINE__)

int SharedMemoryVector(struct iovec *vector, void *base) {
  vector->iov_base = base;
  vector->iov_len = 4096;
  return vector->iov_base == base && vector->iov_len == 4096;
}
int SharedMemoryLegacy(void) {
  unsigned char *p;
  struct iovec vector = {0};
  CHECK(!BlinkHostMemoryBegin(1048576));
  p = blink_host_mmap(0, 4096, PROT_READ | PROT_WRITE, MAP_PRIVATE | MAP_ANONYMOUS, -1, 0);
  CHECK(p != MAP_FAILED && BlinkHostMemoryContains(p, 4096));
  for (int i = 0; i < 4096; ++i) CHECK(!p[i]);
  CHECK(SharedMemoryVector(&vector, p));
  ((unsigned char *)vector.iov_base)[vector.iov_len - 4096] = 42;
  CHECK(!blink_host_mprotect(p, 4096, PROT_READ));
  CHECK(BlinkHostMemoryProtection(p) == PROT_READ && p[0] == 42);
  CHECK(!blink_host_mprotect(p, 4096, PROT_READ | PROT_WRITE));
  CHECK(!blink_host_munmap(p, 4096));
  CHECK(!BlinkHostMemoryBytes() && !BlinkHostMemoryMappings());
  CHECK(!BlinkHostMemoryEnd());
  CHECK(!BlinkHostMemoryBegin(1048576));
  p = blink_host_mmap(0, 4096, PROT_READ, MAP_PRIVATE | MAP_ANONYMOUS, -1, 0);
  CHECK(p != MAP_FAILED);
  BlinkHostMemoryDisposeWorker();
  CHECK(!BlinkHostMemoryLimit() && !BlinkHostMemoryBytes() && !BlinkHostMemoryMappings());
  puts("legacy zero/protect/unmap/end/dispose=pass");
  return 0;
}

#ifdef BLINK_MANAGED_GUEST_THREADS
static uint64_t test_token;
static uint64_t previous_token;
static int test_fd;
static unsigned char *test_pages[2];
static unsigned char *test_files[2];
static size_t test_peak[2];
static size_t previous_peak;
static pthread_mutex_t test_gate = PTHREAD_MUTEX_INITIALIZER;
static pthread_cond_t test_changed = PTHREAD_COND_INITIALIZER;
static int test_arrivals;
static int test_generation;

static void Rendezvous(void) {
  CHECK(!pthread_mutex_lock(&test_gate));
  int generation = test_generation;
  if (++test_arrivals == 2) {
    test_arrivals = 0;
    ++test_generation;
    CHECK(!pthread_cond_broadcast(&test_changed));
  } else {
    while (generation == test_generation) CHECK(!pthread_cond_wait(&test_changed, &test_gate));
  }
  CHECK(!pthread_mutex_unlock(&test_gate));
}
static ssize_t ReadAt(int fd, void *destination, size_t length, off_t offset) {
  return pread(fd, destination, length, offset);
}
static int ReadLength(int fd, off_t *length) {
#ifdef BLINK_TEST_MANAGED
  return blink_io_read_at_length(fd, length);
#else
  struct stat info;
  if (fstat(fd, &info)) return -1;
  *length = info.st_size;
  return 0;
#endif
}
int SharedMemoryStart(int fd) {
  size_t bytes;
  size_t mappings;
  size_t attachments;
  test_fd = fd;
  test_token = BlinkHostMemoryCreateShared(1048576);
  CHECK(test_token && test_token != previous_token);
  previous_token = test_token;
  CHECK(!BlinkHostMemorySharedSnapshot(test_token, &bytes, &mappings, &attachments));
  CHECK(bytes == BlinkHostMemorySharedOverhead() && !mappings && !attachments);
  CHECK(!BlinkHostMemoryAttach(test_token));
  CHECK(!BlinkHostMemorySetFileReader(ReadAt, ReadLength));
  CHECK(!BlinkHostMemoryDetach());
  return 0;
}
int SharedMemoryWorker(int id) {
  size_t bytes;
  size_t mappings;
  size_t attachments;
  int other = 1 - id;
  CHECK(id == 0 || id == 1);
  CHECK(!BlinkHostMemoryAttach(test_token));
  CHECK(BlinkHostMemoryLimit() == 1048576);
  test_pages[id] = blink_host_mmap(0, 8192, PROT_READ | PROT_WRITE,
                                  MAP_PRIVATE | MAP_ANONYMOUS, -1, 0);
  CHECK(test_pages[id] != MAP_FAILED);
  for (int i = 0; i < 8192; ++i) {
    CHECK(!test_pages[id][i]);
    test_pages[id][i] = (unsigned char)(id + 1);
  }
  test_files[id] = blink_host_mmap(0, 4096, PROT_READ, MAP_PRIVATE, test_fd, 4096);
  CHECK(test_files[id] != MAP_FAILED);
  for (int i = 0; i < 4096; ++i)
    CHECK(test_files[id][i] == (i < 1904 ? (unsigned char)((i + 4096) * 13 + 7) : 0));
  Rendezvous();
  CHECK(BlinkHostMemoryContains(test_pages[other], 8192));
  CHECK(BlinkHostMemoryContains(test_files[other], 4096));
  CHECK(!blink_host_mprotect(test_pages[other] + 4096, 4096, PROT_READ));
  Rendezvous();
  CHECK(BlinkHostMemoryProtection(test_pages[id] + 4096) == PROT_READ);
  CHECK(test_pages[other][4095] == other + 1 && test_pages[other][4096] == other + 1);
  CHECK(!blink_host_mprotect(test_pages[id] + 4096, 4096, PROT_READ | PROT_WRITE));
  Rendezvous();
  for (int i = 0; i < 64; ++i) {
    unsigned char *p = blink_host_mmap(0, 4096, PROT_READ | PROT_WRITE,
                                      MAP_PRIVATE | MAP_ANONYMOUS, -1, 0);
    CHECK(p != MAP_FAILED && !p[4095]);
    p[4095] = (unsigned char)(i + 1);
    CHECK(BlinkHostMemoryContains(p, 4096));
    CHECK(!blink_host_munmap(p, 4096));
  }
  Rendezvous();
  CHECK(!BlinkHostMemorySharedSnapshot(test_token, &bytes, &mappings, &attachments));
  CHECK(mappings == 4 && attachments == 2 && bytes > BlinkHostMemorySharedOverhead() + 24576);
  test_peak[id] = bytes;
  Rendezvous();
  CHECK(!blink_host_munmap(test_pages[id], 8192));
  CHECK(!blink_host_munmap(test_files[id], 4096));
  Rendezvous();
  CHECK(!BlinkHostMemorySharedSnapshot(test_token, &bytes, &mappings, &attachments));
  CHECK(!mappings && bytes == BlinkHostMemorySharedOverhead() && attachments == 2);
  Rendezvous();
  CHECK(!BlinkHostMemoryDetach());
  return 0;
}
int SharedMemoryFinish(int cycle) {
  size_t bytes;
  size_t mappings;
  size_t attachments;
  CHECK(!BlinkHostMemorySharedSnapshot(test_token, &bytes, &mappings, &attachments));
  CHECK(!mappings && !attachments && bytes == BlinkHostMemorySharedOverhead());
  CHECK(test_peak[0] == test_peak[1]);
  if (cycle) CHECK(test_peak[0] == previous_peak);
  previous_peak = test_peak[0];
  CHECK(!BlinkHostMemoryDestroyShared(test_token));
  printf("cycle=%d workers=2 live_maps=4 payload=24576 transient_maps=128 drained_maps=0 detached=2 file_tail=1904 zero_tail=2192\n", cycle);
  printf("abi context_registry=%zu mapping_records=%zu\n", BlinkHostMemorySharedOverhead(),
         previous_peak - BlinkHostMemorySharedOverhead() - 24576);
  return 0;
}
int SharedMemoryTestEnd(void) {
  CHECK(!pthread_cond_destroy(&test_changed));
  CHECK(!pthread_mutex_destroy(&test_gate));
  return 0;
}
#endif

#ifndef BLINK_TEST_MANAGED
#ifdef BLINK_MANAGED_GUEST_THREADS
static void *NativeWorker(void *argument) {
  int id = *(int *)argument;
  CHECK(!SharedMemoryWorker(id));
  return 0;
}
#endif
int main(void) {
  CHECK(!SharedMemoryLegacy());
#ifdef BLINK_MANAGED_GUEST_THREADS
  int fd = open("seed.bin", O_RDONLY);
  CHECK(fd >= 0 && lseek(fd, 17, SEEK_SET) == 17);
  for (int cycle = 0; cycle < 2; ++cycle) {
    pthread_t workers[2];
    int ids[2] = {0, 1};
    CHECK(!SharedMemoryStart(fd));
    CHECK(!pthread_create(&workers[0], 0, NativeWorker, &ids[0]));
    CHECK(!pthread_create(&workers[1], 0, NativeWorker, &ids[1]));
    CHECK(!pthread_join(workers[0], 0));
    CHECK(!pthread_join(workers[1], 0));
    CHECK(!SharedMemoryFinish(cycle));
    CHECK(lseek(fd, 0, SEEK_CUR) == 17);
  }
  CHECK(!close(fd));
  CHECK(!SharedMemoryTestEnd());
  puts("file_cursor=17 cleanup=joined-detached-destroyed");
#endif
  return 0;
}
#endif
