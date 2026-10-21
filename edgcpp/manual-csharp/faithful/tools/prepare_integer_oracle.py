#!/usr/bin/env python3
"""Supervisor-only test instrumentation build; never changes upstream/build tree."""
import argparse
import hashlib
import json
from pathlib import Path
import platform
import shlex
import subprocess

parser = argparse.ArgumentParser()
parser.add_argument("--output", type=Path, required=True)
parser.add_argument("--sanitize", action="store_true", help="Test-only undefined-behavior instrumentation")
args = parser.parse_args()
root = Path(__file__).resolve().parents[4]
entries = json.loads((root / "edgcpp/build/compile_commands.json").read_text())
entry = next(e for e in entries if e["file"].endswith("/const_ints.c") and
             "/cmake/cpfe/" in e.get("output", ""))
original = shlex.split(entry["command"])
flags = []
i = 1
while i < len(original):
    word = original[i]
    if word == "-o":
        i += 2
    elif word == "-c" or word == entry["file"]:
        i += 1
    else:
        flags.append(word)
        i += 1
out = args.output.resolve()
out.mkdir(parents=True, exist_ok=True)
source = Path(entry["file"])
wrapper = Path(__file__).with_name("integer_oracle.cpp")
instrumentation = ["-fsanitize=undefined", "-fno-sanitize-recover=undefined"] if args.sanitize else []
commands = [
    [original[0], *flags, *instrumentation, "-ffunction-sections", "-fdata-sections", "-MD", "-MF", str(out / "const_ints.d"), "-c", str(source), "-o", str(out / "const_ints.o")],
    [original[0], *flags, *instrumentation, "-iquote", str(source.parent), "-ffunction-sections", "-fdata-sections", "-c", str(wrapper), "-o", str(out / "wrapper.o")],
    [original[0], "-O3", "-flto", *instrumentation, "-Wl,--gc-sections", str(out / "const_ints.o"), str(out / "wrapper.o"), "-o", str(out / "integer-oracle")],
]
receipt = {"native_entry": entry, "instrumentation_changes": ["function/data sections", "dead-section removal", "test wrapper main", "wrapper-only quote include path reproduces original source directory header lookup", "isolated dynamic/default-linker oracle rather than full cpfe static/gold link; frontend initialization not linked"],
           "sanitizer": instrumentation,
           "commands": commands, "results": [], "source_sha256": hashlib.sha256(source.read_bytes()).hexdigest(),
           "wrapper_sha256": hashlib.sha256(wrapper.read_bytes()).hexdigest(),
           "compiler_path": str(Path(original[0]).resolve()),
           "compiler_sha256": hashlib.sha256(Path(original[0]).read_bytes()).hexdigest(),
           "host": platform.uname()._asdict(), "compiler_observations": []}
try:
    for option in ["--version", "-dumpmachine", "-dumpfullversion"]:
        identity = subprocess.run([original[0], option], text=True, stdout=subprocess.PIPE, stderr=subprocess.STDOUT)
        receipt["compiler_observations"].append({"command": [original[0], option], "returncode": identity.returncode, "output": identity.stdout})
    macro_command = [original[0], *flags, "-dM", "-E", str(source)]
    macro_run = subprocess.run(macro_command, cwd=entry["directory"], stdout=subprocess.PIPE, stderr=subprocess.PIPE)
    (out / "effective-macros.txt").write_bytes(macro_run.stdout)
    receipt["effective_macro_dump"] = {"command": macro_command, "returncode": macro_run.returncode,
                                       "sha256": hashlib.sha256(macro_run.stdout).hexdigest(),
                                       "stderr": macro_run.stderr.decode(errors="replace")}
    if macro_run.returncode:
        raise SystemExit(macro_run.returncode)
    for command in commands:
        run = subprocess.run(command, cwd=entry["directory"], text=True, stdout=subprocess.PIPE, stderr=subprocess.STDOUT)
        receipt["results"].append({"returncode": run.returncode, "output": run.stdout})
        if run.returncode:
            raise SystemExit(run.returncode)
    receipt["binary_sha256"] = hashlib.sha256((out / "integer-oracle").read_bytes()).hexdigest()
    dependencies = shlex.split((out / "const_ints.d").read_text().replace("\\\n", " "))[1:]
    receipt["expanded_include_files"] = []
    for name in sorted(set(dependencies)):
        path = Path(name)
        if not path.is_absolute():
            path = Path(entry["directory"]) / path
        receipt["expanded_include_files"].append({"path": str(path), "sha256": hashlib.sha256(path.read_bytes()).hexdigest()})
finally:
    (out / "prepare-receipt.json").write_text(json.dumps(receipt, indent=2) + "\n")
