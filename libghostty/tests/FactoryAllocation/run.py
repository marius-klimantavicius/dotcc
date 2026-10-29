#!/usr/bin/env python3
"""Private, source-identical JIT/NativeAOT allocation diagnostic. No product builds."""
from __future__ import annotations

import argparse
from datetime import datetime, timezone
import hashlib
import json
import os
from pathlib import Path
import platform
import re
import signal
import subprocess
import sys
import time
import xml.etree.ElementTree as ET


HERE = Path(__file__).resolve().parent
ROOT = HERE.parents[2]
LIB = ROOT / "libghostty"
OWNER = "Managed.GhosttyVt+"
IDENTIFIER = r"[A-Za-z_][A-Za-z_0-9]*"
PAYLOAD = r"(?:B[0-9A-F]{64}|P(?:ushort|uint|ulong)_[0-9A-F]{64})"
BACKINGS = {"Byte": "byte", "UInt16": "ushort", "UInt32": "uint", "UInt64": "ulong"}
GROUP_SIZE = 64


def sha(path: Path) -> str:
    return hashlib.sha256(path.read_bytes()).hexdigest().upper()


def write_json(path: Path, value) -> None:
    path.write_text(json.dumps(value, indent=2) + "\n")


def require(condition, message: str) -> None:
    if not condition:
        raise ValueError(message)


def snapshot(project: Path) -> dict:
    directory = project.parent
    paths = sorted(directory.glob("*.cs"))
    require(paths, f"No top-level C# sources: {directory}")
    manifest = directory / "Dotcc.SourceFiles.txt"
    require(manifest.is_file(), f"Missing canonical source manifest: {manifest}")
    listed = set(manifest.read_text().splitlines())
    require(listed == {path.name for path in paths}, "Canonical source manifest differs from top-level *.cs")
    files = [{"path": str(path), "name": path.name, "bytes": path.stat().st_size, "sha256": sha(path)} for path in paths]
    fingerprint = hashlib.sha256("".join(f"{f['name']}\0{f['sha256']}\n" for f in files).encode()).hexdigest().upper()
    return {"project": str(project), "project_sha256": sha(project),
            "manifest": str(manifest), "manifest_sha256": sha(manifest),
            "source_fingerprint": fingerprint, "files": files,
            "source_files": len(files), "source_bytes": sum(f["bytes"] for f in files)}


