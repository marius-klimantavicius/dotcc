"""Reviewed guest signal adaptations for fresh host-bound source copies.

Linux sigaltstack registration records a descriptor, including any guard page.
The existing signal delivery path checks the actual frame with CopyToUserWrite.
"""
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
INPUTS = ('scripts/signal_bindings.py',)


def adapt(path, content):
    if path != 'blink/syscall.c':
        return content
    source = content.decode()
    marker = 'static int SysSigaltstack(struct Machine *m, i64 newaddr, i64 oldaddr) {'
    if source.count(marker) != 1:
        raise ValueError('Guest signal binding SysSigaltstack boundary differs')
    start = source.index(marker)
    end = source.index('\n}', start) + 3
    original = source[start:end]
    probe = '''      if (!IsValidMemory(m, Read64(ss->sp), Read64(ss->size),
                         PROT_READ | PROT_WRITE)) {
        LOGF("sigaltstack ss_sp=%#" PRIx64 " ss_size=%#" PRIx64
             " didn't exist with read+write permission",
             Read64(ss->sp), Read64(ss->size));
        return -1;
      }
'''
    if original.count(probe) != 1:
        raise ValueError('Guest signal binding registration probe differs')
    replacement = original.replace(probe,
        '      // Linux registers the descriptor without probing stack pages.\n'
        '      // Delivery checks the actual frame with CopyToUserWrite.\n', 1)
    return (source[:start] + replacement + source[end:]).encode()
