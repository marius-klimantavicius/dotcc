#!/usr/bin/env python3
"""Prepare the existing BCL provider against unmodified EDG generated sources.

Authored provider/test sources are copied with explicit ABI spelling adapters;
their input hashes and all replacements are recorded. Generated C# is linked,
never edited. Original dotcc projects and build directories remain untouched.
"""
from __future__ import annotations

import argparse
import hashlib
import json
import re
from pathlib import Path
import xml.etree.ElementTree as ET

ROOT = Path(__file__).resolve().parents[1]
HERE = Path(__file__).resolve().parent


def digest(path: Path) -> str:
    return hashlib.sha256(path.read_bytes()).hexdigest()


def adapt(source: str, filename: str, emitted_types: set[str], handshake_union: str) -> tuple[str, list[dict]]:
    changes = []
    # Dotcc used C tag spellings; EDG preserves public typedef spellings.
    for old in sorted(set(re.findall(r"\b(?:st|en)_ptls_[A-Za-z0-9_]+\b", source))):
        new = old[3:]
        if new not in emitted_types:
            continue
        count = len(re.findall(r"\b" + re.escape(old) + r"\b", source))
        source = re.sub(r"\b" + re.escape(old) + r"\b", new, source)
        changes.append({"old": old, "new": new, "count": count})
    replacements = {
        ".__anon___Anon16.": "." + handshake_union + ".",
        "@base = Libc.L(": "@base = (byte*)Libc.L(",
        "ptls_set_server_name(client, name,": "ptls_set_server_name(client, (sbyte*)name,",
        "byte* name)": "sbyte* name)",
        "name.Length == 0 ? null : serverName,": "name.Length == 0 ? null : (sbyte*)serverName,",
        "ptls_server_name_is_ipaddr(p)": "ptls_server_name_is_ipaddr((sbyte*)p)",
        "ptls_base64_decode(p,": "ptls_base64_decode((sbyte*)p,",
        "PicoTls.ptls_jsonescape(output, p,": "(byte*)PicoTls.ptls_jsonescape((sbyte*)output, (sbyte*)p,",
        "ptls_set_server_name(_native, data,": "ptls_set_server_name(_native, (sbyte*)data,",
        "ptls_export_secret(_native, target, (ulong)length, encoded,": "ptls_export_secret(_native, target, (ulong)length, (sbyte*)encoded,",
        "ptls_set_server_name(tls, input->server_name.@base,": "ptls_set_server_name(tls, (sbyte*)input->server_name.@base,",
        "ptls_set_negotiated_protocol(tls, selected,": "ptls_set_negotiated_protocol(tls, (sbyte*)selected,",
        "ReadCString(byte* value)": "ReadCString(sbyte* value)",
        "table->Sha256.empty_digest,": "&table->Sha256.empty_digest[0],",
        "table->Sha384.empty_digest,": "&table->Sha384.empty_digest[0],",
        "algorithm->empty_digest,": "&algorithm->empty_digest[0],",
        "supplementary->output,": "&supplementary->output[0],",
        "supplementary.output,": "&supplementary.output[0],",
        "ptls_t*, byte*, delegate*": "ptls_t*, sbyte*, delegate*",
        "ptls_t* tls, byte* serverName,": "ptls_t* tls, sbyte* serverName,",
    }
    for old, new in replacements.items():
        count = source.count(old)
        if count:
            source = source.replace(old, new)
            changes.append({"old": old, "new": new, "count": count})
    return "using Libc = Managed.Security.EdgProviderMemory;\n" + source, changes


