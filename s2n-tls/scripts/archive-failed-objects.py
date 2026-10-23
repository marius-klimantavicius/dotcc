"""Reversibly archive serialized objects from inactive failed campaign attempts.

Successful product attempts, tools, sources, logs and executable outputs are
outside this operation. Every archive member is checked before files are removed.
"""
import argparse
import hashlib
import json
from pathlib import Path
import tarfile

ROOT = Path(__file__).resolve().parents[1]


def sha(path):
    with path.open('rb') as stream:
        return hashlib.file_digest(stream, 'sha256').hexdigest()


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--run', action='append', required=True)
    parser.add_argument('--restore', action='store_true')
    args = parser.parse_args()
    output = ROOT / 'artifacts/failed-object-archives'
    output.mkdir(exist_ok=True)
    for name in args.run:
        if Path(name).name != name:
            raise RuntimeError('A campaign run basename is required')
        record_path = ROOT / 'artifacts/campaign' / name / 'receipt.json'
        record = json.loads(record_path.read_text())
        if record['status'] not in ('failed', 'cancelled'):
            raise RuntimeError('Only failed/cancelled serialized objects may be archived')
        directory = ROOT / 'build/campaign' / name / 'objects'
        archive = output / (name + '.tar.gz')
        receipt = output / (name + '.json')
        if args.restore:
            metadata = json.loads(receipt.read_text())
            if sha(archive) != metadata['archive_sha256']:
                raise RuntimeError('Archive identity changed')
            directory.mkdir(exist_ok=True)
            with tarfile.open(archive, 'r:gz') as source:
                members = source.getmembers()
                if {m.name for m in members} != set(metadata['files']):
                    raise RuntimeError('Archive member inventory differs')
                for member in members:
                    if not member.isfile() or Path(member.name).name != member.name:
                        raise RuntimeError('Only flat regular object files may be restored')
                    data = source.extractfile(member).read()
                    if hashlib.sha256(data).hexdigest() != metadata['files'][member.name]['sha256']:
                        raise RuntimeError('Archived object identity differs')
                    target = directory / member.name
                    if target.exists() and sha(target) != metadata['files'][member.name]['sha256']:
                        raise RuntimeError('Refusing to replace a different existing object')
                    if not target.exists():
                        target.write_bytes(data)
                        target.chmod(member.mode)
            print('RESTORED', directory)
            continue
        # A process may hold a snapshot through a command argument or cwd.
        # Refuse the operation if any live local process names this attempt.
        for process in Path('/proc').iterdir():
            if not process.name.isdigit():
                continue
            try:
                command = (process / 'cmdline').read_bytes()
                cwd = (process / 'cwd').resolve()
            except (OSError, PermissionError):
                continue
            if str(directory.parent).encode() in command or cwd == directory.parent or directory.parent in cwd.parents:
                raise RuntimeError('A live process still references the failed attempt: ' + process.name)
        if receipt.exists() or archive.exists():
            raise RuntimeError('A failed-object archive already exists; inspect it before reuse')
        files = sorted(directory.iterdir())
        if not files or any(not path.is_file() or path.is_symlink() or path.suffix != '.cs' for path in files):
            raise RuntimeError('Expected a nonempty flat serialized C object directory')
        inventory = {p.name: {'sha256': sha(p), 'size': p.stat().st_size} for p in files}
        temporary = archive.with_suffix('.gz.tmp')
        with tarfile.open(temporary, 'w:gz', compresslevel=3) as target:
            for path in files:
                target.add(path, arcname=path.name, recursive=False)
        with tarfile.open(temporary, 'r:gz') as source:
            members = source.getmembers()
            if len(members) != len(inventory) or {m.name for m in members} != set(inventory):
                raise RuntimeError('Archive did not preserve every object exactly once')
            for member in members:
                actual = hashlib.file_digest(source.extractfile(member), 'sha256').hexdigest()
                if actual != inventory[member.name]['sha256']:
                    raise RuntimeError('Archive verification failed: ' + member.name)
        temporary.replace(archive)
        metadata = {'status': 'verified-archive', 'scope': __doc__, 'run': name,
                    'failed_receipt': str(record_path), 'failed_receipt_sha256': sha(record_path),
                    'directory': str(directory), 'files': inventory, 'archive': str(archive),
                    'archive_sha256': sha(archive), 'original_bytes': sum(x['size'] for x in inventory.values()),
                    'archive_bytes': archive.stat().st_size,
                    'restore_command': ['python3', str(Path(__file__).resolve()), '--run', name, '--restore']}
        receipt.write_text(json.dumps(metadata, indent=2) + '\n')
        for path in files:
            if sha(path) != inventory[path.name]['sha256']:
                raise RuntimeError('Object changed before verified archival removal: ' + str(path))
        for path in files:
            path.unlink()
        print('ARCHIVED', name, metadata['original_bytes'] - metadata['archive_bytes'], 'bytes recovered')


if __name__ == '__main__':
    main()
