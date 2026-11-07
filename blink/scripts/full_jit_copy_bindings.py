"""R74 actual-use copies, applied after full_jit_memory_bindings.

Independently authored source adapters. All anchors are fail-closed; this module
does not run a compiler/emitter, change generated C#, or enable native atomics.
The managed callbacks own finally regions (C begin/end pairs cannot unwind).
"""
import re
import hashlib

INPUTS = ('scripts/full_jit_copy_bindings.py',)
PROTOCOL = 1

# Audited selected native4 ownership inventory, before this adapter. A source
# version introducing a new caller/consumer cannot silently acquire tag rights.
FREELIST_ARGUMENTS = {
    'blink/memory.c': ['copy', 'copy', 'strdup(s)', 'list'],
    'blink/syscall.c': ['malloc(n/8)', 'malloc(n/8)', 'calloc(1,rc)',
        'calloc(1,kMaxAncillary)', 'calloc(1,optvalsize)', 'malloc(maxchunk)',
        'malloc(bufsiz)', 'malloc(n)', 'malloc(gfdssize)',
        'malloc(size*sizeof(gid_t))', 'malloc(size*sizeof(gid_t))',
        'calloc(maxevents,sizeof(structepoll_event))',
        'calloc(maxevents,sizeof(structepoll_event_linux))'],
    'blink/ioctl.c': ['malloc(bufsize)', 'malloc(bufsize)'],
    'blink/ancillary.c': ['calloc(1,kMaxAncillary)'],
    'blink/loader.c': ['calloc(1,n+1)', 'calloc(1,n+1)',
        'malloc(((n=CountStrList(a)+CountStrList(b))+2)*sizeof(*c))'],
}
FREELIST_MEMBERS = {
    'blink/memory.c': ['p=m->freelist.p;', 'n=m->freelist.n+1;',
        'if((p=realloc(p,n*sizeof(*m->freelist.p)))){',
        'm->freelist.p=(void**)p;', 'm->freelist.n=n;', 'm->freelist.p[n-1]=mem;'],
    'blink/memorymalloc.c': ['free(m->freelist.p);',
        'memset(&m->freelist,0,sizeof(m->freelist));',
        'for(i=mark;i<m->freelist.n;++i){', 'free(m->freelist.p[i]);', 'm->freelist.n=mark;'],
    'blink/syscall.c': ['mark=m->freelist.n;'],
    'blink/reset.c': ['memset(&m->freelist,0,sizeof(m->freelist));'],
}
OWNING_VARIABLE_REGIONS = [
    ('void *AddToFreeList(', '// Returns pointer to memory in guest memory.',
     'c505e53eccbcb6d867872d542da76fd354dc13f4daae66164905bc275acf6038'),
    ('static void *BlinkGuestSchlepAccess(', '\n\nvoid *Schlep(',
     'f373d8f49db60d7a9049b102116e73da3ed8fc8d71876aa2b049aed9359183aa'),
    ('char *LoadStr(', '// Copies string from guest memory.',
     '14a9436522c268c4b429ede9685d116709d5f9a4f51a169753bbf55c1c45516f'),
    ('char **CopyStrList(', '\nint BlinkGuestPhysicalCodeProtocol',
     'b7ba1fd637211881af7b47e2c39972a794a3bbd6e76c3801cb3201be8e54a316'),
]


