#!/usr/bin/env python3
"""Compare actual emitted upstream C layouts with a matching native header probe."""
import argparse
import hashlib
import json
from pathlib import Path
import subprocess

ROOT = Path(__file__).resolve().parents[2]


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--compiler", type=Path, required=True)
    parser.add_argument("--mode", choices=("jit", "aot", "all"), default="all")
    parser.add_argument("--work", type=Path, default=ROOT / "build/core-abi")
    parser.add_argument("--artifacts", type=Path, default=ROOT / "artifacts/core-abi")
    args = parser.parse_args()
    compiler = args.compiler.resolve()
    work, artifacts = args.work.resolve(), args.artifacts.resolve()
    work.mkdir(parents=True, exist_ok=True)
    artifacts.mkdir(parents=True, exist_ok=True)
    report = {"scope": "Actual upstream C type/offset/bitfield native-emitted probe; not whole-library product qualification", "commands": [],
              "compiler": str(compiler), "tool_closure": {file.name: hashlib.sha256(file.read_bytes()).hexdigest()
              for file in compiler.parent.iterdir() if file.suffix in (".dll", ".json")}}

    def save(status, **extra):
        (artifacts / "receipt.json").write_text(json.dumps({**report, "status": status, **extra}, indent=2) + "\n")

    def run(argv, label):
        command = list(map(str, argv))
        log = artifacts / (label + ".log")
        row = {"argv": command, "log": str(log)}
        report["commands"].append(row)
        with log.open("w") as output:
            process = subprocess.run(command, cwd=ROOT.parent, stdout=output, stderr=subprocess.STDOUT, timeout=1200)
        row["exit_code"] = process.returncode
        if process.returncode:
            save("failed", stage=label)
            raise RuntimeError("Core ABI probe failed: " + str(log))

    save("running")
    provider = json.loads((ROOT / "config/provider.json").read_text())
    definitions = ["-D" + value for value in provider["compiler_definitions"]]
    includes = [ROOT / "config/headers", ROOT / "ref/openssl-legacy/include", ROOT / "ref/s2n-tls", ROOT / "ref/s2n-tls/api"]
    flags = [*definitions, *(part for path in includes for part in ("-I", str(path)))]
    source = ROOT / "tests/Abi/s2n-layout.c"
    run(["cc", "-std=c11", "-DNATIVE_LAYOUT_CONTROL=1", *flags, source, "-o", work / "native-layout"], "native-build")
    run([work / "native-layout"], "native")
    run(["dotnet", compiler, "-std=c11", *flags, source, "--emit=managedlib", "--class-name", "S2nLayout", "--namespace", "Managed.Security.S2n",
         "--nest-types", "--runtime=c", "--literal-pool", "--deduplicate-inline", "--prune-unused-inline", "-o", work / "Emitted"], "emit")
    reference = "-p:CoreAbiProject=" + str(work / "Emitted/Emitted.csproj")
    if args.mode in ("jit", "all"):
        run(["dotnet", "run", "--project", ROOT / "tests/CoreAbi/CoreAbi.csproj", "-c", "Release", reference,
             "--artifacts-path", work / "dotnet/jit", "--", artifacts / "native.log", artifacts / "jit.json"], "jit")
    if args.mode in ("aot", "all"):
        run(["dotnet", "publish", ROOT / "tests/CoreAbi/CoreAbi.csproj", "-c", "Release", "-r", "linux-x64", "-p:PublishAot=true", reference,
             "--artifacts-path", work / "dotnet/aot", "-m:1", "-o", work / "aot"], "aot-build")
        run([work / "aot/CoreAbi", artifacts / "native.log", artifacts / "aot.json"], "aot")
    save("passed", mode=args.mode)
    print("Core ABI probe passed: " + str(artifacts / "receipt.json"))


if __name__ == "__main__":
    main()
