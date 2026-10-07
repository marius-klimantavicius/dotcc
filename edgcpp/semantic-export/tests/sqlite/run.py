#!/usr/bin/env python3
"""Export pinned SQLite and its C memory VFS, emit C#, and run differential tests.

The supplied compiler must be an existing semantic-export frontend with its
cpfe-semantic.producer.json provenance file. This driver does not build EDG.
Every command, log, package, generated file, and verification result is retained
under a fresh output directory. Add --nativeaot to verify the published binary
as well as JIT execution.
"""

import argparse
import hashlib
import json
from pathlib import Path
import subprocess
import sys
import time


HERE = Path(__file__).resolve().parent
SEMANTIC = HERE.parents[1]
EDG = SEMANTIC.parent
REPOSITORY = EDG.parent
SQLITE = REPOSITORY / "sqlite"
AMALGAMATION = SQLITE / "ref/sqlite-amalgamation-3530400"


def write_json(path, value):
    path.write_text(json.dumps(value, indent=2) + "\n")


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--compiler", required=True, type=Path,
                        help="Existing cpfe-semantic executable")
    parser.add_argument("--output", required=True, type=Path,
                        help="Fresh directory for the complete pipeline evidence")
    parser.add_argument("--nativeaot", action="store_true",
                        help="Also publish and compare NativeAOT execution")
    args = parser.parse_args()
    compiler = args.compiler.resolve()
    output = args.output.resolve()
    definitions = SQLITE / "config/defines.txt"
    sqlite_source = AMALGAMATION / "sqlite3.c"
    vfs_source = SQLITE / "tests/memory_vfs.c"
    required = [compiler, compiler.parent / "cpfe-semantic.producer.json",
                definitions, sqlite_source, AMALGAMATION / "sqlite3.h",
                vfs_source, SQLITE / "tests/memory_vfs.h", HERE / "verify.py"]
    for path in required:
        if not path.is_file():
            parser.error("Required input is missing: " + str(path))
    if output.exists():
        parser.error("Output directory must be fresh: " + str(output))
    output.mkdir(parents=True)
    logs = output / "logs"
    logs.mkdir()
    checks = []
    evidence = {
        "schema": "edg-semantic-sqlite-pipeline/1",
        "status": "running",
        "nativeAot": args.nativeaot,
        "checks": checks,
    }

    def record():
        write_json(output / "pipeline.json", evidence)

    def run(name, command, timeout):
        command = [str(arg) for arg in command]
        write_json(logs / (name + ".command.json"), {
            "arguments": command, "cwd": str(REPOSITORY), "timeoutSeconds": timeout,
        })
        print(name, flush=True)
        started = time.monotonic()
        check = {"name": name, "command": "logs/" + name + ".command.json"}
        checks.append(check)
        record()
        try:
            with (logs / (name + ".stdout")).open("w") as stdout, \
                    (logs / (name + ".stderr")).open("w") as stderr:
                result = subprocess.run(command, cwd=REPOSITORY, stdout=stdout,
                                        stderr=stderr, timeout=timeout)
            check["exit"] = result.returncode
            if result.returncode:
                raise RuntimeError(f"{name} failed with exit {result.returncode}; "
                                   f"see {logs / name}.stdout/.stderr")
        except subprocess.TimeoutExpired:
            check["timeout"] = True
            raise RuntimeError(f"{name} exceeded {timeout} seconds; logs retained in {logs}") from None
        finally:
            check["seconds"] = round(time.monotonic() - started, 3)
            record()

    # Keep the established SQLite feature profile unchanged. The atomic macro
    # supplies the compiler predefined value expected by the portable headers;
    # it does not enable SQLite threading or change SQLite's configuration.
    defines = [line.strip() for line in definitions.read_text().splitlines()
               if line.strip() and not line.lstrip().startswith("#")]
    flags = ["--c17", "--gnu_version", "130300", "-I",
             str(EDG / "backend/runtime/libc/include"),
             *["-D" + definition for definition in defines],
             "-D__ATOMIC_RELAXED=0", "-I", str(AMALGAMATION)]
    write_json(output / "profile.json", {
        "definitionsFile": str(definitions), "definitions": defines,
        "frontendFlags": flags,
        "inputs": [{"path": str(path),
                    "sha256": hashlib.sha256(path.read_bytes()).hexdigest()}
                   for path in required],
    })
    wrapper = output / "sqlite-memory.c"
    wrapper.write_text("#include " + json.dumps(str(sqlite_source)) + "\n" +
                       "#include " + json.dumps(str(vfs_source)) + "\n")
    standalone = output / "package-sqlite"
    memory = output / "package-memory"
    emitter = output / "emitter"
    generated = output / "generated"
    verification = output / "verification"
    evidence.update({
        "sqlitePackage": str(standalone), "memoryVfsPackage": str(memory),
        "generated": str(generated), "verification": str(verification),
    })
    record()
    try:
        for name, source, destination in [
                ("export-sqlite", sqlite_source, standalone),
                ("export-memory", wrapper, memory)]:
            run(name, [sys.executable, SEMANTIC / "export.py", "--compiler", compiler,
                       "--output", destination, source, "--", *flags], timeout=1800)
        run("build-emitter", ["dotnet", "build", SEMANTIC / "managed/Edg.Semantic.csproj",
                              "-c", "Release", "-o", emitter], timeout=600)
        run("emit-project", ["dotnet", emitter / "Edg.Semantic.dll", "emit-project",
                             memory, "--output-directory", generated], timeout=1800)
        verify = [sys.executable, HERE / "verify.py", "--package", memory,
                  "--generated", generated, "--output", verification]
        if args.nativeaot:
            verify.append("--nativeaot")
        run("verify", verify, timeout=3600 if args.nativeaot else 1800)
        results = verification / "results.json"
        if not results.is_file():
            raise RuntimeError("Verification exited without writing results.json")
        evidence["verificationResults"] = json.loads(results.read_text())
        evidence["status"] = "passed"
        record()
    except Exception as error:
        evidence["status"] = "failed"
        evidence["error"] = str(error)
        record()
        raise
    print("PASS SQLite semantic export pipeline: " + str(output / "pipeline.json"))


if __name__ == "__main__":
    main()