def project(path: Path, name: str, sources: list[Path], references: list[Path], executable: bool = False) -> None:
    root = ET.Element("Project", Sdk="Microsoft.NET.Sdk")
    properties = ET.SubElement(root, "PropertyGroup")
    for key, value in {
        "TargetFramework": "net10.0", "LangVersion": "14", "Nullable": "enable",
        "ImplicitUsings": "enable", "AllowUnsafeBlocks": "true", "IsAotCompatible": "true",
        "EnableDefaultCompileItems": "false", "AssemblyName": name,
        "OutputType": "Exe" if executable else "Library",
        "NoWarn": "$(NoWarn);CS0162;CS0164;CS0219;CS0414;CS0675;CS1717;CS1718;CS8981",
    }.items():
        ET.SubElement(properties, key).text = value
    items = ET.SubElement(root, "ItemGroup")
    for source in sources:
        ET.SubElement(items, "Compile", Include=str(source.resolve()))
    for reference in references:
        ET.SubElement(items, "ProjectReference", Include=str(reference.resolve()))
    if name == "BclProvider":
        for friend in ("ProviderVectors", "ManagedConsumer"):
            ET.SubElement(items, "InternalsVisibleTo", Include=friend)
    ET.indent(root, space="  ")
    path.write_text(ET.tostring(root, encoding="unicode") + "\n")


def prepare(core: Path, output: Path) -> dict:
    core = core.resolve()
    output = output.resolve()
    output.mkdir(parents=True, exist_ok=True)
    source_project = core / "Generated.csproj"
    tree = ET.parse(source_project)
    sources = [(core / node.attrib["Include"]).resolve() for node in tree.findall(".//Compile")]
    generated_text = "\n".join(path.read_text() for path in sources if path.name.startswith("Translated"))
    emitted_types = set(re.findall(r"\b(?:struct|enum)\s+([A-Za-z_][A-Za-z0-9_]*)", generated_text))
    match = re.search(r"struct ptls_handshake_properties_t\s*\{.*?public \w+ (\w+);", generated_text, re.S)
    if not match:
        raise ValueError("Cannot identify the translated handshake-properties anonymous union")
    handshake_union = match.group(1)
    core_dir = output / "Core"
    core_dir.mkdir(exist_ok=True)
    core_project = core_dir / "EdgPicotls.csproj"
    project(core_project, "EdgPicotls", sources + [HERE / "managed/ProviderMemory.cs"], [])
    report = {"generatedProject": str(source_project), "generatedProjectSha256": digest(source_project),
              "generatedSources": [{"path": str(p), "sha256": digest(p)} for p in sources],
              "authoredSources": [{"path": str(HERE / "managed/ProviderMemory.cs"), "sha256": digest(HERE / "managed/ProviderMemory.cs"), "replacements": []}], "projects": {"Core": str(core_project)}}
    provider_project = output / "BclProvider/BclProvider.csproj"
    sets = {"BclProvider": ROOT / "src/BclProvider", "ProviderVectors": ROOT / "tests/ProviderVectors",
            "UpstreamVectors": ROOT / "tests/UpstreamVectors", "TranslatedAbi": ROOT / "tests/TranslatedAbi",
            "ManagedConsumer": ROOT / "samples/ManagedConsumer", "TlsTests": ROOT / "tests/TlsTests"}
    for name, directory in sets.items():
        target = output / name
        target.mkdir(exist_ok=True)
        files = []
        for source in sorted(directory.glob("*.cs")):
            text, replacements = adapt(source.read_text(), source.name, emitted_types, handshake_union)
            destination = target / source.name
            destination.write_text(text)
            files.append(destination)
            report["authoredSources"].append({"path": str(source), "sha256": digest(source),
                "adaptedPath": str(destination), "adaptedSha256": digest(destination), "replacements": replacements})
        if name == "TlsTests":
            credential_source = ROOT / "tests/IndependentPeer/Credentials.cs"
            files.append(credential_source)
            report["authoredSources"].append({"path": str(credential_source), "sha256": digest(credential_source), "replacements": []})
        path = target / (name + ".csproj")
        references = [core_project] if name in ("BclProvider", "TranslatedAbi") else [provider_project]
        project(path, name, files, references, executable=name != "BclProvider")
        report["projects"][name] = str(path)
    (output / "provider-manifest.json").write_text(json.dumps(report, indent=2) + "\n")
    return report


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--core", required=True, type=Path)
    parser.add_argument("--output", required=True, type=Path)
    args = parser.parse_args()
    print(json.dumps(prepare(args.core, args.output), indent=2))
