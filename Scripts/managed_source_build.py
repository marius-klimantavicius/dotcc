"""Reconstruct a source-only generated .NET project and its portable source package.

Only build commands run. Sources stay at their original paths for the first
build; a second clean build checks the portable package. No compiler, provider,
test entrypoint, project-reference build or campaign suite is launched.
"""
import argparse
import hashlib
import json
import os
from pathlib import Path
import re
import shutil
import time
import xml.etree.ElementTree as ET
import zipfile

from managed_test_storage import require_storage_reserve, validate_output_root, verify_files
from production_support import REPO, locked_command, subprocess


def digest(path):
    with Path(path).open("rb") as stream:
        return hashlib.file_digest(stream, "sha256").hexdigest()


def source_inputs(project, references=()):
    project = Path(project)
    manifest = project.parent / "Dotcc.SourceFiles.txt"
    names = manifest.read_text().splitlines()
    if (not names or len(names) != len(set(names)) or
            any(Path(name).name != name or not name.endswith(".cs") for name in names)):
        raise RuntimeError("Expected an exact flat generated-source manifest")
    actual = {p.name for p in project.parent.glob("*.cs")}
    if set(names) != actual:
        raise RuntimeError("Unmanifested or missing generated source")
    tree = ET.parse(project).getroot()
    if tree.attrib != {"Sdk": "Microsoft.NET.Sdk"}:
        raise RuntimeError("Only source-only Microsoft.NET.Sdk projects can be reconstructed")
    allowed = {"OutputType", "TargetFramework", "RootNamespace", "AssemblyName", "AllowUnsafeBlocks",
               "Nullable", "LangVersion", "ImplicitUsings"}
    properties = {}
    for group in tree:
        if group.tag != "PropertyGroup" or group.attrib:
            raise RuntimeError("Project dependencies/imports must be explicitly declared as assembly references")
        for node in group:
            if node.tag not in allowed or node.attrib or node.tag in properties:
                raise RuntimeError("Unsupported or conditional source project property: " + node.tag)
            properties[node.tag] = node.text or ""
    if (not re.fullmatch(r"net\d+\.\d+", properties.get("TargetFramework", "")) or
            properties.get("OutputType") not in ("Exe", "Library")):
        raise RuntimeError("Expected an explicit .NET framework and output type")
    sources = [project.parent / name for name in names]
    references = list(map(Path, references))
    if len({p.stem for p in references}) != len(references):
        raise RuntimeError("Duplicate declared assembly reference")
    paths = [project, manifest, *sources, *references]
    expected = {str(p): digest(p) for p in paths}
    verify_files(expected)
    return properties, sources, references, expected


def write_project(destination, properties, sources, references):
    tree = ET.Element("Project", Sdk="Microsoft.NET.Sdk")
    group = ET.SubElement(tree, "PropertyGroup")
    for name, value in dict(properties, EnableDefaultCompileItems="false",
                            ImportDirectoryBuildProps="false", ImportDirectoryBuildTargets="false").items():
        ET.SubElement(group, name).text = value
    items = ET.SubElement(tree, "ItemGroup")
    for name, value in sources:
        ET.SubElement(items, "Compile", Include=str(value), Link=name)
    for name, value in references:
        node = ET.SubElement(items, "Reference", Include=name)
        ET.SubElement(node, "HintPath").text = str(value)
        ET.SubElement(node, "Private").text = "true"
    target = ET.SubElement(tree, "Target", Name="RecordActualBuildInputs", BeforeTargets="CoreCompile")
    for filename, item in (("resolved-references.txt", "ReferencePath"), ("resolved-compile.txt", "Compile")):
        ET.SubElement(target, "WriteLinesToFile", File="$(MSBuildProjectDirectory)/" + filename,
                      Lines="@(" + item + "->'%(FullPath)')", Overwrite="true")
    ET.indent(tree, space="  ")
    ET.ElementTree(tree).write(destination, encoding="unicode")


