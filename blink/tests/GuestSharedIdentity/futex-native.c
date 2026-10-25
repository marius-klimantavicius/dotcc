/* Execute the authored wait/wake algorithms with real pthread waits and the
 * real shared C memory owner. The tiny address adapter models two valid guest
 * aliases; this is a focused algorithm check, not a native Blink run. */
#define _POSIX_C_SOURCE 200809L
#define _DEFAULT_SOURCE 1
#include "HostMemory.h"
#include "blink/bus.h"
#include "blink/linux.h"
#include "blink/timespec.h"
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <sys/mman.h>

struct Machine { bool killed; };
static struct Bus bus;
struct Bus *g_bus = &bus;
static uint64_t owner;
static unsigned char *backing, *other_backing, *anonymous;
static unsigned leases;
static i64 address;
static i32 operation;
static int result;

_Noreturn void AssertFailed(const char *file, int line, const char *expression) {
  fprintf(stderr, "%s:%d: %s\n", file, line, expression); exit(100);
}
#define CHECK(x) unassert(x)
static int einval(void) { errno = EINVAL; return -1; }
static int enomem(void) { errno = ENOMEM; return -1; }
static int eagain(void) { errno = EAGAIN; return -1; }
static bool CheckInterrupt(struct Machine *m, bool restart) { (void)m; (void)restart; return false; }
static u8 *LookupAddress(struct Machine *m, i64 addr) {
  (void)m;
  if (addr >= 0x100000 && addr < 0x101000) return backing + addr - 0x100000;
  if (addr >= 0x200000 && addr < 0x201000) return backing + addr - 0x200000;
  if (addr >= 0x300000 && addr < 0x301000) return other_backing + addr - 0x300000;
  CHECK(addr >= 0x400000 && addr < 0x401000);
  return anonymous + addr - 0x400000;
}
static const void *SchlepR(struct Machine *m, i64 addr, size_t length) {
  (void)m; (void)length; return (void*)(uintptr_t)addr;
}
i64 Load32(const u8 p[4]) { return atomic_load_explicit((const _Atomic(u32)*)p, memory_order_seq_cst); }
#include "futex-wake.inc"
static struct Futex *NewFutex(i64 addr) {
  struct Dll *entry = dll_first(bus.futexes.free);
  if (!entry) { enomem(); return 0; }
  struct Futex *f = FUTEX_CONTAINER(entry);
  dll_remove(&bus.futexes.free, entry); f->addr = addr; f->waiters = 1; return f;
}
static void FreeFutex(struct Futex *f) { dll_make_first(&bus.futexes.free, &f->elem); }
#include "futex-wait.inc"

