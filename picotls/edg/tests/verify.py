#!/usr/bin/env python3
"""Compare the pinned unchanged picotls C core under native, EDG JIT and NativeAOT.

This is a utility/parser/allocation/stdio integration test, not a TLS or crypto
validation. Cryptography and managed-provider integration have separate tests.
Every command and original stdout/stderr stream is retained, including failure.
"""
import argparse
import hashlib
import json
import os
from pathlib import Path
import platform
import subprocess
import sys
import time

ROOT = Path(__file__).resolve().parents[3]
TESTS = Path(__file__).resolve().parent
EXPECTED = (
    b"buffers: stack, growth, 64-byte alignment, preservation, ASN.1 passed\n"
    b"integers: 12 QUIC boundaries, every truncation, network byte order passed\n"
    b"base64: RFC 4648 vectors, streaming, whitespace, malformed input passed\n"
    b"utilities: memory callbacks, escaping, hex, IPv4/IPv6 passed\n"
    b"PEM: real filesystem, two objects, allocation ownership passed\n"
    b"picotls unchanged core differential fixture passed\n"
)


def digest(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--compiler", type=Path, required=True)
    parser.add_argument("--output-dir", type=Path, required=True)
    parser.add_argument("--native-only", action="store_true")
    parser.add_argument("--libc-pruning", choices=("reachable", "all"), default="reachable")
    args = parser.parse_args()
    out = args.output_dir.resolve()
    out.mkdir(parents=True, exist_ok=False)
    pin = json.loads((ROOT / "picotls/config/inputs.json").read_text())["picotls"]
    upstream = ROOT / "picotls/ref" / pin["directory"]
    sources = [upstream / "lib" / name for name in ("picotls.c", "hpke.c", "pembase64.c")]
    sources.append(TESTS / "core.c")
    inputs = sources + sorted((upstream / "include").rglob("*.h")) + [Path(__file__).resolve()]
    report = {"passed": False, "revision": pin["revision"], "archiveSha256": pin["sha256"],
              "sources": {str(p.relative_to(ROOT)): digest(p) for p in inputs},
              "compiler": {"path": str(args.compiler.resolve()), "sha256": digest(args.compiler)},
              "checks": [], "commands": [], "libcPruning": args.libc_pruning,
              "comparison": "exact stdout bytes; no normalization",
              "scope": "actual unchanged core APIs; no TLS handshake or cryptographic provider in this fixture"}

    def save():
        (out / "results.json").write_text(json.dumps(report, indent=2) + "\n")

    def run(label, command, work=None):
        command = list(map(str, command))
        entry = {"label": label, "command": command, "cwd": str(work or ROOT)}
        report["commands"].append(entry)
        save()
        start = time.monotonic()
        with (out / (label + ".stdout")).open("wb") as stdout, (out / (label + ".stderr")).open("wb") as stderr:
            result = subprocess.run(command, cwd=work or ROOT, stdout=stdout, stderr=stderr, timeout=1800,
                                    env={**os.environ, "DOTNET_PROCESSOR_COUNT": "4"})
        entry.update(exitCode=result.returncode, elapsedSeconds=time.monotonic() - start)
        entry["stdoutSha256"] = digest(out / (label + ".stdout"))
        entry["stderrSha256"] = digest(out / (label + ".stderr"))
        save()
        if result.returncode:
            raise RuntimeError(f"{label} failed with exit {result.returncode}; see retained logs in {out}")
        return (out / (label + ".stdout")).read_bytes()

    def execute(label, command):
        work = out / (label + "-work")
        work.mkdir()
        actual = run(label, command, work)
        if actual != EXPECTED:
            raise RuntimeError(f"{label}: output differs from independently checked native fixture contract")
        report["checks"].append(label + ": exact expected bytes and zero exit status")
        save()

    try:
        definitions = ["-DPTLS_HAVE_LOG=0", "-DPICOTLS_USE_DTRACE=0"]
        native = out / "native"
        run("native-build", ["cc", "-std=c11", "-D_GNU_SOURCE", *definitions,
                             "-I", upstream / "include", *sources, "-o", native])
        execute("native-run", [native])
        if not args.native_only:
            generated = out / "managed"
            run("translate", [sys.executable, ROOT / "edgcpp/dotnet.py", "--compiler", args.compiler.resolve(),
                              "--language", "c", "--std", "c11", "--target", "portable-lp64-binary80-v2",
                              "--libc", "musl", "--libc-pruning", args.libc_pruning, "--posix", "--namespace", "Picotls.Edg.Tests", "--class-name", "Core",
                              "--unbound-externals", "reject", "--split", "source", "-I", upstream / "include",
                              *definitions, "--output-dir", generated, "--raw-output", out / "raw.cs",
                              "--frontend-invocation", out / "frontend-invocation.json", *sources])
            manifest = json.loads((generated / "dot-gen.json").read_text())
            if manifest["unboundExternals"]["symbols"]:
                raise RuntimeError("unbound externals in generated core")
            report["checks"].append("all three unchanged core source files translated with no unbound externals")
            project = generated / "Generated.csproj"
            run("jit-build", ["dotnet", "build", project, "-c", "Release", "--nologo", "-p:TreatWarningsAsErrors=true"])
            execute("jit-run", ["dotnet", generated / "bin/Release/net10.0/Generated.dll"])
            system = {"Linux": "linux", "Darwin": "osx", "Windows": "win"}[platform.system()]
            rid = system + ("-arm64" if platform.machine().lower() in ("aarch64", "arm64") else "-x64")
            aot = out / "aot"
            run("aot-publish", ["dotnet", "publish", project, "-c", "Release", "-r", rid,
                                "-p:PublishAot=true", "-p:IlcTreatWarningsAsErrors=true", "-p:TreatWarningsAsErrors=true",
                                "-o", aot])
            execute("aot-run", [aot / ("Generated.exe" if system == "win" else "Generated")])
            if any(digest(ROOT / path) != checksum for path, checksum in report["sources"].items()):
                raise RuntimeError("source inputs changed during verification")
        report["passed"] = True
        print("picotls core differential fixture passed: " + str(len(report["checks"])) + " checks")
    except BaseException as error:
        report["failure"] = str(error)
        raise
    finally:
        save()


if __name__ == "__main__":
    main()
