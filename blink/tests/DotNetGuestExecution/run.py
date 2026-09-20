#!/usr/bin/env python3
"""Prepare a private valid .NET ELF diagnostic; --run explicitly enables raw JIT."""
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
    parser.add_argument("--image-mode", choices=("static", "dynamic"), required=True,
                        help="Explicit reviewed guest image profile; never inferred from filenames")
    parser.add_argument("--run", action="store_true")
    args = parser.parse_args()
    base = ROOT / "artifacts/dotnet-guest-execution"
    base.mkdir(parents=True, exist_ok=True)
    attempt = Path(tempfile.mkdtemp(prefix="attempt-", dir=base))
    receipt = dict(passed=False, prepared=False, diagnostic_completed=False, guest_passed=False,
                   scope="Verified raw delivery with separately recorded current C# owner; producer compiler identity comes from that delivery",
                   attempt=str(attempt), commands=[], frozen={}, runner_sha256=sha(__file__))
    print(attempt, flush=True)
    env = dict(os.environ, TMPDIR=str(attempt / "tmp"), LC_ALL="C")
    (attempt / "tmp").mkdir()

    def verify(path, expected=None):
        path = Path(path).resolve()
        digest = sha(path)
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

    try:
        delivery_path = verify(args.translation_receipt)
        delivery = json.loads(delivery_path.read_text())
        guest_path = verify(args.guest_receipt)
        guest = json.loads(guest_path.read_text())
        if not delivery.get("passed") or not guest.get("passed"):
            raise RuntimeError("Passing delivery and native guest receipts are required")
        assembly_path = verify(delivery["assembly"]["path"], delivery["assembly"]["sha256"])
        assembly = json.loads(assembly_path.read_text())
        inputs_path = verify(Path(delivery["profile"]) / "inputs.json", delivery["profile_inputs_sha256"])
        if not assembly.get("linked") or assembly.get("diagnostic_replay") or assembly["identity"]["profile_inputs_sha256"] != sha(inputs_path):
            raise RuntimeError("Canonical delivery chain differs")
        receipt.update(delivery_receipt=str(delivery_path), native_guest_receipt=str(guest_path),
                       producer_compiler=json.loads(inputs_path.read_text())["compiler"],
                       baseline_limits=dict(memory_bytes=64 * 1024 * 1024, instructions=20_000_000, deadline_seconds=30))
        raw = Path(delivery["raw_snapshot"])
        for relative, digest in delivery["raw_files"].items():
            source = verify(raw / relative, digest)
            if relative.startswith(("Sources/", "Bridges/", "Host/")) and source.suffix in (".cs", ".csproj"):
                target = attempt / "raw" / relative
                target.parent.mkdir(parents=True, exist_ok=True)
                shutil.copyfile(source, target)
        for name in ("Directory.Build.props", "Directory.Packages.props", "nuget.config"):
            shutil.copyfile(verify(REPO / name), attempt / name)
        shutil.copyfile(verify(ROOT / "src/Managed.Emulation.Execution/GuestExecution.cs"), attempt / "GuestExecution.cs")
        shutil.copyfile(verify(Path(__file__).with_name("Program.cs")), attempt / "Program.cs")
        image = attempt / "image"
        image.mkdir()
        binary = verify(guest["binary"]["path"], guest["binary"]["sha256"])
        guest_trace = verify(guest_path.parent / "native.strace", guest["trace_sha256"])
        native_trace = guest_trace.read_text()
        program_headers = run(["readelf", "-lW", binary], "guest-elf-program-headers")
        guest_dynamic = run(["readelf", "-dW", binary], "guest-elf-dynamic")
        interpreter = re.findall(r"Requesting program interpreter: ([^\]]+)", program_headers)
        needed = re.findall(r"\(NEEDED\).*\[([^\]]+)\]", guest_dynamic)
        if interpreter != guest["elf"]["interpreter"] or needed != guest["elf"]["needed"]:
            raise RuntimeError("Actual ELF differs from native receipt")
        dependencies = {}
        environment = ["LANG=C"]
        if args.image_mode == "static":
            if guest.get("static_elf_verified") is not True or interpreter or needed or re.search(r"^\s*INTERP\s", program_headers, re.M):
                raise RuntimeError("Static mode requires verified ELF without INTERP or NEEDED")
        else:
            if interpreter != ["/lib64/ld-linux-x86-64.so.2"]:
                raise RuntimeError("Review required for a different ELF interpreter")
            dependencies = {"ld-linux-x86-64.so.2": Path(interpreter[0]),
                            "libc.so.6": Path("/lib/x86_64-linux-gnu/libc.so.6"),
                            "libm.so.6": Path("/lib/x86_64-linux-gnu/libm.so.6")}
            if set(needed) != set(dependencies):
                raise RuntimeError("Review required for a different native dependency closure")
            environment.append("LD_LIBRARY_PATH=/lib/x86_64-linux-gnu:/lib64")
        image_configuration = dict(mode=args.image_mode, allow_interpreter=args.image_mode == "dynamic",
                                   path="/bin/dotnet-service", argv=["dotnet-service", "8080"], environment=environment)
        (image / "configuration.json").write_text(json.dumps(image_configuration, indent=2) + "\n")
        receipt["image_configuration"] = image_configuration
        mounts = [("/bin/dotnet-service", binary)] + [(str(path), path) for path in dependencies.values()]
        rows = []
        for index, (guest_name, source) in enumerate(mounts):
            source = verify(source)
            dynamic = run(["readelf", "-dW", source], "elf-dynamic-" + str(index))
            needed = re.findall(r"\(NEEDED\).*\[([^\]]+)\]", dynamic)
            if set(needed) - set(dependencies):
                raise RuntimeError("Unreviewed transitive ELF dependency")
            if index > 1 and guest_name not in native_trace:
                raise RuntimeError("Native trace does not identify selected library")
            filename = str(index) + ".elf"
            shutil.copyfile(source, image / filename)
            rows.append(dict(guest_path=guest_name, file=filename, source=str(source),
                             sha256=sha(source), size=source.stat().st_size, needed=needed))
        (image / "manifest.json").write_text(json.dumps(rows, indent=2) + "\n")
        receipt["private_image"] = rows
        oracle = attempt / "oracle"
        oracle.mkdir()
        for row in guest["native_cases"]:
            name = row["path"].lstrip("/")
            if name not in ("health", "stop") or not row["passed"]:
                raise RuntimeError("Unexpected native case")
            for kind in ("request", "response"):
                source = verify(guest_path.parent / (name + "." + kind), row[kind + "_sha256"])
                shutil.copyfile(source, oracle / source.name)
        if sorted(p.name for p in oracle.iterdir()) != ["health.request", "health.response", "stop.request", "stop.response"]:
            raise RuntimeError("Incomplete native HTTP oracle")
        library = attempt / "library/TranslatedBlink.csproj"
        execution = attempt / "execution/Managed.Emulation.Execution.csproj"
        consumer = attempt / "consumer/DotNetGuestExecution.csproj"
        project(library, "TranslatedBlink", "Library", [attempt / "raw/Sources/*.cs", attempt / "raw/Bridges/*.cs"],
                [attempt / "raw/Host/Managed.Emulation.Host.csproj"], "BLINK_FULL_CORE")
        project(execution, "Managed.Emulation.Execution", "Library", [attempt / "GuestExecution.cs"], [library])
        project(consumer, "DotNetGuestExecution", "Exe", [attempt / "Program.cs"], [execution])
        receipt["prepared_files"] = {str(p.relative_to(attempt)): sha(p) for p in attempt.rglob("*") if p.is_file()}
        receipt["prepared"] = True
        if args.run:
            run(["dotnet", "--info"], "dotnet-info")
            run(["dotnet", "build", consumer, "-c", "Release"], "raw-build", 600)
            executable = consumer.parent / "bin/Release/net10.0/DotNetGuestExecution.dll"
            receipt["binaries"] = {str(p): sha(p) for p in executable.parent.iterdir() if p.is_file()}
            result_directory = attempt / "result"
            run(["dotnet", executable, image, oracle, result_directory], "raw-jit", 60, required=False)
            result_path = result_directory / "result.json"
            if result_path.exists():
                result = json.loads(result_path.read_text())
                receipt["result"] = result
                receipt["diagnostic_completed"] = True
                receipt["guest_passed"] = bool(result["guest_passed"] and receipt["commands"][-1]["exit_code"] == 0)
            for name, digest in receipt["binaries"].items():
                if sha(name) != digest:
                    raise RuntimeError("Diagnostic binary changed")
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
                                if p.is_file() and p.suffix in (".log", ".txt", ".response")}
        (attempt / "receipt.json").write_text(json.dumps(receipt, indent=2) + "\n")
        print(json.dumps({"prepared": receipt["prepared"], "guest_passed": receipt["guest_passed"],
                          "receipt": str(attempt / "receipt.json")}), flush=True)
    return 1 if args.run and not receipt["guest_passed"] else 0


def interrupted(signum, frame):
    raise InterruptedError("Diagnostic runner received signal " + str(signum))


if __name__ == "__main__":
    signal.signal(signal.SIGTERM, interrupted)
    raise SystemExit(main())
