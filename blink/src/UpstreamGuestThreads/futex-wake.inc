/* Queue membership is protected by the bus futex lock; selection is protected
 * by the corresponding Futex.lock. Heap records also survive terminal signal
 * unwind until ClearChildTid performs worker cleanup. The translated global
 * belongs to the owning BlinkCore instance. */
struct BlinkFutexWaiter {
  struct BlinkFutexWaiter *next;
  struct Futex *futex;
  struct Machine *machine;
  bool selected;
};
static struct BlinkFutexWaiter *g_blink_futex_waiters;
static void FreeFutex(struct Futex *);

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

static int SysFutexWake(struct Machine *m, i64 uaddr, u32 count) {
  int rc = 0;
  struct Futex *f;
  struct BlinkFutexWaiter *waiter;
  if (!count) return 0;
  LOCK(&g_bus->futexes.lock);
  if ((f = FindFutex(m, uaddr))) {
    LOCK(&f->lock);
    for (waiter = g_blink_futex_waiters; waiter && (u32)rc < count;
         waiter = waiter->next) {
      if (waiter->futex == f && !waiter->selected) {
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
