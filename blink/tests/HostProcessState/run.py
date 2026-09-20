#!/usr/bin/env python3
"""Qualify normal shared registries with native threads and four managed modes."""
import hashlib
import json
import os
from pathlib import Path
import shutil
import signal
import subprocess
import tempfile
import xml.etree.ElementTree as ET

ROOT = Path(__file__).resolve().parents[2]
REPO = ROOT.parent
CLI = REPO / "DotCC/bin/Release/net10.0/dotcc.dll"
POST = REPO / "DotCC.PostProcess/bin/Release/net10.0/dotcc-postprocess.dll"
EXPECTED = b"shared disposition; callbacks BCA once; clean end\n"


def sha(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def main():
    base = ROOT / "artifacts/host-process-state"
    base.mkdir(parents=True, exist_ok=True)
    attempt = Path(tempfile.mkdtemp(prefix="attempt-", dir=base))
    source = attempt / "inputs"
    source.mkdir()
    temporary = attempt / "tmp"
    temporary.mkdir()
    receipt = dict(scope="normal private shared signal/atexit registries, no guest execution",
                   passed=False, results={}, source_inputs={}, tool_inputs={})

    def save():
        (attempt / "receipt.json").write_text(json.dumps(receipt, indent=2) + "\n")

    def copy(path, target):
        receipt["source_inputs"][str(path)] = sha(path)
        target.parent.mkdir(parents=True, exist_ok=True)
        shutil.copyfile(path, target)
        if sha(path) != sha(target):
            raise RuntimeError("Source changed while copying: " + str(path))

    def terminate(process):
        try:
            os.killpg(process.pid, signal.SIGTERM)
        except ProcessLookupError:
            pass
        try:
            process.wait(timeout=5)
        except subprocess.TimeoutExpired:
            try:
                os.killpg(process.pid, signal.SIGKILL)
            except ProcessLookupError:
                pass
            process.wait(timeout=5)

    def run(command, name, timeout=180):
        command = list(map(str, command))
        log = attempt / (name + ".log")
        entry = dict(command=command, exit=None)
        receipt["results"][name] = entry
        save()
        with log.open("wb") as stream:
            process = subprocess.Popen(command, cwd=REPO, stdout=stream, stderr=subprocess.STDOUT,
                env=dict(os.environ, LC_ALL="C", TMPDIR=str(temporary)), start_new_session=True)
            try:
                entry["exit"] = process.wait(timeout=timeout)
            except BaseException:
                terminate(process)
                entry["exit"] = process.returncode
                raise
            finally:
                entry["log_sha256"] = sha(log)
                save()
        if entry["exit"]:
            raise RuntimeError(name + " failed: " + str(log))
        return log.read_bytes()

    def check_identity():
        for group in ("source_inputs", "tool_inputs"):
            for path, expected in receipt[group].items():
                if sha(Path(path)) != expected:
                    raise RuntimeError("Frozen input changed: " + path)
        for path, expected in receipt["snapshot"].items():
            if sha(source / path) != expected:
                raise RuntimeError("Private source snapshot changed: " + path)

    def interrupted(_signal, _frame):
        raise KeyboardInterrupt("Runner terminated")

    signal.signal(signal.SIGTERM, interrupted)
    try:
        for name in ("run.py", "probe.c", "native-main.c", "Program.cs"):
            copy(ROOT / "tests/HostProcessState" / name, source / name)
        for name in ("HostSignalActions.c", "HostExitCallbacks.c"):
            copy(ROOT / "src/Host" / name, source / name)
        for name in ("HostSignalActions.h", "HostExitCallbacks.h", "host-guest-threads.h"):
            copy(ROOT / "src/Host/include" / name, source / "host" / name)
        for directory, target in ((ROOT / "config/managed-host", "profile"),
                                  (ROOT / "config/managed-threaded", "threaded"),
                                  (REPO / "DotCC.Lib/include", "generic")):
            for path in directory.rglob("*"):
                if path.is_file():
                    copy(path, source / target / path.relative_to(directory))
        copy(ROOT / "tests/HostProcessState/config.h", source / "threaded/config.h")
        for tool in (CLI, POST):
            for path in tool.parent.iterdir():
                if path.is_file() and (path.suffix == ".dll" or path.name.endswith((".deps.json", ".runtimeconfig.json"))):
                    receipt["tool_inputs"][str(path)] = sha(path)
        receipt["snapshot"] = {str(path.relative_to(source)): sha(path)
                               for path in source.rglob("*") if path.is_file()}
        save()
        run(["dotnet", "--info"], "dotnet-info")
        run(["cc", "--version"], "native-tool")
        native = attempt / "native"
        run(["cc", "-std=c17", "-D_POSIX_C_SOURCE=200809L", "-DBLINK_MANAGED_GUEST_THREADS",
             "-Wall", "-Wextra", "-Werror", "-pthread", "-I", source / "host",
             source / "probe.c", source / "native-main.c", source / "HostSignalActions.c",
             source / "HostExitCallbacks.c", "-o", native], "native-build")
        if run([native], "native", 30) != EXPECTED:
            raise RuntimeError("Native shared state differs from the exact normal contract")
        receipt["native_sha256"] = sha(native)
        objects = []
        for name in ("probe", "HostSignalActions", "HostExitCallbacks"):
            output = attempt / (name + ".obj.cs")
            run(["dotnet", CLI, "-std=c17", "-DBLINK_MANAGED_GUEST_THREADS", "-DHAVE_THREADS",
                 "-DBLINK_MANAGED_PROCESS_STATE", "-DNOLINEAR", "-DDISABLE_JIT",
                 "-I", source / "generic", "-I", source / "profile", "-I", source / "host",
                 "-I", source / "threaded", source / (name + ".c"),
                 "--emit=obj", "-o", output], "emit-" + name)
            objects.append(output)
        raw = attempt / "raw"
        run(["dotnet", CLI, *objects, "--runtime=c", "--emit=managedlib", "--nest-types",
             "--class-name", "Blink", "--namespace", "Managed.Emulation", "-o", raw], "link")
        receipt["objects"] = {path.name: sha(path) for path in objects}
        receipt["raw"] = {path.name: sha(path) for path in raw.glob("*.cs")}
        optimized = attempt / "optimized"
        shutil.copytree(raw, optimized)
        for label, generated in (("raw", raw), ("optimized", optimized)):
            consumer = attempt / (label + "-consumer")
            consumer.mkdir()
            shutil.copyfile(source / "Program.cs", consumer / "Program.cs")
            xml = ET.Element("Project", Sdk="Microsoft.NET.Sdk")
            properties = ET.SubElement(xml, "PropertyGroup")
            for key, value in (("TargetFramework", "net10.0"), ("OutputType", "Exe"),
                               ("AllowUnsafeBlocks", "true"), ("Nullable", "enable"),
                               ("AssemblyName", "HostProcessStateProbe"), ("WarningsAsErrors", "CS8500")):
                ET.SubElement(properties, key).text = value
            items = ET.SubElement(xml, "ItemGroup")
            ET.SubElement(items, "Compile", Include=str(generated / "*.cs"))
            ET.SubElement(items, "TrimmerRootAssembly", Include="HostProcessStateProbe")
            project = consumer / "HostProcessStateProbe.csproj"
            ET.ElementTree(xml).write(project, encoding="unicode")
            if label == "optimized":
                run(["dotnet", "restore", project], "optimized-restore")
                run(["dotnet", POST, project, "--in-place"], "postprocess")
            run(["dotnet", "build", project, "-c", "Release"], label + "-build")
            binary = consumer / "bin/Release/net10.0/HostProcessStateProbe.dll"
            if run(["dotnet", binary], label + "-jit", 30) != EXPECTED:
                raise RuntimeError(label + " JIT differs from native")
            receipt[label + "_jit_sha256"] = sha(binary)
            publish = attempt / (label + "-aot")
            run(["dotnet", "publish", project, "-c", "Release", "-r", "linux-x64",
                 "-p:PublishAot=true", "-o", publish], label + "-publish", 300)
            executable = publish / "HostProcessStateProbe"
            if run([executable], label + "-aot", 30) != EXPECTED:
                raise RuntimeError(label + " AOT differs from native")
            receipt[label + "_aot_sha256"] = sha(executable)
        check_identity()
        if receipt["raw"] != {path.name: sha(path) for path in raw.glob("*.cs")}:
            raise RuntimeError("Raw generated source changed")
        receipt["optimized"] = {path.name: sha(path) for path in optimized.glob("*.cs")}
        receipt["passed"] = True
    except BaseException as error:
        receipt["passed"] = False
        receipt["failure"] = str(error)
        raise
    finally:
        save()
        print(attempt / "receipt.json")


if __name__ == "__main__":
    main()