static int Acquire(int fd, off_t offset, void **pointer, uint64_t *lease) {
  CHECK((fd == 7 || fd == 8) && offset == 0);
  *pointer = fd == 7 ? backing : other_backing; *lease = ++leases; return 0;
}
static int Release(uint64_t lease) { CHECK(lease && leases); --leases; return 0; }
static int Available(uint64_t lease) { CHECK(lease); return 1; }
static int ReadOnly(int fd) { CHECK(fd == 7 || fd == 8); return 0; }
static void *Wait(void *argument) {
  struct Machine *m = argument;
  struct timespec_linux timeout;
  struct timespec end = operation == 9 ? AddTime(GetMonotonic(), FromSeconds(2)) : FromSeconds(2);
  Write64(timeout.sec, end.tv_sec); Write64(timeout.nsec, end.tv_nsec);
  CHECK(!BlinkHostMemoryAttach(owner));
  result = SysFutexWait(m, address, operation, 0, (i64)(uintptr_t)&timeout, 2);
  CHECK(!BlinkHostMemoryDetach());
  return 0;
}
int main(void) {
  struct Machine m = {0};
  CHECK(!posix_memalign((void**)&backing, 4096, 4096));
  CHECK(!posix_memalign((void**)&other_backing, 4096, 4096));
  memset(backing, 0, 4096); memset(other_backing, 0, 4096);
  owner = BlinkHostMemoryCreateShared(65536); CHECK(owner && !BlinkHostMemoryAttach(owner));
  CHECK(!BlinkHostMemorySetSharedFiles(Acquire, Release, Available, ReadOnly));
  void *a = blink_host_mmap(0, 4096, PROT_READ | PROT_WRITE, MAP_SHARED, 7, 0);
  void *b = blink_host_mmap(0, 4096, PROT_READ | PROT_WRITE, MAP_SHARED, 7, 0);
  void *other = blink_host_mmap(0, 4096, PROT_READ | PROT_WRITE, MAP_SHARED, 8, 0);
  anonymous = blink_host_mmap(0, 4096, PROT_READ | PROT_WRITE, MAP_PRIVATE | MAP_ANONYMOUS, -1, 0);
  CHECK(a == backing && b == a && other == other_backing && anonymous != MAP_FAILED);
  CHECK(!pthread_mutex_init(&bus.futexes.lock, 0));
  struct Futex *f = &bus.futexes.mem[0];
  CHECK(!pthread_mutex_init(&f->lock, 0) && !pthread_cond_init(&f->cond, 0));
  dll_init(&f->elem); dll_make_first(&bus.futexes.free, &f->elem);
  for (int phase = 0; phase < 5; ++phase) {
    pthread_t worker;
    address = phase >= 3 ? 0x400040 : 0x100040;
    operation = phase == 1 || phase == 4 ? 128 : phase == 2 ? 9 : 0;
    result = -100;
    CHECK(!pthread_create(&worker, 0, Wait, &m));
    struct timespec deadline = AddTime(GetMonotonic(), FromSeconds(1));
    for (;;) {
      LOCK(&bus.futexes.lock);
      bool queued = g_blink_futex_waiters != 0;
      UNLOCK(&bus.futexes.lock);
      if (queued) break;
      CHECK(CompareTime(GetMonotonic(), deadline) < 0);
      struct timespec delay = FromMilliseconds(1); nanosleep(&delay, 0);
    }
    if (phase < 3) {
      CHECK(!BlinkFutexWakeOp(&m, 0x200044, 1, 1, ~0u));
      CHECK(!BlinkFutexWakeOp(&m, 0x300040, 1, 1, ~0u));
      CHECK(!BlinkFutexWakeOp(&m, 0x200040, 129, 1, ~0u));
      CHECK(!BlinkFutexWakeOp(&m, address, operation == 128 ? 1 : 129, 1, ~0u));
      if (phase == 2) CHECK(!BlinkFutexWakeOp(&m, 0x200040, 10, 1, 1));
      CHECK(BlinkFutexWakeOp(&m, phase == 1 ? address : 0x200040,
          phase == 1 ? 129 : 1, 1, 2) == 1);
    } else {
      CHECK(!BlinkFutexWakeOp(&m, address, operation == 128 ? 1 : 129, 1, ~0u));
      CHECK(BlinkFutexWakeOp(&m, address, operation == 128 ? 129 : 1, 1, ~0u) == 1);
    }
    CHECK(!pthread_join(worker, 0) && result == 0 && !g_blink_futex_waiters && !bus.futexes.active);
  }
  CHECK(SysFutexWake(&m, 0x100040, 0) == 0);
  BlinkGuestFutexCleanup(&m);
  CHECK(!blink_host_munmap(a, 4096) && !blink_host_munmap(b, 4096));
  CHECK(!blink_host_munmap(other, 4096) && !blink_host_munmap(anonymous, 4096));
  CHECK(!leases && BlinkHostMemoryMappings() == 0);
  CHECK(!BlinkHostMemoryDetach() && !BlinkHostMemoryDestroyShared(owner));
  CHECK(!pthread_cond_destroy(&f->cond) && !pthread_mutex_destroy(&f->lock));
  CHECK(!pthread_mutex_destroy(&bus.futexes.lock));
  free(backing); free(other_backing);
  puts("authored C futex live waits: shared alias/offset/file/private/anon/bitset, real C owner, cleanup: PASS");
}
