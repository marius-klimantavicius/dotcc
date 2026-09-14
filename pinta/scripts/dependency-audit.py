#!/usr/bin/env python3
"""Audit emitted source/project dependencies; optionally inspect consumer deps files.

This is a static audit, not proof of runtime reachability or execution parity.
Run after translation; reports include hashes so later edits invalidate evidence.
"""
import argparse
import hashlib
import json
from pathlib import Path
import re
import sys
import xml.etree.ElementTree as ET

ROOT = Path(__file__).resolve().parents[1]
FORBIDDEN = re.compile(r"Marius\.(?:Script|Pinta\.(?:Managed|Script|Web|Debugger))|"
                       r"(?:libpinta\.(?:so|dylib)|pinta\.dll)|"
                       r"System\.Reflection\.Emit|AssemblyBuilder|DynamicMethod", re.I)


def sha(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--profile", choices=["release", "debug"], default="release")
    parser.add_argument("--pristine", action="store_true", help="Audit untouched-source translation variant")
    parser.add_argument("--deps", type=Path, action="append", default=[],
                        help="Consumer .deps.json to inspect (repeat for each form/runtime)")
    args = parser.parse_args()
    variant = args.profile + ("-pristine" if args.pristine else "")
    report = {"format": "pinta-dependency-audit-v1", "profile": args.profile,
              "variant": variant,
              "scope": "static emitted source, explicit projects, optional consumer deps",
              "files": {}, "native_imports": [], "dependencies": {}, "errors": []}
    errors = report["errors"]

    def read(path):
        if not path.is_file():
            errors.append(f"Missing required file: {path}")
            return ""
        report["files"][str(path.relative_to(ROOT)) if path.is_relative_to(ROOT) else str(path)] = sha(path)
        return path.read_text()

    manifest = read(ROOT / "config/core-sources.txt")
    units = [line.strip() for line in manifest.splitlines() if line.strip() and not line.startswith("#")]
    if len(units) != 29 or len(set(units)) != 29:
        errors.append("Core manifest must contain 29 distinct translation units")
    if any(not unit.startswith("Marius.Pinta/src/") or Path(unit).name in
           {"platform-windows.c", "sample.c", "native-function.c"} for unit in units):
        errors.append("Core source closure contains an unselected unit")

    base = ROOT / "generated" / (variant if variant != "release" else "")
    receipt_path = ROOT / "artifacts/translation" / variant / "success.json"
    receipt_text = read(receipt_path)
    receipt = json.loads(receipt_text) if receipt_text else {}
    for name, digest in receipt.get("inputs", {}).get("sha256", {}).items():
        path = ROOT.parent / name
        if not path.is_file() or sha(path) != digest:
            errors.append(f"Translation input changed since emission: {name}")
    projects = list((ROOT / "src").rglob("*.csproj")) + list((ROOT / "tests").rglob("*.csproj"))
    sources = [p for p in (ROOT / "src").rglob("*.cs") if not {"bin", "obj"} & set(p.parts)]
    for form, name in [("raw", "TranslatedPintaRaw"), ("optimized", "TranslatedPinta")]:
        directory = base / name
        names = read(directory / "Dotcc.SourceFiles.txt").splitlines()
        if not names or len(names) != len(set(names)):
            errors.append(f"{form}: missing or duplicate emitted source manifest")
        actual = {str(path.relative_to(directory)) for path in directory.rglob("*.cs")
                  if not {"bin", "obj"} & set(path.relative_to(directory).parts)}
        if actual != set(names):
            errors.append(f"{form}: missing or unmanifested C# source files")
        for name in names:
            if Path(name).name != name or not name.endswith(".cs"):
                errors.append(f"{form}: invalid emitted source filename {name!r}")
                continue
            if (directory / name).is_symlink():
                errors.append(f"{form}: emitted source is a symlink: {name}")
            sources.append(directory / name)
        projects.append(directory / "TranslatedPinta.csproj")
        for name, digest in receipt.get(form, {}).items():
            path = directory / name
            if Path(name).name != name or not path.is_file() or sha(path) != digest:
                errors.append(f"{form}: output no longer matches translation receipt: {name}")
        if not receipt.get(form):
            errors.append(f"{form}: no successful translation hashes")

    for project in projects:
        content = read(project)
        if not content:
            continue
        for node in ET.fromstring(content).iter():
            tag = node.tag.rsplit("}", 1)[-1]
            if tag in {"PackageReference", "Reference", "NativeReference", "COMReference"}:
                errors.append(f"Explicit external dependency in {project}: {tag} {node.attrib}")
            if tag == "ProjectReference":
                target = node.get("Include", "")
                if FORBIDDEN.search(target) or re.search(r"DotCC(?:\.|[/\\])", target):
                    errors.append(f"Forbidden product project reference in {project}: {target}")

    for source in sources:
        content = read(source)
        # Search executable text; comments documenting excluded dependencies are harmless.
        content = re.sub(r"/\*.*?\*/|//[^\n]*", "", content, flags=re.S)
        if FORBIDDEN.search(content):
            errors.append(f"Forbidden dependency or runtime code generation in {source}")
        for match in re.finditer(r'(?:DllImport|LibraryImport|NativeLibrary\.Load)\s*\(\s*"([^"]+)"', content):
            report["native_imports"].append({"source": str(source.relative_to(ROOT)), "library": match[1]})

    for deps in args.deps:
        deps = deps.resolve()
        content = read(deps)
        if not content:
            continue
        libraries = json.loads(content).get("libraries", {})
        report["dependencies"][str(deps)] = libraries
        for library, metadata in libraries.items():
            if FORBIDDEN.search(library) or library.startswith("DotCC"):
                errors.append(f"Forbidden consumer dependency: {library}")
            if metadata.get("type") == "package" and not library.startswith(("Microsoft.NETCore.App", "runtime.")):
                errors.append(f"Consumer has non-BCL package dependency: {library}")

    report["passed"] = not errors
    report["limitations"] = ["Static source audit does not establish native-call reachability.",
                              "No consumer dependency evidence unless --deps is supplied.",
                              "Runtime, NativeAOT and platform qualification require execution receipts."]
    output = ROOT / "artifacts" / f"dependency-audit-{variant}.json"
    output.parent.mkdir(parents=True, exist_ok=True)
    output.write_text(json.dumps(report, indent=2) + "\n")
    for error in errors:
        print(error, file=sys.stderr)
    print(f"{'PASS' if report['passed'] else 'FAIL'} static dependency audit: {output}")
    return int(bool(errors))


if __name__ == "__main__":
    sys.exit(main())
