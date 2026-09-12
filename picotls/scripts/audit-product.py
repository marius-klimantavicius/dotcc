#!/usr/bin/env python3
"""Inventory actual products without loading assemblies or executing their code."""
import argparse
import hashlib
import json
from pathlib import Path
import re
import sys
import tempfile
import unittest
import xml.etree.ElementTree as ET

ROOT = Path(__file__).resolve().parents[1]
MANIFEST = "Dotcc.SourceFiles.txt"
PROJECT = "TranslatedPicotls.csproj"
RUNTIME_MARKER = "// ---- Embedded DotCC.Libc runtime — single source of truth."
RULES = {
    "native import": r"\b(?:DllImport|LibraryImport)(?:Attribute)?\b",
    "native loader": r"\b(?:NativeLibrary|NativeImports|DotCcImports)\b",
    "process creation": r"\b(?:ProcessStartInfo|System\s*\.\s*Diagnostics\s*\.\s*Process)\b|\bProcess\s*\.\s*(?:Start|GetProcess)",
    "reflection": r"\b(?:System\s*\.\s*Reflection|AssemblyLoadContext|Assembly|Activator|MethodInfo|ConstructorInfo|FieldInfo|PropertyInfo|BindingFlags)\b|\.\s*(?:GetMethod|GetMethods|GetField|GetFields|GetProperty|GetProperties|MakeGenericType|MakeGenericMethod|DynamicInvoke)\s*\(",
    "dynamic or code generation": r"\b(?:dynamic|DynamicMethod|AssemblyBuilder|ReflectionEmit|CSharpCompilation|CSharpScript|CodeDomProvider)\b|\bExpression\s*\.\s*(?:Compile|Lambda)|\.\s*Compile\s*\(",
    "unmanaged callback": r"\bdelegate\s*\*\s*unmanaged\b|\bUnmanagedCallersOnly(?:Attribute)?\b",
    "native/process libc wrapper": r"\b(?:dlopen|dlsym|dlclose|dlerror|system|popen|pclose|getppid|getrusage|link|chown|mkfifo|kill|raise|fork|execv|execvp)\s*\(",
}
PATTERNS = {name: re.compile(pattern) for name, pattern in RULES.items()}
# Tokenize comments and strings together so a // inside a string is not mistaken
# for a comment. Preserve newlines and offsets for useful source locations.
TOKENS = re.compile(r'//[^\n]*|/\*[\s\S]*?\*/|@"(?:""|[^"])*"|"(?:\\.|[^"\\])*"|\'(?:\\.|[^\'\\])*\'')


def code_only(source):
    return TOKENS.sub(lambda match: re.sub(r"[^\n]", " ", match[0]), source)


def findings(source, path):
    code = code_only(source)
    return [{"path": str(path), "line": code.count("\n", 0, match.start()) + 1,
             "kind": kind, "token": match[0]}
            for kind, pattern in PATTERNS.items() for match in pattern.finditer(code)]


def native_imports(source, path, line_offset=0):
    # Keep attribute string arguments in this evidence; comment-only matches are
    # removed by requiring the same attribute token in the code-only view.
    code = code_only(source)
    result = []
    for match in re.finditer(r'\b(?:DllImport|LibraryImport)(?:Attribute)?\s*\(\s*"([^"\n]+)"([^\]]*)\]', source):
        if not code[match.start():].startswith(('DllImport', 'LibraryImport')):
            continue
        entry = re.search(r'EntryPoint\s*=\s*"([^"\n]+)"', match[2])
        result.append({'path': str(path), 'line': line_offset + source.count('\n', 0, match.start()) + 1,
                       'library': match[1], 'entry_point': entry[1] if entry else None})
    return result


