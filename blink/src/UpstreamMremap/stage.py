#!/usr/bin/env python3
"""Stage only old-mapping validation ahead of the existing unsupported mremap path."""
import argparse
import difflib
import hashlib
import importlib.util
import json
from pathlib import Path
import sys

HERE = Path(__file__).resolve().parent
ROOT = HERE.parents[1]
REVISION = 'f006a4fc6f9b8de9272504fdff0dbbe5ce5dc580'
ORIGINAL_PIN = '4eb3f54173ba37341b300e7668cc4ba3650578cc3d23d713573ffa486ae0e2c3'
PREDECESSOR_PIN = 'b744c1a33c984b04f9809f534e364a1fd0535041c019b63b67a152653e2be05c'
BLOCK_PIN = '1ac9b2d866051b6ae2d2554ab8ebdf0f5992474df744d76ea67cd62188034f97'
MEMORY_PIN = '589becd0e214d5f422e75a9b63b1bf5d5280b3f8ca4e00dc212ede120e945b12'
sha = lambda data: hashlib.sha256(data).hexdigest()
VALIDATION = '''  // Validate ordinary source ranges using guest page-table reservations.
  // This does not implement resizing or relocation: mapped requests retain
  // the existing unsupported ENOMEM fallback below.
  if (!flags && old_size && new_size && old_size <= 0x1000000000000ull &&
      new_size <= 0x1000000000000ull && !(old_address & 4095) &&
      old_address >= -0x800000000000ll && old_address < 0x800000000000ll) {
    u64 old_pages = (old_size + 4095) & ~(u64)4095;
    u64 new_pages = (new_size + 4095) & ~(u64)4095;
    if (IsValidAddrSize(old_address, (i64)old_pages) &&
        IsValidAddrSize(old_address, (i64)new_pages)) {
      bool mapped;
      BEGIN_NO_PAGE_FAULTS;
      LOCK(&m->system->mmap_lock);
      mapped = IsFullyMapped(m->system, old_address, (i64)old_pages);
      UNLOCK(&m->system->mmap_lock);
      END_NO_PAGE_FAULTS;
      if (!mapped) return efault();
    }
  }
'''