def audit_freelist(path, source):
    clean = re.sub(r'/\*.*?\*/|//[^\n]*', '', source, flags=re.S)
    arguments = []
    for match in re.finditer(r'\bAddToFreeList\s*\(', clean):
        first = match.end()
        depth, last = 1, first
        while depth and last < len(clean):
            if clean[last] == '(':
                depth += 1
            elif clean[last] == ')':
                depth -= 1
            last += 1
        argument = clean[first:last - 1]
        if re.match(r'\s*m\s*,', argument):
            arguments.append(re.sub(r'\s+', '', argument).removeprefix('m,'))
        elif not argument.startswith('struct Machine *m,'):
            raise ValueError('R74 unknown freelist owner: ' + path)
    if arguments != FREELIST_ARGUMENTS.get(path, []):
        raise ValueError('R74 owning-allocation inventory differs: ' + path)
    members = [''.join(line.split()) for line in clean.splitlines() if '->freelist' in line]
    if members != FREELIST_MEMBERS.get(path, []):
        raise ValueError('R74 freelist consumer inventory differs: ' + path)
    if path == 'blink/memory.c':
        for begin, end, expected in OWNING_VARIABLE_REGIONS:
            first = source.index(begin)
            part = source[first:source.index(end, first)]
            if hashlib.sha256(part.encode()).hexdigest() != expected:
                raise ValueError('R74 allocator-root provenance differs: ' + begin)


def once(source, before, after):
    if source.count(before) != 1:
        raise ValueError('R74 copy boundary anchor differs: ' + before[:100])
    return source.replace(before, after, 1)


def region(source, begin, end, transform):
    if source.count(begin) != 1 or source.count(end) != 1:
        raise ValueError('R74 function boundary differs: ' + begin)
    first = source.index(begin)
    last = source.index(end, first)
    return source[:first] + transform(source[first:last]) + source[last:]


def calls(source, before, after, count):
    if source.count(before) != count:
        raise ValueError('R74 call inventory differs: ' + before)
    return source.replace(before, after)


COPY_HELPERS = r'''
/* PAGE_LOCK/passive lifetime is distinct from actual-use exclusion. */
u8 *BlinkGuestLookupAddressCopy(struct Machine *m, i64 addr, u64 mask, u64 need) {
  return BlinkGuestLookupAddress2Access(m, addr, mask, need, false);
}

static void BlinkGuestReadCopy(struct Machine *m, void *dst, void *src, u64 n) {
#ifdef BLINK_MANAGED_GUEST_THREADS
  blink_host_guest_read_copy(m, dst, src, n);
#else
  memcpy(dst, src, n);
#endif
}

static void BlinkGuestWriteCopy(struct Machine *m, void *dst, void *src, u64 n) {
#ifdef BLINK_MANAGED_GUEST_THREADS
  blink_host_guest_write_copy(m, dst, src, n);
#else
  memcpy(dst, src, n);
#endif
}

static size_t BlinkGuestStringCopy(struct Machine *m, char *dst, char *src,
                                  size_t n) {
#ifdef BLINK_MANAGED_GUEST_THREADS
  return blink_host_guest_string_copy(m, (u8 *)dst, (u8 *)src, n);
#else
  size_t i;
  for (i = 0; i < n; ++i) if (!(dst[i] = src[i])) return i + 1;
  return 0;
#endif
}
'''

SNAPSHOT = r'''
static void *BlinkGuestSnapshotAlloc(struct Machine *m, size_t size) {
#ifdef BLINK_MANAGED_GUEST_THREADS
  return blink_host_guest_snapshot_alloc(m, size);
#else
  return AddToFreeList(m, malloc(size));
#endif
}

static void *BlinkGuestSnapshotResize(struct Machine *m, void *copy,
                                      size_t size, int slot) {
#ifdef BLINK_MANAGED_GUEST_THREADS
  return blink_host_guest_snapshot_resize(m, copy, size, slot);
#else
  void *grown;
  if (!(grown = realloc(copy, size))) return 0;
  m->freelist.p[slot] = grown;
  return grown;
#endif
}

/* Owned descriptors never escape as guest aliases, even within one page.
 * Keep the caller's exact mask/need checks; each actual copy is <= one page. */
void *BlinkGuestSnapshot(struct Machine *m, i64 addr, size_t size,
                         u64 mask, u64 need) {
  char *copy;
  u8 *page;
  size_t copied, n;
  if (!size) return 0;
  if (!(copy = (char *)BlinkGuestSnapshotAlloc(m, size))) return 0;
  for (copied = 0; copied < size; copied += n) {
    if (!(page = BlinkGuestLookupAddressCopy(m, addr + copied, mask, need))) {
      return 0; /* Already freelisted, including managed-exception unwind. */
    }
    n = MIN(size - copied, 4096 - ((addr + copied) & 4095));
    BlinkGuestReadCopy(m, copy + copied, page, n);
  }
  return copy;
}

'''

