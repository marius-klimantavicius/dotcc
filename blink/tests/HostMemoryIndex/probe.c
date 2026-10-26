/* Public ownership semantics plus optional structural AVL checks. Including
 * the authored C permits fresh standalone translation without product edits. */
#include "HostMemory.c"
#include <stdio.h>

#define PAGE 4096
#define CAPACITY 4096
#define CHECK(x) do { if (!(x)) { \
  fprintf(stderr, "mapping index line %d errno %d\n", __LINE__, errno); abort(); \
} } while (0)

struct LiveMapping {
  unsigned char *address;
  size_t length;
  unsigned char protections[3];
};
static struct LiveMapping live[CAPACITY];
static unsigned order[CAPACITY];
static uint32_t random_state;
static size_t initial_bytes;
static int ledger_bound;
static size_t ledger_bytes, ledger_limit;
static unsigned char *shared_pages;
static off_t shared_length, snapshot_length;
static uint64_t next_lease;
static unsigned char leases[CAPACITY];
static off_t lease_offsets[CAPACITY];
static unsigned lease_count;

static unsigned Random(void) {
  random_state ^= random_state << 13;
  random_state ^= random_state >> 17;
  random_state ^= random_state << 5;
  return random_state;
}
static int BindBudget(size_t limit, size_t bytes, uint64_t *token) {
  CHECK(!ledger_bound && bytes <= limit);
  ledger_bound = 1; ledger_bytes = bytes; ledger_limit = limit; *token = 13;
  return 0;
}
static int Account(uint64_t token, size_t bytes) {
  CHECK(token == 13 && ledger_bound && bytes <= ledger_limit);
  ledger_bytes = bytes;
  return 0;
}
static int ReleaseBudgetToken(uint64_t token) {
  CHECK(token == 13 && ledger_bound);
  ledger_bound = 0; ledger_bytes = 0;
  return 0;
}

#ifdef MAPPING_INDEX_VALIDATE
/* This checks the structural bound, without timing assertions. In-order keys
 * may repeat for aliases; every subtree must satisfy the AVL height bound. */