def adapt(predecessor):
    if sha(predecessor) != PREDECESSOR_PIN:
        raise ValueError('Reviewed GuestThreads predecessor differs')
    source = predecessor.decode()
    marker = 'static i64 SysMremap('
    if source.count(marker) != 1:
        raise ValueError('SysMremap definition boundary differs')
    begin = source.index(marker)
    end = source.index('\n}', begin) + 3
    block = source[begin:end]
    if sha(block.encode()) != BLOCK_PIN:
        raise ValueError('Reviewed original SysMremap block differs')
    brace = block.index('{\n') + 2
    replacement = block[:brace] + VALIDATION + block[brace:]
    staged = source[:begin] + replacement + source[end:]
    patch = ''.join(difflib.unified_diff(source.splitlines(True), staged.splitlines(True),
        fromfile='a/blink/syscall.c', tofile='b/blink/syscall.c')).encode()
    return staged.encode(), patch


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--predecessor', type=Path, required=True)
    parser.add_argument('--predecessor-receipt', type=Path, required=True)
    parser.add_argument('--output', type=Path, required=True)
    parser.add_argument('--receipt', type=Path, required=True)
    args = parser.parse_args()
    outputs = [args.output / 'syscall.c', args.output / 'mremap.patch', args.receipt]
    resolved = [path.resolve() for path in outputs]
    if len(set(resolved)) != len(resolved):
        raise SystemExit('Output paths must be distinct')
    for path in resolved:
        if not any(path.is_relative_to((ROOT / name).resolve()) for name in ('generated', 'artifacts')):
            raise SystemExit('Outputs must stay in generated/artifacts; immutable and authored trees are forbidden')
        if path.exists() or path.is_symlink():
            raise SystemExit('Use fresh output paths; preserve earlier evidence')
    frozen = {}

    def read(path):
        path = Path(path).resolve()
        data = path.read_bytes()
        digest = sha(data)
        if frozen.setdefault(str(path), digest) != digest:
            raise SystemExit('Input changed while staging: ' + str(path))
        return data

    def module(relative, name):
        path = ROOT / relative
        read(path)
        spec = importlib.util.spec_from_file_location(name, path)
        loaded = importlib.util.module_from_spec(spec)
        # Derivation imports must not create cache files in authored modules.
        sys.dont_write_bytecode = True
        spec.loader.exec_module(loaded)
        return loaded

    stage_hash = sha(read(Path(__file__)))
    original = read(ROOT / 'ref' / ('blink-' + REVISION) / 'blink/syscall.c')
    memory = read(ROOT / 'ref' / ('blink-' + REVISION) / 'blink/memorymalloc.c')
    if sha(original) != ORIGINAL_PIN or sha(memory) != MEMORY_PIN:
        raise SystemExit('Immutable upstream source pin differs')
    predecessor = read(args.predecessor)
    prior_bytes = read(args.predecessor_receipt)
    prior = json.loads(prior_bytes)
    stop = module('src/UpstreamExecutionStop/stage.py', 'reviewed_mremap_stop')
    runtime = module('src/UpstreamGuestRuntime/stage.py', 'reviewed_mremap_runtime')
    threads = module('src/UpstreamGuestThreads/stage.py', 'reviewed_mremap_threads')
    stop_source, stop_patch, _ = stop.adapt(original)
    if stop_patch != read(ROOT / 'src/UpstreamExecutionStop/execution-stop.patch'):
        raise SystemExit('Execution-stop patch does not reproduce')
    runtime_source, runtime_patch = runtime.adapt(stop_source)
    if runtime_patch != read(ROOT / 'src/UpstreamGuestRuntime/guest-runtime.patch'):
        raise SystemExit('Guest-runtime patch does not reproduce')
    thread_sources, thread_patch = threads.adapt(runtime_source, memory)
    checked_thread_patch = read(ROOT / 'src/UpstreamGuestThreads/guest-threads.patch')
    if (thread_sources['syscall.c'] != predecessor or sha(predecessor) != PREDECESSOR_PIN
            or thread_patch != checked_thread_patch
            or prior.get('kind') != 'reviewed-managed-guest-thread-foundation'
            or prior.get('upstream') != REVISION
            or prior.get('stage_sha256') != sha(read(ROOT / 'src/UpstreamGuestThreads/stage.py'))
            or prior.get('patch_sha256') != sha(thread_patch)
            or prior['sources']['syscall.c'] != dict(source_sha256=ORIGINAL_PIN,
                predecessor_sha256=threads.PREDECESSOR_PIN, staged_sha256=PREDECESSOR_PIN)
            or prior['sources']['memorymalloc.c'] != dict(source_sha256=MEMORY_PIN,
                staged_sha256=sha(thread_sources['memorymalloc.c']))
            or prior['required_defines'] != threads.REQUIRED
            or prior['forbidden_defines'] != threads.FORBIDDEN):
        raise SystemExit('Predecessor receipt/source does not reproduce the reviewed chain')
    previous = prior['predecessor']
    runtime_receipt = read(previous['receipt'])
    runtime_prior = json.loads(runtime_receipt)
    if (sha(runtime_receipt) != previous['receipt_sha256']
            or read(previous['path']) != runtime_source or previous['sha256'] != sha(runtime_source)
            or runtime_prior.get('kind') != 'reviewed-private-single-thread-membarrier-adaptation'
            or runtime_prior.get('upstream') != REVISION
            or runtime_prior.get('stage_sha256') != sha(read(ROOT / 'src/UpstreamGuestRuntime/stage.py'))
            or runtime_prior.get('patch_sha256') != sha(runtime_patch)):
        raise SystemExit('Recorded runtime predecessor chain differs')
    for recorded in (prior, runtime_prior):
        for name, expected in recorded['frozen_inputs'].items():
            if sha(read(name)) != expected:
                raise SystemExit('Predecessor frozen input differs: ' + name)
    if prior['base_headers'] != threads.BASE_HEADERS:
        raise SystemExit('Thread ABI base-header pins differ')
    bases = {name: read(ROOT / name) for name in threads.BASE_HEADERS}
    for name, expected in threads.BASE_HEADERS.items():
        if sha(bases[name]) != expected: raise SystemExit('Base header differs: ' + name)
    overlays = threads.overlay_headers(bases['../DotCC.Lib/include/pthread.h'], bases['config/managed-host/signal.h'])
    if prior['overlays'] != {name: sha(data) for name, data in overlays.items()}:
        raise SystemExit('Thread overlay receipt differs')
    for name, data in overlays.items():
        if read(ROOT / 'config/managed-threaded' / name) != data:
            raise SystemExit('Thread overlay does not reproduce: ' + name)
    for name, expected in prior['required_headers'].items():
        header = (ROOT / name).resolve()
        if not header.is_relative_to((ROOT / 'src/Host/include').resolve()) or sha(read(header)) != expected:
            raise SystemExit('Required thread header differs')
    staged, patch = adapt(predecessor)
    if patch != read(HERE / 'mremap.patch'):
        raise SystemExit('Generated patch differs from reviewed mremap.patch')
    for name, expected in frozen.items():
        if sha(Path(name).read_bytes()) != expected:
            raise SystemExit('Frozen input changed during derivation: ' + name)
    receipt = dict(kind='reviewed-mremap-source-range-validation', upstream=REVISION,
        qualification='source-only; native/managed execution pending',
        scope='Eligible flags-zero source absence returns EFAULT; mapped and other requests retain unsupported ENOMEM; no mapping mutation',
        stage_sha256=stage_hash, patch_sha256=sha(patch),
        required_defines=threads.REQUIRED, forbidden_defines=threads.FORBIDDEN,
        required_headers=prior['required_headers'],
        predecessor=dict(path=str(args.predecessor.resolve()), sha256=sha(predecessor),
            receipt=str(args.predecessor_receipt.resolve()), receipt_sha256=sha(prior_bytes),
            stage_sha256=prior['stage_sha256'], patch_sha256=prior['patch_sha256']),
        sources={'syscall.c': dict(source_sha256=ORIGINAL_PIN, predecessor_sha256=PREDECESSOR_PIN,
            predecessor_block_sha256=BLOCK_PIN, staged_sha256=sha(staged))}, frozen_inputs=frozen)
    for path, data in zip(outputs, (staged, patch, (json.dumps(receipt, indent=2) + '\n').encode())):
        path.parent.mkdir(parents=True, exist_ok=True)
        with path.open('xb') as stream: stream.write(data)


if __name__ == '__main__':
    main()
