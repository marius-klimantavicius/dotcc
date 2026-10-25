static int SysFutexWait(struct Machine *m, i64 uaddr, i32 op, u32 expect,
                        i64 timeout_addr, u32 mask) {
  int rc, kind;
  u64 key;
  u8 *mem;
  struct Futex *f;
  struct BlinkFutexWaiter *waiter, **link;
  const struct timespec_linux *gtimeout;
  struct timespec now, tick, timeout, deadline, slice;
  bool realtime = (op & FUTEX_CLOCK_REALTIME_LINUX) != 0;
  bool absolute = (op & ~(FUTEX_CLOCK_REALTIME_LINUX | FUTEX_PRIVATE_FLAG_LINUX)) == FUTEX_WAIT_BITSET_LINUX;
  if (!mask) return einval();
  now = realtime ? GetTime() : GetMonotonic();
  if (timeout_addr) {
    if (!(gtimeout = (const struct timespec_linux *)SchlepR(
              m, timeout_addr, sizeof(*gtimeout)))) return -1;
    timeout.tv_sec = Read64(gtimeout->sec);
    timeout.tv_nsec = Read64(gtimeout->nsec);
    if (timeout.tv_sec < 0 ||
        !(0 <= timeout.tv_nsec && timeout.tv_nsec < 1000000000)) return einval();
    deadline = absolute ? timeout : AddTime(now, timeout);
  } else {
    deadline = GetMaxTime();
  }
  if (!(mem = LookupAddress(m, uaddr))) return -1;
  key = BlinkFutexKey(uaddr, op, mem, &kind);
  if (!(waiter = calloc(1, sizeof(*waiter)))) return enomem();
  LOCK(&g_bus->futexes.lock);
  if (Load32(mem) != expect) {
    UNLOCK(&g_bus->futexes.lock);
    free(waiter);
    return eagain();
  }
  if ((f = BlinkFindFutex(key, kind))) {
    LOCK(&f->lock);
    ++f->waiters;
  } else if ((f = NewFutex(key))) {
    LOCK(&f->lock);
    dll_make_first(&g_bus->futexes.active, &f->elem);
  } else {
    UNLOCK(&g_bus->futexes.lock);
    free(waiter);
    return -1;
  }
  waiter->futex = f;
  waiter->machine = m;
  waiter->key_kind = kind;
  waiter->mask = mask;
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
    if (waiter->selected) {
      rc = 0;
    } else {
      now = realtime ? GetTime() : GetMonotonic();
      if (CompareTime(now, deadline) >= 0) {
        rc = ETIMEDOUT;
      } else {
        slice = FromMilliseconds(kPollingMs);
        if (CompareTime(AddTime(now, slice), deadline) > 0) {
          slice = SubtractTime(deadline, now);
        }
        /* Host conditions use CLOCK_REALTIME. Convert each bounded slice,
         * retaining the original guest deadline in its selected clock. */
        tick = AddTime(GetTime(), slice);
        rc = pthread_cond_timedwait(&f->cond, &f->lock, &tick);
      }
      if (waiter->selected) {
        rc = 0;
      } else if (!rc || rc == ETIMEDOUT) {
        /* A broadcast/spurious host notification grants no guest wake. Use
         * the selected clock, not accumulated ticks, for the deadline. A word
         * change after enqueue does not grant a guest wake either. */
        rc = ETIMEDOUT;
        if (CompareTime(realtime ? GetTime() : GetMonotonic(), deadline) < 0) {
          UNLOCK(&f->lock);
          continue;
        }
      }
    }
    /* Stop counting this waiter before dropping the selection lock on timeout
     * or host error. Deregistration takes the bus lock in the usual order. */
    if (rc) waiter->departing = true;
    UNLOCK(&f->lock);
    break;
  }
  LOCK(&g_bus->futexes.lock);
  LOCK(&f->lock);
  /* A wake can win against a signal/cancellation checkpoint outside locks. */
  if (waiter->selected) rc = 0;
  waiter->departing = true;
  UNLOCK(&f->lock);
  for (link = &g_blink_futex_waiters; *link != waiter;
       link = &(*link)->next) {}
  BlinkRemoveFutexWaiter(link);
  UNLOCK(&g_bus->futexes.lock);
  if (rc) { errno = rc; return -1; }
  return 0;
}
