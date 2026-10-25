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
  BlinkHostMemoryAcquireShared acquire_shared;
  BlinkHostMemoryReleaseShared release_shared;
  BlinkHostMemorySharedAvailable shared_available;
  BlinkHostMemorySharedReadOnly shared_readonly;
  uint64_t file_budget;
  BlinkHostMemoryAccountBudget account_budget;
  BlinkHostMemoryReleaseBudget release_budget;
};
#ifdef BLINK_MANAGED_GUEST_THREADS
#include <pthread.h>
struct SharedMemoryOwner {
  struct SharedMemoryOwner *next;
  uint64_t token;
  pthread_t creator;
  size_t attachments;
  struct MemoryOwner memory;
};
static pthread_mutex_t shared_memory_gate = PTHREAD_MUTEX_INITIALIZER;
static struct SharedMemoryOwner *shared_memory_owners;
static uint64_t shared_memory_next_token;
static _Thread_local struct MemoryOwner legacy_memory_owner;
static _Thread_local struct SharedMemoryOwner *shared_memory_owner;
#define owner (*(shared_memory_owner ? &shared_memory_owner->memory : &legacy_memory_owner))
/* Keep the allocation algorithms common. Public shared-profile entrypoints below
 * serialize complete operations, including synchronous file callbacks. */
#define MEMORY_LOCAL static
#define BlinkHostMemoryProtection MemoryImplProtection
#define BlinkHostMemoryFilePageShared MemoryImplFilePageShared
#define BlinkHostMemoryFilePageAvailable MemoryImplFilePageAvailable
#define BlinkHostMemoryFilePageWritable MemoryImplFilePageWritable
#define BlinkHostMemoryBegin MemoryImplBegin
#define BlinkHostMemoryLimit MemoryImplLimit
#define BlinkHostMemoryBytes MemoryImplBytes
#define BlinkHostMemoryMappings MemoryImplMappings
#define BlinkHostMemorySetFileReader MemoryImplSetFileReader
#define BlinkHostMemorySetSharedFiles MemoryImplSetSharedFiles
#define BlinkHostMemorySetFileBudget MemoryImplSetFileBudget
#define BlinkHostMemoryContains MemoryImplContains
#define BlinkHostMemoryEnd MemoryImplEnd
#define BlinkHostMemoryDisposeWorker MemoryImplDisposeWorker
#define blink_host_mmap MemoryImplMmap
#define blink_host_munmap MemoryImplMunmap
#define blink_host_mprotect MemoryImplMprotect
#else
#define MEMORY_LOCAL
static _Thread_local struct MemoryOwner owner;
#endif

/* This is the reviewed non-linear guest-layout policy, not a measured native
 * page size or a claim about the range of actual host allocation addresses. */
long BlinkHostMemoryPageSize(void) { return 4096; }
int BlinkHostMemoryAddressBits(void) { return 47; }

static int Fail(int error) { errno = error; return -1; }
static int AccountBudget(struct MemoryOwner *memory, size_t bytes) {
  return memory->file_budget ? memory->account_budget(memory->file_budget, bytes) : 0;
}
static int ReleaseBudget(struct MemoryOwner *memory) {
  if (memory->file_budget && memory->release_budget(memory->file_budget)) return -1;
  memory->file_budget = 0;
  memory->account_budget = 0;
  memory->release_budget = 0;
  return 0;
}
static size_t RoundLength(size_t length) {
  if (!length || length > SIZE_MAX - 4095) return 0;
  return (length + 4095) & ~(size_t)4095;
}

static size_t MetadataSize(size_t rounded) {
  return sizeof(struct OwnedMapping) + rounded / 4096;
}
static unsigned char *Protections(struct OwnedMapping *record) {
  return (unsigned char *)(record + 1);
}
/* Kept alongside protection metadata, but never cleared by mprotect. Host
 * storage stays RW/NX; the guest page walk rejects these absent file pages. */
