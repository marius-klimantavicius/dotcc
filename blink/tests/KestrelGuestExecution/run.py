#!/usr/bin/env python3
"""One frozen optimized-JIT Kestrel startup/HTTP diagnostic; failures remain failures."""
import argparse
import glob
import hashlib
import json
import os
from pathlib import Path
import shutil
import signal
import subprocess
import sys
import tempfile
import time
import xml.etree.ElementTree as ET

HERE = Path(__file__).resolve().parent
BLINK = HERE.parents[1]
REPO = BLINK.parent
GUEST_SHA = "7bc07c1e8d01dd3d326fdbb436473ff0b2b8dcaf2910aea6fffebdaa7b119865"
PROFILE_SHA = "e1e3c2ecf4c929f6f13d0f4937757cdc0dc82ee2b55d2c76d1fd88c4ec7db01a"
ENVIRONMENT = {"LANG": "C", "DOTNET_GCHeapHardLimit": "1000000",
               "DOTNET_GCRegionRange": "2000000", "DOTNET_GCRegionSize": "100000",
               "DOTNET_HOSTBUILDER__RELOADCONFIGONCHANGE": "false", "DOTNET_EnableDiagnostics": "0"}
CASES = ["health", "large", "fragmented", "missing", "stop"]


def sha(path):
    return hashlib.sha256(Path(path).read_bytes()).hexdigest()


def manifest(folder, outputs=False):
    return {str(p.relative_to(folder)): sha(p) for p in sorted(folder.rglob("*"))
            if p.is_file() and (outputs or not {"bin", "obj", "__pycache__"}.intersection(p.relative_to(folder).parts))}


def closure(project):
    result, visited = {}, set()
    def visit(path):
        path = path.resolve()
        if path in visited:
            return
        visited.add(path)
        if not path.is_relative_to(BLINK):
            raise RuntimeError("External project")
        result[path] = sha(path)
        tree = ET.parse(path).getroot()
        if tree.findall(".//Import"):
            raise RuntimeError("Unreviewed project import")
        if tree.findtext(".//EnableDefaultCompileItems") != "false":
            for name, digest in manifest(path.parent).items():
                if name.endswith(".cs"):
                    result[path.parent / name] = digest
        for node in tree.findall(".//Compile"):
            include = node.get("Include", "")
            if not include or "$" in include or ";" in include or node.get("Condition"):
                raise RuntimeError("Unreviewed compile item")
            selected = [Path(p).resolve() for p in glob.glob(str(path.parent / include), recursive=True)]
            if not selected:
                raise RuntimeError("Empty compile selection")
            for source in selected:
                if not source.is_relative_to(BLINK):
                    raise RuntimeError("External source")
                if source.is_file():
                    result[source] = sha(source)
        for node in tree.findall(".//ProjectReference"):
            include = node.get("Include", "")
            if not include or "$" in include or ";" in include or node.get("Condition"):
                raise RuntimeError("Unreviewed project reference")
            visit(path.parent / include)
    visit(project)
    return result


