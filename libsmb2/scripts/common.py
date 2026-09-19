"""Campaign paths, immutable source acquisition, and command receipts."""
import hashlib
import json
from pathlib import Path
import subprocess
import tarfile
import urllib.request

ROOT = Path(__file__).resolve().parents[1]
REPO = ROOT.parent
SOURCE_SPEC = json.loads((ROOT / 'config/source.json').read_text())


def sha(path):
    return hashlib.sha256(Path(path).read_bytes()).hexdigest()


def fetch():
    ref = ROOT / 'ref'
    ref.mkdir(exist_ok=True)
    archive = ref / (SOURCE_SPEC['directory'] + '.tar.gz')
    if not archive.exists():
        temporary = archive.with_suffix('.download')
        try:
            urllib.request.urlretrieve(SOURCE_SPEC['archive'], temporary)
            if sha(temporary) != SOURCE_SPEC['sha256']:
                raise RuntimeError('Downloaded source checksum mismatch')
            temporary.replace(archive)
        finally:
            temporary.unlink(missing_ok=True)
    if sha(archive) != SOURCE_SPEC['sha256']:
        raise RuntimeError('Source archive checksum mismatch')
    source = ref / SOURCE_SPEC['directory']
    with tarfile.open(archive) as bundle:
        if not source.exists():
            bundle.extractall(ref, filter='data')
        for member in bundle:
            if member.isfile() and (ref / member.name).read_bytes() != bundle.extractfile(member).read():
                raise RuntimeError('Upstream input modified: ' + member.name)
    return source


def run(command, log, receipt, *, cwd=REPO, timeout=600, env=None):
    command = list(map(str, command))
    log = Path(log)
    log.parent.mkdir(parents=True, exist_ok=True)
    entry = dict(command=command, cwd=str(cwd), log=str(log.relative_to(ROOT)))
    receipt.setdefault('commands', []).append(entry)
    with log.open('w') as output:
        result = subprocess.run(command, cwd=cwd, env=env, stdout=output,
                                stderr=subprocess.STDOUT, timeout=timeout)
    entry['exit_code'] = result.returncode
    if result.returncode:
        raise RuntimeError(f'Command failed ({result.returncode}); see {log}')
    return log.read_text()


if __name__ == '__main__':
    print(fetch())
