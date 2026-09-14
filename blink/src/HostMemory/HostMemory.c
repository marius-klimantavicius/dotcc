#define _POSIX_C_SOURCE 200809L
#define _DEFAULT_SOURCE 1
#include "HostMemory.h"
#include <errno.h>
#include <stdint.h>
#include <stdlib.h>
#include <string.h>
#include <sys/mman.h>

struct OwnedMapping {
  struct OwnedMapping *next;
  void *address;
  size_t length;
};
struct MemoryOwner {
  struct OwnedMapping *head;
  size_t limit;
  size_t bytes;
  size_t mappings;
};
static _Thread_local struct MemoryOwner owner;

/* This is the reviewed non-linear guest-layout policy, not a measured native
 * page size or a claim about the range of actual host allocation addresses. */
long BlinkHostMemoryPageSize(void) { return 4096; }
int BlinkHostMemoryAddressBits(void) { return 47; }

static int Fail(int error) { errno = error; return -1; }
static size_t RoundLength(size_t length) {
  if (!length || length > SIZE_MAX - 4095) return 0;
  return (length + 4095) & ~(size_t)4095;
}

int BlinkHostMemoryBegin(size_t limit) {
  if (owner.limit) return Fail(EBUSY);
  /* The shared C library currently has int-sized malloc/memset lengths. This
   * campaign limit makes every conversion lossless, including metadata. */
  if (limit < 4096 + sizeof(struct OwnedMapping) || limit > 256 * 1024 * 1024)
    return Fail(EINVAL);
  owner.limit = limit;
  return 0;
}
size_t BlinkHostMemoryBytes(void) { return owner.bytes; }
size_t BlinkHostMemoryMappings(void) { return owner.mappings; }
int BlinkHostMemoryEnd(void) {
  if (!owner.limit) return Fail(ENODEV);
  if (owner.head) return Fail(EBUSY);
  owner.limit = 0;
  return 0;
}

void *blink_host_mmap(void *address, size_t length, int prot, int flags,
                      int fd, off_t offset) {
  struct OwnedMapping *record;
  void *allocation;
  int error;
  size_t rounded;
  if (!owner.limit) { Fail(ENODEV); return MAP_FAILED; }
  if (!(rounded = RoundLength(length))) { Fail(EINVAL); return MAP_FAILED; }
  if (address || flags != (MAP_PRIVATE | MAP_ANONYMOUS) ||
      prot != (PROT_READ | PROT_WRITE) || fd != -1 || offset != 0) {
    Fail(ENOTSUP);
    return MAP_FAILED;
  }
  if (rounded > owner.limit - owner.bytes ||
      sizeof(struct OwnedMapping) > owner.limit - owner.bytes - rounded) {
    Fail(ENOMEM);
    return MAP_FAILED;
  }
  if (!(record = malloc(sizeof(*record)))) { Fail(ENOMEM); return MAP_FAILED; }
  error = posix_memalign(&allocation, 4096, rounded);
  if (error) { free(record); Fail(error); return MAP_FAILED; }
  memset(allocation, 0, rounded);
  record->address = allocation;
  record->length = rounded;
  record->next = owner.head;
  owner.head = record;
  owner.bytes += rounded + sizeof(*record);
  ++owner.mappings;
  return allocation;
}

int blink_host_munmap(void *address, size_t length) {
  struct OwnedMapping *previous = 0;
  struct OwnedMapping *record = owner.head;
  size_t rounded = RoundLength(length);
  if (!owner.limit) return Fail(ENODEV);
  if (!address || !rounded) return Fail(EINVAL);
  while (record) {
    if (record->address == address) {
      if (record->length != rounded) return Fail(EINVAL);
      if (previous) previous->next = record->next;
      else owner.head = record->next;
      owner.bytes -= record->length + sizeof(*record);
      --owner.mappings;
      free(record->address);
      free(record);
      return 0;
    }
    previous = record;
    record = record->next;
  }
  return Fail(EINVAL);
}

int blink_host_mprotect(void *address, size_t length, int prot) {
  (void)address; (void)length; (void)prot;
  return Fail(ENOTSUP);
}
int blink_host_msync(void *address, size_t length, int flags) {
  (void)address; (void)length; (void)flags;
  return Fail(ENOTSUP);
}
void BlinkHostMemoryDisposeWorker(void) {
  struct OwnedMapping *record;
  while ((record = owner.head)) {
    owner.head = record->next;
    free(record->address);
    free(record);
  }
  owner.bytes = 0;
  owner.mappings = 0;
  owner.limit = 0;
}
