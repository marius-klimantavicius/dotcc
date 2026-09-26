"""Campaign paths, immutable source acquisition, and command receipts."""
import hashlib
import json
from pathlib import Path
import subprocess
import tarfile
import urllib.request

ROOT = Path(__file__).resolve().parents[1]
REPO = ROOT.parent
import sys
sys.path.insert(0, str(REPO / "Scripts"))
from campaigns.compat import references
from campaigns.process import execute
SOURCE_SPEC = json.loads((ROOT / 'config/source.json').read_text())


def sha(path):
    return hashlib.sha256(Path(path).read_bytes()).hexdigest()


def fetch():
    return references(ROOT)["product"]


def run(command, log, receipt, *, cwd=REPO, timeout=600, env=None):
    entry = {}
    receipt.setdefault('commands', []).append(entry)
    execute(command, label=Path(log).stem, cwd=cwd, output=log, timeout=timeout,
            env=env, record=entry)
    return Path(log).read_text()


if __name__ == '__main__':
    print(fetch())
