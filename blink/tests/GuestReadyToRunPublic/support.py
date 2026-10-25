"""Pinned reviewed helpers; no builds or process creation at import time."""
import hashlib
import importlib.util
from pathlib import Path
import sys

sys.dont_write_bytecode = True
HERE = Path(__file__).resolve().parent
BLINK = HERE.parents[1]
REPO = BLINK.parent
PINS = {
    'blink/tests/CoreClrRuntime/evidence.py': '57d7b1a5bb404165b09e7adf3956789b64b92d5b8955e394a27823b92304317c',
    'blink/tests/CoreClrRuntime/run.py': '8a489cfd2d61cb8c1bc81659beee1b9fc20ca4fa3af001c0ea0e6ec6e747d7da',
    'blink/tests/MachineApi/run.py': '421824405f119d947b361848c38fc2f3f6d9f63641dca59f49c53fd78e854315',
    'blink/tests/WorkerInstances/run.py': '6b209134326d330584a21e5a38ca1c07f7d0e01e2bd11d9b1c3841fed77d787f',
    'Scripts/production_support.py': '9530aec5f2d3713c3c6a4b69aa1c1dd975a30e1b4f3c572ff93da9d2bcb8ec1d',
    'blink/scripts/campaign_delivery.py': '872e482b03116d56397430a4de259c28d8b4961989e8ceea6d996f0ff29a8d4b',
    'blink/tests/CoreClrReadyToRun/inspect_pe.py': '63e3808857871908838d346d0b90167c8ab046631fb6dabe617cc78703dbab54',
}


def sha(path):
    return hashlib.sha256(Path(path).read_bytes()).hexdigest()


def load_helpers():
    # Check every direct import before executing any helper's module body.
    for name, digest in PINS.items():
        if sha(REPO / name) != digest:
            raise RuntimeError('Reviewed helper changed; explicit ownership thaw required: ' + name)
    sys.path.insert(0, str(REPO / 'Scripts'))
    sys.path.insert(0, str(BLINK / 'scripts'))
    evidence = load(REPO / 'blink/tests/CoreClrRuntime/evidence.py', 'evidence')
    api = load(REPO / 'blink/tests/MachineApi/run.py', 'r2r_public_api')
    core = load(REPO / 'blink/tests/CoreClrRuntime/run.py', 'r2r_public_core')
    inspector = load(REPO / 'blink/tests/CoreClrReadyToRun/inspect_pe.py', 'r2r_public_pe')
    return evidence, api, core, inspector


def load(path, name):
    spec = importlib.util.spec_from_file_location(name, path)
    result = importlib.util.module_from_spec(spec)
    sys.modules[name] = result
    spec.loader.exec_module(result)
    return result
