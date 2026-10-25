/* Included in staged memorymalloc.c, after its page retirement helpers.
 * Guest mappings, never guest addresses cast to host pointers, own discard. */
static int BlinkGuestAdvicePte(struct System *s, i64 addr, u8 **slot,
                              i64 *next) {
  u64 entry = s->cr3;
  unsigned level;
  for (level = 39;; level -= 9) {
    *slot = GetPageAddress(s, entry, level == 39) +
            (((u64)addr >> level) & 511) * 8;
    entry = LoadPte(*slot);
    *next = ((u64)addr | (((u64)1 << level) - 1)) + 1;
    if (!(entry & PAGE_V)) return 0;
    if (level == 12) return 1;
    if (entry & PAGE_PS) return -1;
  }
}

int BlinkGuestMadvise(struct Machine *m, i64 addr, u64 len, int advice) {
  struct System *s = m->system;
  i64 page, end, next;
  u64 entry, replacement;
  u8 *slot;
  int found, rc = 0;
  long rss_delta = 0;
  bool executable_changed = false;
  bool discarded = false;
  /* The four conventional hints have no application-visible side effects.
   * All other advice except DONTNEED is explicitly unimplemented. In
   * particular FREE, DONTFORK, DONTDUMP and population are not success stubs. */
  if (advice < 0 || advice > 4) return einval();
  if ((addr & 4095) || len > (u64)-1 - 4095) return einval();
  len = (len + 4095) & ~(u64)4095;
  if ((u64)addr + len < (u64)addr) return einval();
  if (!len) return 0;
  /* IsValidAddrSize canonicalizes its address argument internally. Check the
   * original too, so noncanonical addresses cannot alias a real reservation. */
  if (addr < -0x800000000000ll || addr >= 0x800000000000ll ||
      len > 0x1000000000000ull || !IsValidAddrSize(addr, len)) return enomem();
  end = addr + len;
  BEGIN_NO_PAGE_FAULTS;
  LOCK(&s->mmap_lock);
  /* Check backing before changing any page. DONTNEED on file mappings needs
   * a file reload contract; never zero their private copies or host backing.
   * Missing intervals are skipped but remembered, matching Linux ENOMEM with
   * the mapped portions still advised. A walk never commits reserved pages. */
  for (page = addr; page < end; page = next) {
    found = BlinkGuestAdvicePte(s, page, &slot, &next);
    if (!found) { rc = ENOMEM; continue; }
    entry = LoadPte(slot);
    if (found < 0 || (advice == 4 &&
        ((entry & (PAGE_FILE | PAGE_MAP | PAGE_MUG)) ||
         !(entry & (PAGE_HOST | PAGE_RSRV))))) {
      rc = ENOTSUP;
      goto FinishedAdvice;
    }
  }
  if (advice != 4) goto FinishedAdvice;
  for (page = addr; page < end; page = next) {
    if (!BlinkGuestAdvicePte(s, page, &slot, &next)) continue;
    for (;;) {
      entry = LoadPte(slot);
      if (entry & PAGE_LOCKS) {
        WaitForPageToNotBeLocked(s, page, slot);
        continue;
      }
      if (entry & PAGE_RSRV) break;
      replacement = (entry & ~(PAGE_TA | PAGE_HOST)) | PAGE_RSRV;
      if (!CasPte(slot, entry, replacement)) continue;
      /* FreePage clears anonymous backing before returning it to the normal
       * allocator. Preserve VSS and permissions; the next access zero-faults. */
      FreePage(s, page, entry, 4096, &executable_changed, &rss_delta);
      ++s->memstat.reserved;
      discarded = true;
      break;
    }
  }
  s->rss += rss_delta;
  InvalidateSystem(s, discarded, executable_changed);
FinishedAdvice:
  UNLOCK(&s->mmap_lock);
  END_NO_PAGE_FAULTS;
  if (rc) { errno = rc; return -1; }
  return 0;
}
