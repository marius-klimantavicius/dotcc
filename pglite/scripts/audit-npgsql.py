#!/usr/bin/env python3
"""Observe copied Npgsql source provenance without fetching or building it."""
import argparse
import hashlib
import json
from pathlib import Path
import xml.etree.ElementTree as ET


def digest(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--output", type=Path)
    args = parser.parse_args()
    root = Path(__file__).resolve().parents[1]
    source = root / "src/Npgsql"
    metadata_path = root / "config/npgsql-source.json"
    metadata = json.loads(metadata_path.read_text())
    originals = metadata["original_source_sha256"]
    observations, missing = {}, []
    for name, original in sorted(originals.items()):
        path = source / name
        if not path.is_file():
            missing.append(name)
            continue
        observed = digest(path)
        observations[name] = {
            "original": original, "current": observed,
            "status": "unchanged" if observed == original else "adapted",
        }
    current = {
        str(path.relative_to(source)): digest(path)
        for path in sorted(source.rglob("*"))
        if path.is_file() and not {"obj", "bin"}.intersection(path.relative_to(source).parts)
    }
    project = ET.parse(source / "Npgsql.csproj")
    packages = [element.attrib for element in project.findall(".//PackageReference")]
    if any(element.get("Include", "").lower() == "npgsql" for element in packages):
        raise RuntimeError("The copied library cannot consume stock Npgsql as a package.")
    for name in ("NpgsqlCommand.cs", "NpgsqlDataReader.cs", "NpgsqlParameter.cs",
                 "NpgsqlTransaction.cs", "Internal/NpgsqlConnector.cs"):
        if name not in observations:
            raise RuntimeError("Required upstream machinery is absent: " + name)
    if missing:
        raise RuntimeError("Copied upstream files are missing: " + ", ".join(missing))
    adapted = {name for name, row in observations.items() if row["status"] == "adapted"}
    ledger = metadata.get("adapted_files", {})
    if set(ledger) != adapted:
        raise RuntimeError("Adapted source inventory differs from the patch ledger.")
    for name in adapted:
        if not ledger[name].get("reason") or ledger[name].get("sha256") != observations[name]["current"]:
            raise RuntimeError("Adapted source reason/hash needs reconciliation: " + name)
    notices = source / "Notices/NPGSQL-LICENSE.txt"
    if not notices.is_file() or "Copyright (c)" not in notices.read_text():
        raise RuntimeError("The copied Npgsql license is required.")
    result = {
        "repository": metadata["repository"], "release": metadata["release"],
        "revision": metadata["revision"], "archive_sha256": metadata["archive_sha256"],
        "metadata_sha256": digest(metadata_path),
        "original_file_count": len(originals), "current_file_count": len(current),
        "unchanged_count": sum(row["status"] == "unchanged" for row in observations.values()),
        "adapted_files": [name for name, row in observations.items() if row["status"] == "adapted"],
        "additional_files": [name for name in current if name not in originals],
        "originals": observations, "current_sha256": current,
        "package_references": packages,
        "note": "Adapted hashes are observations. Review the patch ledger and behavioral evidence separately.",
    }
    if args.output:
        args.output.parent.mkdir(parents=True, exist_ok=True)
        args.output.write_text(json.dumps(result, indent=2) + "\n")
    print(json.dumps({key: result[key] for key in (
        "release", "revision", "original_file_count", "current_file_count",
        "unchanged_count", "adapted_files", "additional_files")}, indent=2))


if __name__ == "__main__":
    main()