LOAD_STRING = r'''char *LoadStr(struct Machine *m, i64 addr) {
  char *copy, *grown, *page;
  size_t have, capacity, n, found;
  int slot;
  if (!addr) return 0;
  capacity = 4096 - (addr & 4095);
  slot = m->freelist.n;
  if (!(copy = (char *)BlinkGuestSnapshotAlloc(m, capacity))) return 0;
  for (have = 0;;) {
    if (!(page = (char *)BlinkGuestLookupAddressCopy(m, addr + have,
                                                    PAGE_U, PAGE_U))) break;
    n = capacity - have;
    found = BlinkGuestStringCopy(m, copy + have, page, n);
    if (found) {
      SetReadAddr(m, addr, have + found);
      return copy;
    }
    have = capacity;
    if (capacity > (size_t)-1 - 4096) { errno = ENOMEM; break; }
    capacity += 4096;
    if (!(grown = (char *)BlinkGuestSnapshotResize(m, copy, capacity, slot))) break;
    copy = grown;
  }
  return 0; /* Freelist also owns the buffer on fault/exception exits. */
}

'''


def memory(source):
    if 'static u8 *BlinkGuestLookupAddress2Access(' not in source:
        raise ValueError('R74 requires the authored physical memory adapter first')
    # The vector's retained capacity is a real allocation, not an allowance
    # released with the last payload. Keep it charged through its actual free.
    source = once(source, 'void *AddToFreeList(struct Machine *m, void *mem) {',
                  '''void *AddToFreeList(struct Machine *m, void *mem) {
#ifdef BLINK_MANAGED_GUEST_THREADS
  void *result = blink_host_guest_freelist_append(m, mem);
  if (result) STATISTIC(++freelisted);
  return result;
#else''')
    source = once(source, '''    free(mem);
    return 0;
  }
}''', '''    free(mem);
    return 0;
  }
#endif
}''')
    source = once(source, 'int VirtualCopy(struct Machine *m, i64 v, char *r, u64 n, bool d) {',
                  COPY_HELPERS + '\nint VirtualCopy(struct Machine *m, i64 v, char *r, u64 n, bool d) {')

    def virtual_copy(part):
        part = once(part, 'BlinkGuestLookupAddress2Access(m, v, need, need, !d)',
                    'BlinkGuestLookupAddressCopy(m, v, need, need)')
        part = once(part, 'memcpy(r, p, k);', 'BlinkGuestReadCopy(m, r, p, k);')
        return once(part, 'memcpy(p, r, k);', 'BlinkGuestWriteCopy(m, p, r, k);')

    source = region(source, 'int VirtualCopy(', '\nint CopyFromUser(', virtual_copy)
    source = once(source, 'void *SchlepR(struct Machine *m, i64 addr, size_t size) {',
                  SNAPSHOT + 'void *SchlepR(struct Machine *m, i64 addr, size_t size) {')
    source = once(source, 'return BlinkGuestSchlepAccess(m, addr, size, PAGE_U, PAGE_U, false);',
                  'return BlinkGuestSnapshot(m, addr, size, PAGE_U, PAGE_U);')
    source = region(source, 'char *LoadStr(struct Machine *m, i64 addr) {',
                    '// Copies string from guest memory.', lambda part: LOAD_STRING)
    # LoadStr is now already owned and freelisted. Avoid a duplicate strdup
    # allocation escaping the new quota when CopyStr/CopyStrList consume it.
    source = once(source, '  return (char *)AddToFreeList(m, strdup(s));',
                  '  return s;')
    # CopyStrList must not consume its uninitialized descriptor after a fault.
    source = once(source, '    CopyFromUserRead(m, b, addr + n * 8 - 8, 8);',
                  '    if (CopyFromUserRead(m, b, addr + n * 8 - 8, 8) == -1) {\n'
                  '      free(list);\n      return 0;\n    }')
    source += '\nint BlinkGuestCopyProtocol(void) { return 1; }\n'
    return source


