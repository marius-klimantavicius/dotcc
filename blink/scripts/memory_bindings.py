"""Reviewed memory-only adaptations applied to fresh host-bound source copies.

The original function hash gates the one whole-function replacement. Other
changes require unique exact anchors. Native focused checks use this same code.
"""
import hashlib
import json
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
INPUTS = ('scripts/memory_bindings.py', 'src/Host/guest-private-mapping.inc',
          'config/private-mapping-adaptations.json', 'src/Host/guest-memfd.inc')


def replace_once(source, before, after):
    if source.count(before) != 1:
        raise ValueError('Memory binding anchor differs: ' + before[:120])
    return source.replace(before, after, 1)


def adapt(path, content):
    source = content.decode()
    if path == 'blink/elf.c':
        # GNU ld may emit an unused, empty PT_LOAD at address zero when a
        # valid static executable has no writable data. It contributes no
        # address range; Linux executes the remaining nonempty load segments.
        source = replace_once(source,
            '      if (Read32(phdr->type) == PT_LOAD_) {\n',
            '      if (Read32(phdr->type) == PT_LOAD_ &&\n'
            '          (Read64(phdr->memsz) || Read64(phdr->filesz))) {\n')
    elif path == 'blink/memorymalloc.c':
        marker = 'i64 ReserveVirtual('
        if source.count(marker) != 1:
            raise ValueError('ReserveVirtual boundary differs')
        start = source.index(marker)
        original = source[start:source.index('\n}', start) + 3]
        # The preceding thread adapter adds only this reviewed early guard.
        normalized = original.replace(
            '  // The authored host has no shared backing. Reject before RemoveVirtual.\n'
            '  if (shared) return enotsup();\n\n', '')
        pins = json.loads((ROOT / INPUTS[2]).read_text())
        if hashlib.sha256(normalized.encode()).hexdigest() != pins['ReserveVirtual_sha256']:
            raise ValueError('Reviewed ReserveVirtual source identity differs')
        source = replace_once(source, original, (ROOT / INPUTS[1]).read_text())
        # An absent table entry covers only the remainder of its span from
        # this address. A full-span stride can skip an occupied next entry
        # when an mmap hint starts partway through the absent subtree.
        source = replace_once(source,
            '    got += (u64)1 << i;\n',
            '    got += ((u64)1 << i) - (((u64)virt + got) & (((u64)1 << i) - 1));\n')
        source = replace_once(source,
            '  for (end = virt + size; virt < end; virt += (u64)1 << i) {\n',
            '  for (end = virt + size; virt < end;\n'
            '       virt += ((u64)1 << i) - ((u64)virt & (((u64)1 << i) - 1))) {\n')
        source = replace_once(source, '  key = SetProtection(prot);\n',
            '#ifdef BLINK_MANAGED_GUEST_THREADS\n'
            '  blink_host_guest_mapping_revoke(virt, size);\n'
            '#endif\n'
            '  if (BlinkGuestFileProtection(s, virt, size, prot)) return -1;\n'
            '  key = SetProtection(prot);\n')
        source = replace_once(source,
            '  MEM_LOGF("RemoveVirtual(%#" PRIx64 ", %#" PRIx64 ")", virt, size);\n',
            '#ifdef BLINK_MANAGED_GUEST_THREADS\n'
            '  blink_host_guest_mapping_revoke(virt, size);\n'
            '#endif\n'
            '  MEM_LOGF("RemoveVirtual(%#" PRIx64 ", %#" PRIx64 ")", virt, size);\n')
        source = replace_once(source,
            'static void FreeHostPages(struct System *s) {\n',
            'static void FreeHostPages(struct System *s) {\n'
            '#ifdef BLINK_MANAGED_GUEST_THREADS\n'
            '  blink_host_guest_mapping_revoke(0, 0x800000000000l);\n'
            '#endif\n')
        # The threaded predecessor appends this reviewed advice implementation.
        # Keep the new protocol in the host adaptation, after that derivation,
        # without rewriting its independently pinned thread/mremap patch.
        if 'int BlinkGuestMadvise(' in source:
            source = replace_once(source,
                '  /* Check backing before changing any page. DONTNEED on file mappings needs\n',
                '#ifdef BLINK_MANAGED_GUEST_THREADS\n'
                '  blink_host_guest_mapping_revoke(addr, len);\n'
                '#endif\n'
                '  /* Check backing before changing any page. DONTNEED on file mappings needs\n')
    elif path == 'blink/memory.c':
        # AllocateAnonymousPage returns a PAGE_HOST registry identity. A lost
        # publication CAS must recycle its resolved host page, never that index.
        source = replace_once(source,
            '        FreeAnonymousPage(m->system, (u8 *)(uintptr_t)(page & PAGE_TA));\n',
            '        FreeAnonymousPage(m->system, FindHostPage(page));\n')
        source = replace_once(source,
            'u8 *LookupAddress2(struct Machine *m, i64 virt, u64 mask, u64 need) {\n',
            '#ifdef BLINK_MANAGED_GUEST_THREADS\n'
            '/* Callable producer capability: all revocation anchors must stage\n'
            ' * successfully before this profile can be emitted. */\n'
            'int BlinkGuestCodeRevocationProtocol(void) { return 1; }\n'
            'int BlinkGuestMappingRevocationProtocol(void) { return 1; }\n'
            '#endif\n\n'
            'u8 *LookupAddress2(struct Machine *m, i64 virt, u64 mask, u64 need) {\n')
        source = replace_once(source,
            '  u8 *host;\n  u64 entry;\n',
            '  u8 *host;\n  u64 entry;\n'
            '#ifdef BLINK_MANAGED_GUEST_THREADS\n'
            '  /* Only instruction fetch is allowed to borrow an enrolled page.\n'
            '   * Revoke before every data lookup, including unchecked writers\n'
            '   * and benign reads, before a pointer or stash can escape. */\n'
            '  if (mask != PAGE_XD || need)\n'
            '    blink_host_guest_code_revoke(virt & -4096, 4096);\n'
            '#endif\n')
        # CopyToUser and CopyToUserWrite both reach VirtualCopy, including
        # syscall outputs with no separate IsValidMemory check (e.g. uname).
        # Check each destination page at the actual lookup, before memcpy;
        # access bookkeeping after the copy does not enforce permissions.
        # Keep CopyFromUser and non-user-mode lookup behavior unchanged.
        source = replace_once(source,
            'int VirtualCopy(struct Machine *m, i64 v, char *r, u64 n, bool d) {\n'
            '  u8 *p;\n'
            '  u64 k;\n',
            'int VirtualCopy(struct Machine *m, i64 v, char *r, u64 n, bool d) {\n'
            '  u8 *p;\n'
            '  u64 k, need;\n'
            '  need = Cpl(m) == 3 ? PAGE_U | (d ? 0 : PAGE_RW) : 0;\n')
        source = replace_once(source,
            '    if (!(p = LookupAddress(m, v))) return -1;\n',
            '    if (!(p = LookupAddress2(m, v, need, need))) return -1;\n')
        source = replace_once(source,
            '  if ((entry & PAGE_RSRV) && !(entry = HandlePageFault(m, pslot, entry))) {',
            '  /* Never commit or cache a whole file page beyond snapshot EOF. */\n'
            '  if ((entry & PAGE_MUG) &&\n'
            '      !BlinkHostMemoryFilePageAvailable(FindHostPage(entry))) {\n'
            '    return entry;  /* permission checks precede EOF faults */\n'
            '  }\n'
            '  if ((entry & PAGE_RSRV) && !(entry = HandlePageFault(m, pslot, entry))) {')
        source = replace_once(source,
            '#ifndef DISABLE_JIT\n  if ((need & PAGE_RW) &&',
            '  if ((entry & PAGE_MUG) &&\n'
            '      !BlinkHostMemoryFilePageAvailable(FindHostPage(entry))) {\n'
            '    m->segvcode = -BUS_ADRERR_LINUX;\n'
            '    return (u8 *)efault0();\n'
            '  }\n'
            '#ifndef DISABLE_JIT\n  if ((need & PAGE_RW) &&')
        source = replace_once(source,
            '    if ((pte & mask) != need) {\n',
            '    if ((pte & mask) != need || ((pte & PAGE_MUG) &&\n'
            '        !BlinkHostMemoryFilePageAvailable(FindHostPage(pte)))) {\n')
    elif path == 'blink/loader.c':
        source = replace_once(source,
            '  elf = &m->system->elf;\n',
            '#ifdef BLINK_MANAGED_GUEST_THREADS\n'
            '  /* A subsequent LoadProgram replaces the root table directly. */\n'
            '  blink_host_guest_mapping_revoke(0, 0x800000000000l);\n'
            '#endif\n'
            '  elf = &m->system->elf;\n')
    elif path == 'blink/throw.c':
        source = replace_once(source,
            '      DeliverSignalToUser(m, SIGSEGV_LINUX,\n'
            '                          m->segvcode ? m->segvcode : SEGV_MAPERR_LINUX);',
            '      DeliverSignalToUser(m, m->segvcode == -BUS_ADRERR_LINUX ?\n'
            '                          SIGBUS_LINUX : SIGSEGV_LINUX,\n'
            '                          m->segvcode == -BUS_ADRERR_LINUX ? BUS_ADRERR_LINUX :\n'
            '                          m->segvcode ? m->segvcode : SEGV_MAPERR_LINUX);')
    elif path == 'blink/open.c':
        source = replace_once(source,
            '  RESTARTABLE(fildes = VfsOpen(GetDirFildes(dirfildes), path, sysflags, mode));\n'
            '  if (fildes != -1) {\n',
            '  RESTARTABLE(fildes = VfsOpen(GetDirFildes(dirfildes), path, sysflags, mode));\n'
            '  if (fildes != -1) {\n'
            '    if (oflags & O_TRUNC_LINUX) InvalidateSystem(m->system, true, true);\n')
    elif path == 'blink/syscall.c':
        source = replace_once(source,
            '  rc = VfsFtruncate(fd, length);\n  VfsClose(fd);\n  return rc;\n',
            '  rc = VfsFtruncate(fd, length);\n  VfsClose(fd);\n'
            '  if (!rc) InvalidateSystem(m->system, true, true);\n  return rc;\n')
        source = replace_once(source,
            '  RESTARTABLE(rc = VfsFtruncate(fildes, length));\n'
            '  return rc;\n',
            '  RESTARTABLE(rc = VfsFtruncate(fildes, length));\n'
            '  /* Shared memfd EOF changes must also retire cached codehost pointers. */\n'
            '  if (!rc) InvalidateSystem(m->system, true, true);\n'
            '  return rc;\n')
        # The thread-stage predecessor rejects shared mappings. Replace only
        # that exact authored guard; base single-thread staging has no guard.
        shared_guard = (
            '  if ((flags & MAP_TYPE_LINUX) == MAP_SHARED_LINUX ||\n'
            '      (flags & MAP_TYPE_LINUX) == (MAP_SHARED_LINUX | MAP_PRIVATE_LINUX)) {\n'
            '    return enotsup();\n'
            '  }\n'
            '  if ((flags & MAP_TYPE_LINUX) != MAP_PRIVATE_LINUX) return einval();\n')
        if shared_guard in source:
            source = replace_once(source, shared_guard,
                '  if ((flags & MAP_TYPE_LINUX) == (MAP_SHARED_LINUX | MAP_PRIVATE_LINUX))\n'
                '    return enotsup();\n'
                '  if ((flags & MAP_TYPE_LINUX) != MAP_PRIVATE_LINUX &&\n'
                '      (flags & MAP_TYPE_LINUX) != MAP_SHARED_LINUX) return einval();\n'
                '  if ((flags & MAP_TYPE_LINUX) == MAP_SHARED_LINUX &&\n'
                '      (flags & MAP_ANONYMOUS_LINUX)) return enotsup();\n')
        source = replace_once(source,
            'static int SysSocket(struct Machine *m, i32 family, i32 type, i32 protocol) {',
            (ROOT / INPUTS[3]).read_text() + '\n'
            'static int SysSocket(struct Machine *m, i32 family, i32 type, i32 protocol) {')
        source = replace_once(source,
            '    SYSCALL(3, 0x13E, "getrandom", SysGetrandom, STRACE_GETRANDOM);',
            '    SYSCALL(3, 0x13E, "getrandom", SysGetrandom, STRACE_GETRANDOM);\n'
            '    SYSCALL(2, 0x13F, "memfd_create", SysMemfdCreate, STRACE_2);')
        source = replace_once(source,
            '  i64 newautomap;\n  if (!IsValidAddrSize(virt, size)) return einval();',
            '  i64 newautomap;\n'
            '  if (!(flags & (MAP_FIXED_LINUX | MAP_FIXED_NOREPLACE_LINUX))) virt &= -4096;\n'
            '  if (!IsValidAddrSize(virt, size)) return einval();\n'
            '  size = ROUNDUP(size, 4096);\n'
            '  if (!IsValidAddrSize(virt, size)) return einval();')
        source = replace_once(source,
            '  if (flags & MAP_FIXED_LINUX) {\n    fixedmap = true;',
            '  if ((flags & MAP_FIXED_LINUX) && !(flags & MAP_FIXED_NOREPLACE_LINUX)) {\n'
            '    fixedmap = true;')
    return source.encode()
