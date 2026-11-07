"""Checked fetch and data/page-walk lifetime protocol before C emission."""
INPUTS = ('scripts/full_jit_memory_bindings.py',)


def replace_once(source, before, after):
    if source.count(before) != 1:
        raise ValueError('Full-JIT fetch lifetime anchor differs: ' + before[:100])
    return source.replace(before, after, 1)


def adapt(path, content):
    if path == 'blink/bit.c':
        source = content.decode()
        # OpBit already owns the selected nonrecursive bus mutex for LOCK.
        # Store64 would acquire it a second time via WriteRegisterOrMemory.
        # Keep the current outer exclusion and use its unlocked 64-bit store.
        source = replace_once(source, '  WriteRegisterOrMemory(rde, p, z);',
            '  if (Lock(rde) && Rexw(rde) && !IsModrmRegister(rde)) {\n    Store64Unlocked(p, z);\n  } else {\n    WriteRegisterOrMemory(rde, p, z);\n  }')
        return source.encode()
    if path == 'blink/machine.c':
        source = content.decode()
        # Both pair-CAS handlers write on success. Read-only resolution skips
        # PAGE_RW and marks a crossing stash nonwritable, losing its commit.
        for bits, width in ((8, 8), (16, 16)):
            start = source.index('static void OpCmpxchg%db(P) {' % bits)
            end = source.index('\n}', start) + 2
            body = source[start:end]
            body = replace_once(body,
                'p = GetModrmRegisterXmmPointerRead%d(A);' % width,
                'p = GetModrmRegisterXmmPointerWrite%d(A);' % width)
            source = source[:start] + body + source[end:]
        return source.encode()
    if path == 'blink/stack.c':
        source = content.decode()
        # Long-mode stack effects commit only after the checked memory access.
        # Keep the guest's original RSP visible to a synchronous fault handler.
        source = replace_once(source, '''      v = (Get64(m->sp) - osz) & 0xffffffffffffffff;
      Put64(m->sp, v);
      break;''', '''      v = (Get64(m->sp) - osz) & 0xffffffffffffffff;
      break;''')
        source = replace_once(source, '''  WriteStackWord(w, rde, osz, x);
  EndStore(m, v, osz, p, b);
}''', '''  WriteStackWord(w, rde, osz, x);
  EndStore(m, v, osz, p, b);
  if (mode == XED_MODE_LONG) Put64(m->sp, v);
}''')
        source = replace_once(source, '''      v = Get64(m->sp);
      Put64(m->sp, v + osz + extra);
      break;''', '''      v = Get64(m->sp);
      break;''')
        source = replace_once(source, '''  return ReadStackWord(AccessRam(m, v, osz, p, b, true), osz);
}''', '''  u64 x = ReadStackWord(AccessRam(m, v, osz, p, b, true), osz);
  if (Mode(rde) == XED_MODE_LONG) Put64(m->sp, v + osz + extra);
  return x;
}''')
        return source.encode()
    if path == 'blink/memory.c':
        source = replace_once(content.decode(),
            'u64 FindPageTableEntry(struct Machine *m, u64 page) {',
            'u64 BlinkGuestFindPageTableEntry(struct Machine *m, u64 page) {')
        # Access permissions do not identify pointer use: LookupAddress is
        # also used by stores which deliberately bypass PAGE_RW checks. Keep
        # that public interface conservative. Only audited, local read paths
        # below opt out of code-write authority (their lifetime pins remain).
        source = replace_once(source,
            'u8 *LookupAddress2(struct Machine *m, i64 virt, u64 mask, u64 need) {',
            'static u8 *BlinkGuestLookupAddress2Access(struct Machine *m, i64 virt, '
            'u64 mask, u64 need, bool writable) {')
        source = replace_once(source,
            '  if (mask != PAGE_XD || need)\n    blink_host_guest_code_revoke(virt & -4096, 4096);',
            '  if (writable)\n    blink_host_guest_code_revoke(virt & -4096, 4096);')
        source = replace_once(source,
            '  if ((host = GetPageAddress(m->system, entry, false))) {\n    return host + (virt & 4095);',
            '  if ((host = GetPageAddress(m->system, entry, false))) {\n'
            '#ifdef BLINK_MANAGED_GUEST_THREADS\n'
            '    if (writable) blink_host_guest_physical_expose(m, host);\n'
            '#endif\n'
            '    return host + (virt & 4095);')
        source = replace_once(source, 'u8 *LookupAddress(struct Machine *m, i64 virt) {', '''
u8 *LookupAddress2(struct Machine *m, i64 virt, u64 mask, u64 need) {
  /* Only instruction fetch has an unambiguous read-only public contract. */
  return BlinkGuestLookupAddress2Access(m, virt, mask, need,
                                       mask != PAGE_XD || need != 0);
}

static u8 *BlinkGuestLookupAddressAccess(struct Machine *m, i64 virt,
                                        bool writable) {
  u64 need = Cpl(m) == 3 ? PAGE_U : 0;
  return BlinkGuestLookupAddress2Access(m, virt, need, need, writable);
}

u8 *LookupAddress(struct Machine *m, i64 virt) {''')
        source = replace_once(source,
            '    if (!(p = LookupAddress2(m, v, need, need))) return -1;',
            '    if (!(p = BlinkGuestLookupAddress2Access(m, v, need, need, !d))) return -1;')
        # ReserveAddress carries the actual read/write intent even at CPL0.
        if source.count('LookupAddress2(m, v, mask, need)') != 2:
            raise ValueError('Full-JIT ReserveAddress access anchors differ')
        source = source.replace('LookupAddress2(m, v, mask, need)',
                                'BlinkGuestLookupAddress2Access(m, v, mask, need, writable)')
        source = replace_once(source,
            'LookupAddress2(m, v + k, mask, need)',
            'BlinkGuestLookupAddress2Access(m, v + k, mask, need, writable)')
        source = replace_once(source, 'static u8 *AccessRam2(', '''
static u8 *BlinkGuestResolveAddressRead(struct Machine *m, i64 v) {
  u8 *r;
  if (HasLinearMapping()) return ToHost(v);
  if ((r = BlinkGuestLookupAddressAccess(m, v, false))) return r;
  ThrowSegmentationFault(m, v);
}

static u8 *AccessRam2(''')
        # AccessRam(..., true) is Load's read path. Read/modify/write paths
        # preserve conservative write admission, including ROM handling.
        access_start = source.index('static u8 *AccessRam2(')
        access_end = source.index('\nu8 *AccessRam(', access_start)
        access = source[access_start:access_end]
        if access.count('ResolveAddress(m, v)') != 2 or access.count('ResolveAddress(m, v + k)') != 1:
            raise ValueError('Full-JIT AccessRam read anchors differ')
        access = access.replace('ResolveAddress(m, v)',
            '(copy && !protect_rom ? BlinkGuestResolveAddressRead(m, v) : ResolveAddress(m, v))')
        access = access.replace('ResolveAddress(m, v + k)',
            '(copy && !protect_rom ? BlinkGuestResolveAddressRead(m, v + k) : ResolveAddress(m, v + k))')
        source = source[:access_start] + access + source[access_end:]
        # Schlep itself remains conservative. Its read-only named wrapper is
        # the sole caller admitted through the private read variant.
        source = replace_once(source,
            'void *Schlep(struct Machine *m, i64 addr, size_t size, u64 mask, u64 need) {',
            'static void *BlinkGuestSchlepAccess(struct Machine *m, i64 addr, size_t size, '
            'u64 mask, u64 need, bool writable) {')
        source = replace_once(source, 'LookupAddress2(m, addr, mask, need)',
            'BlinkGuestLookupAddress2Access(m, addr, mask, need, writable)')
        source = replace_once(source, 'LookupAddress2(m, addr + have, mask, need)',
            'BlinkGuestLookupAddress2Access(m, addr + have, mask, need, writable)')
        source = replace_once(source, 'void *SchlepR(struct Machine *m, i64 addr, size_t size) {', '''
void *Schlep(struct Machine *m, i64 addr, size_t size, u64 mask, u64 need) {
  return BlinkGuestSchlepAccess(m, addr, size, mask, need, true);
}

void *SchlepR(struct Machine *m, i64 addr, size_t size) {''')
        source = replace_once(source, 'return Schlep(m, addr, size, PAGE_U, PAGE_U);',
            'return BlinkGuestSchlepAccess(m, addr, size, PAGE_U, PAGE_U, false);')
        source = replace_once(source, 'LookupAddress2(m, addr, PAGE_U, PAGE_U)',
            'BlinkGuestLookupAddress2Access(m, addr, PAGE_U, PAGE_U, false)')
        source = replace_once(source, 'LookupAddress2(m, addr + have, PAGE_U, PAGE_U)',
            'BlinkGuestLookupAddress2Access(m, addr + have, PAGE_U, PAGE_U, false)')
        # A translated capability prevents accidentally pairing the retained
        # native engine with an older producer missing physical alias revocation.
        source += '\nint BlinkGuestPhysicalCodeProtocol(void) { return 2; }\n'
        # The managed boundary owns try/finally for short walk-to-PAGE_LOCK
        # protection. Neither guest jumps nor host exceptions skip its release.
        return (source + """

u64 FindPageTableEntry(struct Machine *m, u64 page) {
#ifdef BLINK_MANAGED_GUEST_THREADS
  return blink_host_guest_page_table_entry(m, page);
#else
  return BlinkGuestFindPageTableEntry(m, page);
#endif
}
""").encode()
    if path == 'blink/instruction.c':
        source = content.decode()
        source = replace_once(source, '''    } else {
      return ReadInstruction(m, addr, i);
    }
  } else {
    return kMachineSegmentationFault;
  }
}''', '''    } else {
      struct XedDecodedInst xedd[1];
      int error = DecodeInstruction(xedd, addr, i, m->mode.omode);
      if (!error) {
        memcpy(m->xedd, xedd, kInstructionBytes);
        return 0; /* A complete short instruction needs no next page. */
      }
      if (error == XED_ERROR_BUFFER_TOO_SHORT) {
        m->faultaddr = ip + i;
        return kMachineSegmentationFault;
      }
      return kMachineDecodeError;
    }
  } else {
    m->faultaddr = ip;
    return kMachineSegmentationFault;
  }
}''')
        source = replace_once(source, '''    } else {
      return kMachineSegmentationFault;
    }
    if (IsOpcodeEqual''', '''    } else {
      m->faultaddr = pc;
      return kMachineSegmentationFault;
    }
    if (IsOpcodeEqual''')
        source = replace_once(source, '''    case kMachineSegmentationFault:
      m->faultaddr = pc;
      HaltMachine(m, rc);''', '''    case kMachineSegmentationFault:
      HaltMachine(m, rc);''')
        return source.encode()
    if path != 'blink/memorymalloc.c':
        return content
    source = content.decode()
    before = '''void FreeAnonymousPage(struct System *s, u8 *page) {
  struct HostPage *h;'''
    source = replace_once(source, before, '''/* Called under the managed instruction-read gate when full mode is active.
 * Clear AND allocator publication are one retirement operation. No mmap lock
 * is acquired here; callers may already hold it. */
void BlinkGuestRecycleAnonymousPage(struct System *s, u8 *page, int clear) {
  struct HostPage *h;
  if (clear) ClearPage(page);''')
    marker = '\nstatic size_t GetBigSize(size_t n) {'
    source = replace_once(source, marker, '''
static void BlinkGuestRetireAnonymousPage(struct System *s, u8 *page, int clear) {
#ifdef BLINK_MANAGED_GUEST_THREADS
  blink_host_guest_recycle_page(s, page, clear);
#else
  BlinkGuestRecycleAnonymousPage(s, page, clear);
#endif
}

void FreeAnonymousPage(struct System *s, u8 *page) {
  BlinkGuestRetireAnonymousPage(s, page, 0);
}
''' + marker)
    source = replace_once(source, '''    ClearPage((page = FindHostPage(entry)));
    FreeAnonymousPage(s, page);''', '''    page = FindHostPage(entry);
    BlinkGuestRetireAnonymousPage(s, page, 1);''')
    # A page must be unreachable before its retirement gate can publish it to
    # the allocator. An old reader already in the gate drains before recycling;
    # a later reader can no longer discover the detached table.
    source = replace_once(source, '''        FreePageTable(s, GetPageAddress(s, LoadPte(pde), i == 39));
        StorePte(pde, 0);''', '''        pt = LoadPte(pde);
        StorePte(pde, 0);
        FreePageTable(s, GetPageAddress(s, pt, i == 39));''')
    source = replace_once(source, '''        if (FreeEmptyPageTables(s, pt, level + 1)) {
          StorePte(mi + i * 8, 0);''', '''        if (FreeEmptyPageTables(s, pt, level + 1)) {
          StorePte(mi + i * 8, 0);
          FreePageTable(s, GetPageAddress(s, pt, false));''')
    source = replace_once(source, '''  if (isempty) {
    FreePageTable(s, mi);
  }
  return isempty;''', '''  /* Caller detaches this table before retiring it. The live root is kept
   * by CleanseMemory and released only during final FreeHostPages. */
  return isempty;''')
    source = replace_once(source, '''    unassert(FreeEmptyPageTables(s, s->cr3, 1));''', '''    unassert(FreeEmptyPageTables(s, s->cr3, 1));
    {
      u64 root = s->cr3;
      s->cr3 = 0;
      FreePageTable(s, GetPageAddress(s, root, true));
    }''')
    return source.encode()