STORE_HELPER = r'''
void BlinkGuestTidStoreRaw(struct Machine *m, u8 *pointer, u32 value,
                           int sequential) {
  if (sequential) {
    atomic_store_explicit((_Atomic(u32) *)pointer, value, memory_order_seq_cst);
  } else {
    atomic_store_explicit((_Atomic(u32) *)pointer, value, memory_order_release);
  }
}

static void BlinkGuestTidStore(struct Machine *m, u8 *pointer, u32 value,
                               int sequential) {
#ifdef BLINK_MANAGED_GUEST_THREADS
  blink_host_guest_store32(m, pointer, value, sequential);
#else
  BlinkGuestTidStoreRaw(m, pointer, value, sequential);
#endif
}

'''


def syscall(source):
    source = once(source, '''static int SysSigaction(struct Machine *m, int sig, i64 act, i64 old,
                        u64 sigsetsize) {''', '''static int SysSigaction(struct Machine *m, int sig, i64 act, i64 old,
                        u64 sigsetsize) {
#ifdef BLINK_MANAGED_GUEST_THREADS
  return blink_host_guest_sigaction(m, sig, act, old, sigsetsize);
#else
  return BlinkGuestSigaction(m, sig, act, old, sigsetsize);
#endif
}

int BlinkGuestSigaction(struct Machine *m, int sig, i64 act, i64 old,
                       u64 sigsetsize) {''')
    source = once(source, 'void ClearChildTid(struct Machine *m) {',
                  STORE_HELPER + 'void ClearChildTid(struct Machine *m) {')
    source = once(source, 'static i64 HandleSigpipe(struct Machine *m, i64 rc, int flags) {', '''static i64 HandleSigpipe(struct Machine *m, i64 rc, int flags) {
#ifdef BLINK_MANAGED_GUEST_THREADS
  return blink_host_guest_handle_sigpipe(m, rc, flags);
#else
  return BlinkGuestHandleSigpipe(m, rc, flags);
#endif
}

i64 BlinkGuestHandleSigpipe(struct Machine *m, i64 rc, int flags) {''')
    # Only immediate self delivery needs frame admission; no reader survives
    # the foreign-thread notification syscall or recursive SignalActor.
    start = '      LOCK(&m->system->sig_lock);\n      switch (Read64(m->system->hands[sig - 1].handler)) {'
    end = '      UNLOCK(&m->system->sig_lock);\n      return rc;'
    if source.count(start) != 1 or source.count(end) != 1:
        raise ValueError('R74 immediate tkill signal-lock scope differs')
    first = source.index(start); last = source.index(end, first) + len(end)
    block = source[first:last]
    helper = 'int BlinkGuestTkillSelf(struct Machine *m, int sig) {\n  int rc;\n' + block + '\n}\n\n'
    source = source[:first] + '''#ifdef BLINK_MANAGED_GUEST_THREADS
      return blink_host_guest_tkill_self(m, sig);
#else
      return BlinkGuestTkillSelf(m, sig);
#endif''' + source[last:]
    source = once(source, 'static int SysTkill(struct Machine *m, int tid, int sig) {',
                  helper + 'static int SysTkill(struct Machine *m, int tid, int sig) {')
    # Every listed LookupAddress site in this selected syscall version is
    # lifetime-only: wake identity, futex compare, or TID stores bracketed below.
    # A new call invalidates the inventory rather than inheriting an exemption.
    source = calls(source, 'LookupAddress(m, ', 'BlinkGuestLookupAddressCopy(m, ', 7)
    source, found = re.subn(r'BlinkGuestLookupAddressCopy\(m, (uaddr|m->ctid|ctid|ptid)\)',
                            r'BlinkGuestLookupAddressCopy(m, \1, Cpl(m) == 3 ? PAGE_U : 0, Cpl(m) == 3 ? PAGE_U : 0)', source)
    if found != 7:
        raise ValueError('R74 syscall lifetime resolver argument inventory differs')
    for before, after in [
        ('atomic_store_explicit(ctid, 0, memory_order_seq_cst);',
         'BlinkGuestTidStore(m, (u8 *)ctid, 0, 1);'),
        ('atomic_store_explicit(ctid_ptr, Little32(newpid), memory_order_release);',
         'BlinkGuestTidStore(m, (u8 *)ctid_ptr, Little32(newpid), 0);'),
        ('atomic_store_explicit(ctid_ptr, Little32(tid), memory_order_release);',
         'BlinkGuestTidStore(m, (u8 *)ctid_ptr, Little32(tid), 0);'),
        ('atomic_store_explicit(ptid_ptr, Little32(tid), memory_order_release);',
         'BlinkGuestTidStore(m, (u8 *)ptid_ptr, Little32(tid), 0);'),
    ]:
        source = once(source, before, after)

    # Move exactly the initial compare/queue registration into an exported
    # helper. Admission is acquired by its managed wrapper before either lock.
    start = '  LOCK(&g_bus->futexes.lock);\n  if (Load32(mem) != expect) {'
    end = '  UNLOCK(&g_bus->futexes.lock);\n  for (;;) {'
    if source.count(start) != 1 or source.count(end) != 1:
        raise ValueError('R74 futex compare/enqueue version differs')
    a = source.index(start); b = source.index(end, a) + len('  UNLOCK(&g_bus->futexes.lock);\n')
    block = source[a:b]
    if block.count('free(waiter);') != 2 or block.count('Load32(mem)') != 1:
        raise ValueError('R74 futex enqueue exits differ')
    helper = '''int BlinkGuestFutexEnqueue(struct Machine *m, u8 *mem, u32 expect,
                           u64 key, int kind, u32 mask, void *opaque,
                           void **result) {
  struct Futex *f;
  struct BlinkFutexWaiter *waiter = (struct BlinkFutexWaiter *)opaque;
''' + block + '''  *result = f;
  return 0;
}

'''
    invoke = '''#ifdef BLINK_MANAGED_GUEST_THREADS
  rc = blink_host_guest_futex_enqueue(m, mem, expect, key, kind, mask, waiter,
                                       (void **)&f);
#else
  rc = BlinkGuestFutexEnqueue(m, mem, expect, key, kind, mask, waiter, (void **)&f);
#endif
  if (rc == -1) return -1;
'''
    source = source[:a] + invoke + source[b:]
    source = once(source, 'static int SysFutexWait(struct Machine *m,',
                  helper + 'static int SysFutexWait(struct Machine *m,')

    # Read/write descriptor validation must not lend writable pointers over
    # blocking operations. These exact selected sites copy back explicitly.
    source = calls(source, 'SchlepRW(', 'BlinkGuestSnapshotRW(', 8)
    # This target belongs to currently disabled robust cleanup. Keep it a real
    # writable alias, never a snapshot whose CAS would update only owned bytes.
    # Enabling robust cleanup later still requires its own short RMW wrapper.
    source = once(source, '(_Atomic(u32) *)SchlepR(m, futex_addr, 4)',
                  '(_Atomic(u32) *)SchlepRW(m, futex_addr, 4)')
    source = once(source, 'static int LoadTimespec(struct Machine *m,',
                  '''static void *BlinkGuestSnapshotRW(struct Machine *m, i64 addr, size_t n) {
  SetReadAddr(m, addr, n);
  SetWriteAddr(m, addr, n);
  return BlinkGuestSnapshot(m, addr, n, PAGE_U | PAGE_RW, PAGE_U | PAGE_RW);
}

static int LoadTimespec(struct Machine *m,''')
    source = once(source, 'Schlep(m, addr, sizeof(*gt), mask,',
                  'BlinkGuestSnapshot(m, addr, sizeof(*gt), mask,')
    source = once(source, '    Write32(psize, sizeof(gval));',
                  '    Write32(psize, sizeof(gval));\n    CopyToUserWrite(m, optvalsizeaddr, psize, 4);')
    source = once(source, '    Write32(psize, sizeof(gl));',
                  '    Write32(psize, sizeof(gl));\n    CopyToUserWrite(m, optvalsizeaddr, psize, 4);')
    source = once(source, '    if (offsetp) Write64(offsetp, offset + toto + got);',
                  '    if (offsetp) {\n      Write64(offsetp, offset + toto + got);\n'
                  '      if (CopyToUserWrite(m, offsetaddr, offsetp, 8) == -1) goto OnFailure;\n    }')
    # getresuid/gid used SchlepW aliases for direct stores. Preserve preflight
    # write validation, stage the scalar on the C stack, then bounded copyback.
    for kind, values in [('uid', ('uid', 'suid', 'euid')), ('gid', ('gid', 'sgid', 'egid'))]:
        begin = 'static i32 SysGetres' + kind + '('
        end = '\nstatic i32 SysGetresgid(' if kind == 'uid' else '\n#include "host-membarrier.h"'

        def identity(part, values=values):
            part = once(part, '  u8 *real = 0;\n  u8 *saved = 0;\n  u8 *effective = 0;',
                        '  u8 real[4], saved[4], effective[4];')
            for name in ('real', 'saved', 'effective'):
                before = f'!({name} = (u8 *)SchlepW(m, {name}addr, 4))'
                part = once(part, before, f'!IsValidMemory(m, {name}addr, 4, PROT_WRITE)')
            for name, value in zip(('real', 'saved', 'effective'), values):
                part = once(part, f'  if ({name}) Write32({name}, {value});',
                            f'  if ({name}addr) {{\n    Write32({name}, {value});\n'
                            f'    if (CopyToUserWrite(m, {name}addr, {name}, 4) == -1) return -1;\n  }}')
            return part

        source = region(source, begin, end, identity)
    return source