#define FILE_PAGE_ABSENT 128
#define FILE_PAGE_SHARED 64
#define FILE_PAGE_READONLY_SHARED 32
#define FILE_PAGE_METADATA (FILE_PAGE_ABSENT | FILE_PAGE_SHARED | FILE_PAGE_READONLY_SHARED)
/* Preserve the existing private record size/accounting. Only shared records
 * carry a lease after their protection bytes; memcpy handles its alignment. */
static uint64_t SharedLease(struct OwnedMapping *record) {
  uint64_t lease = 0;
  if (Protections(record)[0] & FILE_PAGE_SHARED)
    memcpy(&lease, Protections(record) + record->length / 4096, sizeof(lease));
  return lease;
}
static size_t MappingMetadataSize(struct OwnedMapping *record) {
  return MetadataSize(record->length) +
         ((Protections(record)[0] & FILE_PAGE_SHARED) ? sizeof(uint64_t) : 0);
}
static int ValidProtection(int prot) {
  if (prot & ~(PROT_READ | PROT_WRITE | PROT_EXEC)) return Fail(EINVAL);
  if (prot & PROT_EXEC) return Fail(ENOTSUP);
  return 0;
}
MEMORY_LOCAL int BlinkHostMemoryProtection(const void *pointer) {
  uintptr_t address = (uintptr_t)pointer;
  struct OwnedMapping *record = owner.head;
  if (!owner.limit || !address) return -1;
  while (record) {
    uintptr_t base = (uintptr_t)record->address;
    if (address >= base && address - base < record->length)
      return Protections(record)[(address - base) / 4096] & ~FILE_PAGE_METADATA;
    record = record->next;
  }
  return -1;
}
MEMORY_LOCAL int BlinkHostMemoryFilePageShared(const void *pointer) {
  uintptr_t address = (uintptr_t)pointer;
  struct OwnedMapping *record = owner.head;
  while (record) {
    uintptr_t base = (uintptr_t)record->address;
    if (address >= base && address - base < record->length)
      return !!(Protections(record)[(address - base) / 4096] &
                (FILE_PAGE_SHARED | FILE_PAGE_READONLY_SHARED));
    record = record->next;
  }
  return 0;
}
MEMORY_LOCAL int BlinkHostMemoryFilePageAvailable(const void *pointer) {
  uintptr_t address = (uintptr_t)pointer;
  struct OwnedMapping *record = owner.head;
  while (record) {
    uintptr_t base = (uintptr_t)record->address;
    if (address >= base && address - base < record->length) {
      if (Protections(record)[0] & FILE_PAGE_SHARED)
        return owner.shared_available(SharedLease(record));
      return !(Protections(record)[(address - base) / 4096] & FILE_PAGE_ABSENT);
    }
    record = record->next;
  }
  return 0;
}
MEMORY_LOCAL int BlinkHostMemoryFilePageWritable(const void *pointer) {
  uintptr_t address = (uintptr_t)pointer;
  struct OwnedMapping *record = owner.head;
  while (record) {
    uintptr_t base = (uintptr_t)record->address;
    if (address >= base && address - base < record->length)
      return !(Protections(record)[(address - base) / 4096] & FILE_PAGE_READONLY_SHARED);
    record = record->next;
  }
  return 0;
}

