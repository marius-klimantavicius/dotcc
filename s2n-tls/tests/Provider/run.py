#!/usr/bin/env python3
"""Build/run only this provider and independent oracle; never rebuild shared DotCC tools."""
import argparse
import json
from pathlib import Path
import platform
import shutil
import subprocess
import sys

ROOT = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(ROOT.parent / "Scripts"))
from campaigns.identity import HashPolicy
from campaigns.inputs import acquire
from campaigns.model import Source


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--fetch", choices=("missing", "never"), default="missing")
    parser.add_argument("--hashes", choices=("warn", "off", "strict"), default="warn")
    parser.add_argument("--mode", choices=("jit", "aot", "all"), default="all")
    parser.add_argument("--openssl", type=Path, default=ROOT / "build/native/openssl-legacy")
    parser.add_argument("--artifacts", type=Path, default=ROOT / "artifacts/provider")
    args = parser.parse_args()
    if platform.system() != "Linux" or platform.machine() not in ("x86_64", "AMD64"):
        parser.error("This native oracle is qualified only on Linux x64; no portable execution claim.")
    metadata = json.loads((ROOT / "config/provider.json").read_text())
    pin = metadata["managed_dependency"]
    args.artifacts.mkdir(parents=True, exist_ok=True)
    commands = []

    def status(value, **extra):
        receipt = {"scope": "provider primitive capability and pinned native differential tests; not emitted ABI or TLS qualification",
                   "commands": commands, "dependency": pin, "mode": args.mode, "status": value, **extra}
        (args.artifacts / "receipt.json").write_text(json.dumps(receipt, indent=2) + "\n")

    status("running")
    source = Source("provider-managed", pin["url"], ROOT / "ref" / pin["archive"], ROOT / "ref" / pin["directory"], pin["prefix"],
                    required=(pin["license_path"], "crypto/src/crypto/engines/RC4Engine.cs"))
    # Match the shared campaign policy: never replace an existing usable local tree.
    try:
        acquire(source, HashPolicy(args.hashes), args.fetch)
    except Exception as error:
        status("failed", error=str(error))
        raise
    work = ROOT / "build/provider"
    work.mkdir(parents=True, exist_ok=True)
    args.artifacts.mkdir(parents=True, exist_ok=True)
    shutil.copyfile(source.directory / pin["license_path"], args.artifacts / "BouncyCastle-LICENSE.md")

    def run(argv, log):
        command = [str(item) for item in argv]
        commands.append(command)
        try:
            with (args.artifacts / log).open("w") as output:
                process = subprocess.run(command, cwd=ROOT.parent, stdout=output, stderr=subprocess.STDOUT, timeout=600)
        except Exception as error:
            status("failed", error=str(error), log=log)
            raise
        if process.returncode:
            status("failed", exit_code=process.returncode, log=log)
            print((args.artifacts / log).read_text(), file=sys.stderr)
            raise RuntimeError(f"Command failed ({process.returncode}); {args.artifacts / log}")

    headers, library = args.openssl / "include", args.openssl / "libcrypto.a"
    if not (headers / "openssl/opensslconf.h").is_file() or not library.is_file():
        status("failed", error="Missing pinned native backend")
        raise RuntimeError("Pinned native oracle backend missing; run tests/NativeOracle/run.py first. No system libcrypto fallback.")
    oracle = work / "native-vectors"
    run(["cc", "-std=c11", "-O2", "-Wall", "-Wextra", "-I", headers, ROOT / "tests/Provider/native-vectors.c", library, "-ldl", "-pthread", "-o", oracle], "native-build.log")
    run(["cc", "-std=c11", "-I", headers, ROOT / "tests/Abi/provider-layout.c", "-o", work / "provider-layout"], "layout-build.log")
    run([work / "provider-layout"], "native-layout.json")
    if args.mode in ("jit", "all"):
        run(["dotnet", "run", "--project", ROOT / "tests/Provider/ProviderTests.csproj", "-c", "Release", "--artifacts-path", work / "dotnet", "--", oracle, args.artifacts / "jit", metadata["native_version_prefix"]], "jit.log")
    if args.mode in ("aot", "all"):
        run(["dotnet", "publish", ROOT / "tests/Provider/ProviderTests.csproj", "-c", "Release", "-r", "linux-x64", "-p:PublishAot=true", "-o", work / "aot", "--nologo", "-v", "minimal", "-m:1", "--artifacts-path", work / "dotnet"], "aot-build.log")
        run([work / "aot/ProviderTests", oracle, args.artifacts / "aot", metadata["native_version_prefix"]], "aot.log")
    status("passed")
    print(f"Provider checks passed ({args.mode}): {args.artifacts / 'receipt.json'}")


if __name__ == "__main__":
    main()