def ioctl(source):
    # All four scalar outputs previously exposed SchlepW across a host call.
    for begin, end, name, value in [
        ('static int IoctlTiocgpgrp(', 'static int IoctlTiocspgrp(', 'pgrp', 'rc'),
        ('static int IoctlTiocgsid(', 'static int XlatFlushQueue(', 'sid', 'rc'),
        ('static int IoctlSiocatmark(', 'static int IoctlGetInt32(', 'p', 'rc'),
        ('static int IoctlGetInt32(', 'static int IoctlSetInt32(', 'p', 'val'),
    ]:
        def scalar(part, name=name, value=value):
            part = once(part, f'  u8 *{name};', f'  u8 {name}[4];')
            part = once(part, f'  if (!({name} = (u8 *)SchlepW(m, addr, 4))) return -1;',
                        '  if (!IsValidMemory(m, addr, 4, PROT_WRITE)) return -1;')
            return once(part, f'  Write32({name}, {value});',
                        f'  Write32({name}, {value});\n'
                        f'  if (CopyToUserWrite(m, addr, {name}, 4) == -1) return -1;')
        source = region(source, begin, end, scalar)
    source = once(source, 'SchlepRW(\n            m, ifconf_addr, sizeof(*ifconf_linuxp))',
                  'BlinkGuestSnapshot(\n            m, ifconf_addr, sizeof(*ifconf_linuxp), PAGE_U | PAGE_RW, PAGE_U | PAGE_RW)')
    return source


