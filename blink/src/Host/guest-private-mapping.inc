/* Managed nonlinear profile only. Prepare every fallible allocation before
 * replacing any guest leaf. mmap_lock serializes mapping/table mutations;
 * ordinary guest readers may continue using the old leaves during prepare. */
struct BlinkPreparedMappingPage {
  u8 *slot;
  u8 *new_tables[3];
  u8 *backing;
  u64 entry;
};

/* ProtectVirtual otherwise updates guest PTEs even after a host-side failure.
 * Preflight the whole range so immutable shared files cannot become writable,
 * and a later rejected leaf cannot partially change earlier permissions. */
static int BlinkGuestFileProtection(struct System *s, i64 virt, i64 size, int prot) {
  i64 address;
  if (!(prot & PROT_WRITE)) return 0;
  for (address = virt; address < virt + size; address += 4096) {
    u64 entry = s->cr3;
    unsigned level;
    for (level = 39;; level -= 9) {
      entry = LoadPte(GetPageAddress(s, entry, level == 39) +
                      (((u64)address >> level) & 511) * 8);
      if (!(entry & PAGE_V) || level == 12 || (entry & PAGE_PS)) break;
    }
    if (level == 12 && (entry & PAGE_MUG) &&
        !BlinkHostMemoryFilePageWritable(FindHostPage(entry))) {
      errno = EACCES;
      return -1;
    }
  }
  return 0;
}

i64 ReserveVirtual(struct System *s, i64 virt, i64 size, u64 flags, int fd,
                   i64 offset, bool shared, bool fixedmap) {
  struct BlinkPreparedMappingPage *prepared;
  i64 count, index, address;
  size_t scratch_size;
  int error;
  bool executable_changed = false;
  bool file_pinned = false;
  long rss_delta = 0;
  (void)fixedmap;  /* guest callers select/check their address under mmap_lock */
  unassert(!HasLinearMapping());
  unassert(s->mode.omode == XED_MODE_LONG);
  unassert(!(flags & (PAGE_TA | PAGE_MAP | PAGE_HOST | PAGE_RSRV)));
  if (shared && fd == -1) return enotsup();
  if (virt < -0x800000000000ll || virt >= 0x800000000000ll ||
      !IsValidAddrSize(virt, size)) return einval();
  size = ROUNDUP(size, 4096);
  if (!IsValidAddrSize(virt, size)) return einval();
#ifdef BLINK_MANAGED_GUEST_THREADS
  /* Drain and permanently revoke old identities before table publication,
   * replacement or retirement. No gate spans preparation or file callbacks. */
  blink_host_guest_mapping_revoke(virt, size);
#endif
  if (fd != -1 && (offset < 0 || (offset & 4095))) return einval();
  if (fd != -1 && offset > 0x7fffffffffffffffll - size) return eoverflow();
  count = size / 4096;
  /* Use the same bounded owner for transaction storage, without putting it
   * into guest page identities or an unbounded malloc side cache. */
  if ((u64)count > (u64)BlinkHostMemoryLimit() / sizeof(*prepared)) return enomem();
  scratch_size = count * sizeof(*prepared);
  prepared = blink_host_mmap(0, scratch_size, PROT_READ | PROT_WRITE,
                            MAP_PRIVATE | MAP_ANONYMOUS_, -1, 0);
  if (prepared == MAP_FAILED) return -1;
  if (fd != -1) {
    if (BlinkHostFileMappingBegin(fd)) goto Rollback;
    file_pinned = true;
  }
  for (index = 0, address = virt; index < count; ++index, address += 4096) {
    u64 table = s->cr3;
    unsigned level;
    for (level = 39;; level -= 9) {
      u8 *slot = GetPageAddress(s, table, level == 39) +
                 (((u64)address >> level) & 511) * 8;
      if (level == 12) { prepared[index].slot = slot; break; }
      table = LoadPte(slot);
      if (!(table & PAGE_V)) {
        table = AllocatePageTable(s);
        if (table == (u64)-1) goto Rollback;
        prepared[index].new_tables[(39 - level) / 9] = slot;
        StorePte(slot, table);
      }
      unassert(!(table & PAGE_PS));
    }
    prepared[index].entry = flags | PAGE_V | PAGE_RSRV;
    if (fd != -1) {
      /* Immutable shared images must remain read-only. Mutable memfd backing
       * is physically RW/NX regardless of the independently enforced guest PTE. */
      int backing_prot = shared ? GetProtection(flags) & ~PROT_EXEC : PROT_READ | PROT_WRITE;
      u8 *backing = blink_host_mmap(0, 4096, backing_prot,
                                   shared ? MAP_SHARED : MAP_PRIVATE,
                                   fd, offset + index * 4096);
      if (backing == MAP_FAILED) goto Rollback;
      prepared[index].backing = backing;
      prepared[index].entry |= PAGE_HOST | PAGE_MAP | PAGE_MUG | PAGE_FILE;
    }
  }
  /* Publish file identities only after every backing/table allocation succeeds.
   * Retired successful identities remain an inherited per-instance high-water
   * registry; failed backing preparation adds no dangling file identities. */
  if (fd != -1) {
    for (index = 0; index < count; ++index)
      prepared[index].entry |= TrackHostPage(prepared[index].backing);
  }
  /* No more mapping/table allocations below this point. Preserve original
   * per-page retirement, lock waits, permissions and accounting. */
  for (index = 0, address = virt; index < count; ++index, address += 4096) {
    u8 *slot = prepared[index].slot;
    u64 old;
    for (;;) {
      old = LoadPte(slot);
      if (old & PAGE_LOCKS) WaitForPageToNotBeLocked(s, address, slot);
      else if (CasPte(slot, old, prepared[index].entry)) break;
    }
    if (old & PAGE_V) FreePage(s, address, old, 4096, &executable_changed, &rss_delta);
    else ++s->vss;
    ++s->memstat.reserved;
  }
  s->rss += rss_delta;
  if (fd != -1 && !(flags & PAGE_FILE)) AddFileMapViaMap(s, virt, size, fd, offset);
  /* Also invalidate when replacing a reserved file PTE: neither old nor new
   * need have resident backing, but stale cached translation must disappear. */
  InvalidateSystem(s, true, executable_changed);
  if (file_pinned) unassert(!BlinkHostFileMappingEnd());
  unassert(!blink_host_munmap(prepared, scratch_size));
  return virt;
Rollback:
  error = errno;
  /* The prepared leaf values have never been published. Newly installed
   * intermediate tables are empty, and are detached deepest-first. */
  for (index = count; index-- > 0;) {
    if (prepared[index].backing)
      unassert(!blink_host_munmap(prepared[index].backing, 4096));
    for (int depth = 3; depth-- > 0;) {
      u8 *slot = prepared[index].new_tables[depth];
      if (slot) {
        u64 table = LoadPte(slot);
        StorePte(slot, 0);
        FreePageTable(s, GetPageAddress(s, table, false));
      }
    }
  }
  if (file_pinned) unassert(!BlinkHostFileMappingEnd());
  unassert(!blink_host_munmap(prepared, scratch_size));
  errno = error;
  return -1;
}
