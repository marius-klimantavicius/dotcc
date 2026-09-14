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
  BlinkHostMemoryReadAt read_at;
  BlinkHostMemoryReadLength read_length;
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
int BlinkHostMemorySetFileReader(BlinkHostMemoryReadAt read_at,
                                  BlinkHostMemoryReadLength read_length) {
  if (!owner.limit) return Fail(ENODEV);
  if (!read_at || !read_length) return Fail(EINVAL);
  owner.read_at = read_at;
  owner.read_length = read_length;
  return 0;
}
int BlinkHostMemoryContains(const void *pointer, size_t length) {
  uintptr_t start = (uintptr_t)pointer;
  struct OwnedMapping *record = owner.head;
  if (!owner.limit || !start || !length || length > UINTPTR_MAX - start) return 0;
  while (record) {
    uintptr_t base = (uintptr_t)record->address;
    if (start >= base && length <= record->length &&
        start - base <= record->length - length) return 1;
    record = record->next;
  }
  return 0;
}
int BlinkHostMemoryEnd(void) {
  if (!owner.limit) return Fail(ENODEV);
  if (owner.head) return Fail(EBUSY);
  owner.limit = 0;
  owner.read_at = 0;
  owner.read_length = 0;
  return 0;
}

void *blink_host_mmap(void *address, size_t length, int prot, int flags,
                      int fd, off_t offset) {
  struct OwnedMapping *record;
  void *allocation;
  int error;
  size_t rounded;
  size_t file_bytes = 0;
  if (!owner.limit) { Fail(ENODEV); return MAP_FAILED; }
  if (!(rounded = RoundLength(length))) { Fail(EINVAL); return MAP_FAILED; }
  if (address || prot != (PROT_READ | PROT_WRITE)) {
    Fail(ENOTSUP);
    return MAP_FAILED;
  }
  if (flags == (MAP_PRIVATE | MAP_ANONYMOUS)) {
    if (fd != -1 || offset != 0) { Fail(ENOTSUP); return MAP_FAILED; }
  } else if (flags == MAP_PRIVATE) {
    off_t file_length;
    if (!owner.read_at || !owner.read_length) { Fail(ENOTSUP); return MAP_FAILED; }
    if (offset < 0 || (offset & 4095)) { Fail(EINVAL); return MAP_FAILED; }
    if (owner.read_length(fd, &file_length)) return MAP_FAILED;
    if (file_length < 0) { Fail(EIO); return MAP_FAILED; }
    /* Only the existing file pages are supported. Whole pages at/beyond EOF
     * require SIGBUS on access; silently making them readable would be wrong. */
    if (offset >= file_length) { Fail(ENOTSUP); return MAP_FAILED; }
    file_bytes = (size_t)(file_length - offset);
    if (rounded > file_bytes && rounded - file_bytes >= 4096) {
      Fail(ENOTSUP); return MAP_FAILED;
    }
    if (file_bytes > rounded) file_bytes = rounded;
  } else { Fail(ENOTSUP); return MAP_FAILED; }
  if (rounded > owner.limit - owner.bytes ||
      sizeof(struct OwnedMapping) > owner.limit - owner.bytes - rounded) {
    Fail(ENOMEM);
    return MAP_FAILED;
  }
  if (!(record = malloc(sizeof(*record)))) { Fail(ENOMEM); return MAP_FAILED; }
  error = posix_memalign(&allocation, 4096, rounded);
  if (error) { free(record); Fail(error); return MAP_FAILED; }
  memset(allocation, 0, rounded);
  for (size_t copied = 0; copied < file_bytes;) {
    size_t request = file_bytes - copied;
    if (request > 65536) request = 65536;
    ssize_t count = owner.read_at(fd, (unsigned char *)allocation + copied,
                                 request, offset + copied);
    if (count <= 0 || (size_t)count > request) {
      error = count < 0 ? errno : EIO;
      free(allocation);
      free(record);
      Fail(error);
      return MAP_FAILED;
    }
    copied += count;
  }
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
  owner.read_at = 0;
  owner.read_length = 0;
}