def adapt(path, content):
    if b'/* Full-JIT operation-specific semantic clones. */' in content:
        raise ValueError('R74 copy boundaries require pre-clone source: ' + path)
    if (path == 'blink/signal.c' and b'int BlinkGuestConsumeSignal(' in content or
            path == 'blink/throw.c' and b'void BlinkGuestDeliverSignalToUser(' in content):
        raise ValueError('R74 duplicate signal adaptation: ' + path)
    if path.startswith('blink/') and path.endswith('.c'):
        audit_freelist(path, content.decode())
    if path not in ('blink/memory.c', 'blink/memorymalloc.c', 'blink/reset.c', 'blink/signal.c', 'blink/throw.c', 'blink/iovs.c', 'blink/syscall.c', 'blink/ioctl.c'):
        return content
    source = content.decode()
    if path == 'blink/memory.c':
        source = memory(source)
    elif path == 'blink/memorymalloc.c':
        source = once(source, '  free(m->freelist.p);', '''#ifdef BLINK_MANAGED_GUEST_THREADS
  blink_host_guest_freelist_free(m);
#else
  free(m->freelist.p);
#endif''')
        # Selected AddToFreeList callers supply owning malloc/calloc/realloc/
        # strdup roots. Their low bit is zero. Only R74 allocation publication
        # tags a root; CollectGarbage is the sole selected freelist consumer.
        source = once(source, '    free(m->freelist.p[i]);', '''    void *item = m->freelist.p[i];
    m->freelist.p[i] = 0; /* Retry after exceptional cleanup cannot double-free. */
#ifdef BLINK_MANAGED_GUEST_THREADS
    if ((uintptr_t)item & 1) {
      blink_host_guest_snapshot_free(m, (void *)((uintptr_t)item & ~(uintptr_t)1));
    } else
#endif
    {
      free(item);
    }''')
    elif path == 'blink/reset.c':
        source = once(source, '  memset(&m->freelist, 0, sizeof(m->freelist));',
                      '  /* Keep owning staging alive through CPU reset. NewMachine initializes\n'
                      '   * its own empty list; only CollectGarbage may release allocations. */')
    elif path == 'blink/signal.c':
        source = once(source, 'int ConsumeSignal(struct Machine *m, int *delivered, bool *restart) {',
                      'int BlinkGuestConsumeSignal(struct Machine *m, int *delivered, bool *restart) {')
        source = once(source, 'void EnqueueSignal(struct Machine *m, int sig) {', '''int ConsumeSignal(struct Machine *m, int *delivered, bool *restart) {
#ifdef BLINK_MANAGED_GUEST_THREADS
  return blink_host_guest_consume_signal(m, delivered, restart);
#else
  return BlinkGuestConsumeSignal(m, delivered, restart);
#endif
}

void EnqueueSignal(struct Machine *m, int sig) {''')
    elif path == 'blink/throw.c':
        source = once(source, 'void DeliverSignalToUser(struct Machine *m, int sig, int code) {', '''void DeliverSignalToUser(struct Machine *m, int sig, int code) {
#ifdef BLINK_MANAGED_GUEST_THREADS
  blink_host_guest_deliver_signal_user(m, sig, code);
#else
  BlinkGuestDeliverSignalToUser(m, sig, code);
#endif
}

void BlinkGuestDeliverSignalToUser(struct Machine *m, int sig, int code) {''')
    elif path == 'blink/iovs.c':
        source = once(source, 'LookupAddress2(m, addr, mask, need)',
                      'BlinkGuestLookupAddressCopy(m, addr, mask, need)')
    elif path == 'blink/syscall.c':
        source = syscall(source)
    else:
        source = ioctl(source)
    return source.encode()