static int Validate(struct OwnedMapping *record, uintptr_t *previous,
                    uintptr_t *previous_end, size_t *count, size_t *bytes) {
  int left, right;
  if (!record) return 0;
  left = Validate(record->left, previous, previous_end, count, bytes);
  CHECK((uintptr_t)record->address >= *previous);
  if ((uintptr_t)record->address == *previous) {
    CHECK(record->length == PAGE && *previous_end == *previous + PAGE);
  } else CHECK((uintptr_t)record->address >= *previous_end);
  *previous = (uintptr_t)record->address;
  *previous_end = *previous + record->length;
  ++*count;
  *bytes += record->length + sizeof(struct OwnedMapping) + record->length / PAGE;
  if (Protections(record)[0] & FILE_PAGE_SHARED) *bytes += sizeof(uint64_t);
  right = Validate(record->right, previous, previous_end, count, bytes);
  CHECK(left - right >= -1 && left - right <= 1);
  CHECK(record->height == 1 + (left > right ? left : right));
  return record->height;
}
#endif
static void CheckIndex(void) {
#ifdef MAPPING_INDEX_VALIDATE
  uintptr_t previous = 0, previous_end = 0;
  size_t count = 0, bytes = initial_bytes;
  Validate(owner.root, &previous, &previous_end, &count, &bytes);
  CHECK(count == BlinkHostMemoryMappings() && bytes == BlinkHostMemoryBytes());
#endif
  if (ledger_bound) CHECK(ledger_bytes == BlinkHostMemoryBytes());
}
#ifdef MAPPING_INDEX_VALIDATE
static unsigned PathDepth(uintptr_t address) {
  struct OwnedMapping *record = owner.root;
  unsigned depth = 0;
  while (record) {
    ++depth;
    record = address < (uintptr_t)record->address ? record->left : record->right;
  }
  CHECK(depth <= (unsigned)MappingHeight(owner.root));
  return depth;
}
static void StructuralEvidence(int shared, unsigned count) {
  unsigned maximum = 0, total = 0;
  for (unsigned i = 0; i < 1024; ++i) {
    unsigned index = Random() % count;
    uintptr_t address = (uintptr_t)live[index].address + live[index].length - 1;
    CHECK(FindMapping(address)->address == live[index].address);
    unsigned depth = PathDepth(address);
    total += depth;
    if (depth > maximum) maximum = depth;
  }
  fprintf(stderr, "index shared=%d count=%u height=%d oldest_depth=%u newest_depth=%u random_samples=1024 random_total_depth=%u random_max_depth=%u miss_depth=%u\n",
          shared, count, MappingHeight(owner.root), PathDepth((uintptr_t)live[0].address),
          PathDepth((uintptr_t)live[count - 1].address), total, maximum, PathDepth(1));
}
#endif
static uint64_t Begin(int shared, size_t limit) {
  uint64_t token = 0;
  initial_bytes = 0;
#ifdef BLINK_MANAGED_GUEST_THREADS
  if (shared) {
    initial_bytes = BlinkHostMemorySharedOverhead();
    token = BlinkHostMemoryCreateShared(limit + initial_bytes);
    CHECK(token && !BlinkHostMemoryAttach(token));
  } else
#else
  (void)shared;
#endif
  CHECK(!BlinkHostMemoryBegin(limit));
  CHECK(!BlinkHostMemorySetFileBudget(BindBudget, Account, ReleaseBudgetToken));
  CheckIndex();
  return token;
}
static void End(uint64_t token, int retained) {
  CheckIndex();
#ifdef BLINK_MANAGED_GUEST_THREADS
  if (token) {
    CHECK(!BlinkHostMemoryDetach());
    CHECK(!BlinkHostMemoryDestroyShared(token));
  } else
#else
  (void)token;
#endif
  if (retained) BlinkHostMemoryDisposeWorker();
  else CHECK(!BlinkHostMemoryEnd());
  CHECK(!ledger_bound && !BlinkHostMemoryLimit() && !BlinkHostMemoryBytes());
  CHECK(!BlinkHostMemoryMappings());
}
static unsigned char *Map(size_t length, int prot) {
  unsigned char *p = blink_host_mmap(0, length, prot, MAP_PRIVATE | MAP_ANONYMOUS, -1, 0);
  CHECK(p != MAP_FAILED);
  CHECK(!p[0] && !p[length - 1]);
  return p;
}
static void CheckLive(unsigned i) {
  struct LiveMapping *m = &live[i];
  CHECK(BlinkHostMemoryContains(m->address, m->length));
  CHECK(!BlinkHostMemoryContains(m->address + m->length - 1, 2));
  CHECK(!BlinkHostMemoryContains(m->address, 0));
  for (size_t page = 0; page < m->length / PAGE; ++page) {
    unsigned char *p = m->address + page * PAGE + PAGE - 1;
    CHECK(BlinkHostMemoryProtection(p) == m->protections[page]);
    CHECK(BlinkHostMemoryFilePageAvailable(p));
    CHECK(BlinkHostMemoryFilePageWritable(p));
    CHECK(!BlinkHostMemoryFilePageShared(p));
  }
}
static void Unowned(uintptr_t address) {
  const void *p = (const void *)address;
  errno = EDOM;
  CHECK(!BlinkHostMemoryContains(p, 1) && BlinkHostMemoryProtection(p) == -1);
  CHECK(!BlinkHostMemoryFilePageAvailable(p) && !BlinkHostMemoryFilePageShared(p));
  CHECK(!BlinkHostMemoryFilePageWritable(p) && errno == EDOM);
}
static void Ranges(int shared, unsigned count) {
  uint64_t token = Begin(shared, 64 * 1024 * 1024);
  random_state = 0x928f7a19;
  Unowned(1); Unowned(UINTPTR_MAX);
  CHECK(!BlinkHostMemoryContains((void *)(UINTPTR_MAX - 1), PAGE));
  for (unsigned i = 0; i < count; ++i) {
    live[i].length = (i % 3 + 1) * PAGE;
    live[i].address = Map(live[i].length, PROT_READ | PROT_WRITE);
    memset(live[i].protections, PROT_READ | PROT_WRITE, 3);
    order[i] = i;
    CheckLive(i);
    if (!(i & 15)) CheckIndex();
  }
  CHECK(BlinkHostMemoryMappings() == count);
#ifdef MAPPING_INDEX_VALIDATE
  CheckIndex();
  StructuralEvidence(shared, count);
#endif
  for (unsigned i = count; i > 1; --i) {
    unsigned j = Random() % i, saved = order[i - 1];
    order[i - 1] = order[j]; order[j] = saved;
  }
  for (unsigned step = 0; step < count; ++step) {
    unsigned i = order[step];
    /* Interleave oldest/newest/random lookups with permission mutations. */
    CheckLive(0); CheckLive(count - 1); CheckLive(i);
    CHECK(!blink_host_mprotect(live[i].address, PAGE, PROT_READ));
    live[i].protections[0] = PROT_READ;
    CheckLive(i);
    uintptr_t old = (uintptr_t)live[i].address;
    CHECK(!blink_host_munmap(live[i].address, live[i].length));
    Unowned(old);
    /* malloc may reuse this address; either result must have fresh metadata. */
    live[i].address = Map(live[i].length, PROT_NONE);
    memset(live[i].protections, PROT_NONE, 3);
    CheckLive(i);
    /* An aligned allocator can coalesce/repartition free storage and return a
     * different base whose new live range still contains the old address. */
    uintptr_t replacement = (uintptr_t)live[i].address;
    if (old >= replacement && old - replacement < live[i].length) {
      CHECK(BlinkHostMemoryContains((void *)old, 1));
      CHECK(BlinkHostMemoryProtection((void *)old) == PROT_NONE);
      CHECK(BlinkHostMemoryFilePageAvailable((void *)old));
    } else Unowned(old);
    if (!(step & 15)) CheckIndex();
  }
  for (unsigned step = 0; step < count; ++step) {
    unsigned i = order[count - step - 1];
    uintptr_t old = (uintptr_t)live[i].address;
    CHECK(!blink_host_munmap(live[i].address, live[i].length));
    Unowned(old);
    if (!(step & 15)) CheckIndex();
  }
  CHECK(BlinkHostMemoryBytes() == initial_bytes && !BlinkHostMemoryMappings());
  End(token, 0);
}
static ssize_t ReadAt(int fd, void *destination, size_t length, off_t offset) {
  (void)fd;
  for (size_t i = 0; i < length; ++i)
    ((unsigned char *)destination)[i] = (unsigned char)((offset + i) % 251);
  return length;
}
static int ReadLength(int fd, off_t *length) {
  (void)fd; *length = snapshot_length; return 0;
}
static int SharedReadOnly(int fd) { return fd == 9 ? 1 : fd == 8 ? 2 : 0; }
static int Acquire(int fd, off_t offset, void **pointer, uint64_t *lease) {
  (void)fd;
  CHECK(offset == 0 || offset == PAGE);
  CHECK(++next_lease < CAPACITY);
  *pointer = shared_pages + offset; *lease = next_lease;
  leases[next_lease] = 1; lease_offsets[next_lease] = offset; ++lease_count;
  return 0;
}
static int Release(uint64_t lease) {
  CHECK(lease && lease <= next_lease && leases[lease]);
  leases[lease] = 0; --lease_count; return 0;
}
static int Available(uint64_t lease) {
  CHECK(lease && lease <= next_lease && leases[lease]);
  return lease_offsets[lease] < shared_length;
}
static unsigned char *MapFileFixture(int fd, off_t offset, size_t length, int flags, int prot) {
  unsigned char *p = blink_host_mmap(0, length, prot, flags, fd, offset);
  CHECK(p != MAP_FAILED);
  return p;
}
static void Files(int shared) {
  uint64_t token = Begin(shared, 16 * 1024 * 1024);
  unsigned char *aliases[257];
  CHECK(!posix_memalign((void **)&shared_pages, PAGE, 2 * PAGE));
  memset(shared_pages, 0, 2 * PAGE);
  shared_length = 2 * PAGE; snapshot_length = PAGE + 1;
  CHECK(!BlinkHostMemorySetFileReader(ReadAt, ReadLength));
  CHECK(!BlinkHostMemorySetSharedFiles(Acquire, Release, Available, SharedReadOnly));
  unsigned char *snapshot = MapFileFixture(7, 0, 3 * PAGE, MAP_PRIVATE, PROT_READ);
  CHECK(snapshot[PAGE] == PAGE % 251 && !snapshot[PAGE + 1]);
  CHECK(BlinkHostMemoryFilePageAvailable(snapshot + PAGE));
  CHECK(!BlinkHostMemoryFilePageAvailable(snapshot + 2 * PAGE));
  CHECK(!blink_host_mprotect(snapshot, 3 * PAGE, PROT_READ | PROT_WRITE));
  snapshot_length = 0;
  CHECK(BlinkHostMemoryFilePageAvailable(snapshot + PAGE));
  CHECK(!BlinkHostMemoryFilePageAvailable(snapshot + 2 * PAGE));
  snapshot_length = PAGE + 1;
  unsigned char *immutable = MapFileFixture(9, 0, PAGE, MAP_SHARED, PROT_READ);
  CHECK(BlinkHostMemoryFilePageShared(immutable));
  CHECK(!BlinkHostMemoryFilePageWritable(immutable));
  for (unsigned i = 0; i < 257; ++i) {
    aliases[i] = MapFileFixture(7, 0, PAGE, MAP_SHARED, i % 4);
    CHECK(aliases[i] == shared_pages);
    CHECK(BlinkHostMemoryProtection(aliases[i] + 123) == (int)(i % 4));
    CHECK(BlinkHostMemoryFilePageShared(aliases[i]));
    CHECK(BlinkHostMemoryFilePageWritable(aliases[i]));
    if (!(i & 15)) CheckIndex();
  }
  /* A read-only regular-file lease at the same backing address is newest. */
  unsigned char *readonly_alias = MapFileFixture(8, 0, PAGE, MAP_SHARED, PROT_READ);
  CHECK(readonly_alias == shared_pages);
  CHECK(!BlinkHostMemoryFilePageWritable(shared_pages));
  CHECK(blink_host_mprotect(shared_pages, PAGE, PROT_READ | PROT_WRITE) == -1);
  CHECK(BlinkHostMemoryProtection(shared_pages) == PROT_READ);
  CHECK(!blink_host_munmap(readonly_alias, PAGE));
  CHECK(BlinkHostMemoryFilePageWritable(shared_pages));
  unsigned char *second = MapFileFixture(7, PAGE, PAGE, MAP_SHARED, PROT_READ);
  shared_length = 0;
  CHECK(!BlinkHostMemoryFilePageAvailable(shared_pages));
  CHECK(!BlinkHostMemoryFilePageAvailable(second));
  CHECK(!blink_host_mprotect(shared_pages, PAGE, PROT_READ));
  shared_length = PAGE;
  CHECK(BlinkHostMemoryFilePageAvailable(shared_pages));
  CHECK(!BlinkHostMemoryFilePageAvailable(second));
  shared_length = 2 * PAGE;
  CHECK(BlinkHostMemoryFilePageAvailable(second));
  CHECK(!blink_host_munmap(second, PAGE));
  for (unsigned i = 257; i > 0; --i) {
    CHECK(BlinkHostMemoryProtection(shared_pages) == (i == 257 ? PROT_READ : (int)((i - 1) % 4)));
    CHECK(!blink_host_munmap(aliases[i - 1], PAGE));
    if (i > 1) CHECK(BlinkHostMemoryProtection(shared_pages) == (int)((i - 2) % 4));
    if (!(i & 15)) CheckIndex();
  }
  Unowned((uintptr_t)shared_pages);
  /* Deterministic address reuse through a new lease, with different metadata. */
  unsigned char *replacement = MapFileFixture(7, 0, PAGE, MAP_SHARED, PROT_WRITE);
  CHECK(replacement == shared_pages && BlinkHostMemoryProtection(replacement) == PROT_WRITE);
  shared_length = 0;
  CHECK(!BlinkHostMemoryFilePageAvailable(replacement));
  CHECK(!blink_host_munmap(replacement, PAGE));
  CHECK(!blink_host_munmap(snapshot, 3 * PAGE));
  CHECK(!blink_host_munmap(immutable, PAGE));
  CHECK(!lease_count);
  /* Retained mixed backing is released through both disposal entrypoints. */
  for (unsigned i = 0; i < 97; ++i) Map(PAGE, PROT_READ);
  MapFileFixture(7, 0, PAGE, MAP_SHARED, PROT_READ);
  MapFileFixture(7, 0, PAGE, MAP_SHARED, PROT_WRITE);
  MapFileFixture(8, PAGE, PAGE, MAP_SHARED, PROT_READ);
  End(token, 1);
  CHECK(!lease_count);
  free(shared_pages); shared_pages = 0;
}
static void Quota(int shared) {
  size_t limit = PAGE + sizeof(struct OwnedMapping) + 1;
  uint64_t token = Begin(shared, limit);
  unsigned char *p = Map(PAGE, PROT_READ);
  CHECK(BlinkHostMemoryBytes() == limit + initial_bytes);
  CHECK(blink_host_mmap(0, PAGE, PROT_READ, MAP_PRIVATE | MAP_ANONYMOUS, -1, 0) == MAP_FAILED);
  CHECK(errno == ENOMEM && BlinkHostMemoryMappings() == 1);
  CHECK(BlinkHostMemoryContains(p, PAGE) && BlinkHostMemoryProtection(p) == PROT_READ);
  CheckIndex();
  CHECK(!blink_host_munmap(p, PAGE));
  End(token, 0);
}
int MappingIndexRun(void) {
  static const unsigned counts[] = {1, 64, 1024, 4096};
  int modes = 1;
#ifdef BLINK_MANAGED_GUEST_THREADS
  modes = 2;
#endif
  for (int shared = 0; shared < modes; ++shared) {
    for (unsigned i = 0; i < sizeof(counts) / sizeof(counts[0]); ++i)
      Ranges(shared, counts[i]);
    Files(shared); Quota(shared);
  }
  puts("mapping ranges/protections/remap/reuse/aliases/truncate/quota/cleanup=pass");
  return 0;
}
#ifndef BLINK_TEST_MANAGED
int main(void) { return MappingIndexRun(); }
#endif
