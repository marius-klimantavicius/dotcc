#!/usr/bin/env python3
"""Capture source/test/config/ledger bytes without touching those inputs."""
import argparse
import hashlib
import json
from pathlib import Path

p = argparse.ArgumentParser()
p.add_argument("--output", type=Path, required=True)
a = p.parse_args()
faithful = Path(__file__).resolve().parents[1]
root = faithful.parents[2]
paths = [faithful / "Directory.Build.props"]
for sub in ["src", "tests", "ledgers", "tools"]:
    paths += [x for x in (faithful / sub).rglob("*") if x.is_file() and not
              any(part in {"bin", "obj", "__pycache__"} for part in x.parts)]
paths += [root / rel for rel in ["edgcpp/upstream/src/const_ints.c",
          "edgcpp/upstream/src/const_ints.h", "edgcpp/upstream/src/targ_def.h",
          "edgcpp/upstream/src/basics.h", "edgcpp/build/compile_commands.json",
          "edgcpp/build/src/includes/cmake_defines.h"]]
manifest = {str(x.relative_to(root)): hashlib.sha256(x.read_bytes()).hexdigest()
            for x in sorted(set(paths))}
canonical = json.dumps(manifest, sort_keys=True, separators=(",", ":")).encode()
result = {"schema": 1, "manifest_sha256": hashlib.sha256(canonical).hexdigest(), "files": manifest}
a.output.parent.mkdir(parents=True, exist_ok=True)
a.output.write_text(json.dumps(result, indent=2) + "\n")
print(json.dumps({"files": len(manifest), "manifest_sha256": result["manifest_sha256"]}))