MEMORY_LOCAL int BlinkHostMemoryBegin(size_t limit) {
  if (owner.limit) return Fail(EBUSY);
  /* The shared C library currently has int-sized malloc/memset lengths. This
   * campaign limit makes every conversion lossless, including metadata. */
  if (limit < 4096 + MetadataSize(4096) || limit > 256 * 1024 * 1024)
    return Fail(EINVAL);
  owner.limit = limit;
  return 0;
}
MEMORY_LOCAL size_t BlinkHostMemoryLimit(void) { return owner.limit; }
MEMORY_LOCAL size_t BlinkHostMemoryBytes(void) { return owner.bytes; }
MEMORY_LOCAL size_t BlinkHostMemoryMappings(void) { return owner.mappings; }
MEMORY_LOCAL int BlinkHostMemorySetFileReader(BlinkHostMemoryReadAt read_at,
                                  BlinkHostMemoryReadLength read_length) {
  if (!owner.limit) return Fail(ENODEV);
  if (!read_at || !read_length) return Fail(EINVAL);
  owner.read_at = read_at;
  owner.read_length = read_length;
  return 0;
}
MEMORY_LOCAL int BlinkHostMemorySetSharedFiles(BlinkHostMemoryAcquireShared acquire,
                                  BlinkHostMemoryReleaseShared release,
                                  BlinkHostMemorySharedAvailable available,
                                  BlinkHostMemorySharedReadOnly readonly_file) {
  if (!owner.limit) return Fail(ENODEV);
  if (!acquire || !release || !available || !readonly_file) return Fail(EINVAL);
  if (owner.acquire_shared && (owner.acquire_shared != acquire ||
      owner.release_shared != release || owner.shared_available != available ||
      owner.shared_readonly != readonly_file))
    return Fail(EBUSY);
  owner.acquire_shared = acquire;
  owner.release_shared = release;
  owner.shared_available = available;
  owner.shared_readonly = readonly_file;
  return 0;
}
MEMORY_LOCAL int BlinkHostMemorySetFileBudget(BlinkHostMemoryBindBudget bind,
                                  BlinkHostMemoryAccountBudget account,
                                  BlinkHostMemoryReleaseBudget release) {
  uint64_t token;
  if (!owner.limit) return Fail(ENODEV);
  if (!bind || !account || !release) return Fail(EINVAL);
  if (owner.file_budget)
    return owner.account_budget == account && owner.release_budget == release ? 0 : Fail(EBUSY);
  if (bind(owner.limit, owner.bytes, &token)) return -1;
  owner.file_budget = token;
  owner.account_budget = account;
  owner.release_budget = release;
  return 0;
}
MEMORY_LOCAL int BlinkHostMemoryContains(const void *pointer, size_t length) {
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
MEMORY_LOCAL int BlinkHostMemoryEnd(void) {
  if (!owner.limit) return Fail(ENODEV);
  if (owner.head) return Fail(EBUSY);
  if (ReleaseBudget(&owner)) return -1;
  owner.limit = 0;
  owner.read_at = 0;
  owner.read_length = 0;
  owner.acquire_shared = 0;
  owner.release_shared = 0;
  owner.shared_available = 0;
  owner.shared_readonly = 0;
  return 0;
}

MEMORY_LOCAL void *blink_host_mmap(void *address, size_t length, int prot, int flags,
                      int fd, off_t offset) {
  struct OwnedMapping *record;
  void *allocation;
  int error;
  size_t rounded;
  size_t metadata_size;
  size_t file_bytes = 0;
  size_t available = 0;
  uint64_t shared_lease = 0;
  int readonly_shared = 0;
  if (!owner.limit) { Fail(ENODEV); return MAP_FAILED; }
  if (!(rounded = RoundLength(length))) { Fail(EINVAL); return MAP_FAILED; }
  if (ValidProtection(prot)) return MAP_FAILED;
  if (address) {
    Fail(ENOTSUP);
    return MAP_FAILED;
  }
  if (flags == (MAP_PRIVATE | MAP_ANONYMOUS)) {
    if (fd != -1 || offset != 0) { Fail(ENOTSUP); return MAP_FAILED; }
  } else if (flags == MAP_PRIVATE || flags == MAP_SHARED) {
    off_t file_length;
    if (offset < 0 || (offset & 4095)) { Fail(EINVAL); return MAP_FAILED; }
    if (flags == MAP_SHARED) {
      if (rounded != 4096 || !owner.acquire_shared) { Fail(ENOTSUP); return MAP_FAILED; }
      if ((readonly_shared = owner.shared_readonly(fd)) < 0) return MAP_FAILED;
      if (readonly_shared && (prot & PROT_WRITE)) { Fail(EACCES); return MAP_FAILED; }
    }
    if (flags == MAP_PRIVATE || readonly_shared == 1) {
      if (!owner.read_at || !owner.read_length) { Fail(ENOTSUP); return MAP_FAILED; }
      if (owner.read_length(fd, &file_length)) return MAP_FAILED;
      if (file_length < 0) { Fail(EIO); return MAP_FAILED; }
      if (offset < file_length) {
        off_t remaining = file_length - offset;
        file_bytes = remaining < (off_t)rounded ? (size_t)remaining : rounded;
      }
      available = file_bytes ? RoundLength(file_bytes) : 0;
    }
  } else { Fail(ENOTSUP); return MAP_FAILED; }
  metadata_size = MetadataSize(rounded) + (flags == MAP_SHARED && readonly_shared != 1 ? sizeof(uint64_t) : 0);
  if (rounded > owner.limit - owner.bytes ||
      metadata_size > owner.limit - owner.bytes - rounded) {
    Fail(ENOMEM);
    return MAP_FAILED;
  }
  if (AccountBudget(&owner, owner.bytes + rounded + metadata_size)) return MAP_FAILED;
  if (!(record = malloc(metadata_size))) {
    AccountBudget(&owner, owner.bytes); Fail(ENOMEM); return MAP_FAILED;
  }
  if (flags == MAP_SHARED && readonly_shared != 1) {
    if (owner.acquire_shared(fd, offset, &allocation, &shared_lease)) {
      error = errno;
      free(record);
      AccountBudget(&owner, owner.bytes);
      Fail(error);
      return MAP_FAILED;
    }
  } else {
    error = posix_memalign(&allocation, 4096, rounded);
    if (error) {
      free(record); AccountBudget(&owner, owner.bytes); Fail(error); return MAP_FAILED;
    }
    memset(allocation, 0, rounded);
  }
  for (size_t copied = 0; copied < file_bytes;) {
    size_t request = file_bytes - copied;
    if (request > 65536) request = 65536;
    ssize_t count = owner.read_at(fd, (unsigned char *)allocation + copied,
                                 request, offset + copied);
    if (count <= 0 || (size_t)count > request) {
      error = count < 0 ? errno : EIO;
      free(allocation);
      free(record);
      AccountBudget(&owner, owner.bytes);
      Fail(error);
      return MAP_FAILED;
    }
    copied += count;
  }
  memset(Protections(record), prot, rounded / 4096);
  if (flags == MAP_PRIVATE || readonly_shared == 1) {
    for (size_t page = available / 4096; page < rounded / 4096; ++page)
      Protections(record)[page] |= FILE_PAGE_ABSENT;
  }
  if (readonly_shared) Protections(record)[0] |= FILE_PAGE_READONLY_SHARED;
  record->address = allocation;
  record->length = rounded;
  if (shared_lease) {
    Protections(record)[0] |= FILE_PAGE_SHARED;
    memcpy(Protections(record) + rounded / 4096, &shared_lease, sizeof(shared_lease));
  }
  record->next = owner.head;
  owner.head = record;
  owner.bytes += rounded + metadata_size;
  ++owner.mappings;
  return allocation;
}

MEMORY_LOCAL int blink_host_munmap(void *address, size_t length) {
  struct OwnedMapping *previous = 0;
  struct OwnedMapping *record = owner.head;
  size_t rounded = RoundLength(length);
  if (!owner.limit) return Fail(ENODEV);
  if (!address || !rounded) return Fail(EINVAL);
  while (record) {
    if (record->address == address) {
      if (record->length != rounded) return Fail(EINVAL);
      uint64_t lease = SharedLease(record);
      if (lease && owner.release_shared(lease)) return -1;
      if (previous) previous->next = record->next;
      else owner.head = record->next;
      owner.bytes -= record->length + MappingMetadataSize(record);
      AccountBudget(&owner, owner.bytes);
      --owner.mappings;
      if (!lease) free(record->address);
      free(record);
      return 0;
    }
    previous = record;
    record = record->next;
  }
  return Fail(EINVAL);
}

MEMORY_LOCAL int blink_host_mprotect(void *address, size_t length, int prot) {
  uintptr_t start = (uintptr_t)address;
  size_t rounded;
  struct OwnedMapping *record = owner.head;
  if (!owner.limit) return Fail(ENODEV);
  if (!start || (start & 4095) || !(rounded = RoundLength(length)) ||
      rounded > UINTPTR_MAX - start) return Fail(EINVAL);
  if (ValidProtection(prot)) return -1;
  while (record) {
    uintptr_t base = (uintptr_t)record->address;
    if (start >= base && rounded <= record->length &&
        start - base <= record->length - rounded) {
      if ((prot & PROT_WRITE) && (Protections(record)[0] & FILE_PAGE_READONLY_SHARED))
        return Fail(EACCES);
      /* Pure software bookkeeping for owned private backing. Raw C memory
       * remains RW; upstream guest PTEs enforce guest access permissions. */
      for (size_t page = (start - base) / 4096;
           page < (start - base + rounded) / 4096; ++page)
        Protections(record)[page] =
            (Protections(record)[page] & FILE_PAGE_METADATA) | prot;
      return 0;
    }
    record = record->next;
  }
  return Fail(ENOMEM);
}
int blink_host_msync(void *address, size_t length, int flags) {
  (void)address; (void)length; (void)flags;
  return Fail(ENOTSUP);
}
MEMORY_LOCAL void BlinkHostMemoryDisposeWorker(void) {
  struct OwnedMapping *record;
  while ((record = owner.head)) {
    uint64_t lease = SharedLease(record);
    if (lease) {
      if (owner.release_shared(lease)) return;
    } else free(record->address);
    owner.head = record->next;
    owner.bytes -= record->length + MappingMetadataSize(record);
    AccountBudget(&owner, owner.bytes);
    --owner.mappings;
    free(record);
  }
  owner.bytes = 0;
  owner.mappings = 0;
  if (ReleaseBudget(&owner)) return;
  owner.limit = 0;
  owner.read_at = 0;
  owner.read_length = 0;
  owner.acquire_shared = 0;
  owner.release_shared = 0;
  owner.shared_available = 0;
  owner.shared_readonly = 0;
}

#undef MEMORY_LOCAL
#ifdef BLINK_MANAGED_GUEST_THREADS
#undef BlinkHostMemoryProtection
#undef BlinkHostMemoryFilePageShared
#undef BlinkHostMemoryFilePageAvailable
#undef BlinkHostMemoryFilePageWritable
#undef BlinkHostMemoryBegin
#undef BlinkHostMemoryLimit
#undef BlinkHostMemoryBytes
#undef BlinkHostMemoryMappings
#undef BlinkHostMemorySetFileReader
#undef BlinkHostMemorySetSharedFiles
#undef BlinkHostMemorySetFileBudget
#undef BlinkHostMemoryContains
#undef BlinkHostMemoryEnd
#undef BlinkHostMemoryDisposeWorker
#undef blink_host_mmap
#undef blink_host_munmap
#undef blink_host_mprotect

static int SharedMemoryLock(void) {
  int error = pthread_mutex_lock(&shared_memory_gate);
  return error ? Fail(error) : 0;
}
static void SharedMemoryUnlock(void) {
  /* An unlock failure is an internal synchronization invariant violation.
   * Never report an unlocked mutation as successfully serialized. */
  if (pthread_mutex_unlock(&shared_memory_gate)) abort();
}
static struct SharedMemoryOwner *FindSharedMemory(uint64_t token) {
  struct SharedMemoryOwner *record = shared_memory_owners;
  while (record && record->token != token) record = record->next;
  return record;
}
size_t BlinkHostMemorySharedOverhead(void) {
  /* Charge the context plus a conservative full share of registry globals per
   * owner. Native pthread ABI sizes may differ from the managed scalar ABI. */
  return sizeof(struct SharedMemoryOwner) + sizeof(shared_memory_gate) +
         sizeof(shared_memory_owners) + sizeof(shared_memory_next_token);
}
uint64_t BlinkHostMemoryCreateShared(size_t limit) {
  struct SharedMemoryOwner *record;
  uint64_t token;
  size_t overhead = BlinkHostMemorySharedOverhead();
  if (limit < overhead + 4096 + MetadataSize(4096) || limit > 256 * 1024 * 1024) {
    Fail(EINVAL); return 0;
  }
  if (SharedMemoryLock()) return 0;
  if (shared_memory_next_token == UINT64_MAX) {
    SharedMemoryUnlock(); Fail(EOVERFLOW); return 0;
  }
  record = calloc(1, sizeof(struct SharedMemoryOwner));
  if (!record) { SharedMemoryUnlock(); Fail(ENOMEM); return 0; }
  token = ++shared_memory_next_token;
  record->token = token;
  record->creator = pthread_self();
  record->memory.limit = limit;
  record->memory.bytes = overhead;
  record->next = shared_memory_owners;
  shared_memory_owners = record;
  SharedMemoryUnlock();
  return token;
}
int BlinkHostMemoryAttach(uint64_t token) {
  struct SharedMemoryOwner *record;
  if (shared_memory_owner || legacy_memory_owner.limit) return Fail(EBUSY);
  if (SharedMemoryLock()) return -1;
  record = FindSharedMemory(token);
  if (!record) { SharedMemoryUnlock(); return Fail(ENOENT); }
  if (record->attachments == SIZE_MAX) { SharedMemoryUnlock(); return Fail(EOVERFLOW); }
  ++record->attachments;
  shared_memory_owner = record;
  SharedMemoryUnlock();
  return 0;
}
int BlinkHostMemoryDetach(void) {
  if (!shared_memory_owner) return Fail(ENODEV);
  if (SharedMemoryLock()) return -1;
  --shared_memory_owner->attachments;
  shared_memory_owner = 0;
  SharedMemoryUnlock();
  return 0;
}
int BlinkHostMemorySharedSnapshot(uint64_t token, size_t *bytes,
                                 size_t *mappings, size_t *attachments) {
  struct SharedMemoryOwner *record;
  if (!bytes || !mappings || !attachments) return Fail(EINVAL);
  if (SharedMemoryLock()) return -1;
  record = FindSharedMemory(token);
  if (!record) { SharedMemoryUnlock(); return Fail(ENOENT); }
  *bytes = record->memory.bytes;
  *mappings = record->memory.mappings;
  *attachments = record->attachments;
  SharedMemoryUnlock();
  return 0;
}
int BlinkHostMemoryDestroyShared(uint64_t token) {
  struct SharedMemoryOwner *record;
  struct SharedMemoryOwner *previous = 0;
  struct OwnedMapping *mapping;
  if (SharedMemoryLock()) return -1;
  record = shared_memory_owners;
  while (record && record->token != token) { previous = record; record = record->next; }
  if (!record) { SharedMemoryUnlock(); return Fail(ENOENT); }
  if (!pthread_equal(record->creator, pthread_self())) {
    SharedMemoryUnlock(); return Fail(EPERM);
  }
  if (record->attachments) { SharedMemoryUnlock(); return Fail(EBUSY); }
  /* Caller has joined all workers and discarded every upstream pointer before
   * this final retained-pool release. Attachments alone do not prove joins. */
  while ((mapping = record->memory.head)) {
    uint64_t lease = SharedLease(mapping);
    if (lease) {
      if (record->memory.release_shared(lease)) {
        SharedMemoryUnlock(); return -1;
      }
    } else free(mapping->address);
    record->memory.head = mapping->next;
    record->memory.bytes -= mapping->length + MappingMetadataSize(mapping);
    AccountBudget(&record->memory, record->memory.bytes);
    --record->memory.mappings;
    free(mapping);
  }
  if (ReleaseBudget(&record->memory)) { SharedMemoryUnlock(); return -1; }
  if (previous) previous->next = record->next;
  else shared_memory_owners = record->next;
  free(record);
  SharedMemoryUnlock();
  return 0;
}

int BlinkHostMemoryBegin(size_t limit) {
  if (shared_memory_owner) return Fail(EBUSY);
  return MemoryImplBegin(limit);
}
int BlinkHostMemoryEnd(void) {
  if (shared_memory_owner) return Fail(EBUSY);
  return MemoryImplEnd();
}
void BlinkHostMemoryDisposeWorker(void) {
  /* Legacy void API cannot certify shared teardown: explicit detach/destroy is
   * required, and no live shared backing is freed by this entrypoint. */
  if (shared_memory_owner) { Fail(EBUSY); return; }
  MemoryImplDisposeWorker();
}
#define MEMORY_WRAPPER(type, name, impl, parameters, arguments, failure) \
  type name parameters { \
    type result; \
    if (shared_memory_owner && SharedMemoryLock()) return failure; \
    result = impl arguments; \
    if (shared_memory_owner) SharedMemoryUnlock(); \
    return result; \
  }
MEMORY_WRAPPER(size_t, BlinkHostMemoryLimit, MemoryImplLimit, (void), (), 0)
MEMORY_WRAPPER(size_t, BlinkHostMemoryBytes, MemoryImplBytes, (void), (), 0)
MEMORY_WRAPPER(size_t, BlinkHostMemoryMappings, MemoryImplMappings, (void), (), 0)
MEMORY_WRAPPER(int, BlinkHostMemoryProtection, MemoryImplProtection,
               (const void *pointer), (pointer), -1)
MEMORY_WRAPPER(int, BlinkHostMemoryFilePageShared, MemoryImplFilePageShared,
               (const void *pointer), (pointer), 0)
MEMORY_WRAPPER(int, BlinkHostMemoryFilePageAvailable, MemoryImplFilePageAvailable,
               (const void *pointer), (pointer), 0)
MEMORY_WRAPPER(int, BlinkHostMemoryFilePageWritable, MemoryImplFilePageWritable,
               (const void *pointer), (pointer), 0)
MEMORY_WRAPPER(int, BlinkHostMemoryContains, MemoryImplContains,
               (const void *pointer, size_t length), (pointer, length), 0)
MEMORY_WRAPPER(int, BlinkHostMemorySetFileReader, MemoryImplSetFileReader,
               (BlinkHostMemoryReadAt read_at, BlinkHostMemoryReadLength read_length),
               (read_at, read_length), -1)
MEMORY_WRAPPER(int, BlinkHostMemorySetSharedFiles, MemoryImplSetSharedFiles,
               (BlinkHostMemoryAcquireShared acquire, BlinkHostMemoryReleaseShared release,
                BlinkHostMemorySharedAvailable available, BlinkHostMemorySharedReadOnly readonly_file),
                (acquire, release, available, readonly_file), -1)
MEMORY_WRAPPER(int, BlinkHostMemorySetFileBudget, MemoryImplSetFileBudget,
               (BlinkHostMemoryBindBudget bind, BlinkHostMemoryAccountBudget account,
                BlinkHostMemoryReleaseBudget release), (bind, account, release), -1)
MEMORY_WRAPPER(void *, blink_host_mmap, MemoryImplMmap,
               (void *address, size_t length, int prot, int flags, int fd, off_t offset),
               (address, length, prot, flags, fd, offset), MAP_FAILED)
MEMORY_WRAPPER(int, blink_host_munmap, MemoryImplMunmap,
               (void *address, size_t length), (address, length), -1)
MEMORY_WRAPPER(int, blink_host_mprotect, MemoryImplMprotect,
               (void *address, size_t length, int prot), (address, length, prot), -1)
#undef MEMORY_WRAPPER
#endif