def reconstruct(project, output_root, references=(), notices=()):
    properties, sources, references, expected = source_inputs(project, references)
    notices = list(map(Path, notices))
    if len({p.name for p in notices}) != len(notices):
        raise RuntimeError("Duplicate notice filename")
    expected.update({str(p): digest(p) for p in notices})
    verify_files(expected)
    output_root = validate_output_root(output_root, protected_roots=(REPO, project.parent, *(p.parent for p in references)))
    before_reserves = require_storage_reserve([REPO, output_root])
    output_root.mkdir(parents=True, exist_ok=False, mode=0o700)
    report = dict(status="failed", scope="Source-only build and portable source-package reconstruction",
                  execution="unrun-build-only", original_inputs_sha256=expected,
                  storage_reserves_before=before_reserves, commands=[], builds=[],
                  infrastructure_sha256={str(p): digest(p) for p in
                      (Path(__file__).resolve(), REPO / "Scripts/managed_test_storage.py", REPO / "Scripts/production_support.py")})
    receipt = output_root / "receipt.json"

    def save():
        receipt.write_text(json.dumps(report, indent=2) + "\n")

    def build(directory, label):
        require_storage_reserve([REPO, output_root])
        environment = dict(os.environ, DOTNET_CLI_TELEMETRY_OPTOUT="1", DOTNET_SKIP_FIRST_TIME_EXPERIENCE="1",
                           DOTNET_GENERATE_ASPNET_CERTIFICATE="false", DOTNET_NOLOGO="1",
                           DOTNET_ADD_GLOBAL_TOOLS_TO_PATH="false",
                           DOTNET_CLI_HOME=str(output_root / "dotnet-home"),
                           NUGET_PACKAGES=str(output_root / "nuget/packages"),
                           NUGET_HTTP_CACHE_PATH=str(output_root / "nuget/http"),
                           DOTNET_BUNDLE_EXTRACT_BASE_DIR=str(output_root / "bundles"),
                           TMPDIR=str(output_root / "tmp"), TEMP=str(output_root / "tmp"), TMP=str(output_root / "tmp"))
        project = directory / "Reconstructed.csproj"
        artifacts = output_root / (label + "-artifacts")
        command = ["dotnet", "build", str(project), "-c", "Release", "--artifacts-path", str(artifacts),
                   "--configfile", str(directory / "NuGet.Config"), "-p:ImportDirectoryBuildProps=false",
                   "-p:ImportDirectoryBuildTargets=false", "-p:NuGetAudit=false"]
        log = output_root / (label + ".log")
        row = dict(command=command, cwd=str(directory), environment={k: v for k, v in environment.items()
                   if k.startswith(("DOTNET_", "NUGET_")) or k in ("TMPDIR", "TEMP", "TMP")},
                   started_unix=time.time(), log=str(log))
        row["actual_command"] = locked_command(command, env=environment)
        report["commands"].append(row)
        try:
            with log.open("x") as stream:
                completed = subprocess.run(command, cwd=directory, env=environment, stdout=stream,
                                           stderr=subprocess.STDOUT, timeout=600)
            row["exit_code"] = completed.returncode
        except BaseException as error:
            row.update(exit_code=None, error_type=type(error).__name__, error=str(error))
            raise
        finally:
            row["finished_unix"] = time.time()
            row["log_sha256"] = digest(log)
            save()
        if completed.returncode:
            raise RuntimeError("Source reconstruction build failed: " + str(log))
        reference_paths = Path(directory / "resolved-references.txt").read_text().splitlines()
        compile_paths = Path(directory / "resolved-compile.txt").read_text().splitlines()
        resolved = {p: digest(p) for p in reference_paths}
        compiled = {p: digest(p) for p in compile_paths}
        verify_files(resolved)
        verify_files(compiled)
        outputs = {str(p): digest(p) for p in (artifacts / "bin").rglob("*") if p.is_file()}
        assets = list((artifacts / "obj").rglob("project.assets.json"))
        if len(assets) != 1 or not outputs or not resolved:
            raise RuntimeError("Missing actual build outputs, resolved references or dependency assets")
        dependencies = json.loads(assets[0].read_text())
        if dependencies["libraries"]:
            raise RuntimeError("Undeclared package/project dependency in source reconstruction")
        result = dict(label=label, project=str(project), project_sha256=digest(project),
                      output_sha256=outputs, resolved_reference_sha256=resolved, compiled_sha256=compiled,
                      assets=str(assets[0]), assets_sha256=digest(assets[0]),
                      framework_references=dependencies["project"]["frameworks"],
                      declared_assembly_references={str(p): digest(p) for p in references})
        expected_compile = {str(directory / value) if not Path(value).is_absolute() else value
                            for value in [n.get("Include") for n in ET.parse(project).iter("Compile")]}
        if not expected_compile.issubset(compiled):
            raise RuntimeError("Actual compiler inputs omitted a declared original source")
        report["builds"].append(result)
        verify_files(expected)
        save()

    save()
    try:
        (output_root / "tmp").mkdir()
        linked, package = output_root / "linked", output_root / "source-package"
        linked.mkdir(); package.mkdir()
        config = '<configuration><packageSources><clear /></packageSources></configuration>\n'
        for directory in (linked, package):
            (directory / "NuGet.Config").write_text(config)
        write_project(linked / "Reconstructed.csproj", properties, [(p.name, p) for p in sources],
                      [(p.stem, p) for p in references])
        build(linked, "original-paths")
        entries = {}
        for collection, folder in ((sources, "sources"), (references, "references"), (notices, "notices")):
            if collection:
                (package / folder).mkdir()
            for path in collection:
                relative = folder + "/" + path.name
                shutil.copyfile(path, package / relative)
                entries[relative] = dict(original=str(path), sha256=expected[str(path)])
        (package / "provenance").mkdir()
        for path, name in ((project, "original-project.xml"),
                           (project.parent / "Dotcc.SourceFiles.txt", "Dotcc.SourceFiles.txt")):
            relative = "provenance/" + name
            shutil.copyfile(path, package / relative)
            entries[relative] = dict(original=str(path), sha256=expected[str(path)])
        write_project(package / "Reconstructed.csproj", properties,
                      [(p.name, "sources/" + p.name) for p in sources],
                      [(p.stem, "references/" + p.name) for p in references])
        metadata = dict(original_project=str(project), original_project_sha256=expected[str(project)],
                        target_framework=properties["TargetFramework"], files=entries,
                        package_dependencies=[], execution="unrun-build-only")
        (package / "source-package.json").write_text(json.dumps(metadata, indent=2) + "\n")
        archive = output_root / "source-package.zip"
        with zipfile.ZipFile(archive, "x", compression=zipfile.ZIP_DEFLATED) as zipped:
            for path in sorted(package.rglob("*")):
                if path.is_file():
                    zipped.write(path, path.relative_to(package).as_posix())
        rebuilt = output_root / "reconstructed-package"
        with zipfile.ZipFile(archive) as zipped:
            zipped.extractall(rebuilt)
            for relative, entry in entries.items():
                if hashlib.sha256(zipped.read(relative)).hexdigest() != entry["sha256"]:
                    raise RuntimeError("Packaged source/reference bytes changed")
        build(rebuilt, "package")
        verify_files(expected)
        report.update(status="passed-build-only", package=str(archive), package_sha256=digest(archive),
                      source_package=metadata, storage_reserves_after=require_storage_reserve([REPO, output_root]))
    except BaseException as error:
        report["error"] = str(error)
        raise
    finally:
        save()
    return report


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--source-project", type=Path, required=True)
    parser.add_argument("--reference", type=Path, action="append", default=[])
    parser.add_argument("--notice", type=Path, action="append", default=[])
    parser.add_argument("--output-root", type=Path, required=True)
    args = parser.parse_args()
    reconstruct(args.source_project, args.output_root, args.reference, args.notice)
    print("BUILD-ONLY RECEIPT", args.output_root / "receipt.json")


if __name__ == "__main__":
    main()
