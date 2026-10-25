static int SysFutexWait(struct Machine *m, i64 uaddr, i32 op, u32 expect,
                        i64 timeout_addr) {
  int rc;
  u8 *mem;
  struct Futex *f;
  struct BlinkFutexWaiter *waiter, **link;
  const struct timespec_linux *gtimeout;
  struct timespec now, tick, timeout, deadline;
  now = GetTime();
  if (timeout_addr) {
    if (!(gtimeout = (const struct timespec_linux *)SchlepR(
              m, timeout_addr, sizeof(*gtimeout)))) return -1;
    timeout.tv_sec = Read64(gtimeout->sec);
    timeout.tv_nsec = Read64(gtimeout->nsec);
    if (!(0 <= timeout.tv_nsec && timeout.tv_nsec < 1000000000)) return einval();
    deadline = AddTime(now, timeout);
  } else {
    deadline = GetMaxTime();
  }
  if (!(mem = LookupAddress(m, uaddr))) return -1;
  if (!(waiter = calloc(1, sizeof(*waiter)))) return enomem();
  LOCK(&g_bus->futexes.lock);
  if (Load32(mem) != expect) {
    UNLOCK(&g_bus->futexes.lock);
    free(waiter);
    return eagain();
  }
  if ((f = FindFutex(m, uaddr))) {
    LOCK(&f->lock);
    ++f->waiters;
  } else if ((f = NewFutex(uaddr))) {
    LOCK(&f->lock);
    dll_make_first(&g_bus->futexes.active, &f->elem);
  } else {
    UNLOCK(&g_bus->futexes.lock);
    free(waiter);
    return -1;
  }
  waiter->futex = f;
  waiter->machine = m;
  waiter->next = g_blink_futex_waiters;
  g_blink_futex_waiters = waiter;
  UNLOCK(&f->lock);
  UNLOCK(&g_bus->futexes.lock);
  for (;;) {
    /* Keep cancellation/signal checkpoints outside locks, as upstream does. */
    if (m->killed) { rc = EAGAIN; break; }
    if (CheckInterrupt(m, true)) { rc = EINTR; break; }
    if (!(mem = LookupAddress(m, uaddr))) { rc = errno; break; }
    LOCK(&f->lock);
    if (waiter->selected || Load32(mem) != expect) {
      rc = 0;
    } else {
      now = GetTime();
      tick = AddTime(now, FromMilliseconds(kPollingMs));
      if (CompareTime(tick, deadline) > 0) tick = deadline;
      rc = pthread_cond_timedwait(&f->cond, &f->lock, &tick);
      if (waiter->selected) {
        rc = 0;
      } else if (!rc || rc == ETIMEDOUT) {
        /* A broadcast/spurious host notification grants no guest wake. Use
         * real time, not accumulated ticks, to retain the original deadline. */
        rc = ETIMEDOUT;
        if (CompareTime(GetTime(), deadline) < 0) {
          UNLOCK(&f->lock);
          continue;
        }
      }
    }
    UNLOCK(&f->lock);
    break;
  }
  LOCK(&g_bus->futexes.lock);
  for (link = &g_blink_futex_waiters; *link != waiter;
       link = &(*link)->next) {}
  BlinkRemoveFutexWaiter(link);
  UNLOCK(&g_bus->futexes.lock);
  if (rc) { errno = rc; return -1; }
  return 0;
}
