#define _GNU_SOURCE 1
#include "HostMemory.h"
#include <errno.h>
#include <fcntl.h>
#include <stdio.h>
#include <stdlib.h>
#include <sys/mman.h>
#include <sys/stat.h>
#include <unistd.h>
#include "blink/bus.h"
#include "blink/machine.h"
#include "blink/map.h"

#define PAGE 4096
#define BASE 0x800000
#define CHECK(x) do { if (!(x)) { result = __LINE__; goto Cleanup; } } while (0)
void TerminateSignal(struct Machine *m, int signal, int code) {
  (void)m; (void)signal; (void)code;
  abort();  /* ordinary metadata workload must never enter a signal path */
}
static ssize_t ReadAt(int fd, void *data, size_t count, off_t offset) {
  return pread(fd, data, count, offset);
}
static int ReadLength(int fd, off_t *length) {
  struct stat st;
  if (fstat(fd, &st)) return -1;
  *length = st.st_size;
  return 0;
}
static u64 Leaf(struct System *s, i64 address) {
  u64 entry = s->cr3;
  for (unsigned level = 39;; level -= 9) {
    u8 *slot = GetPageAddress(s, entry, level == 39) +
               (((u64)address >> level) & 511) * 8;
    entry = LoadPte(slot);
    if (!(entry & PAGE_V) || level == 12) return entry;
  }
}
int main(void) {
  int fd = -1, result = 0;
  struct Machine *m = 0;
  struct System *s = 0;
  unsigned char data[PAGE + 37], copied[PAGE];
  CHECK(!BlinkHostMemoryBegin(512 * 1024));
  CHECK(!BlinkHostMemorySetFileReader(ReadAt, ReadLength));
  FLAG_nolinear = true;
  InitMap(); InitBus();
  fd = open("invariant-data", O_CREAT | O_TRUNC | O_RDWR, 0600);
  CHECK(fd >= 0);
  for (unsigned i = 0; i < sizeof(data); ++i) data[i] = i % 251;
  CHECK(write(fd, data, sizeof(data)) == sizeof(data));
  s = NewSystem(XED_MACHINE_MODE_LONG); CHECK(s);
  m = NewMachine(s, 0); CHECK(m); g_machine = m;
  s->cr3 = AllocatePageTable(s); CHECK(s->cr3 != (u64)-1);
  CHECK(ReserveVirtual(s, BASE, PAGE * 4, PAGE_U | PAGE_RW | PAGE_XD,
                       fd, 0, false, false) == BASE);
  CHECK(s->vss == 4 && s->memstat.reserved == 4);
  for (unsigned i = 0; i < 4; ++i) {
    u64 leaf = Leaf(s, BASE + i * PAGE);
    CHECK((leaf & (PAGE_V | PAGE_FILE | PAGE_MUG | PAGE_RSRV)) ==
          (PAGE_V | PAGE_FILE | PAGE_MUG | PAGE_RSRV));
    CHECK(BlinkHostMemoryFilePageAvailable(FindHostPage(leaf)) == (i < 2));
  }
  CHECK(!CopyFromUserRead(m, copied, BASE + PAGE, PAGE));
  for (unsigned i = 0; i < PAGE; ++i) CHECK(copied[i] == (i < 37 ? data[PAGE + i] : 0));
  CHECK(!ProtectVirtual(s, BASE, PAGE * 4, PROT_NONE, false));
  CHECK(!ProtectVirtual(s, BASE, PAGE * 4, PROT_READ | PROT_WRITE, false));
  CHECK(!BlinkHostMemoryFilePageAvailable(FindHostPage(Leaf(s, BASE + PAGE * 2))));
  CHECK(Leaf(s, BASE + PAGE * 2) & PAGE_RSRV);
  /* A normal larger request exceeds this owner's configured cap. It prepares
   * new tables/backing, then must roll back while preserving existing bytes. */
  unsigned char marker = 253;
  CHECK(!CopyToUserWrite(m, BASE + 17, &marker, 1));
  long rss = s->rss, vss = s->vss;
  long reserved = s->memstat.reserved, committed = s->memstat.committed;
  long tables = s->memstat.tables;
  size_t owned_bytes = BlinkHostMemoryBytes(), owned_maps = BlinkHostMemoryMappings();
  CHECK(ReserveVirtual(s, BASE - PAGE, PAGE * 64, PAGE_U | PAGE_RW | PAGE_XD,
                       fd, 0, false, true) == -1 && errno == ENOMEM);
  CHECK(s->rss == rss && s->vss == vss && s->memstat.reserved == reserved &&
        s->memstat.committed == committed && s->memstat.tables == tables);
  CHECK(BlinkHostMemoryBytes() == owned_bytes && BlinkHostMemoryMappings() == owned_maps);
  CHECK(IsFullyUnmapped(s, BASE - PAGE, PAGE));
  CHECK(!CopyFromUserRead(m, copied, BASE, PAGE) && copied[17] == marker);
  CHECK(!BlinkHostMemoryFilePageAvailable(FindHostPage(Leaf(s, BASE + PAGE * 2))));
  /* Replace an absent page; only then perform the ordinary BSS access. */
  CHECK(ReserveVirtual(s, BASE + PAGE * 2, PAGE, PAGE_U | PAGE_RW | PAGE_XD,
                       -1, 0, false, true) == BASE + PAGE * 2);
  CHECK(s->vss == 4 && !(Leaf(s, BASE + PAGE * 2) & PAGE_FILE));
  CHECK(!CopyFromUserRead(m, copied, BASE + PAGE * 2, PAGE));
  for (unsigned i = 0; i < PAGE; ++i) CHECK(!copied[i]);
  CHECK(Leaf(s, BASE + PAGE * 3) & PAGE_RSRV);
  CHECK(!FreeVirtual(s, BASE, PAGE * 4) && !s->vss);
  CHECK(!s->memstat.reserved && !s->memstat.committed);
Cleanup:
  if (m) { FreeMachine(m); g_machine = 0; s = 0; }
  else if (s) FreeSystem(s);
  if (fd >= 0) close(fd);
  unlink("invariant-data");
  /* No later upstream call may retain slab pointers after final disposal. */
  BlinkHostMemoryDisposeWorker();
  if (BlinkHostMemoryBytes() || BlinkHostMemoryMappings()) result = __LINE__;
  if (result) fprintf(stderr, "mapping invariants failed at line %d\n", result);
  else puts("guest-private-mapping invariants OK eof metadata protection replacement rollback accounting cleanup");
  return !!result;
}
