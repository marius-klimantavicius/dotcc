#!/usr/bin/env python3
"""Prepare the pinned static .NET guest against threaded delivery; --run enables raw JIT only."""
import argparse
import hashlib
import json
import os
from pathlib import Path
import re
import shutil
import signal
import subprocess
import tempfile
import xml.etree.ElementTree as ET

ROOT = Path(__file__).resolve().parents[2]
REPO = ROOT.parent
sha = lambda path: hashlib.sha256(Path(path).read_bytes()).hexdigest()


def project(path, name, kind, sources, references, constants=None):
    tree = ET.Element("Project", Sdk="Microsoft.NET.Sdk")
    props = ET.SubElement(tree, "PropertyGroup")
    values = dict(TargetFramework="net10.0", AssemblyName=name, OutputType=kind,
                  AllowUnsafeBlocks="true", ImplicitUsings="enable", Nullable="enable",
                  EnableDefaultCompileItems="false")
    if constants:
        values["DefineConstants"] = constants
    for key, value in values.items():
        ET.SubElement(props, key).text = value
    items = ET.SubElement(tree, "ItemGroup")
    for source in sources:
        ET.SubElement(items, "Compile", Include=str(source))
    for reference in references:
        ET.SubElement(items, "ProjectReference", Include=str(reference))
    path.parent.mkdir(parents=True, exist_ok=True)
    ET.ElementTree(tree).write(path, encoding="unicode")


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--translation-receipt", type=Path, required=True)
    parser.add_argument("--guest-receipt", type=Path, required=True)
    parser.add_argument("--native-gc-receipt", type=Path, required=True)
    parser.add_argument("--run", action="store_true")
    args = parser.parse_args()
    base = ROOT / "artifacts/dotnet-threaded-guest-execution"
    base.mkdir(parents=True, exist_ok=True)
    attempt = Path(tempfile.mkdtemp(prefix="attempt-", dir=base))
    receipt = dict(passed=False, prepared=False, diagnostic_completed=False, guest_passed=False,
                   scope="Threaded delivery diagnostic with separately frozen C# owner; raw JIT only on explicit --run",
                   attempt=str(attempt), commands=[], frozen={}, runner_sha256=sha(__file__))
    print(attempt, flush=True)
    receipt["environment_overrides"] = dict(TMPDIR=str(attempt / "tmp"), LC_ALL="C", MSBUILDDISABLENODEREUSE="1")
    env = dict(os.environ, **receipt["environment_overrides"])
    (attempt / "tmp").mkdir()
    receipt["host_runtime_environment"] = {name: os.environ.get(name) for name in (
        "DOTNET_GCHeapHardLimit", "DOTNET_GCRegionRange", "DOTNET_GCRegionSize",
        "DOTNET_gcServer", "DOTNET_TieredCompilation", "DOTNET_TieredPGO",
        "COMPlus_GCHeapHardLimit", "COMPlus_gcServer", "COMPlus_TieredCompilation", "COMPlus_TieredPGO")}

    def verify(path, expected=None):
        path = Path(path).resolve()
        digest = sha(path)
        if str(path) in receipt["frozen"] and receipt["frozen"][str(path)] != digest:
            raise RuntimeError("Input changed while preparing: " + str(path))
        if expected is not None and digest != expected:
            raise RuntimeError("Input identity differs: " + str(path))
        receipt["frozen"][str(path)] = digest
        return path

    def stop_group(process):
        for signum, timeout in ((signal.SIGTERM, 2), (signal.SIGKILL, 5)):
            try:
                os.killpg(process.pid, signum)
            except ProcessLookupError:
                pass
            try:
                process.wait(timeout=timeout)
            except subprocess.TimeoutExpired:
                if signum == signal.SIGKILL:
                    raise RuntimeError("Process group did not finish")

    def run(command, label, timeout=60, required=True):
        log = attempt / (label + ".log")
        row = dict(argv=list(map(str, command)), label=label)
        receipt["commands"].append(row)
        with log.open("wb") as output:
            process = subprocess.Popen(row["argv"], cwd=attempt, env=env, stdout=output,
                                       stderr=subprocess.STDOUT, start_new_session=True)
            try:
                row["exit_code"] = process.wait(timeout=timeout)
            except BaseException:
                stop_group(process)
                raise
        row["log_sha256"] = sha(log)
        if required and row["exit_code"] != 0:
            raise RuntimeError(label + " failed: " + str(log))
        return log.read_text()

    def check_tree(directory, expected):
        actual = {str(p.relative_to(directory)): sha(p) for p in directory.rglob("*")
                  if p.is_file() and not {"bin", "obj"}.intersection(p.relative_to(directory).parts)}
        if actual != expected:
            raise RuntimeError("Source closure differs: " + str(directory))

    try:
        shutil.copyfile(verify(__file__), attempt / "runner.py")
        tools = {name: str(verify(shutil.which(name))) for name in ("dotnet", "readelf")}
        receipt["tools"] = tools
        delivery_path = verify(args.translation_receipt)
        delivery = json.loads(delivery_path.read_text())
        guest_path = verify(args.guest_receipt)
        guest = json.loads(guest_path.read_text())
        gc_path = verify(args.native_gc_receipt, "f145da71d5032421fdd40bd368d248cad4918065b0b07ee545e32772fab7bb55")
        gc = json.loads(gc_path.read_text())
        if sha(guest_path) != "8876eaf0cda1c0cc9ec81e163a9293745caa436215dec90790ff6f57acce8505":
            raise RuntimeError("Only the reviewed pinned musl producer is selected")
        if not gc.get("passed") or gc["producer"]["sha256"] != sha(guest_path) or gc["binary"] != guest["binary"]:
            raise RuntimeError("Native GC witness does not identify this guest")
        for name, digest in gc["inputs"].items(): verify(name, digest)
        for name, digest in gc["artifacts"].items(): verify(gc_path.parent / name, digest)
        if not delivery.get("passed") or not guest.get("passed"):
            raise RuntimeError("Passing delivery and native guest receipts are required")
        assembly_path = verify(delivery["assembly"]["path"], delivery["assembly"]["sha256"])
        assembly = json.loads(assembly_path.read_text())
        inputs_path = verify(Path(delivery["profile"]) / "inputs.json", delivery["profile_inputs_sha256"])
        inputs = json.loads(inputs_path.read_text())
        if (not assembly.get("linked") or assembly.get("failures") or assembly.get("diagnostic_replay") or
                assembly["identity"]["profile_inputs_sha256"] != sha(inputs_path) or
                inputs["compiler"] != delivery["compiler"] or assembly["identity"]["compiler_sha256"] != inputs["compiler"] or
                len(assembly["selected"]) != 108 or len(set(assembly["selected"])) != 108 or
                set(assembly["selected"]) != set(assembly["objects"])):
            raise RuntimeError("Threaded delivery chain differs")
        profile = Path(delivery["profile"])
        for name, digest in inputs["staged_headers"].items(): verify(profile / name, digest)
        config = (profile / "config.h").read_text()
        if "#define BLINK_MANAGED_GUEST_THREADS 1" not in config or "#define DISABLE_THREADS 1" in config:
            raise RuntimeError("Reviewed threaded profile required")
        for produced in assembly["objects"].values(): verify(produced["object_path"], produced["object_sha256"])
        for name, digest in delivery["inputs"].items(): verify(name, digest)
        for name, digest in delivery["authored_sources"].items(): verify(ROOT / name, digest)
        for name, digest in delivery["host_source_files"].items(): verify(ROOT / "src/Managed.Emulation.Host" / name, digest)
        final = Path(delivery["output"])
        check_tree(final, delivery["final_files"])
        check_tree(ROOT / "src/Managed.Emulation.Host", delivery["host_source_files"])
        for name, digest in delivery["final_files"].items(): verify(final / name, digest)
        receipt["final_delivery_files"] = delivery["final_files"]
        receipt["native_gc_receipt"] = str(gc_path)
        receipt.update(delivery_receipt=str(delivery_path), native_guest_receipt=str(guest_path),
                       producer_compiler=json.loads(inputs_path.read_text())["compiler"],
                       baseline_limits=dict(memory_bytes=64 * 1024 * 1024, instructions=20_000_000, deadline_seconds=30))
        raw = Path(delivery["raw_snapshot"])
        check_tree(raw, delivery["raw_files"])
        for relative, digest in delivery["raw_files"].items():
            source = verify(raw / relative, digest)
            if relative.startswith("Host/") or (relative.startswith(("Sources/", "Bridges/")) and source.suffix == ".cs"):
                target = attempt / "raw" / relative
                target.parent.mkdir(parents=True, exist_ok=True)
                shutil.copyfile(source, target)
        for name in delivery["product_source_links"]:
            copied = attempt / "raw/Bridges" / Path(name).name
            if sha(copied) != delivery["authored_sources"][name]:
                raise RuntimeError("Archived bridge differs from original source: " + name)
        check_tree(attempt / "raw/Host", delivery["host_source_files"])
        verify(ROOT / "src/Managed.Emulation.ThreadedExecution/Managed.Emulation.ThreadedExecution.csproj")
        sdk_selection = REPO / "global.json"
        receipt["global_json"] = dict(path=str(sdk_selection), present=sdk_selection.is_file())
        if sdk_selection.is_file(): shutil.copyfile(verify(sdk_selection), attempt / "global.json")
        for name in ("Directory.Build.props", "Directory.Packages.props", "nuget.config"):
            shutil.copyfile(verify(REPO / name), attempt / name)
        shutil.copyfile(verify(ROOT / "src/Managed.Emulation.ThreadedExecution/ThreadedGuestExecution.cs"), attempt / "ThreadedGuestExecution.cs")
        shutil.copyfile(verify(Path(__file__).with_name("Program.cs")), attempt / "Program.cs")
        image = attempt / "image"
        image.mkdir()
        binary = verify(guest["binary"]["path"], guest["binary"]["sha256"])
        verify(guest_path.parent / "native.strace", guest["trace_sha256"])
        program_headers = run([tools["readelf"], "-lW", binary], "guest-elf-program-headers")
        guest_dynamic = run([tools["readelf"], "-dW", binary], "guest-elf-dynamic")
        interpreter = re.findall(r"Requesting program interpreter: ([^\]]+)", program_headers)
        needed = re.findall(r"\(NEEDED\).*\[([^\]]+)\]", guest_dynamic)
        if interpreter != guest["elf"]["interpreter"] or needed != guest["elf"]["needed"]:
            raise RuntimeError("Actual ELF differs from native receipt")
        expected_environment = dict(LANG="C", DOTNET_GCHeapHardLimit="1000000",
                                    DOTNET_GCRegionRange="2000000", DOTNET_GCRegionSize="100000")
        if gc["environment"] != expected_environment:
            raise RuntimeError("Unreviewed native GC environment")
        if guest.get("static_elf_verified") is not True or interpreter or needed or re.search(r"^\s*INTERP\s", program_headers, re.M):
            raise RuntimeError("Static image without interpreter/dependencies required")
        image_configuration = dict(mode="static", allow_interpreter=False,
                                   path="/bin/dotnet-service", argv=["dotnet-service", "8080"],
                                   environment=[key + "=" + value for key, value in expected_environment.items()])
        (image / "configuration.json").write_text(json.dumps(image_configuration, indent=2) + "\n")
        receipt["image_configuration"] = image_configuration
        shutil.copyfile(binary, image / "0.elf")
        rows = [dict(guest_path="/bin/dotnet-service", file="0.elf", source=str(binary),
                     sha256=sha(binary), size=binary.stat().st_size, needed=[])]
        (image / "manifest.json").write_text(json.dumps(rows, indent=2) + "\n")
        receipt["private_image"] = rows
        oracle = attempt / "oracle"
        oracle.mkdir()
        for row in gc["native_cases"]:
            name = row["path"].lstrip("/")
            if name not in ("health", "stop") or not row["passed"]:
                raise RuntimeError("Unexpected native case")
            for kind in ("request", "response"):
                source = verify(gc_path.parent / (name + "." + kind), row[kind + "_sha256"])
                shutil.copyfile(source, oracle / source.name)
        if sorted(p.name for p in oracle.iterdir()) != ["health.request", "health.response", "stop.request", "stop.response"]:
            raise RuntimeError("Incomplete native HTTP oracle")
        library = attempt / "library/TranslatedBlink.csproj"
        execution = attempt / "execution/Managed.Emulation.ThreadedExecution.csproj"
        consumer = attempt / "consumer/DotNetThreadedGuestExecution.csproj"
        project(library, "TranslatedBlink", "Library", [attempt / "raw/Sources/*.cs", attempt / "raw/Bridges/*.cs"],
                [attempt / "raw/Host/Managed.Emulation.Host.csproj"], "BLINK_FULL_CORE")
        project(execution, "Managed.Emulation.ThreadedExecution", "Library", [attempt / "ThreadedGuestExecution.cs"], [library])
        project(consumer, "DotNetThreadedGuestExecution", "Exe", [attempt / "Program.cs"], [execution])
        receipt["prepared_files"] = {str(p.relative_to(attempt)): sha(p) for p in attempt.rglob("*") if p.is_file()}
        receipt["prepared"] = True
        if args.run:
            run([tools["dotnet"], "--info"], "dotnet-info")
            run([tools["dotnet"], "build", consumer, "-c", "Release", "--disable-build-servers",
                 "-p:UseSharedCompilation=false"], "raw-build", 600)
            executable = consumer.parent / "bin/Release/net10.0/DotNetThreadedGuestExecution.dll"
            receipt["binaries"] = {str(p): sha(p) for p in executable.parent.iterdir() if p.is_file()}
            result_directory = attempt / "result"
            run([tools["dotnet"], executable, image, oracle, result_directory], "raw-jit", 60, required=False)
            result_path = result_directory / "result.json"
            if result_path.exists():
                result = json.loads(result_path.read_text())
                receipt["result"] = result
                receipt["diagnostic_completed"] = True
                receipt["guest_passed"] = bool(result["guest_passed"] and receipt["commands"][-1]["exit_code"] == 0)
            for name, digest in receipt["binaries"].items():
                if sha(name) != digest:
                    raise RuntimeError("Diagnostic binary changed")
        check_tree(raw, delivery["raw_files"])
        check_tree(final, delivery["final_files"])
        check_tree(ROOT / "src/Managed.Emulation.Host", delivery["host_source_files"])
        if sdk_selection.is_file() != receipt["global_json"]["present"]:
            raise RuntimeError("global.json presence changed")
        for name, digest in receipt["frozen"].items():
            if sha(name) != digest:
                raise RuntimeError("Input changed: " + name)
        for name, digest in receipt["prepared_files"].items():
            if sha(attempt / name) != digest:
                raise RuntimeError("Prepared input changed: " + name)
        receipt["passed"] = receipt["guest_passed"]
    except BaseException as error:
        receipt["passed"] = receipt["guest_passed"] = False
        receipt["error"] = type(error).__name__ + ": " + str(error)
        raise
    finally:
        receipt["artifacts"] = {str(p.relative_to(attempt)): sha(p) for p in attempt.rglob("*")
                                if p.is_file() and p.suffix in (".log", ".txt", ".response", ".json")}
        (attempt / "receipt.json").write_text(json.dumps(receipt, indent=2) + "\n")
        print(json.dumps({"prepared": receipt["prepared"], "guest_passed": receipt["guest_passed"],
                          "receipt": str(attempt / "receipt.json")}), flush=True)
    return 1 if args.run and not receipt["guest_passed"] else 0


def interrupted(signum, frame):
    raise InterruptedError("Diagnostic runner received signal " + str(signum))


if __name__ == "__main__":
    signal.signal(signal.SIGTERM, interrupted)
    raise SystemExit(main())
