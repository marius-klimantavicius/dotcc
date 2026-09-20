#!/usr/bin/env python3
"""Native Linux and four managed modes for the normal private membarrier subset."""
import hashlib
import json
import os
from pathlib import Path
import shutil
import signal
import subprocess
import tempfile
import time
import xml.etree.ElementTree as ET

ROOT = Path(__file__).resolve().parents[3]
BLINK = ROOT / "blink"
HERE = Path(__file__).resolve().parent


def sha(path):
    return hashlib.sha256(Path(path).read_bytes()).hexdigest()


def files(path):
    return {str(p.relative_to(path)): sha(p) for p in path.rglob("*") if p.is_file()}


def stop(process):
    if process.poll() is not None:
        return
    for sig, seconds in ((signal.SIGTERM, 3), (signal.SIGKILL, 5)):
        try:
            os.killpg(process.pid, sig)
        except ProcessLookupError:
            return
        try:
            process.wait(timeout=seconds)
            return
        except subprocess.TimeoutExpired:
            pass


def main():
    base = BLINK / "artifacts/host-membarrier"
    base.mkdir(parents=True, exist_ok=True)
    attempt = Path(tempfile.mkdtemp(prefix="attempt-", dir=base))
    work = attempt / "work"
    work.mkdir()
    receipt = {"passed": False, "scope": "normal query/register/process barrier callback, not guest SYSCALL",
               "attempt": str(attempt), "commands": [], "modes": {}}
    print(attempt, flush=True)
    env = dict(os.environ, LC_ALL="C", TMPDIR=str(work / "tmp"))
    (work / "tmp").mkdir()

    def save():
        temporary = attempt / "receipt.tmp"
        temporary.write_text(json.dumps(receipt, indent=2) + "\n")
        temporary.replace(attempt / "receipt.json")

    def run(name, argv, timeout=180, binary=None):
        log = attempt / (name + ".log")
        row = {"name": name, "argv": list(map(str, argv)), "log": str(log), "cwd": str(work)}
        if binary is not None:
            row["binary"] = str(binary)
            row["binary_before"] = sha(binary)
            row["execution_files_before"] = files(binary.parent)
        receipt["commands"].append(row)
        started = time.monotonic()
        process = None
        try:
            with log.open("wb") as output:
                process = subprocess.Popen(row["argv"], cwd=work, env=env, stdout=output,
                                           stderr=subprocess.STDOUT, start_new_session=True)
                row["exit_code"] = process.wait(timeout=timeout)
        except BaseException as error:
            row["error"] = f"{type(error).__name__}: {error}"
            raise
        finally:
            if process is not None:
                stop(process)
                row["final_exit_code"] = process.poll()
            row["elapsed_seconds"] = time.monotonic() - started
            if log.exists():
                row["log_sha256"] = sha(log)
            if binary is not None:
                row["binary_after"] = sha(binary)
                row["execution_files_after"] = files(binary.parent)
            save()
        if row["exit_code"] != 0:
            raise RuntimeError(f"{name} failed: {log}")
        if binary is not None and row["binary_before"] != row["binary_after"]:
            raise RuntimeError(f"execution binary changed: {name}")
        if binary is not None and row["execution_files_before"] != row["execution_files_after"]:
            raise RuntimeError(f"execution dependency changed: {name}")
        return log.read_bytes()

    try:
        receipt["runner_sha256"] = sha(__file__)
        inputs = work / "inputs"
        inputs.mkdir()
        authored = [HERE / "Program.cs", HERE / "probe.c", Path(__file__),
                    BLINK / "src/Host/HostMembarrierBridge.cs", BLINK / "src/Host/include/host-membarrier.h"]
        build_config = [ROOT / name for name in ("Directory.Build.props", "Directory.Packages.props", "nuget.config")]
        authored += build_config
        host_source = BLINK / "src/Managed.Emulation.Host"
        authored += [p for p in host_source.rglob("*") if p.is_file() and not {"bin", "obj"}.intersection(p.relative_to(host_source).parts)]
        receipt["authored_sources"] = {str(p.relative_to(ROOT)): sha(p) for p in authored}
        for p in [HERE / "Program.cs", HERE / "probe.c", Path(__file__), BLINK / "src/Host/HostMembarrierBridge.cs",
                  BLINK / "src/Host/include/host-membarrier.h"]:
            shutil.copy2(p, inputs / p.name)
        for p in build_config:
            shutil.copy2(p, inputs / p.name)
            shutil.copy2(p, work / p.name)
        shutil.copytree(host_source, inputs / "host", ignore=shutil.ignore_patterns("bin", "obj"))
        headers = ROOT / "DotCC.Lib/include"
        receipt["header_sources"] = files(headers)
        shutil.copytree(headers, inputs / "headers")
        tools = work / "tools"
        tools.mkdir()
        producer_dirs = {"cli": ROOT / "DotCC/bin/Release/net10.0",
                         "post": ROOT / "DotCC.PostProcess/bin/Release/net10.0"}
        receipt["producer_sources"] = {name: files(path) for name, path in producer_dirs.items()}
        for name, path in producer_dirs.items():
            shutil.copytree(path, tools / name)
        receipt["frozen_inputs"] = files(inputs)
        receipt["frozen_tools"] = files(tools)
        cli = tools / "cli/dotcc.dll"
        post = tools / "post/dotcc-postprocess.dll"
        dotnet = shutil.which("dotnet")
        cc = shutil.which("cc")
        if not dotnet or not cc:
            raise RuntimeError("dotnet and cc required")
        receipt["host_tools"] = {name: {"path": p, "resolved": str(Path(p).resolve()), "sha256": sha(p)}
                                 for name, p in (("dotnet", dotnet), ("cc", cc))}
        run("dotnet-info", [dotnet, "--info"])
        run("cc-version", [cc, "--version"])
        native = work / "native"
        run("native-build", [cc, "-std=c17", "-Wall", "-Wextra", "-Werror", inputs / "probe.c", "-o", native])
        # Native execution has no managed dependency closure; put it in its own directory.
        native_dir = work / "native-bin"
        native_dir.mkdir()
        native.rename(native_dir / "probe")
        native = native_dir / "probe"
        expected = run("native", [native], 30, native)
        receipt["native_stdout_sha256"] = hashlib.sha256(expected).hexdigest()
        raw = work / "raw"
        run("translate", [dotnet, cli, "-std=c17", "-DBLINK_MANAGED_MEMBARRIER", "-I", inputs, "-I", inputs / "headers",
                          inputs / "probe.c", "--runtime=c", "--emit=managedlib", "--nest-types",
                          "--class-name", "Blink", "--namespace", "Managed.Emulation", "-o", raw])
        receipt["raw_generated"] = files(raw)
        optimized = work / "optimized"
        shutil.copytree(raw, optimized)
        # Host build outputs stay outside the immutable inputs tree.
        shutil.copytree(inputs / "host", work / "host")
        receipt["host_build_sources"] = files(work / "host")
        receipt["consumer_authored_sources"] = {}
        for label, generated in (("raw", raw), ("optimized", optimized)):
            consumer = work / (label + "-consumer")
            consumer.mkdir()
            for name in ("Program.cs", "HostMembarrierBridge.cs"):
                shutil.copy2(inputs / name, consumer / name)
            xml = ET.Element("Project", Sdk="Microsoft.NET.Sdk")
            props = ET.SubElement(xml, "PropertyGroup")
            for key, value in (("TargetFramework", "net10.0"), ("OutputType", "Exe"),
                               ("AllowUnsafeBlocks", "true"), ("Nullable", "enable"),
                               ("AssemblyName", "HostMembarrierProbe"), ("WarningsAsErrors", "CS8500")):
                ET.SubElement(props, key).text = value
            items = ET.SubElement(xml, "ItemGroup")
            ET.SubElement(items, "Compile", Include=str(generated / "*.cs"))
            ET.SubElement(items, "ProjectReference", Include=str(work / "host/Managed.Emulation.Host.csproj"))
            ET.SubElement(items, "TrimmerRootAssembly", Include="HostMembarrierProbe")
            project = consumer / "HostMembarrierProbe.csproj"
            ET.ElementTree(xml).write(project, encoding="unicode")
            if label == "optimized":
                run("optimized-restore", [dotnet, "restore", project])
                run("postprocess", [dotnet, post, project, "--in-place"])
                for name in ("Program.cs", "HostMembarrierBridge.cs"):
                    shutil.copy2(inputs / name, consumer / name)
            receipt["consumer_authored_sources"][label] = {
                name: sha(consumer / name) for name in ("Program.cs", "HostMembarrierBridge.cs")}
            for name, digest in receipt["consumer_authored_sources"][label].items():
                if digest != sha(inputs / name):
                    raise RuntimeError(f"consumer authored source differs before build: {label}/{name}")
            run(label + "-build", [dotnet, "build", project, "-c", "Release"])
            dll = consumer / "bin/Release/net10.0/HostMembarrierProbe.dll"
            value = run(label + "-jit", [dotnet, dll], 30, dll)
            if value != expected:
                raise RuntimeError(label + " JIT stdout differs from native")
            receipt["modes"][label + "-jit"] = True
            publish = work / (label + "-publish")
            run(label + "-publish", [dotnet, "publish", project, "-c", "Release", "-r", "linux-x64",
                                     "-p:PublishAot=true", "-o", publish], 300)
            executable = publish / "HostMembarrierProbe"
            value = run(label + "-aot", [executable], 30, executable)
            if value != expected:
                raise RuntimeError(label + " AOT stdout differs from native")
            receipt["modes"][label + "-aot"] = True
        if set(receipt["modes"]) != {"raw-jit", "raw-aot", "optimized-jit", "optimized-aot"}:
            raise RuntimeError("incomplete managed matrix")
        if files(raw) != receipt["raw_generated"] or files(inputs) != receipt["frozen_inputs"] or files(tools) != receipt["frozen_tools"]:
            raise RuntimeError("immutable snapshot changed")
        host_after = {str(p.relative_to(work / "host")): sha(p) for p in (work / "host").rglob("*")
                      if p.is_file() and not {"bin", "obj"}.intersection(p.relative_to(work / "host").parts)}
        if host_after != receipt["host_build_sources"]:
            raise RuntimeError("compiled Host sources changed")
        for p in build_config:
            if sha(work / p.name) != sha(inputs / p.name):
                raise RuntimeError("copied build configuration changed")
        for label, sources in receipt["consumer_authored_sources"].items():
            for name, digest in sources.items():
                if sha(work / (label + "-consumer") / name) != digest or digest != sha(inputs / name):
                    raise RuntimeError(f"compiled consumer authored source changed: {label}/{name}")
        for path, digest in receipt["authored_sources"].items():
            if sha(ROOT / path) != digest:
                raise RuntimeError(f"authored source changed: {path}")
        if files(headers) != receipt["header_sources"] or {name: files(path) for name, path in producer_dirs.items()} != receipt["producer_sources"]:
            raise RuntimeError("shared compiler/header identity changed")
        for tool in receipt["host_tools"].values():
            if sha(tool["path"]) != tool["sha256"]:
                raise RuntimeError("host tool changed")
        if sha(__file__) != receipt["runner_sha256"]:
            raise RuntimeError("runner changed")
        receipt["optimized_generated"] = files(optimized)
        receipt["final_identities_stable"] = True
        receipt["passed"] = True
    except BaseException as error:
        receipt["error"] = f"{type(error).__name__}: {error}"
        raise
    finally:
        save()
        print(json.dumps({"passed": receipt["passed"], "receipt": str(attempt / "receipt.json")}), flush=True)


if __name__ == "__main__":
    def interrupted(number, frame):
        raise InterruptedError(f"received signal {number}")
    signal.signal(signal.SIGTERM, interrupted)
    main()
