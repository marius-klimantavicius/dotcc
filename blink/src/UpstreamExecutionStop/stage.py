#!/usr/bin/env python3
"""Stage two explicit cooperative execution-stop safe points in pinned Blink."""
import argparse
import difflib
import hashlib
import json
from pathlib import Path

HERE = Path(__file__).resolve().parent
ROOT = HERE.parents[1]
REVISION = 'f006a4fc6f9b8de9272504fdff0dbbe5ce5dc580'
SOURCE_PIN = '4eb3f54173ba37341b300e7668cc4ba3650578cc3d23d713573ffa486ae0e2c3'
BLOCK_PINS = {
    'bool CheckInterrupt(': 'e907f75f5d88bdfda6f10b49deb9a450637489ee2da8ece9e85ce085073c635c',
    'static int Poll(': 'c270251f91a5249085c6317057279a6738cb1d80e5ae10cfde6c399895e1c61b',
}
GUARD = '''#include "host-execution-stop.h"
#if !defined(DISABLE_JIT) || !defined(NOLINEAR)
#error Private execution-stop staging requires the nonlinear interpreter profile
#endif

'''
sha = lambda value: hashlib.sha256(value).hexdigest()


def adapt(original):
    if sha(original) != SOURCE_PIN:
        raise ValueError('Immutable upstream syscall.c hash differs')
    source = original.decode()
    blocks = []
    for marker, digest in BLOCK_PINS.items():
        if source.count(marker) != 1:
            raise ValueError('Function boundary differs: ' + marker)
        start = source.index(marker)
        end = source.index('\n}', start) + 3
        before = source[start:end]
        if sha(before.encode()) != digest:
            raise ValueError('Function hash differs: ' + marker)
        if marker.startswith('bool CheckInterrupt'):
            old = 'HandleSomeMoreInterrupts:\n'
            new = old + '''  /* Private owner stop is not guest signal delivery. Return normally so
     OpSyscall releases page locks, temporaries and syscall nesting state. */
  if (blink_host_execution_stop_reason()) {
    Put64(m->ax, -EINTR_LINUX);
    m->interrupted = true;
    errno = EINTR;
    return true;
  }
'''
        else:
            old = '      for (;;) {\n        for (i = 0; i < nfds; ++i) {'
            new = '''      for (;;) {
        /* Empty polls never enter the per-descriptor interrupt checkpoint. */
        if (blink_host_execution_stop_reason() && CheckInterrupt(m, false)) {
          rc = eintr();
          break;
        }
        for (i = 0; i < nfds; ++i) {'''
        if before.count(old) != 1:
            raise ValueError('Safe-point anchor differs: ' + marker)
        after = before.replace(old, new)
        source = source[:start] + after + source[end:]
        blocks.append({'function': marker, 'original_sha256': digest,
                       'replacement_sha256': sha(after.encode())})
    source = source.replace('bool CheckInterrupt(', GUARD + 'bool CheckInterrupt(', 1)
    patch = ''.join(difflib.unified_diff(original.decode().splitlines(True), source.splitlines(True),
                                       fromfile='a/blink/syscall.c', tofile='b/blink/syscall.c'))
    return source.encode(), patch.encode(), blocks


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--output', type=Path, required=True)
    parser.add_argument('--receipt', type=Path, required=True)
    args = parser.parse_args()
    for target in (args.output.resolve(), args.receipt.resolve()):
        if target.is_relative_to((ROOT/'ref').resolve()) or target.is_relative_to(HERE):
            raise SystemExit('Output must not replace immutable reference or authored staging sources')
    script_hash = sha(Path(__file__).read_bytes())
    patch_path = HERE/'execution-stop.patch'
    reviewed = patch_path.read_bytes()
    header = ROOT/'src/Host/include/host-execution-stop.h'
    header_hash = sha(header.read_bytes())
    original_path = ROOT/'ref'/('blink-' + REVISION)/'blink/syscall.c'
    original = original_path.read_bytes()
    staged, patch, blocks = adapt(original)
    if patch != reviewed:
        raise SystemExit('Generated adaptation differs from the reviewed execution-stop.patch')
    if (sha(Path(__file__).read_bytes()) != script_hash or patch_path.read_bytes() != reviewed
            or original_path.read_bytes() != original or sha(header.read_bytes()) != header_hash):
        raise SystemExit('Staging inputs changed during derivation')
    outputs = [args.output/'syscall.c', args.output/'execution-stop.patch', args.receipt]
    if any(path.exists() for path in outputs):
        raise SystemExit('Use fresh output and receipt paths; previous evidence is immutable')
    receipt = {
        'kind': 'reviewed-private-execution-stop-adaptation', 'upstream': REVISION,
        'stage_sha256': script_hash, 'patch_sha256': sha(reviewed),
        'scope': 'CheckInterrupt normal EINTR return and Poll outer-loop owner-stop safe point; nonlinear interpreter only',
        'required_headers': {'src/Host/include/host-execution-stop.h': header_hash},
        'sources': {'syscall.c': {'source_sha256': SOURCE_PIN, 'staged_sha256': sha(staged), 'blocks': blocks}},
    }
    args.output.mkdir(parents=True, exist_ok=True)
    (args.output/'syscall.c').write_bytes(staged)
    (args.output/'execution-stop.patch').write_bytes(reviewed)
    args.receipt.parent.mkdir(parents=True, exist_ok=True)
    with args.receipt.open('x') as stream:
        stream.write(json.dumps(receipt, indent=2) + '\n')


if __name__ == '__main__':
    main()
