"""Reviewed memory-only adaptations applied to fresh host-bound source copies.

The original function hash gates the one whole-function replacement. Other
changes require unique exact anchors. Native focused checks use this same code.
"""
import hashlib
import json
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
INPUTS = ('scripts/memory_bindings.py', 'src/Host/guest-private-mapping.inc',
          'config/private-mapping-adaptations.json')


def replace_once(source, before, after):
    if source.count(before) != 1:
        raise ValueError('Memory binding anchor differs: ' + before[:120])
    return source.replace(before, after, 1)


def adapt(path, content):
    source = content.decode()
    if path == 'blink/memorymalloc.c':
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
    elif path == 'blink/memory.c':
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
    elif path == 'blink/throw.c':
        source = replace_once(source,
            '      DeliverSignalToUser(m, SIGSEGV_LINUX,\n'
            '                          m->segvcode ? m->segvcode : SEGV_MAPERR_LINUX);',
            '      DeliverSignalToUser(m, m->segvcode == -BUS_ADRERR_LINUX ?\n'
            '                          SIGBUS_LINUX : SIGSEGV_LINUX,\n'
            '                          m->segvcode == -BUS_ADRERR_LINUX ? BUS_ADRERR_LINUX :\n'
            '                          m->segvcode ? m->segvcode : SEGV_MAPERR_LINUX);')
    elif path == 'blink/syscall.c':
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
