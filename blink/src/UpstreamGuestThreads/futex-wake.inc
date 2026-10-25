#include "blink/linux.h"

/* Queue membership is protected by the bus futex lock; selection is protected
 * by the corresponding Futex.lock. Heap records also survive terminal signal
 * unwind until ClearChildTid performs worker cleanup. The translated global
 * belongs to the owning BlinkCore instance. */
struct BlinkFutexWaiter {
  struct BlinkFutexWaiter *next;
  struct Futex *futex;
  struct Machine *machine;
  int key_kind;
  u32 mask;
  bool selected;
  bool departing;
};
static struct BlinkFutexWaiter *g_blink_futex_waiters;
static void FreeFutex(struct Futex *);

/* PRIVATE is a distinct namespace even on a shared mapping. Mutable shared
 * file aliases resolve to the same leased host byte, including the word's
 * page offset. This trusted key never crosses the guest syscall boundary.
 * Native upstream has no authored shared-file owner and retains its VA key. */
static u64 BlinkFutexKey(i64 uaddr, i32 op, u8 *mem, int *kind) {
  *kind = (op & FUTEX_PRIVATE_FLAG_LINUX) ? 1 : 0;
#ifdef BLINK_MANAGED_GUEST_THREADS
  if (!*kind && BlinkHostMemoryFilePageShared(mem)) {
    *kind = 2;
    return (u64)(uintptr_t)mem;
  }
#else
  (void)mem;
#endif
  return uaddr;
}

/* Caller holds the per-instance bus gate. A departing record remains a valid
 * queue identity until deregistration. Futex.addr holds the trusted key. */
static struct Futex *BlinkFindFutex(u64 key, int kind) {
  struct BlinkFutexWaiter *waiter;
  for (waiter = g_blink_futex_waiters; waiter; waiter = waiter->next) {
    if ((u64)waiter->futex->addr == key &&
        waiter->key_kind == kind) return waiter->futex;
  }
  return 0;
}

/* Caller holds the bus futex lock, as in upstream waiter deregistration. */
static void BlinkRemoveFutexWaiter(struct BlinkFutexWaiter **link) {
  struct BlinkFutexWaiter *waiter = *link;
  struct Futex *f = waiter->futex;
  LOCK(&f->lock);
  *link = waiter->next;
  if (!--f->waiters) {
    dll_remove(&g_bus->futexes.active, &f->elem);
    FreeFutex(f);
  }
  UNLOCK(&f->lock);
  free(waiter);
}

static void BlinkGuestFutexCleanup(struct Machine *m) {
  struct BlinkFutexWaiter **link;
  LOCK(&g_bus->futexes.lock);
  for (link = &g_blink_futex_waiters; *link;) {
    if ((*link)->machine == m) BlinkRemoveFutexWaiter(link);
    else link = &(*link)->next;
  }
  UNLOCK(&g_bus->futexes.lock);
}

static int BlinkFutexWakeOp(struct Machine *m, i64 uaddr, i32 op, u32 count, u32 mask) {
  int rc = 0, kind;
  u64 key;
  u8 *mem = 0;
  struct Futex *f;
  struct BlinkFutexWaiter *waiter;
  if (!count) return 0;
#ifdef BLINK_MANAGED_GUEST_THREADS
  if (!(op & FUTEX_PRIVATE_FLAG_LINUX) && !(mem = LookupAddress(m, uaddr))) return -1;
#else
  (void)m;
#endif
  key = BlinkFutexKey(uaddr, op, mem, &kind);
  LOCK(&g_bus->futexes.lock);
  if ((f = BlinkFindFutex(key, kind))) {
    LOCK(&f->lock);
    for (waiter = g_blink_futex_waiters; waiter && (u32)rc < count;
         waiter = waiter->next) {
      if (waiter->futex == f && !waiter->selected && !waiter->departing &&
          (waiter->mask & mask)) {
        waiter->selected = true;
        ++rc;
      }
    }
    /* Host broadcast is only a notification: unselected waiters repark. Mark
     * before notifying so another wake cannot select the same waiter again. */
    if (rc) unassert(!pthread_cond_broadcast(&f->cond));
    UNLOCK(&f->lock);
  }
  UNLOCK(&g_bus->futexes.lock);
  return rc;
}

static int BlinkFutexWake(struct Machine *m, i64 uaddr, u32 count, u32 mask) {
  return BlinkFutexWakeOp(m, uaddr, 0, count, mask);
}

static int SysFutexWake(struct Machine *m, i64 uaddr, u32 count) {
  return BlinkFutexWake(m, uaddr, count, 0xffffffffu);
}
