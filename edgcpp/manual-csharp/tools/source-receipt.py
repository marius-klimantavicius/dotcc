#!/usr/bin/env python3
"""Record consulted source provenance; does not translate or generate code."""

import argparse
import hashlib
import json
import pathlib
import subprocess


def main():
    root = pathlib.Path(__file__).resolve().parents[1]
    repo = root.parents[1]
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("files", nargs="+", help="Paths relative to edgcpp/upstream/src")
    parser.add_argument("--output", type=pathlib.Path, required=True)
    args = parser.parse_args()
    references = []
    for name in args.files:
        path = root.parent / "upstream/src" / name
        references.append({"path": str(path.relative_to(repo)),
                           "sha256": hashlib.sha256(path.read_bytes()).hexdigest()})
    receipt = {
        "purpose": "consulted source provenance, not a claim that complete files are ported",
        "imported_base": "89e67e07c7f6d0fc38622f9778d8e79f40deaa0f",
        "repository_head_at_inspection": subprocess.check_output(
            ["git", "rev-parse", "HEAD"], cwd=repo, text=True).strip(),
        "sources": references,
    }
    args.output.parent.mkdir(parents=True, exist_ok=True)
    args.output.write_text(json.dumps(receipt, indent=2) + "\n")
    print(f"Recorded {len(references)} source hashes in {args.output}")


if __name__ == "__main__":
    main()
