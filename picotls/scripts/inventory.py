#!/usr/bin/env python3
"""Inventory native core symbols and headers; run after scripts/oracle.sh."""
import json
from pathlib import Path
import re
import subprocess

root = Path(__file__).resolve().parents[1]
inputs = json.loads((root / "config/inputs.json").read_text())
source = root / "ref" / inputs["picotls"]["directory"]
archive = root / "build/oracle/libpicotls-core.a"
out = root / "artifacts/compiler"
out.mkdir(parents=True, exist_ok=True)
defined_text = subprocess.check_output(["nm", "-g", "--defined-only", str(archive)], text=True)
undefined_text = subprocess.check_output(["nm", "-u", str(archive)], text=True)
(out / "native-defined.txt").write_text(defined_text)
(out / "native-undefined.txt").write_text(undefined_text)
defined = set(re.findall(r"^\S+\s+[A-Za-z]\s+(\S+)$", defined_text, re.M))
undefined = set(re.findall(r"^\s+U\s+(\S+)$", undefined_text, re.M))
header = (source / "include/picotls.h").read_text()
callbacks = re.findall(r"^PTLS_CALLBACK_TYPE0?\((.*?);", header, re.M | re.S)
sources = [line for line in (root / "config/core-sources.txt").read_text().splitlines()
           if line.strip() and not line.startswith("#")]
includes = {name: re.findall(r'^#include\s+([<"].*[>"])', (source / name).read_text(), re.M)
            for name in [*sources, "include/picotls.h"]}
report = dict(revision=inputs["picotls"]["revision"],
              note="Native symbols are an inventory, not translated runtime validation; includes are lexical, including conditional branches.",
              core_defined=sorted(defined), external_native=sorted(undefined - defined),
              internal_cross_unit=sorted(undefined & defined),
              callback_macro_declarations=[re.sub(r"\s+", " ", item) for item in callbacks],
              includes=includes)
(out / "inventory.json").write_text(json.dumps(report, indent=2) + "\n")
print(f"{len(defined)} native core definitions; {len(undefined - defined)} external dependencies")
print(out / "inventory.json")