def cleanup(process):
    def exists():
        try:
            os.killpg(process.pid, 0)
            return True
        except ProcessLookupError:
            return False
    signals = []
    for sig, seconds in ((signal.SIGTERM, 3), (signal.SIGKILL, 5)):
        process.poll()
        if not exists():
            break
        try:
            os.killpg(process.pid, sig)
            signals.append(sig.name)
        except ProcessLookupError:
            break
        end = time.monotonic() + seconds
        while time.monotonic() < end:
            process.poll()
            if not exists():
                break
            time.sleep(.05)
    return dict(signals=signals, group_gone=not exists(), leader_exit=process.poll())


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--delivery-receipt", type=Path, required=True)
    parser.add_argument("--delivery-sha256", required=True,
                        help="Explicit expected identity of the reviewed public delivery receipt")
    parser.add_argument("--native-receipt", type=Path, required=True)
    parser.add_argument("--profile-receipt", type=Path, required=True)
    args = parser.parse_args()
    if len(args.delivery_sha256) != 64 or any(c not in "0123456789abcdef" for c in args.delivery_sha256):
        parser.error("--delivery-sha256 must contain exactly 64 lowercase hexadecimal characters")
    base = BLINK / "artifacts/kestrel-guest-execution"
    base.mkdir(parents=True, exist_ok=True)
    attempt = Path(tempfile.mkdtemp(prefix="attempt-", dir=base))
    (attempt / "tmp").mkdir()
    receipt = dict(passed=False, prepared=False, diagnostic_completed=False, guest_passed=False,
                   mode="optimized-jit", inputs={}, source_trees={}, optional_inputs={}, commands=[],
                   limits=dict(instructions=100_000_000, wall_seconds=60, join_seconds=5,
                               memory_bytes=128 * 1024 * 1024, maximum_workers=16,
                               trace_rows_per_thread=16384), attempt=str(attempt))
    env = dict(os.environ, LC_ALL="C", TMPDIR=str(attempt / "tmp"),
               MSBUILDDISABLENODEREUSE="1", DOTNET_CLI_USE_MSBUILD_SERVER="0")
    receipt["host_environment"] = {k: env.get(k) for k in (
        "LC_ALL", "TMPDIR", "MSBUILDDISABLENODEREUSE", "DOTNET_CLI_USE_MSBUILD_SERVER",
        "DOTNET_GCHeapHardLimit", "DOTNET_GCRegionRange", "DOTNET_GCRegionSize", "DOTNET_gcServer",
        "DOTNET_TieredCompilation", "DOTNET_TieredPGO", "COMPlus_GCHeapHardLimit", "COMPlus_gcServer")}
    print(attempt, flush=True)

    def save():
        (attempt / "receipt.tmp").write_text(json.dumps(receipt, indent=2) + "\n")
        (attempt / "receipt.tmp").replace(attempt / "receipt.json")

    def pin(path, expected=None):
        path = Path(path).resolve()
        value = sha(path)
        if expected is not None and value != expected:
            raise RuntimeError("Input identity differs: " + str(path))
        if receipt["inputs"].setdefault(str(path), value) != value:
            raise RuntimeError("Input changed during preparation: " + str(path))
        return path

    def tree(folder, expected=None):
        actual = manifest(folder)
        if expected is not None and actual != expected:
            raise RuntimeError("Source closure differs: " + str(folder))
        receipt["source_trees"][str(folder)] = actual
        for name, digest in actual.items():
            pin(folder / name, digest)
        return actual

    def check():
        for name, value in receipt["inputs"].items():
            if sha(name) != value:
                raise RuntimeError("Frozen input changed: " + name)
        for name, value in receipt["source_trees"].items():
            if manifest(Path(name)) != value:
                raise RuntimeError("Source closure changed: " + name)
        for name, value in receipt["optional_inputs"].items():
            if (sha(name) if Path(name).is_file() else None) != value:
                raise RuntimeError("Build configuration changed: " + name)

    def run(argv, label, timeout=60, required=True):
        check()
        row = dict(label=label, argv=list(map(str, argv)), timeout_seconds=timeout)
        receipt["commands"].append(row)
        out, err = attempt / (label + ".stdout"), attempt / (label + ".stderr")
        process = None
        save()
        try:
            with out.open("wb") as stdout, err.open("wb") as stderr:
                process = subprocess.Popen(row["argv"], cwd=attempt, env=env,
                    stdin=subprocess.DEVNULL, stdout=stdout, stderr=stderr, start_new_session=True)
                row["exit_code"] = process.wait(timeout=timeout)
        finally:
            if process is not None:
                row["cleanup"] = cleanup(process)
            row["files"] = {str(p): sha(p) for p in (out, err) if p.exists()}
            save()
        if row["cleanup"]["signals"] or not row["cleanup"]["group_gone"]:
            raise RuntimeError("Command required forced cleanup: " + label)
        if required and row["exit_code"] != 0:
            raise RuntimeError("Command failed: " + label)
        check()
        return out.read_bytes()

    try:
        pin(__file__)
        pin(sys.executable)
        receipt["optional_inputs"] = {str(REPO / n): sha(REPO / n) if (REPO / n).is_file() else None
            for n in ("Directory.Build.props", "Directory.Build.targets", "Directory.Packages.props", "nuget.config", "global.json")}
        delivery_path = pin(args.delivery_receipt, args.delivery_sha256)
        native_path = pin(args.native_receipt, GUEST_SHA)
        profile_path = pin(args.profile_receipt, PROFILE_SHA)
        delivery, native, profile = [json.loads(p.read_text()) for p in (delivery_path, native_path, profile_path)]
        if not delivery.get("passed") or not delivery.get("authored_sources_unchanged") or delivery["selected_profile"] != "threaded":
            raise RuntimeError("Passing public threaded delivery required")
        if not native.get("passed") or not native.get("static_elf_verified") or native["elf"]["interpreter"] or native["elf"]["needed"]:
            raise RuntimeError("Passing static Kestrel native producer required")
        if not profile.get("passed") or not profile.get("final_identities_stable") or profile["native_environment"] != ENVIRONMENT:
            raise RuntimeError("Passing exact reviewed native profile required")
        if profile["binary"] != native["binary"] or profile["native_exit_code"] != 0:
            raise RuntimeError("Native profile binary/status differs")
        for name, digest in native["sources"].items():
            pin(REPO / name, digest)
        for name, digest in native["frozen_inputs"].items():
            pin(native_path.parent / name, digest)
        for name, digest in native["artifacts"].items():
            pin(native_path.parent / name, digest)
        for name, digest in native["package_manifests"].items():
            pin(native_path.parent / name, digest)
        pin(native_path.parent / "source/obj/project.assets.json", native["assets_sha256"])
        for row in native["tools"].values():
            pin(row["path"], row["sha256"])
        for name, digest in profile["inputs"].items():
            pin(name, digest)
        for name, digest in profile["artifacts"].items():
            pin(profile_path.parent / name, digest)
        receipt["native"] = dict(receipt=str(native_path), sha256=GUEST_SHA, profile=str(profile_path),
                                  profile_sha256=PROFILE_SHA, binary=native["binary"], environment=ENVIRONMENT)
        final, raw = Path(delivery["stable_output"]), Path(delivery["raw_snapshot"])
        if final.resolve() != (BLINK / "generated/TranslatedBlink").resolve():
            raise RuntimeError("Wrong public output")
        tree(final, delivery["final_files"])
        tree(raw, delivery["raw_files"])
        for name, digest in delivery["authored_sources"].items():
            pin(BLINK / name, digest)
        assembly_path = pin(delivery["assembly"]["path"], delivery["assembly"]["sha256"])
        assembly = json.loads(assembly_path.read_text())
        profile_inputs = pin(Path(delivery["profile"]) / "inputs.json", delivery["profile_inputs_sha256"])
        inputs = json.loads(profile_inputs.read_text())
        if (not assembly["linked"] or assembly["failures"] or len(assembly["objects"]) != 108 or
                assembly["identity"]["profile_inputs_sha256"] != sha(profile_inputs) or
                inputs["compiler"] != delivery["compiler"]):
            raise RuntimeError("Translation producer identity differs")
        for row in assembly["objects"].values():
            pin(row["object_path"], row["object_sha256"])
        for name, digest in inputs["staged_headers"].items():
            pin(Path(delivery["profile"]) / name, digest)
        for name, digest in delivery["compiler"].items():
            pin(REPO / "DotCC/bin/Release/net10.0" / name, digest)
        for row in delivery["results"].values():
            if row["exit_code"] != 0:
                raise RuntimeError("Failed public producer command")
            pin(row["log"], row["log_sha256"])
        receipt["delivery"] = dict(receipt=str(delivery_path), sha256=args.delivery_sha256, assembly=delivery["assembly"])
        product = closure(final / "TranslatedBlink.csproj")
        for path, digest in product.items():
            if not path.is_relative_to(final) and delivery["authored_sources"].get(str(path.relative_to(BLINK))) != digest:
                raise RuntimeError("Unpinned authored product source")
        private = attempt / "private"
        selected = closure(HERE / "KestrelGuestExecution.csproj")
        for path, digest in selected.items():
            pin(path, digest)
            target = private / path.relative_to(BLINK)
            target.parent.mkdir(parents=True, exist_ok=True)
            shutil.copyfile(path, target)
            pin(target, digest)
        for name, digest in tree(HERE).items():
            target = private / "tests/KestrelGuestExecution" / name
            target.parent.mkdir(parents=True, exist_ok=True)
            shutil.copyfile(HERE / name, target)
            pin(target, digest)
        tree(BLINK / "src/Managed.Emulation.ThreadedExecution")
        for name, digest in receipt["optional_inputs"].items():
            if digest is not None:
                shutil.copyfile(pin(name, digest), attempt / Path(name).name)
                pin(attempt / Path(name).name, digest)
        image, oracle = attempt / "image", attempt / "oracle"
        image.mkdir()
        oracle.mkdir()
        binary = pin(native["binary"]["path"], native["binary"]["sha256"])
        shutil.copyfile(binary, image / "0.elf")
        configuration = dict(mode="static", allow_interpreter=False, path="/bin/kestrel-service",
            argv=["kestrel-service", "8080"], environment=[key + "=" + value for key, value in ENVIRONMENT.items()])
        (image / "configuration.json").write_text(json.dumps(configuration, indent=2) + "\n")
        (image / "manifest.json").write_text(json.dumps([dict(guest_path="/bin/kestrel-service", file="0.elf",
            sha256=sha(binary), size=binary.stat().st_size)], indent=2) + "\n")
        if [row["name"] for row in profile["native_cases"]] != CASES:
            raise RuntimeError("Native case inventory differs")
        for row in profile["native_cases"]:
            if not row["passed"]:
                raise RuntimeError("Native HTTP failure")
            for suffix in ("request", "response"):
                source = pin(profile_path.parent / (row["name"] + "." + suffix), row[suffix + "_sha256"])
                shutil.copyfile(source, oracle / source.name)
        tree(image)
        tree(oracle)
        tree(private)
        dotnet = pin(shutil.which("dotnet"))
        run([dotnet, "--info"], "dotnet-info")
        version = run([dotnet, "--version"], "dotnet-version").decode().strip()
        for name in ("dotnet.dll", "MSBuild.dll", "Roslyn/bincore/csc.dll"):
            pin(dotnet.parent / "sdk" / version / name)
        runtimes = run([dotnet, "--list-runtimes"], "dotnet-runtimes").decode()
        for line in runtimes.splitlines():
            parts = line.split()
            if len(parts) == 3 and parts[0] == "Microsoft.NETCore.App":
                for name in ("libcoreclr.so", "libhostpolicy.so", "System.Private.CoreLib.dll"):
                    pin(Path(parts[2].strip("[]")) / parts[1] / name)
        receipt["prepared"] = True
        project = private / "tests/KestrelGuestExecution/KestrelGuestExecution.csproj"
        run([dotnet, "build", project, "-c", "Release", "--disable-build-servers", "-p:UseSharedCompilation=false"], "build", 600)
        original = project.parent / "bin/Release/net10.0"
        execution = attempt / "execution"
        before = manifest(original, outputs=True)
        shutil.copytree(original, execution)
        if manifest(execution, outputs=True) != before or manifest(original, outputs=True) != before:
            raise RuntimeError("Execution binary copy differs")
        receipt["binaries_before"] = before
        result_folder = attempt / "result"
        run([dotnet, execution / "KestrelGuestExecution.dll", image, oracle, result_folder], "optimized-jit", 75, required=False)
        receipt["binaries_after"] = manifest(execution, outputs=True)
        if receipt["binaries_after"] != before:
            raise RuntimeError("Execution binary closure changed")
        result_path = result_folder / "result.json"
        if result_path.exists():
            result = json.loads(result_path.read_text())
            receipt["result"], receipt["result_sha256"] = result, sha(result_path)
            receipt["diagnostic_completed"] = True
            observed = result.get("execution") or {}
            receipt["guest_passed"] = bool(receipt["commands"][-1]["exit_code"] == 0 and
                all(result.get(k) is True for k in ("guest_passed", "ready", "joined", "is_quiescent", "io_disposed")) and
                all(result.get(k) is None for k in ("diagnostic_error", "execution_error", "notification_error")) and
                observed.get("exited") is True and observed.get("exit_status") == 0 and observed.get("stop_reason") == "None" and
                observed.get("memory_released") is True and observed.get("all_workers_joined") is True and
                0 < observed.get("instructions", 0) <= 100_000_000 and
                len(observed.get("threads", [])) > 0 and all(t["machine_released"] and t["signal"] == 0 and t["halt"] == 0 for t in observed["threads"]) and
                [r["name"] for r in result["cases"]] == CASES and all(r["passed"] for r in result["cases"]))
        check()
        receipt["passed"] = receipt["guest_passed"]
    except BaseException as error:
        receipt["error"] = repr(error)
        receipt["passed"] = receipt["guest_passed"] = False
        raise
    finally:
        receipt["artifacts"] = {str(p.relative_to(attempt)): sha(p) for p in attempt.rglob("*")
            if p.is_file() and p.name not in ("receipt.json", "receipt.tmp") and
            p.suffix in (".json", ".stdout", ".stderr", ".txt", ".response")}
        save()
        print(json.dumps({"passed": receipt["passed"], "diagnostic_completed": receipt["diagnostic_completed"],
                          "receipt": str(attempt / "receipt.json")}), flush=True)
    return 0 if receipt["passed"] else 1


if __name__ == "__main__":
    signal.signal(signal.SIGTERM, lambda n, f: (_ for _ in ()).throw(InterruptedError(str(n))))
    raise SystemExit(main())
