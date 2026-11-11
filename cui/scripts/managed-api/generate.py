#!/usr/bin/env python3
"""Reproduce reviewed managed API templates and derived UTF-8 overloads.

Default: check only. Generation refuses to overwrite unrecognized source changes.
This is deliberately not a header-to-wrapper inference engine: pointer retention,
validation, callback containment and lifecycle repairs require source review.
"""
import argparse
import hashlib
import json
from pathlib import Path
import re
import sys
from utf8_overloads import generate as generate_utf8

HERE = Path(__file__).resolve().parent
REPO = HERE.parents[2]
FILES = ("Types.cs", "Ui.Api.cs", "Ui.Marshalling.cs", "Ui.ScopeDispatch.cs")


def sha(data):
    return hashlib.sha256(data).hexdigest()


def check_storage(directory):
    # Array copies, strings, StateLease and TerminalBuffer owners remain stable.
    # No frame-arena singleton address is retained by this reviewed API surface.
    # Fail closed if an author introduces one; re-audit native retention first.
    for path in sorted(directory.glob("*.cs")):
        if re.search(r"\b_?arena\s*\.\s*Allocate\s*\(\s*(?:checked\s*\(\s*)?sizeof\b", path.read_text()):
            raise ValueError(f"{path.name}: singleton arena allocation needs a retention audit")


def function_source(source, symbol):
    match = re.search(r"^\w[^\n;]*?\b" + re.escape(symbol) + r"\s*\([^;]*?\)\s*\{", source, re.M)
    if match is None:
        raise ValueError(f"Native definition missing: {symbol}")
    # Ignore C comments and literals when balancing function braces.
    tokens = re.finditer(r'/\*[\s\S]*?\*/|//[^\n]*|"(?:\\.|[^"\\])*"|\'(?:\\.|[^\'\\])*\'|[{}]', source[match.end() - 1:])
    depth = 0
    for token in tokens:
        if token[0] == "{":
            depth += 1
        elif token[0] == "}":
            depth -= 1
            if depth == 0:
                return source[match.start():match.end() - 1 + token.end()]
    raise ValueError(f"Unclosed native definition: {symbol}")


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--repo-root", type=Path, default=REPO, help="repository whose reviewed wrapper is checked")
    parser.add_argument("--check", action="store_true", help="compare templates with destination (the default)")
    parser.add_argument("--output-dir", type=Path, help="reproduce the five API files here; do not write unless supplied")
    parser.add_argument("--native-root", type=Path, help="optional upstream root containing src/ and include/cui/cui.h; verify audited definitions")
    args = parser.parse_args()
    if args.check and args.output_dir is not None:
        parser.error("--check and --output-dir are mutually exclusive")
    manifest = json.loads((HERE / "reviewed.json").read_text())
    template_dir = HERE / "templates"
    check_storage(template_dir)  # .cs.in files are checked explicitly below as well.
    wrapper = args.repo_root.resolve() / "cui/src/ManagedCui"
    check_storage(wrapper)
    outputs = {}
    for name in FILES:
        data = (template_dir / (name + ".in")).read_bytes()
        if re.search(rb"\b_?arena\s*\.\s*Allocate\s*\(\s*(?:checked\s*\(\s*)?sizeof\b", data):
            raise ValueError(f"{name}.in: singleton arena allocation needs a retention audit")
        outputs[name] = data
    outputs["Ui.Utf8.g.cs"] = generate_utf8(wrapper, outputs)
    if args.native_root is not None:
        native = args.native_root.resolve()
        if sha((native / "include/cui/cui.h").read_bytes()) != manifest["native_header_sha256"]:
            raise ValueError("Upstream public header changed; review contracts and retention")
        for item in manifest["native_evidence"]:
            source = (native / "src" / item["file"]).read_text()
            if sha(function_source(source, item["symbol"]).encode()) != item["sha256"]:
                raise ValueError(f"Native definition changed: {item['symbol']}; re-audit retention")
        print(f"Native evidence: {len(manifest['native_evidence'])} definitions and public header match")
    destination = args.output_dir.resolve() if args.output_dir else wrapper
    # Preflight every file before any write. Preserve coordinator/handwritten fixes.
    errors = []
    for name, data in outputs.items():
        path = destination / name
        current = path.read_bytes() if path.exists() else None
        if args.output_dir is None:
            if current != data:
                errors.append(f"{name}: differs from authoritative template")
        elif current is not None and current != data and sha(current) != manifest["reviewed_outputs"][name]:
            errors.append(f"{name}: refuses to overwrite unrecognized edits; reconcile into template after review")
    if errors:
        raise ValueError("\n".join(errors))
    if args.output_dir:
        destination.mkdir(parents=True, exist_ok=True)
        for name, data in outputs.items():
            if not (destination / name).exists() or (destination / name).read_bytes() != data:
                (destination / name).write_bytes(data)
        print(f"Reproduced {len(outputs)} reviewed API files")
    else:
        print(f"Round-trip check: {len(outputs)} reviewed API files match; no singleton arena allocations")


if __name__ == "__main__":
    try:
        main()
    except (OSError, ValueError, KeyError) as error:
        sys.exit(str(error))