def prepare(form: str, project: Path, audit_path: Path, work: Path) -> dict:
    directory = work / form
    directory.mkdir()
    source = snapshot(project)
    audit = json.loads(audit_path.read_text())
    require(Path(audit["source_directory"]).resolve() == project.parent, "Audit source-directory mismatch")
    require(audit["source_files"] == source["source_files"] and audit["source_bytes"] == source["source_bytes"],
            "Canonical source size/count differs from accepted audit")
    assembly = Path(audit["assembly"])
    require(sha(assembly) == audit["assembly_sha256"], "Actual accepted PE no longer matches its audit")
    require(audit["bounded_parameters"] and audit["max_parameters"] <= 2, "Unbounded accepted factory")
    text = "\n".join(Path(f["path"]).read_text() for f in source["files"])
    require("__FactoryAllocation" not in text and "__RunFactoryAllocation" not in text,
            "Canonical product already contains diagnostic entrypoints")
    getters = audit["getters"]
    factories = audit["factory_measurements"]
    require(len(getters) == audit["payload_getters"] > 0, "Getter inventory count mismatch")
    require(len(factories) == audit["zero_argument_factory_probes"] > 0, "Factory inventory count mismatch")
    source_payloads = re.findall(r"ReadOnlySpan<(byte|ushort|uint|ulong)>\s+(" + PAYLOAD + r")\s*=>", text)
    expected_payloads = []
    cases = []
    wrappers = []
    calls = []
    for getter in getters:
        require(getter["Getter"].startswith(OWNER + "GhosttyVtLiterals.get_"), "Unexpected payload owner")
        name = getter["Getter"].removeprefix(OWNER + "GhosttyVtLiterals.get_")
        require(re.fullmatch(PAYLOAD, name), "Unexpected payload name")
        backing = BACKINGS[getter["Backing"]]
        expected_payloads.append((backing, name))
        require(getter["NewArrayInstructions"] == 0 and getter["MatchingRvaFields"] > 0 and
                getter["AllocatedBytesAfterWarmup"] == 0, "Input PE getter audit did not pass")
        index = len(cases)
        wrappers.extend(["[MethodImpl(MethodImplOptions.NoInlining)]",
                         f"private static ReadOnlySpan<{backing}> G{index:04d}() => GhosttyVtLiterals.{name};"])
        calls.append(f"ProbeGetter<{backing}>({index}, &G{index:04d});")
        cases.append({"index": index, "kind": "getter", "method": getter["Getter"],
                      "backing": backing, "bytes": getter["Bytes"], "sha256": getter["Sha256"], "iterations": 10000})
    require(len(set(source_payloads)) == len(source_payloads) and sorted(source_payloads) == sorted(expected_payloads),
            "Actual source getter inventory differs from PE audit")
    source_factories = re.findall(r"class\s+(__DotccAggregate\w+)\s*\{\s*public static\s+(" + IDENTIFIER + r")\s+Create\(\)", text)
    expected_factories = []
    for factory in factories:
        require(factory["Method"].startswith(OWNER) and factory["Method"].endswith(".Create"), "Unexpected factory owner")
        owner = factory["Method"].removeprefix(OWNER).removesuffix(".Create")
        result_type = factory["ReturnType"].removeprefix(OWNER)
        require(re.fullmatch(r"__DotccAggregate\w+", owner) and re.fullmatch(IDENTIFIER, result_type), "Unexpected factory type")
        require(factory["Status"] == "pass" and factory["AllocatedBytesAfterWarmup"] == 0 and
                0 < factory["ValueBytes"] <= 1024 * 1024, "Input PE factory audit did not pass")
        expected_factories.append((owner, result_type))
        index = len(cases)
        calls.append(f"ProbeFactory<{result_type}>({index}, &{owner}.Create);")
        cases.append({"index": index, "kind": "factory", "method": factory["Method"], "return_type": factory["ReturnType"],
                      "bytes": factory["ValueBytes"], "checksum": factory["Checksum"], "iterations": factory["Iterations"]})
    require(len(set(source_factories)) == len(source_factories) and sorted(source_factories) == sorted(expected_factories),
            "Actual source Create() inventory/return types differ from PE audit")
    excluded = [f for f in audit["factories"] if f["method"] == "Create" and f["parameters"] != 0]
    for body in audit["factory_helper_il"]:
        require(not any(body[k] for k in ("NewArrayInstructions", "ReferenceConstructions", "AllocationCalls", "UnresolvedCalls", "IndirectCalls")),
                f"Accepted factory IL policy failed: {body['Method']}")

    groups = [calls[start:start + GROUP_SIZE] for start in range(0, len(calls), GROUP_SIZE)]
    require(len(groups) <= GROUP_SIZE, "Use another grouping level before exceeding 64 calls per method")
    generated = ["// Generated diagnostic call sites; canonical product files are referenced, never copied or edited.",
                 "using System;", "using System.Runtime.CompilerServices;", "namespace Managed;",
                 "public static partial class GhosttyVt { private static unsafe partial class __FactoryAllocation {",
                 f'private const string Form = "{form}";',
                 f'private const string SourceFingerprint = "{source["source_fingerprint"]}";',
                 f"private const int GetterCount = {len(getters)}, FactoryCount = {len(factories)};", *wrappers,
                 "private static void ProbeAll() {",
                 *[f"Group{i:03d}();" for i in range(len(groups))], "}"]
    for i, group in enumerate(groups):
        generated.extend([f"private static void Group{i:03d}() {{", *group, "}"])
    generated.append("} }")
    generated_path = directory / "Calls.cs"
    generated_path.write_text("\n".join(generated) + "\n")
    write_json(directory / "cases.json", cases)

    canonical = ET.parse(project).getroot()
    require(not list(canonical.iter("ProjectReference")) and not list(canonical.iter("PackageReference")),
            "Canonical dependencies require an explicit diagnostic design review")
    require(not list(canonical.iter("Compile")), "Canonical explicit Compile items require design review")
    framework = canonical.findtext(".//TargetFramework")
    require(framework == "net10.0", "Review framework/language settings before changing target")
    diagnostic_project = directory / "FactoryAllocation.csproj"
    tree = ET.Element("Project")
    early = ET.SubElement(tree, "PropertyGroup")
    for key in ("ImportDirectoryBuildProps", "ImportDirectoryBuildTargets"):
        ET.SubElement(early, key).text = "false"
    ET.SubElement(tree, "Import", Project="Sdk.props", Sdk="Microsoft.NET.Sdk")
    properties = ET.SubElement(tree, "PropertyGroup")
    for key, value in {
        "OutputType": "Exe", "TargetFramework": framework, "LangVersion": "14",
        "RootNamespace": "Managed", "AssemblyName": f"FactoryAllocation.{form}",
        "StartupObject": "FactoryAllocationDiagnostic.Program", "AllowUnsafeBlocks": "true",
        "Nullable": "disable", "ImplicitUsings": "enable", "EnableDefaultCompileItems": "false",
        "Optimize": "true", "Deterministic": "true", "DebugType": "portable",
        "IlcSingleThreaded": "true", "BuildInParallel": "false", "UseSharedCompilation": "false",
    }.items():
        ET.SubElement(properties, key).text = value
    compile_items = ET.SubElement(tree, "ItemGroup")
    for path in [*[f["path"] for f in source["files"]], str(HERE / "Driver.cs"), str(generated_path)]:
        ET.SubElement(compile_items, "Compile", Include=path)
    roots = ET.SubElement(tree, "ItemGroup")
    ET.SubElement(roots, "TrimmerRootAssembly", Include="$(AssemblyName)")
    ET.SubElement(tree, "Import", Project="Sdk.targets", Sdk="Microsoft.NET.Sdk")
    ET.indent(tree)
    ET.ElementTree(tree).write(diagnostic_project, encoding="unicode")
    return {"form": form, "canonical": source, "accepted_audit": str(audit_path), "accepted_audit_sha256": sha(audit_path),
            "accepted_assembly": str(assembly), "accepted_assembly_sha256": sha(assembly),
            "project": str(diagnostic_project), "project_sha256": sha(diagnostic_project),
            "calls_sha256": sha(generated_path), "cases_sha256": sha(directory / "cases.json"),
            "getters": len(getters), "factories": len(factories), "excluded_argument_factories": excluded,
            "group_count": len(groups), "maximum_calls_per_group": max(map(len, groups)),
            "maximum_probe_parameters": 2, "maximum_wrapper_parameters": 0,
            "nativeaot_root": f"entire diagnostic assembly FactoryAllocation.{form}, including every canonical translated source",
            "runs": []}