def sha(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def generated_files(directory):
    names = (directory / MANIFEST).read_text().splitlines()
    if not names or len(names) != len(set(names)):
        raise ValueError(f"Empty or duplicate entries in {directory / MANIFEST}")
    for name in names:
        if not re.fullmatch(r"[A-Za-z_][A-Za-z_0-9.]*\.cs", name) or ".." in name:
            raise ValueError(f"Unsafe manifest entry: {name!r}")
    paths = [directory / name for name in names + [MANIFEST, PROJECT]]
    for path in paths:
        if path.is_symlink():
            raise ValueError(f"Symlink in generated product: {path}")
        if not path.is_file():
            raise FileNotFoundError(path)
    extra = [str(path.relative_to(directory)) for path in directory.rglob("*.cs")
             if "obj" not in path.relative_to(directory).parts and "bin" not in path.relative_to(directory).parts
             and str(path.relative_to(directory)) not in names]
    if extra:
        raise ValueError(f"Sources outside compiler manifest: {extra}")
    return paths


def project_inventory(path):
    tree = ET.parse(path)
    return {"path": str(path.relative_to(ROOT)), "sha256": sha(path),
            "references": [{"kind": node.tag.split("}")[-1], **node.attrib}
                           for node in tree.iter() if node.tag.split("}")[-1] in
                           ("ProjectReference", "PackageReference", "FrameworkReference", "Reference")],
            "frameworks": [node.text for node in tree.iter() if node.tag.split("}")[-1] in
                           ("TargetFramework", "TargetFrameworks")]}


def deps_inventory(path):
    data = json.loads(path.read_text())
    return {"path": str(path.relative_to(ROOT)), "sha256": sha(path),
            "runtime_target": data.get("runtimeTarget"), "libraries": data.get("libraries", {}),
            "targets": [{"target": target, "library": library,
                         "dependencies": info.get("dependencies", {}),
                         "assembly_assets": sorted(info.get("runtime", {})),
                         "native_assets": sorted(info.get("native", {})),
                         "runtime_targets": info.get("runtimeTargets", {})}
                        for target, libraries in data.get("targets", {}).items()
                        for library, info in libraries.items()]}


def input_state():
    config = ROOT / "config"
    inputs = json.loads((config / "inputs.json").read_text())
    source = ROOT / "ref" / inputs["picotls"]["directory"]
    core_sources = (config / "core-sources.txt").read_text().splitlines()
    host = []
    for line in (config / "host-sources.txt").read_text().splitlines():
        name = line.strip()
        if name and not name.startswith("#"):
            path = ROOT / name
            path.resolve().relative_to(ROOT)
            host.append({"path": name, "sha256": sha(path)})
    return {"inputs": inputs, "core_sources": core_sources,
            "core_wrappers": json.loads((config / "core-wrappers.json").read_text()),
            "core_source_sha256": {name: sha(source / name)
                                   for name in core_sources if name.strip() and not name.startswith("#")},
            "upstream_header_sha256": {str(path.relative_to(source)): sha(path)
                                       for path in sorted((source / "include").rglob("*.h"))},
            "defines": (config / "core-defines.txt").read_text().splitlines(), "host_sources": host}


def audit():
    report = {"format": "picotls-dependencies-v1", "status": "blocked", "blocked": [], "violations": [],
              "provider_sources": {}, "generated": {}, "projects": [], "dependency_manifests": [],
              "provider_findings": [], "translated_findings": [], "embedded_runtime_findings": [],
              "native_import_declarations": [],
              "algorithm_registration_evidence": [], "bcl_crypto_identifiers": []}
    provider = ROOT / "src/BclProvider"
    authored = [path for path in sorted(provider.rglob("*.cs"))
                if "obj" not in path.relative_to(provider).parts and "bin" not in path.relative_to(provider).parts]
    if not authored:
        report["blocked"].append("Provider source is missing")
    crypto = set()
    for path in authored:
        relative = str(path.relative_to(ROOT))
        source = path.read_text()
        report["provider_sources"][relative] = sha(path)
        report["provider_findings"].extend(findings(source, relative))
        report['native_import_declarations'].extend(native_imports(source, relative))
        crypto.update(re.findall(r"\b(?:AesGcm|Aes|SHA256|SHA384|IncrementalHash|ECDiffieHellman|ECDsa|RSA|X509Chain|RandomNumberGenerator)\b", code_only(source)))
        for number, line in enumerate(source.splitlines(), 1):
            if re.search(r"table->.*\.(?:id|name|hash|aead)\s*=|table->(?:Ecdsa|Rsa|First|Second|End)\s*=", line):
                report["algorithm_registration_evidence"].append({"path": relative, "line": number, "source": line.strip()})
    report["bcl_crypto_identifiers"] = sorted(crypto)
    report["violations"].extend(report["provider_findings"])
    success = ROOT / "artifacts/translation/success.json"
    provenance = None
    if success.is_file():
        try:
            provenance = json.loads(success.read_text())
            report["translation_record_sha256"] = sha(success)
            if provenance.get("format") != "picotls-translation-v1":
                raise ValueError("Unsupported translation record format")
            for name, value in input_state().items():
                if provenance.get(name) != value:
                    report["violations"].append(f"Translation input differs from success record: {name}")
            tool_hashes = provenance.get("tool_sha256", {})
            if len(tool_hashes) != 3:
                report["violations"].append("Translation record must contain all three tool hashes")
            for name, expected in tool_hashes.items():
                path = ROOT.parent / name
                path.resolve().relative_to(ROOT.parent)
                if not path.is_file() or sha(path) != expected:
                    report["violations"].append(f"Translation tool changed or missing: {name}")
        except (ValueError, OSError, TypeError, AttributeError) as error:
            report["violations"].append(f"Invalid translation provenance: {error}")
    else:
        report["blocked"].append("No successful translation provenance record")
    projects = [provider / "BclProvider.csproj"]
    for variant, name in (("raw", "TranslatedPicotlsRaw"), ("optimized", "TranslatedPicotls")):
        directory = ROOT / "generated" / name
        try:
            paths = generated_files(directory)
            hashes = {path.name: sha(path) for path in paths}
            report["generated"][variant] = hashes
            if provenance is not None and provenance.get(variant) != hashes:
                report["violations"].append(f"{variant} generated files differ from translation provenance")
            for path in paths:
                if path.suffix != ".cs":
                    continue
                source = path.read_text()
                application, marker, runtime = source.partition(RUNTIME_MARKER)
                relative = path.relative_to(ROOT)
                report["translated_findings"].extend(findings(application, relative))
                report['native_import_declarations'].extend(native_imports(application, relative))
                if marker:
                    offset = application.count("\n")
                    report['native_import_declarations'].extend(native_imports(runtime, relative, offset))
                    for finding in findings(runtime, relative):
                        finding["line"] += offset
                        report["embedded_runtime_findings"].append(finding)
            projects.append(directory / PROJECT)
        except FileNotFoundError as error:
            report["blocked"].append(f"{variant} product is missing: {error}")
        except ValueError as error:
            report["violations"].append(str(error))
    report["violations"].extend(report["translated_findings"])
    for project in projects:
        if not project.is_file():
            report["blocked"].append(f"Missing project: {project.relative_to(ROOT)}")
            continue
        try:
            inventory = project_inventory(project)
            report["projects"].append(inventory)
            for reference in inventory["references"]:
                kind = reference["kind"]
                if kind in ("PackageReference", "Reference"):
                    report["violations"].append(f"Unexpected authored dependency in {inventory['path']}: {reference}")
                if kind == "ProjectReference" and (project != provider / "BclProvider.csproj" or reference.get("Include") != "$(PicotlsProject)"):
                    report["violations"].append(f"Unexpected project dependency in {inventory['path']}: {reference}")
            manifests = sorted((project.parent / "bin/Release").rglob(project.stem + ".deps.json"))
            if not manifests:
                report["blocked"].append(f"No Release dependency manifest for {inventory['path']}; build the actual product")
            for manifest in manifests:
                dependency = deps_inventory(manifest)
                report["dependency_manifests"].append(dependency)
                for name, library in dependency["libraries"].items():
                    if library.get("type") == "package":
                        report["violations"].append(f"Runtime package dependency in {dependency['path']}: {name}")
        except (ValueError, OSError, TypeError, AttributeError, ET.ParseError) as error:
            report["violations"].append(f"Invalid project/dependency manifest: {error}")
    report["status"] = "fail" if report["violations"] else "blocked" if report["blocked"] else "pass"
    report["limits"] = [
        "Source markers and dependency manifests are an inventory, not a whole-program reachability or IL metadata proof.",
        "Existing embedded Libc native facilities are recorded separately; forbidden direct protocol/provider use is rejected.",
        "BCL cryptography, X509, allocation and networking can use platform native libraries. BCL-only does not mean zero native runtime dependencies.",
        "A matching translation record does not prove a dependency manifest or binary is fresh; rebuild, test and publish against recorded source hashes.",
        "No claim of NativeAOT, platform portability, cryptographic correctness or interoperability follows from this audit alone."]
    return report


class AuditTests(unittest.TestCase):
    def test_forbidden_code_and_comment_filter(self):
        for source in ('[DllImport("x")]', 'NativeLibrary.Load("x");', 'Process.Start("x");',
                       'Assembly.Load(bytes);', 'dynamic value;', 'Expression.Lambda(x).Compile();',
                       'delegate* unmanaged[Cdecl]<void> cb;', 'Libc.dlopen(path, 0);'):
            self.assertTrue(findings(source, "test.cs"), source)
        self.assertFalse(findings('// NativeLibrary.Load()\nstring url = "https://example/Assembly";\nAesGcm cipher;', "test.cs"))
        self.assertFalse(findings('public void Process() { } // application method, not process creation', "test.cs"))
        self.assertEqual(native_imports('[DllImport("libc", EntryPoint = "link")]\nvoid method();', 'test.cs')[0]['entry_point'], 'link')
        self.assertFalse(native_imports('// [DllImport("libc")]\n', 'test.cs'))

    def test_manifest_rejects_escape_duplicate_extra_and_symlink(self):
        with tempfile.TemporaryDirectory() as temporary:
            directory = Path(temporary)
            (directory / PROJECT).write_text('<Project/>')
            source = directory / 'Picotls.cs'
            source.write_text('class Picotls {}')
            for names in ('../escape.cs\n', 'Picotls.cs\nPicotls.cs\n', ''):
                (directory / MANIFEST).write_text(names)
                with self.assertRaises(ValueError): generated_files(directory)
            (directory / MANIFEST).write_text('Picotls.cs\n')
            self.assertEqual(len(generated_files(directory)), 3)
            extra = directory / 'extra.cs'; extra.write_text('')
            with self.assertRaises(ValueError): generated_files(directory)
            extra.unlink(); source.unlink(); source.symlink_to(directory / PROJECT)
            with self.assertRaises(ValueError): generated_files(directory)


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--output', type=Path, default=ROOT / 'artifacts/dependencies/report.json')
    parser.add_argument('--self-test', action='store_true')
    args = parser.parse_args()
    if args.self_test:
        suite = unittest.defaultTestLoader.loadTestsFromTestCase(AuditTests)
        return 0 if unittest.TextTestRunner(verbosity=2).run(suite).wasSuccessful() else 1
    report = audit()
    args.output.parent.mkdir(parents=True, exist_ok=True)
    args.output.write_text(json.dumps(report, indent=2) + '\n')
    print(f"{report['status'].upper()}: {len(report['violations'])} violations; {len(report['blocked'])} missing prerequisites; {args.output}")
    for error in report['violations'] + report['blocked']:
        print(error if isinstance(error, str) else json.dumps(error), file=sys.stderr)
    return {'pass': 0, 'fail': 1, 'blocked': 2}[report['status']]


if __name__ == '__main__':
    sys.exit(main())
