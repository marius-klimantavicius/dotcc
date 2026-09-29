#!/usr/bin/env python3
"""Freeze complete textual inputs; these inventories do not prove emitted coverage."""
import argparse
import hashlib
import json
import os
from pathlib import Path
import re

ROOT = Path(__file__).resolve().parents[1]


def read(path):
    return path.read_text(encoding="utf-8", errors="replace")


def guarded_lines(text):
    stack = []
    for number, line in enumerate(text.splitlines(), 1):
        directive = re.match(r"\s*#\s*(if|ifdef|ifndef|elif|else|endif)\b(.*)", line)
        if directive:
            kind, expression = directive.groups()
            expression = expression.strip()
            if kind in ("if", "ifdef", "ifndef"):
                stack.append(f"{kind} {expression}")
            elif kind in ("elif", "else") and stack:
                stack[-1] += f"; {kind} {expression}".rstrip()
            elif kind == "endif" and stack:
                stack.pop()
        yield number, line, list(stack), directive


def inventories(source, pin):
    cmake, make = (read(source / name) for name in ("CMakeLists.txt", "Makefile.am"))
    build_references = {
        name: sorted(set(re.findall(r"\b(?:src|include)/[\w./-]+\.[ch]\b", content)))
        for name, content in (("CMakeLists.txt", cmake), ("Makefile.am", make))}
    files, branches, cases, registrations = [], [], [], []
    for path in sorted(source.rglob("*")):
        if not path.is_file():
            continue
        name = path.relative_to(source).as_posix()
        if name.startswith(("src/", "include/")) and path.suffix in (".c", ".h"):
            scope = "windows" if name.startswith("src/win/") else "unix" if name.startswith("src/unix/") else "common-or-public-header"
            files.append({"path": name, "kind": "translation-unit" if path.suffix == ".c" else "header",
                          "scope": scope, "sha256": hashlib.sha256(path.read_bytes()).hexdigest(),
                          "build_references": [build for build, entries in build_references.items() if name in entries],
                          "emitted_variants": [], "status": "untranslated"})
            for number, line, stack, directive in guarded_lines(read(path)):
                if directive:
                    branches.append({"path": name, "line": number, "directive": line.strip(),
                                     "conditional_stack": stack, "coverage": "unmapped"})
        if name.startswith("test/") and path.suffix in (".c", ".h"):
            for number, line, stack, _ in guarded_lines(read(path)):
                match = re.match(r"\s*(TEST_(?:FS_)?IMPL|HELPER_IMPL|TEST_(?:FS_)?DECLARE|HELPER_DECLARE)\s*\(\s*(\w+)\s*\)", line)
                if match:
                    cases.append({"name": match[2], "kind": match[1].lower(), "path": name,
                                  "line": number, "guards": stack, "translation": "unrun",
                                  "native": "unrun", "managed": "unrun"})
                entry = re.match(r"\s*(TEST_(?:FS_)?ENTRY(?:_\w+)?|HELPER_ENTRY)\s*\(\s*(\w+)\s*([^)]*)\)", line)
                if entry:
                    registrations.append({"name": entry[2], "macro": entry[1], "arguments": entry[3].strip(),
                                          "path": name, "line": number, "guards": stack,
                                          "translation": "unrun", "managed": "unrun"})
    api_text = re.sub(r"/\*.*?\*/", "", read(source / "include/uv.h"), flags=re.S)
    api = []
    for match in re.finditer(r"UV_EXTERN\s+([^;]+);", api_text, flags=re.S):
        declaration = " ".join(match[1].split())
        name = re.search(r"\b(uv_\w+)\s*\(", declaration)
        if name:
            api.append({"symbol": name[1], "declaration": declaration, "status": "unmapped"})
    macro_maps = []
    for name in ("UV_HANDLE_TYPE_MAP", "UV_REQ_TYPE_MAP"):
        match = re.search(r"#define " + name + r"\(XX\)(.*?)(?=\n\s*\n)", api_text, re.S)
        entries = re.findall(r"XX\(\s*(\w+)\s*,\s*(\w+)\s*\)", match[1]) if match else []
        macro_maps.append({"macro": name, "entries": [{"tag": tag, "name": item} for tag, item in entries],
                           "expansion_audit": "unresolved; enums/unions and downstream macro uses must be audited per backend"})
    manifest = {"schema_version": 1, "source_revision": pin["revision"],
                "scope": "all product .c/.h files, build references and textual conditionals; no emitted-branch coverage claimed",
                "files": files, "build_references": build_references,
                "conditionals": branches, "public_api": api, "public_macro_maps": macro_maps,
                "reconciliation": [
                    {"path": "src/win/snprintf.c", "finding": "CMake product source; autotools omits product declaration; retained"},
                    {"path": "src/unix/os390-proctitle.c", "finding": "CMake OS390 selection; autotools selects proctitle.c; both retained; policy unresolved"} ]}
    tests = {"schema_version": 1, "source_revision": pin["revision"],
             "scope": "all files, textual test/helper definitions/declarations and registrations; macro-generated implementations are represented by registrations, not inferred definition counts; guards retained verbatim; applicability not yet resolved",
             "files": [path.relative_to(source).as_posix() for path in sorted((source / "test").rglob("*")) if path.is_file()],
             "cases": cases, "registrations": registrations, "excluded_cases": [],
             "execution_results": "artifacts/campaign native-upstream-results.json and docs/validation.md"}
    return {"source-manifest.json": manifest, "test-inventory.json": tests}


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--check", action="store_true")
    parser.add_argument("--hashes", choices=("off", "warn", "strict"),
                        default=os.environ.get("DOTCC_CAMPAIGN_HASHES", "warn"))
    args = parser.parse_args()
    pin = json.loads(read(ROOT / "config/sources.json"))["product"]
    source = ROOT / "ref" / pin["directory"]
    if not (source / "include/uv.h").is_file():
        parser.error("missing pinned source; run fetch.sh")
    results = inventories(source, pin)
    for name, record in results.items():
        path = ROOT / "config" / name
        text = json.dumps(record, indent=2) + "\n"
        if args.check:
            if not path.is_file():
                raise SystemExit(f"Inventory differs: {path}; regenerate and review edited inputs")
            expected = json.loads(path.read_text())
            if args.hashes != "strict":
                expected_hashes = [row.pop("sha256", None) for row in expected.get("files", []) if isinstance(row, dict)]
                actual = json.loads(text)
                actual_hashes = [row.pop("sha256", None) for row in actual.get("files", []) if isinstance(row, dict)]
                if args.hashes == "warn" and expected_hashes != actual_hashes:
                    print(f"WARNING: source hash drift in {path}; edited inputs retained")
            else:
                actual = record
            if expected != actual:
                raise SystemExit(f"Inventory structure differs: {path}; regenerate and review changed source closure")
        else:
            path.write_text(text)
        print(f"{'checked' if args.check else 'wrote'} {path}")
    product = results["source-manifest.json"]
    print(json.dumps({"product_files": len(product["files"]),
                      "translation_units": sum(row["kind"] == "translation-unit" for row in product["files"]),
                      "conditional_directives": len(product["conditionals"]),
                      "public_api_declarations": len(product["public_api"]),
                      "test_helper_definitions": sum(row["kind"].endswith("impl") for row in results["test-inventory.json"]["cases"])}))


if __name__ == "__main__":
    main()
