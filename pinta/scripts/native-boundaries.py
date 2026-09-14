#!/usr/bin/env python3
"""Run finite native creation, callback, bytecode, and value-boundary probes."""
import argparse
import hashlib
import json
import os
from pathlib import Path
import subprocess
import sys

ROOT = Path(__file__).resolve().parents[1]
PROGRAMS = {
    "boundaries": "native-boundaries.c",
    "repeated": "repeated-execute.c",
    "receipt": "native-receipt.c",
    "callback-return": "native-callback-return.c",
    "value-exhaustion": "native-value-exhaustion.c",
    "module-probes": "native-module-probes.c",
}


def sha(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def fixture_hashes():
    return {path.name: sha(path) for path in sorted((ROOT / "tests/fixtures").glob("*.pint"))}


def verify_fixtures():
    receipt = json.loads((ROOT / "tests/fixtures/receipt.json").read_text())
    callback = json.loads((ROOT / "tests/fixtures/callback-return.json").read_text())
    expected = {"receipt.pint": receipt["sha256"],
                "receipt-swapped.pint": receipt["swapped_sha256"],
                "invalid-opcode.pint": receipt["invalid_opcode_sha256"],
                "callback-return.pint": callback["sha256"]}
    for name, digest in expected.items():
        if sha(ROOT / "tests/fixtures" / name) != digest:
            raise RuntimeError("Authored fixture hash mismatch: " + name)


def check_result(name, log):
    if name == "receipt":
        expected = json.loads((ROOT / "tests/fixtures/receipt.json").read_text())
        total = "".join(format(ord(c), "04x") for c in expected["expected_total"])
        output = expected["expected_output_utf16le_hex"]
        return (f"output bytes={len(bytes.fromhex(output))} hex={output}" in log
                and f"total units={len(total) // 4} hex={total}" in log)
    if name == "callback-return":
        expected = json.loads((ROOT / "tests/fixtures/callback-return.json").read_text())
        units = expected["expected_utf16_units"]
        expected_hex = "".join(format(unit, "04x") for unit in units)
        return (f"answer units={len(units)} hex={expected_hex}" in log
                and "execute status=0 callbacks=1 moved=1 payload_moved=1" in log
                and "files opens=1 reads=43 closes=1" in log)
    if name == "value-exhaustion":
        return "status=4 result_null=1" in log
    if name == "module-probes":
        fields = ("strings_count", "strings_offset", "functions_count",
                  "functions_offset", "data_length", "data_offset")
        return ("short-headers rejected=84 total=84 unchanged=84" in log
                and "unknown-magic rejected=1" in log
                and "bad-opcode status=9" in log
                and all(f"header-field {field} rejected=1" in log for field in fields))
    return True


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--stage", choices=["pristine", "corrected"], default="corrected")
    isolated = parser.add_mutually_exclusive_group()
    for name in ("receipt", "callback-return", "value-exhaustion", "module-probes"):
        isolated.add_argument("--" + name + "-only", action="store_true")
    args = parser.parse_args()
    selected = next((name for name in PROGRAMS
                     if getattr(args, name.replace("-", "_") + "_only", False)), None)
    # Newly authored malformed-module diagnostics are explicitly opt-in and
    # currently unqualified; the ordinary lifetime/callback regression run omits them.
    programs = [selected] if selected else [name for name in PROGRAMS if name != "module-probes"]
    source = ROOT / "ref/upstream"
    subprocess.run([sys.executable, str(ROOT / "scripts/fetch.py"), "--offline"], check=True)
    if args.stage == "corrected":
        subprocess.run([sys.executable, str(ROOT / "scripts/stage.py")], check=True)
        source = ROOT / "generated/native-input"
    verify_fixtures()
    out = ROOT / "build" / ("native-boundaries-" + args.stage)
    out.mkdir(parents=True, exist_ok=True)
    artifacts = ROOT / "artifacts" / out.name
    artifacts.mkdir(parents=True, exist_ok=True)
    cc = os.environ.get("CC", "cc")
    flags = ["-std=c11", "-fshort-wchar", "-funsigned-char", "-fno-strict-aliasing",
             "-O0", "-g", "-DPINTA_DEBUG=0", "-include", str(ROOT / "config/native-preinclude.h"),
             "-I" + str(source / "Marius.Pinta/inc"), "-I" + str(source / "Marius.Pinta/tests")]
    sources = [source / name for name in (ROOT / "config/core-sources.txt").read_text().splitlines()]
    receipt = {"stage": args.stage, "cases": [], "builds": [],
               "source_lock_sha256": sha(ROOT / "config/source-lock.json"),
               "patches_sha256": sha(ROOT / "config/patches.json") if args.stage == "corrected" else None,
               "fixtures": fixture_hashes()}
    receipt_name = selected + "-only.json" if selected else "receipt.json"
    failed = False
    try:
        for name in programs:
            test = ROOT / "tests" / PROGRAMS[name]
            inputs = sources + [ROOT / "src/native-adapter.c", test]
            command = [cc, *flags, *map(str, inputs), "-o", str(out / name)]
            result = subprocess.run(command, capture_output=True, text=True)
            (artifacts / (name + "-build.log")).write_text(result.stdout + result.stderr)
            build = {"program": name, "command": command, "exit": result.returncode,
                     "inputs": {str(path.relative_to(ROOT)): sha(path) for path in inputs}}
            receipt["builds"].append(build)
            if result.returncode:
                failed = True
                continue
            build["binary_sha256"] = sha(out / name)
            arguments = {"boundaries": [["misaligned-short"], ["create-sweep"], ["stack-overflow"]],
                         "value-exhaustion": [["string"], ["character"], ["weak"]],
                         "receipt": [[], ["receipt-swapped.pint"]]}.get(name, [[]])
            for argument in arguments:
                label = ("value-exhaustion-" + argument[0] if name == "value-exhaustion"
                         else "receipt-swapped" if name == "receipt" and argument
                         else argument[0] if argument else name)
                fixture_root = (ROOT / "tests/fixtures" if name in ("receipt", "callback-return", "module-probes")
                                else ROOT / "ref/upstream/Marius.Pinta.Test.Files")
                env = dict(os.environ, PINTA_FIXTURES=str(fixture_root))
                before = fixture_hashes()
                try:
                    result = subprocess.run([str(out / name), *argument], env=env,
                                            capture_output=True, text=True, timeout=60)
                    status, log = result.returncode, result.stdout + result.stderr
                except subprocess.TimeoutExpired as error:
                    status = 124
                    log = (error.stdout or b"").decode(errors="replace") + (error.stderr or b"").decode(errors="replace")
                unchanged = before == fixture_hashes()
                if status == 0 and (not check_result(name, log) or not unchanged):
                    status = 1
                (artifacts / (label + ".log")).write_text(log)
                receipt["cases"].append({"case": label, "exit": status,
                                         "source_bytes_unchanged": unchanged, "source_sha256": before})
                failed |= status != 0
                print(label, status, log.strip(), flush=True)
    finally:
        (artifacts / receipt_name).write_text(json.dumps(receipt, indent=2) + "\n")
    return int(failed)


if __name__ == "__main__":
    sys.exit(main())
