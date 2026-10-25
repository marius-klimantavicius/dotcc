#!/usr/bin/env python3
"""Check exact memory adapters against pinned archive bytes, privately only."""
import hashlib
import importlib.util
import json
from pathlib import Path
import sys
import tarfile
import tempfile

sys.dont_write_bytecode = True
HERE = Path(__file__).resolve().parent
ROOT = HERE.parents[1]
sha = lambda path: hashlib.sha256(Path(path).read_bytes()).hexdigest()


def main():
    attempt = Path(tempfile.mkdtemp(prefix='staging-', dir=HERE / '.artifacts'))
    receipt = dict(passed=False, scope='Exact authored adapters on pinned archive bytes; no shared staging/translation/build', inputs={})
    try:
        module_path = ROOT / 'scripts/memory_bindings.py'
        spec = importlib.util.spec_from_file_location('regular_memory_bindings', module_path)
        module = importlib.util.module_from_spec(spec); spec.loader.exec_module(module)
        manifest_path = ROOT / 'config/source-manifest.json'
        manifest = json.loads(manifest_path.read_text())['upstream']
        archive = ROOT / 'ref' / manifest['archive']
        if sha(archive) != manifest['sha256']: raise RuntimeError('Archive identity differs')
        for source in (Path(__file__), manifest_path, archive, *(ROOT / p for p in module.INPUTS)):
            receipt['inputs'][str(source)] = sha(source)
        with tarfile.open(archive) as stream:
            for path in ('blink/syscall.c', 'blink/open.c'):
                original = stream.extractfile(manifest['directory'] + '/' + path).read()
                adapted = module.adapt(path, original)
                (attempt / Path(path).name).write_bytes(adapted)
                text = adapted.decode()
                if path.endswith('syscall.c'):
                    expected = '  rc = VfsFtruncate(fd, length);\n  VfsClose(fd);\n  if (!rc) InvalidateSystem(m->system, true, true);\n  return rc;\n'
                else:
                    expected = '  if (fildes != -1) {\n    if (oflags & O_TRUNC_LINUX) InvalidateSystem(m->system, true, true);\n'
                if text.count(expected) != 1: raise RuntimeError('Missing exact EOF cache invalidation: ' + path)
                # Alter the exact new anchor. It must fail closed even when all
                # predecessor adaptations still match the archive source.
                anchor = (b'  rc = VfsFtruncate(fd, length);\n  VfsClose(fd);\n  return rc;\n'
                    if path.endswith('syscall.c') else
                    b'  RESTARTABLE(fildes = VfsOpen(GetDirFildes(dirfildes), path, sysflags, mode));\n  if (fildes != -1) {\n')
                if original.count(anchor) != 1: raise RuntimeError('Original anchor identity differs')
                try: module.adapt(path, original.replace(anchor, anchor.replace(b'Vfs', b'ChangedVfs', 1), 1))
                except ValueError: pass
                else: raise RuntimeError('Adapter accepted a changed anchor: ' + path)
        for path, digest in receipt['inputs'].items():
            if sha(path) != digest: raise RuntimeError('Input changed: ' + path)
        receipt['passed'] = True
    except BaseException as error: receipt['error'] = repr(error)
    (attempt / 'receipt.json').write_text(json.dumps(receipt, indent=2) + '\n')
    print('passed=' + str(receipt['passed']) + ' receipt=' + str(attempt / 'receipt.json'))
    return 0 if receipt['passed'] else 1


if __name__ == '__main__': raise SystemExit(main())