def capture(argv: list[str], log: Path, cwd: Path, env: dict, timeout: int) -> dict:
    print(f"Running {log.name}: {' '.join(argv[:3])}", flush=True)
    started = time.monotonic()
    outcome = {"argv": argv, "cwd": str(cwd), "log": str(log), "started_utc": datetime.now(timezone.utc).isoformat()}
    with log.open("wb") as stream:
        process = subprocess.Popen(argv, cwd=cwd, env=env, stdout=stream, stderr=subprocess.STDOUT, start_new_session=True)
        try:
            outcome["exit_code"] = process.wait(timeout=timeout)
        except subprocess.TimeoutExpired:
            os.killpg(process.pid, signal.SIGKILL)
            process.wait()
            outcome.update(exit_code=process.returncode, timed_out=True)
    outcome.update(elapsed_seconds=round(time.monotonic() - started, 3), log_sha256=sha(log))
    print(f"  exit={outcome['exit_code']} seconds={outcome['elapsed_seconds']}", flush=True)
    return outcome


def validate_result(result_path: Path, form: dict, mode: str) -> dict:
    result = json.loads(result_path.read_text())
    cases = json.loads((result_path.parent.parent / "cases.json").read_text())
    require(result["form"] == form["form"] and result["source_fingerprint"] == form["canonical"]["source_fingerprint"], "Driver identity mismatch")
    require(result["runtime"] == mode and result["dynamic_code_supported"] == (mode == "jit"), "Wrong execution runtime")
    require(result["allocation_counter_positive_control_bytes"] > 0, "Positive allocation control failed")
    require(result["error"] is None and result["failures"] == 0 and result["allocated_bytes"] == 0, "Driver reported failure/allocation")
    require(result["getters"] == form["getters"] and result["factories"] == form["factories"], "Runtime case counts differ")
    require(result["completed"] == result["global_warmup_completed"] == len(cases) == len(result["results"]), "Incomplete full-set passes")
    for case, actual in zip(cases, result["results"], strict=True):
        require(actual["index"] == case["index"] and actual["completed"] and actual["bytes_match"] and
                actual["allocated_bytes"] == 0 and actual["bytes"] == case["bytes"] and actual["iterations"] == case["iterations"],
                f"Strict measurement mismatch at {case['method']}")
        key = "sha256" if case["kind"] == "getter" else "checksum"
        require(actual[key] == case[key], f"Returned bytes differ from accepted PE at {case['method']}")
    return {"status": "pass", "getters": form["getters"], "factories": form["factories"],
            "allocated_bytes": result["allocated_bytes"], "positive_control_bytes": result["allocation_counter_positive_control_bytes"],
            "result": str(result_path), "result_sha256": sha(result_path)}


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--work", type=Path, required=True, help="New, empty private work/output directory")
    parser.add_argument("--forms", nargs="+", choices=("raw", "processed"), default=["raw", "processed"])
    parser.add_argument("--modes", nargs="+", choices=("jit", "nativeaot"), default=["jit", "nativeaot"])
    parser.add_argument("--raw-project", type=Path, default=LIB / "generated/TranslatedGhosttyVt.Raw/TranslatedGhosttyVt.csproj")
    parser.add_argument("--processed-project", type=Path, default=LIB / "generated/TranslatedGhosttyVt/TranslatedGhosttyVt.csproj")
    parser.add_argument("--raw-audit", type=Path, default=LIB / "artifacts/aggregate-factories-raw-accepted.json")
    parser.add_argument("--processed-audit", type=Path, default=LIB / "artifacts/aggregate-factories-processed-accepted.json")
    parser.add_argument("--rid", default={"x86_64": "linux-x64", "aarch64": "linux-arm64"}.get(platform.machine(), "linux-x64"))
    parser.add_argument("--timeout", type=int, default=1200, help="Per build/run timeout, seconds")
    parser.add_argument("--prepare-only", action="store_true")
    args = parser.parse_args()
    work = args.work.resolve()
    require(work.is_relative_to(LIB / "build"), "Keep private work beneath libghostty/build")
    require(not work.exists(), "Use a new work directory to preserve every prior failure/log")
    work.mkdir(parents=True)
    (work / "tmp").mkdir()
    env = dict(os.environ, TMPDIR=str(work / "tmp"), DOTNET_PROCESSOR_COUNT="1", MSBUILDDISABLENODEREUSE="1")
    receipt_path = work / "receipt.json"
    receipt = {"created_utc": datetime.now(timezone.utc).isoformat(), "work": str(work), "status": "preparing",
               "driver": str(HERE / "Driver.cs"), "driver_sha256": sha(HERE / "Driver.cs"), "generator_sha256": sha(Path(__file__)),
               "environment": {key: env.get(key) for key in ("DOTNET_PROCESSOR_COUNT", "DOTNET_TieredCompilation", "DOTNET_TieredPGO", "COMPlus_TieredCompilation", "TMPDIR")},
               "measurement": "Full-set warmup pass, then strict zero allocated bytes per getter/factory; each pass includes 256 local warmups and 10000 measured iterations (1000 for factories >=4096 bytes). All bytes consumed by no-inline checksum. BCL/hash/JSON setup, destinations, positive control excluded.",
               "limits": "Private same-source diagnostic executable, not a product binary. Managed per-thread allocation counters only; accepted PE audit covers IL/RVA and allocation services. Argument-taking Create is not executed; no fabricated Args. No terminal qualification or translation/upstream phase.",
               "forms": []}
    write_json(receipt_path, receipt)
    try:
        for form in dict.fromkeys(args.forms):
            receipt["forms"].append(prepare(form, getattr(args, form + "_project").resolve(), getattr(args, form + "_audit").resolve(), work))
            write_json(receipt_path, receipt)
        receipt["status"] = "prepared"
        write_json(receipt_path, receipt)
        if args.prepare_only:
            print(receipt_path)
            return 0
        receipt["sdk"] = capture(["dotnet", "--info"], work / "dotnet-info.log", work, env, args.timeout)
        for form in receipt["forms"]:
            directory = work / form["form"]
            canonical = Path(form["canonical"]["project"])
            for mode in dict.fromkeys(args.modes):
                run = {"mode": mode, "status": "running"}
                form["runs"].append(run)
                write_json(receipt_path, receipt)
                try:
                    require(snapshot(canonical) == form["canonical"], "Canonical inputs changed before build")
                    output = directory / "outputs" / mode
                    artifacts = directory / ("artifacts-" + mode)
                    command = ["dotnet", "build" if mode == "jit" else "publish", form["project"], "-c", "Release",
                               "--artifacts-path", str(artifacts), "--output", str(output), "-m:1", "-nodeReuse:false",
                               "-p:BuildInParallel=false", "-p:UseSharedCompilation=false"]
                    if mode == "nativeaot":
                        command += ["-r", args.rid, "-p:PublishAot=true", "-p:IlcSingleThreaded=true"]
                    run["build"] = capture(command, directory / (mode + "-build.log"), directory, env, args.timeout)
                    require(run["build"]["exit_code"] == 0, "Private build failed; see captured log")
                    require(snapshot(canonical) == form["canonical"], "Canonical inputs changed during build")
                    binary = output / ("FactoryAllocation." + form["form"] + (".dll" if mode == "jit" else ""))
                    run.update(binary=str(binary), binary_sha256=sha(binary))
                    result_path = directory / "outputs" / (mode + "-result.json")
                    argv = (["dotnet", str(binary)] if mode == "jit" else [str(binary)]) + [str(result_path), mode]
                    run["execution"] = capture(argv, directory / (mode + "-run.log"), directory, env, args.timeout)
                    require(run["execution"]["exit_code"] == 0, "Diagnostic execution failed; see captured log/result")
                    run.update(validate_result(result_path, form, mode))
                except Exception as exception:
                    run.update(status="fail", error=f"{type(exception).__name__}: {exception}")
                finally:
                    run["canonical_unchanged"] = snapshot(canonical) == form["canonical"]
                    if not run["canonical_unchanged"]:
                        run.update(status="fail", error="Canonical source/project/manifest changed")
                    write_json(receipt_path, receipt)
        receipt["status"] = "pass" if all(run["status"] == "pass" for form in receipt["forms"] for run in form["runs"]) else "fail"
    except Exception as exception:
        receipt.update(status="fail", error=f"{type(exception).__name__}: {exception}")
    finally:
        write_json(receipt_path, receipt)
    print(f"{receipt['status']}: {receipt_path}", flush=True)
    return 0 if receipt["status"] in ("pass", "prepared") else 1


if __name__ == "__main__":
    sys.exit(main())
