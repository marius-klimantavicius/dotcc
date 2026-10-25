/* Exercise real authored selection with three registered, not-yet-dequeued
 * records. No scheduler can make the second wake accidentally count twice. */
#include <stdio.h>
#include <stdlib.h>
#include "blink/bus.h"

struct Machine { int identity; };
static struct Bus bus;
struct Bus *g_bus = &bus;
static struct Futex *FindFutex(struct Machine *m, i64 addr) {
  (void)m;
  struct Dll *e;
  for (e = dll_first(g_bus->futexes.active); e;
       e = dll_next(g_bus->futexes.active, e)) {
    if (FUTEX_CONTAINER(e)->addr == addr) return FUTEX_CONTAINER(e);
  }
  return 0;
}
#include "futex-wake.inc"
static void FreeFutex(struct Futex *f) {
  dll_make_first(&g_bus->futexes.free, &f->elem);
}
_Noreturn void AssertFailed(const char *file, int line, const char *expression) {
  fprintf(stderr, "%s:%d: %s\n", file, line, expression);
  exit(100);
}
#define CHECK(x) do { if (!(x)) return __LINE__; } while (0)
static int AddWaiters(struct Machine *m, struct Futex *f) {
  int i;
  dll_remove(&g_bus->futexes.free, &f->elem);
  dll_make_first(&g_bus->futexes.active, &f->elem);
  f->waiters = 3;
  for (i = 0; i < 3; ++i) {
    struct BlinkFutexWaiter *w = calloc(1, sizeof(*w));
    if (!w) return 1;
    w->machine = m;
    w->futex = f;
    w->mask = 0xffffffffu;
    w->next = g_blink_futex_waiters;
    g_blink_futex_waiters = w;
  }
  return 0;
}
int main(void) {
  struct Machine m = {1};
  struct Futex *f = &bus.futexes.mem[0];
  struct BlinkFutexWaiter *w;
  int selected;
  CHECK(!pthread_mutex_init(&bus.futexes.lock, 0));
  CHECK(!pthread_mutex_init(&f->lock, 0));
  CHECK(!pthread_cond_init(&f->cond, 0));
  f->addr = 4096;
  dll_init(&f->elem);
  dll_make_first(&bus.futexes.free, &f->elem);
  CHECK(!AddWaiters(&m, f));
  CHECK(SysFutexWake(&m, 4096, 0) == 0);
  CHECK(SysFutexWake(&m, 8192, 2) == 0);
  CHECK(SysFutexWake(&m, 4096, 2) == 2);
  selected = 0;
  for (w = g_blink_futex_waiters; w; w = w->next) selected += w->selected;
  CHECK(selected == 2 && f->waiters == 3);
  CHECK(SysFutexWake(&m, 4096, 2) == 1);
  CHECK(SysFutexWake(&m, 4096, 0x7fffffff) == 0);
  BlinkGuestFutexCleanup(&m);
  CHECK(!g_blink_futex_waiters && !bus.futexes.active && !f->waiters);
  CHECK(!AddWaiters(&m, f));
  CHECK(SysFutexWake(&m, 4096, 1) == 1);
  CHECK(SysFutexWake(&m, 4096, 1) == 1);
  CHECK(SysFutexWake(&m, 4096, 1) == 1);
  CHECK(SysFutexWake(&m, 4096, 1) == 0);
  BlinkGuestFutexCleanup(&m);
  CHECK(!g_blink_futex_waiters && !bus.futexes.active && !f->waiters);
  CHECK(!AddWaiters(&m, f));
  CHECK(SysFutexWake(&m, 4096, 0x7fffffff) == 3);
  BlinkGuestFutexCleanup(&m);
  CHECK(!g_blink_futex_waiters && !bus.futexes.active && !f->waiters);
  CHECK(!AddWaiters(&m, f));
  selected = 0;
  for (w = g_blink_futex_waiters; w; w = w->next) w->mask = 1u << selected++;
  CHECK(BlinkFutexWake(&m, 4096, 2, 8) == 0);
  CHECK(BlinkFutexWake(&m, 4096, 2, 2) == 1);
  CHECK(BlinkFutexWake(&m, 4096, 2, 2) == 0);
  CHECK(BlinkFutexWake(&m, 4096, 1, 5) == 1);
  CHECK(SysFutexWake(&m, 4096, 3) == 1);
  CHECK(BlinkFutexWake(&m, 4096, 3, 0xffffffffu) == 0);
  BlinkGuestFutexCleanup(&m);
  CHECK(!g_blink_futex_waiters && !bus.futexes.active && !f->waiters);
  CHECK(!AddWaiters(&m, f));
  for (w = g_blink_futex_waiters; w; w = w->next) w->departing = true;
  CHECK(SysFutexWake(&m, 4096, 3) == 0);
  BlinkGuestFutexCleanup(&m);
  CHECK(!g_blink_futex_waiters && !bus.futexes.active && !f->waiters);
  CHECK(!pthread_cond_destroy(&f->cond));
  CHECK(!pthread_mutex_destroy(&f->lock));
  CHECK(!pthread_mutex_destroy(&bus.futexes.lock));
  puts("futex queue: bounded selections, no duplicate wakes, cleanup and reuse");
  return 0;
}
