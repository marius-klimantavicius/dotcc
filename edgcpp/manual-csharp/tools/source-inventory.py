#!/usr/bin/env python3
"""Hash all upstream implementation/header paths, including ignored/generated files.

This records provenance metadata only. It never translates frontend source,
copies tables into production, or marks any source file implemented.
"""

import argparse
import collections
import hashlib
import json
import pathlib
import subprocess


def main():
    root = pathlib.Path(__file__).resolve().parents[1]
    repo = root.parents[1]
    upstream = root.parent / "upstream/src"
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--output", type=pathlib.Path, required=True)
    args = parser.parse_args()
    extensions = {".c", ".cpp", ".h"}
    paths = sorted(path for path in upstream.rglob("*")
                   if path.is_file() and path.suffix in extensions)
    records = [{"path": str(path.relative_to(repo)), "bytes": path.stat().st_size,
                "sha256": hashlib.sha256(path.read_bytes()).hexdigest()}
               for path in paths]
    result = {
        "purpose": "full live upstream source/header provenance, not implementation coverage",
        "selection": "recursive .c/.cpp/.h files under edgcpp/upstream/src; ignores are not applied; generated tables/IFC files included",
        "excluded": "other file extensions, build outputs outside upstream/src; no generated/ignored C/C++ source or header excluded",
        "repository_head": subprocess.check_output(
            ["git", "rev-parse", "HEAD"], cwd=repo, text=True).strip(),
        "whole_file_port_claim": False,
        "coverage_ledger": "docs/CORRESPONDENCE.md and docs/FEATURE-INVENTORY.md; per-routine/branch evidence required",
        "counts_by_extension": dict(sorted(collections.Counter(path.suffix for path in paths).items())),
        "files": records,
    }
    args.output.parent.mkdir(parents=True, exist_ok=True)
    args.output.write_text(json.dumps(result, indent=2) + "\n")
    print(f"Recorded {len(paths)} live upstream paths; {result['counts_by_extension']}")


if __name__ == "__main__":
    main()
