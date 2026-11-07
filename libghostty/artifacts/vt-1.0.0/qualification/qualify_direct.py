#!/usr/bin/env python3
"""Bounded direct-engine qualification: native differential, 45 cases, four rows.

Only individual C controls are renamed, and only generated output is copied.
All products, binaries, build state and detailed logs remain disposable.
"""
import argparse
import hashlib
import json
import os
from pathlib import Path
import shutil
import subprocess
import time
import xml.etree.ElementTree as ET


def digest(path):
    h = hashlib.sha256()
    with Path(path).open("rb") as source:
        for block in iter(lambda: source.read(1024 * 1024), b""):
            h.update(block)
    return h.hexdigest()


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("action", choices=("prepare", "qualify"))
    parser.add_argument("--repo", type=Path, default=Path("/home/marius/p/dotcc"))
    parser.add_argument("--work", type=Path, default=Path("/mnt/storage/dotcc-libghostty-disposable/vt-1.0.0/qualification"))
    parser.add_argument("--compiler", type=Path, default=Path("/mnt/storage/dotcc-libghostty-disposable/vt-1.0.0/compiler-build/bin/DotCC/release/dotcc.dll"))
    parser.add_argument("--engine", type=Path)
    parser.add_argument("--postprocessor", type=Path)
    parser.add_argument("--rid", default="linux-x64")
    parser.add_argument("--forms", default="raw,processed")
    parser.add_argument("--modes", default="jit,aot")
    args = parser.parse_args()
    repo, work, compiler = (p.resolve() for p in (args.repo, args.work, args.compiler))
    root = repo / "libghostty"
    evidence = root / "artifacts/vt-1.0.0/qualification"
    boundary_evidence = root / "artifacts/vt-1.0.0/boundary"
    if not work.is_relative_to(Path("/mnt/storage/dotcc-libghostty-disposable/vt-1.0.0")):
        parser.error("qualification work must remain in the release disposable tree")
    work.mkdir(parents=True, exist_ok=True)
    evidence.mkdir(parents=True, exist_ok=True)
    temp = work / "tmp"
    temp.mkdir(exist_ok=True)
    env = dict(os.environ, TMPDIR=str(temp), DOTNET_CLI_TELEMETRY_OPTOUT="1")
    stages_path = evidence / (args.action + "-stages.json")
    stages = []

    def run(label, argv, *, timeout=1800, transcript=False):
        argv = list(map(str, argv))
        log = work / (label + ".log")
        error_log = work / (label + ".stderr")
        print("START " + label, flush=True)
        started = time.monotonic()
        with log.open("w") as stdout, error_log.open("w") as stderr:
            result = subprocess.run(argv, cwd=repo, env=env, stdout=stdout,
                                    stderr=stderr, timeout=timeout)
        row = dict(stage=label, argv=argv, exit_code=result.returncode,
                   seconds=round(time.monotonic() - started, 3),
                   stdout=str(log), stderr=str(error_log))
        stages.append(row)
        stages_path.write_text(json.dumps(stages, indent=2) + "\n")
        print("DONE " + label + " " + str(result.returncode), flush=True)
        if result.returncode:
            (evidence / (label + "-failure.txt")).write_text(
                log.read_text()[-12000:] + "\n" + error_log.read_text()[-12000:])
            raise SystemExit(result.returncode)
        if transcript:
            if error_log.stat().st_size:
                raise RuntimeError(label + ": unexpected stderr")
            return log.read_text()

    controls = [
        ("oracle", root / "tests/native/oracle.c", "dotcc_oracle_main"),
        ("boundary-control", root / "tests/native/boundary_control.c", "dotcc_boundary_main"),
        ("abi-layout", root / "tests/native/abi_layout.c", "dotcc_abi_main"),
        ("version-build-info", boundary_evidence / "version_build_info.c", "dotcc_version_build_info_main"),
        ("api-delta", boundary_evidence / "api-delta-probe.c", "dotcc_api_delta_main"),
    ]
    expected_files = [root / "tests/native/expected.txt", root / "tests/native/boundary-expected.txt",
                      root / "tests/native/abi-expected.txt", boundary_evidence / "version-build-info-native.stdout",
                      boundary_evidence / "api-delta-native.stdout"]
    expected = "".join(p.read_text() for p in expected_files)
    if sum(line.startswith("PASS ") for line in expected.splitlines()) != 45:
        raise RuntimeError("Native differential requires exactly 45 PASS lines")
    (evidence / "expected-native.stdout").write_text(expected)
    pin = json.loads((root / "config/source.json").read_text())
    include = root / "ref" / pin["directory"] / "include"
    source_dir, objects = work / "controls", work / "objects"
    if args.action == "prepare":
        source_dir.mkdir(exist_ok=True)
        objects.mkdir(exist_ok=True)
        inputs = []
        for name, source, entry in controls:
            text = source.read_text()
            anchor = "int main(void)"
            if text.count(anchor) != 1:
                raise RuntimeError("Ambiguous C entrypoint: " + str(source))
            staged = source_dir / (name + ".c")
            staged.write_text(text.replace(anchor, "int " + entry + "(void)"))
            obj = objects / (name + ".csobj")
            run("emit-" + name, ["dotnet", compiler, staged, "--emit=obj", "--aggregate-data",
                                "-std=c11", "-I" + str(include), "-o", obj], timeout=300)
            inputs.append(dict(name=name, source=str(source), source_sha256=digest(source),
                               entry=entry, object=str(obj), object_sha256=digest(obj),
                               object_bytes=obj.stat().st_size))
        source = root / "src/Boundary/ghostty_boundary.c"
        obj = objects / "boundary.csobj"
        run("emit-boundary", ["dotnet", compiler, source, "--emit=obj", "--aggregate-data",
                              "-std=c11", "-I" + str(include), "-o", obj], timeout=300)
        inputs.append(dict(name="boundary", source=str(source), source_sha256=digest(source),
                           object=str(obj), object_sha256=digest(obj), object_bytes=obj.stat().st_size))
        (evidence / "prepared.json").write_text(json.dumps(dict(compiler=str(compiler),
            compiler_sha256=digest(compiler), include=str(include), cases=45,
            inputs=inputs, expected_sha256=digest(evidence / "expected-native.stdout")), indent=2) + "\n")
        return

    if args.engine is None:
        parser.error("qualify requires --engine")
    engine = args.engine.resolve(strict=True)
    prepared = json.loads((evidence / "prepared.json").read_text())
    for item in prepared["inputs"]:
        if digest(item["source"]) != item["source_sha256"] or digest(item["object"]) != item["object_sha256"]:
            raise RuntimeError("Qualification input changed since preparation: " + item["name"])
    forms = args.forms.split(",")
    modes = args.modes.split(",")
    if any(f not in ("raw", "processed") for f in forms) or any(m not in ("jit", "aot") for m in modes):
        parser.error("forms must be raw,processed; modes must be jit,aot")
    raw = work / "raw/TranslatedGhosttyVt"
    project = raw / "TranslatedGhosttyVt.csproj"
    run("link", ["dotnet", compiler, engine, *[x["object"] for x in prepared["inputs"]],
                 "--emit=managedlib", "--nest-types", "--runtime=auto", "--class-name", "GhosttyVt",
                 "--namespace", "Managed", "--split=size", "--split-size=262144", "--literal-pool",
                 "--deduplicate-inline", "--prune-unused-inline", "--export-enum", "DOTCC_STYLE_*", "-o", raw])
    run("raw-build", ["dotnet", "build", project, "-c", "Release", "--nologo", "-m:1"])
    projects = {"raw": project}
    if "processed" in forms:
        if args.postprocessor is None:
            parser.error("processed form requires --postprocessor")
        processed = work / "processed/TranslatedGhosttyVt"
        if processed.exists():
            shutil.rmtree(processed)
        shutil.copytree(raw, processed, ignore=shutil.ignore_patterns("bin", "obj"))
        processed_project = processed / project.name
        run("processed-restore", ["dotnet", "restore", processed_project, "--nologo"])
        run("postprocess", ["dotnet", args.postprocessor.resolve(strict=True), processed_project, "--in-place"])
        run("processed-build", ["dotnet", "build", processed_project, "-c", "Release", "--nologo", "-m:1"])
        projects["processed"] = processed_project
    results = []
    report = dict(cases=45, baseline_cases=24, version_feature_cases=14, api_delta_cases=7,
                  engine=str(engine), engine_sha256=digest(engine), results=results)
    for form in forms:
        library = projects[form]
        runner = work / (form + "/runner")
        runner.mkdir(exist_ok=True)
        driver = runner / "DirectQualification.csproj"
        assembly = ET.parse(library).getroot().findtext(".//AssemblyName") or library.stem
        driver.write_text(f'''<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings><Nullable>enable</Nullable><AllowUnsafeBlocks>true</AllowUnsafeBlocks>
    <IsAotCompatible>true</IsAotCompatible><InvariantGlobalization>true</InvariantGlobalization>
    <AssemblyName>GhosttyVt.DirectQualification</AssemblyName></PropertyGroup>
  <ItemGroup><ProjectReference Include="{library}" />
    <TrimmerRootAssembly Include="{assembly}" Condition="'$(PublishAot)' == 'true'" /></ItemGroup>
</Project>\n''')
        calls = "\n".join(f"if (Core.{entry}() is var result{i} && result{i} != 0) return result{i};"
                          for i, (_, _, entry) in enumerate(controls))
        (runner / "Program.cs").write_text("using Core = Managed.GhosttyVt;\n" + calls + "\nreturn 0;\n")
        for mode in modes:
            label = form + "-" + mode
            output = work / "runs" / label
            if mode == "jit":
                run(label + "-build", ["dotnet", "build", driver, "-c", "Release", "--nologo", "-m:1", "-o", output])
                command = ["dotnet", output / "GhosttyVt.DirectQualification.dll"]
            else:
                run(label + "-publish", ["dotnet", "publish", driver, "-c", "Release", "--nologo", "-m:1",
                                         "-r", args.rid, "-p:PublishAot=true", "-o", output])
                command = [output / "GhosttyVt.DirectQualification"]
            observed = run(label + "-run", command, timeout=300, transcript=True)
            (evidence / (label + ".stdout")).write_text(observed)
            if observed != expected:
                import difflib
                (evidence / (label + "-mismatch.diff")).write_text("".join(difflib.unified_diff(
                    expected.splitlines(keepends=True), observed.splitlines(keepends=True),
                    fromfile="native", tofile=label)))
                raise RuntimeError(label + ": actual transcript differs from native")
            results.append(dict(form=form, mode=mode, passed=45, failed=0, exact_native_match=True,
                                whole_library_rooted=mode == "aot", project=str(library)))
            (evidence / "qualification.json").write_text(json.dumps(report, indent=2) + "\n")
    print(json.dumps(report, indent=2), flush=True)


if __name__ == "__main__":
    main()
