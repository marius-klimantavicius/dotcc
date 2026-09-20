#!/usr/bin/env python3
"""Prepare the pinned .NET guest; --run executes raw JIT, --all-modes executes exact delivery raw/optimized JIT/AOT."""
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
ALL_MODES = ("raw-jit", "raw-aot", "optimized-jit", "optimized-aot")
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
    if kind == "Exe":
        ET.SubElement(items, "TrimmerRootAssembly", Include="TranslatedBlink")
    path.parent.mkdir(parents=True, exist_ok=True)
    ET.ElementTree(tree).write(path, encoding="unicode")


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--translation-receipt", type=Path, required=True)
    parser.add_argument("--guest-receipt", type=Path, required=True)
    parser.add_argument("--native-gc-receipt", type=Path, required=True)
    selection = parser.add_mutually_exclusive_group()
    selection.add_argument("--run", action="store_true", help="execute raw JIT only")
    selection.add_argument("--all-modes", action="store_true", help="execute raw/optimized JIT/NativeAOT")
    args = parser.parse_args()
    selected_modes = ALL_MODES if args.all_modes else (("raw-jit",) if args.run else ())
    base = ROOT / "artifacts/dotnet-threaded-guest-execution"
    base.mkdir(parents=True, exist_ok=True)
    attempt = Path(tempfile.mkdtemp(prefix="attempt-", dir=base))
    receipt = dict(passed=False, prepared=False, diagnostic_completed=False, guest_passed=False,
                   scope="Exact threaded delivery with separate C# owner; explicit raw-only or four-mode guest qualification",
                   requested_modes=list(selected_modes), modes={}, all_modes_passed=False,
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
        # Resolve the delivered project's direct authored references explicitly;
        # the fixture maps exactly these files into its private source projects.
        delivered_project = ET.parse(final / "TranslatedBlink.csproj").getroot()
        if (delivered_project.findall(".//Import") or
                delivered_project.findtext(".//EnableDefaultCompileItems") != "false"):
            raise RuntimeError("Unreviewed final project source selection")
        selected_bridges = []
        generated_selection = 0
        for node in delivered_project.findall(".//Compile"):
            include = node.get("Include", "")
            if node.get("Condition") or "$" in include or ";" in include:
                raise RuntimeError("Unreviewed final Compile item")
            if include == "Sources/**/*.cs":
                generated_selection += 1
            elif "*" not in include:
                selected_bridges.append((final / include).resolve())
            else:
                raise RuntimeError("Unreviewed final Compile glob")
        expected_bridges = [(ROOT / name).resolve() for name in delivery["product_source_links"]]
        if (generated_selection != 1 or len(selected_bridges) != len(set(selected_bridges)) or
                set(selected_bridges) != set(expected_bridges)):
            raise RuntimeError("Final authored Compile references differ from manifest")
        references = delivered_project.findall(".//ProjectReference")
        if (len(references) != 1 or references[0].get("Condition") or
                (final / references[0].get("Include", "")).resolve() !=
                (ROOT / "src/Managed.Emulation.Host/Managed.Emulation.Host.csproj").resolve()):
            raise RuntimeError("Final Host project reference differs")
        receipt["resolved_product_sources"] = {str(path): sha(path) for path in selected_bridges}
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
        receipt["source_modes"] = {"raw": dict(source=str(raw), manifest=delivery["raw_files"])}
        if args.all_modes:
            optimized = attempt / "optimized"
            # Exact final delivered bytes. Never postprocess again for this matrix.
            for relative, digest in delivery["final_files"].items():
                if relative.startswith("Sources/") and Path(relative).suffix == ".cs":
                    source = verify(final / relative, digest)
                    target = optimized / relative
                    target.parent.mkdir(parents=True, exist_ok=True)
                    shutil.copyfile(source, target)
                    verify(target, digest)
            names = set()
            for relative in delivery["product_source_links"]:
                name = Path(relative).name
                if name in names:
                    raise RuntimeError("Duplicate authored bridge basename")
                names.add(name)
                source = verify(ROOT / relative, delivery["authored_sources"][relative])
                target = optimized / "Bridges" / name
                target.parent.mkdir(parents=True, exist_ok=True)
                shutil.copyfile(source, target)
                verify(target, delivery["authored_sources"][relative])
            for relative, digest in delivery["host_source_files"].items():
                source = verify(ROOT / "src/Managed.Emulation.Host" / relative, digest)
                target = optimized / "Host" / relative
                target.parent.mkdir(parents=True, exist_ok=True)
                shutil.copyfile(source, target)
                verify(target, digest)
            check_tree(optimized / "Host", delivery["host_source_files"])
            receipt["source_modes"]["optimized"] = dict(source=str(final), manifest=delivery["final_files"],
                bridges=delivery["product_source_links"], authored=delivery["authored_sources"])
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
        consumers = {}
        for source_mode in (("raw", "optimized") if args.all_modes else ("raw",)):
            private = attempt / "modes" / source_mode if args.all_modes else attempt
            private.mkdir(parents=True, exist_ok=True)
            if args.all_modes:
                for name in ("Program.cs", "ThreadedGuestExecution.cs"):
                    shutil.copyfile(attempt / name, private / name)
                    verify(private / name, sha(attempt / name))
            library = private / "library/TranslatedBlink.csproj"
            execution = private / "execution/Managed.Emulation.ThreadedExecution.csproj"
            consumer = private / "consumer/DotNetThreadedGuestExecution.csproj"
            project(library, "TranslatedBlink", "Library", [attempt / source_mode / "Sources/**/*.cs",
                    attempt / source_mode / "Bridges/*.cs"],
                    [attempt / source_mode / "Host/Managed.Emulation.Host.csproj"], "BLINK_FULL_CORE")
            project(execution, "Managed.Emulation.ThreadedExecution", "Library", [private / "ThreadedGuestExecution.cs"], [library])
            project(consumer, "DotNetThreadedGuestExecution", "Exe", [private / "Program.cs"], [execution])
            consumers[source_mode] = consumer
        receipt["prepared_files"] = {str(p.relative_to(attempt)): sha(p) for p in attempt.rglob("*") if p.is_file()}
        receipt["prepared"] = True
        receipt["binaries"] = {}
        if selected_modes:
            run([tools["dotnet"], "--info"], "dotnet-info")
            built = set()
            for mode in selected_modes:
                source_mode, runtime = mode.split("-")
                consumer = consumers[source_mode]
                row = dict(passed=False, result_directory=str(attempt / "results" / mode))
                receipt["modes"][mode] = row
                if source_mode not in built:
                    run([tools["dotnet"], "build", consumer, "-c", "Release", "--disable-build-servers",
                         "-p:UseSharedCompilation=false"], source_mode + "-build", 600)
                    built.add(source_mode)
                if runtime == "jit":
                    original_directory = consumer.parent / "bin/Release/net10.0"
                    executable_name = "DotNetThreadedGuestExecution.dll"
                else:
                    original_directory = consumer.parent / "publish"
                    run([tools["dotnet"], "publish", consumer, "-c", "Release", "-r", "linux-x64",
                         "-p:PublishAot=true", "--disable-build-servers", "-p:UseSharedCompilation=false",
                         "-o", original_directory], mode + "-publish", 1200)
                    executable_name = "DotNetThreadedGuestExecution"
                def executable_tree(directory):
                    return {str(p.relative_to(directory)): sha(p) for p in directory.rglob("*") if p.is_file()}
                before = executable_tree(original_directory)
                copied_directory = attempt / "executions" / mode
                shutil.copytree(original_directory, copied_directory)
                if executable_tree(copied_directory) != before or executable_tree(original_directory) != before:
                    raise RuntimeError("Executable changed during copy: " + mode)
                row.update(original_directory=str(original_directory), execution_directory=str(copied_directory),
                           binaries_before=before)
                receipt["binaries"].update({str(copied_directory / name): digest for name, digest in before.items()})
                executable = copied_directory / executable_name
                command = [tools["dotnet"], executable] if runtime == "jit" else [executable]
                result_directory = attempt / "results" / mode if args.all_modes else attempt / "result"
                row["result_directory"] = str(result_directory)
                run([*command, image, oracle, result_directory], mode, 60, required=False)
                row["exit_code"] = receipt["commands"][-1]["exit_code"]
                row["binaries_after"] = executable_tree(copied_directory)
                if row["binaries_after"] != before:
                    raise RuntimeError("Execution binary closure changed: " + mode)
                result_path = result_directory / "result.json"
                if result_path.exists():
                    result = json.loads(result_path.read_text())
                    row["result"] = result
                    row["result_sha256"] = sha(result_path)
                    if not args.all_modes:
                        receipt["result"] = result
                    observed = result.get("execution") or {}
                    row["passed"] = bool(row["exit_code"] == 0 and
                        all(result.get(key) is True for key in ("guest_passed", "ready", "joined", "is_quiescent")) and
                        all(result.get(key) is None for key in ("diagnostic_error", "execution_error", "notification_error")) and
                        observed.get("exited") is True and observed.get("exit_status") == 0 and
                        observed.get("stop_reason") == "None" and observed.get("all_workers_joined") is True and
                        observed.get("memory_released") is True and
                        0 < observed.get("instructions", 0) <= 20_000_000 and
                        len(observed.get("threads", [])) > 0 and
                        all(t.get("machine_released") is True and t.get("signal") == 0 and t.get("halt") == 0
                            for t in observed.get("threads", [])) and
                        {c.get("name") for c in result.get("cases", [])} == {"health", "stop"} and
                        len(result.get("cases", [])) == 2 and all(c.get("passed") is True for c in result["cases"]))
                    if row["passed"]:
                        for name in ("health", "stop"):
                            if (result_directory / (name + ".response")).read_bytes() != (oracle / (name + ".response")).read_bytes():
                                raise RuntimeError(mode + " HTTP bytes differ from native")
                        if ((result_directory / "stdout.txt").read_bytes() != b"READY 8080\nSTOPPED\n" or
                                (result_directory / "stderr.txt").read_bytes()):
                            raise RuntimeError(mode + " guest captures differ")
                row["artifacts"] = {str(p.relative_to(result_directory)): sha(p)
                                    for p in result_directory.rglob("*") if p.is_file()}
                print(json.dumps({"mode": mode, "guest_passed": row["passed"]}), flush=True)
                if not row["passed"]:
                    break
            receipt["diagnostic_completed"] = (set(receipt["modes"]) == set(selected_modes) and
                all("result" in row for row in receipt["modes"].values()))
            receipt["guest_passed"] = (set(receipt["modes"]) == set(selected_modes) and
                all(row["passed"] for row in receipt["modes"].values()))
            receipt["all_modes_passed"] = bool(args.all_modes and receipt["guest_passed"] and
                                               set(receipt["modes"]) == set(ALL_MODES))
            for mode, row in receipt["modes"].items():
                if executable_tree(Path(row["execution_directory"])) != row["binaries_before"]:
                    raise RuntimeError("Final executable closure changed: " + mode)
                for name, digest in row["artifacts"].items():
                    if sha(Path(row["result_directory"]) / name) != digest:
                        raise RuntimeError("Result artifact changed: " + mode)
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
        receipt["passed"] = receipt["guest_passed"] = receipt["all_modes_passed"] = False
        receipt["error"] = type(error).__name__ + ": " + str(error)
        raise
    finally:
        receipt["artifacts"] = {str(p.relative_to(attempt)): sha(p) for p in attempt.rglob("*")
                                if p.is_file() and p.suffix in (".log", ".txt", ".response", ".json")}
        (attempt / "receipt.json").write_text(json.dumps(receipt, indent=2) + "\n")
        print(json.dumps({"prepared": receipt["prepared"], "guest_passed": receipt["guest_passed"],
                          "receipt": str(attempt / "receipt.json")}), flush=True)
    return 1 if selected_modes and not receipt["guest_passed"] else 0


def interrupted(signum, frame):
    raise InterruptedError("Diagnostic runner received signal " + str(signum))


if __name__ == "__main__":
    signal.signal(signal.SIGTERM, interrupted)
    raise SystemExit(main())
