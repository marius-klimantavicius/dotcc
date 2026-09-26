"""Fixture paths and executable-level resolver checks for both upstream runners."""
import hashlib
import os
from pathlib import Path
import shutil
import subprocess
import tempfile

ROOT = Path(__file__).resolve().parents[1]


def fixture_directory(value=None):
    path = Path(value or os.environ.get("PINTA_FIXTURES") or
                ROOT / "ref/upstream/Marius.Pinta.Test.Files").expanduser().resolve()
    if not (path / "unicode-string-v2.pint").is_file():
        raise ValueError(f"Pinta fixture directory is missing unicode-string-v2.pint: {path}")
    return path


def check_resolution(invocation, fixtures, artifacts, label):
    results = []
    with tempfile.TemporaryDirectory(prefix="pinta fixture resolution ") as temporary:
        cwd = Path(temporary)
        override = cwd / "fixtures with spaces"
        override.mkdir()
        shutil.copyfile(fixtures / "unicode-string-v2.pint", override / "unicode-string-v2.pint")
        for name, root, expected in [("default", None, 0), ("override", override, 0),
                                      ("missing-root", cwd / "missing", 1)]:
            env = dict(os.environ)
            env.pop("PINTA_FIXTURES", None)
            if root is not None:
                env["PINTA_FIXTURES"] = str(root)
            result = subprocess.run([*map(str, invocation), "--check-fixtures"], cwd=cwd,
                                    env=env, capture_output=True, text=True, timeout=60)
            output = result.stdout + result.stderr
            log = artifacts / f"{label}-fixtures-{name}.log"
            log.write_text(output)
            passed = result.returncode == expected and (expected != 0 or
                "PASS fixture resolution: 6 path forms, exact bytes" in result.stdout)
            results.append({"case": name, "exit": result.returncode, "expected_exit": expected,
                            "pass": passed, "cwd": str(cwd), "fixture_root": str(root) if root else None,
                            "log": str(log), "sha256": hashlib.sha256(log.read_bytes()).hexdigest()})
            print(f"{label} fixture {name}: {'PASS' if passed else 'FAIL'}", flush=True)
    return results
