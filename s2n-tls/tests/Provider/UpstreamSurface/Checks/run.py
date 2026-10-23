"""Check the truthful extra identity API in a private provider snapshot."""
from datetime import datetime, timezone
import argparse
import hashlib
import json
from pathlib import Path
import shutil
import subprocess
import sys
import uuid

CHECKS = Path(__file__).resolve().parent
SURFACE = CHECKS.parent
ROOT = SURFACE.parents[2]
sys.path.insert(0, str(SURFACE))
from provider_snapshot import snapshot_provider


def hashes(paths):
    return {str(p): hashlib.sha256(p.read_bytes()).hexdigest() for p in sorted(paths)}


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--names", action="store_true", help="Compare actual X509 name mutation against the pinned native library")
    args = parser.parse_args()
    stamp = datetime.now(timezone.utc).strftime("%Y%m%dT%H%M%SZ") + "-" + uuid.uuid4().hex[:8]
    work = ROOT / "build/provider/upstream-surface-check" / stamp
    artifacts = ROOT / "artifacts/provider/upstream-surface-check" / stamp
    artifacts.mkdir(parents=True)
    snapshot = snapshot_provider(SURFACE / "CryptoProvider.Upstream.csproj",
                                [SURFACE / "Bindings/Identity.cs"], work / "provider")
    frozen_checks = Path(snapshot["project"]).parent / "Checks"
    frozen_checks.mkdir()
    check_inputs = [CHECKS / "IdentityChecks.csproj", *CHECKS.glob("*.cs")]
    if args.names:
        check_inputs.append(CHECKS / "name-vectors.c")
    original_checks = hashes(check_inputs)
    for path in check_inputs:
        shutil.copyfile(path, frozen_checks / path.name)
    frozen_inputs = hashes([*map(Path, snapshot["snapshot_inputs_sha256"]),
                           *(frozen_checks / p.name for p in check_inputs)])
    report = {"status": "failed", "scope": "Truthful managed test-only provider identity; no original-C qualification claim",
              "provider_snapshot": snapshot, "check_inputs_sha256": original_checks,
              "frozen_inputs_sha256": frozen_inputs, "commands": [], "executions": []}

    def command(argv, label):
        argv = list(map(str, argv))
        log = artifacts / (label + ".log")
        with log.open("w") as output:
            result = subprocess.run(argv, cwd=ROOT, stdout=output, stderr=subprocess.STDOUT, timeout=300)
        report["commands"].append({"command": argv, "exit_code": result.returncode,
                                   "log": str(log), "log_sha256": hashes([log])[str(log)]})
        if result.returncode:
            raise RuntimeError("Check failed: " + str(log))
        if hashes(map(Path, frozen_inputs)) != frozen_inputs:
            raise RuntimeError("Frozen provider/check inputs changed")
        return log

    try:
        native_text = None
        if args.names:
            source = frozen_checks / "name-vectors.c"
            library = ROOT / "build/native/openssl-legacy/libcrypto.a"
            fixture = ROOT / "ref/s2n-tls/tests/pems/rsa_2048_sha256_client_cert.pem"
            native = work / "name-vectors"
            report["native_inputs_sha256"] = hashes([source, library, fixture])
            command(["cc", "-std=c11", "-O2", "-Wall", "-Wextra", "-I", ROOT / "build/native/openssl-legacy/include",
                     source, library, "-ldl", "-pthread", "-o", native], "native-build")
            prefix = json.loads((ROOT / "config/provider.json").read_text())["native_version_prefix"]
            native_text = command([native, fixture, prefix], "native-execute").read_text()
        for mode in ("jit", "aot"):
            target = work / mode
            argv = ["dotnet", "publish" if mode == "aot" else "build", frozen_checks / "IdentityChecks.csproj",
                    "-c", "Release", "--artifacts-path", target / "artifacts", "-m:1",
                    "-p:RestoreLockedMode=true", "-p:UseSharedCompilation=false"]
            if mode == "aot":
                argv += ["-r", "linux-x64", "--self-contained", "true", "-p:PublishAot=true",
                         "-p:DebugSymbols=false", "-p:DebugType=None", "-o", target / "publish"]
            command(argv, mode + "-build")
            executable = target / "publish/IdentityChecks" if mode == "aot" else target / "artifacts/bin/IdentityChecks/release/IdentityChecks.dll"
            launch = [executable] if mode == "aot" else ["dotnet", executable]
            log = command(launch, mode + "-execute")
            if not log.read_text().startswith("PASSED 35 managed identity assertions: DotCC managed s2n crypto /"):
                raise RuntimeError("Missing actual identity assertion transcript")
            report["executions"].append({"mode": mode, "status": "passed", "assertions": 35,
                "log": str(log), "output_sha256": hashes(p for p in executable.parent.iterdir() if p.is_file())})
            if args.names:
                log = command([*launch, "names", fixture], mode + "-names")
                if log.read_text() != native_text:
                    raise RuntimeError("Name mutation differs from pinned native: " + str(log))
                report["executions"].append({"mode": mode, "status": "passed", "comparison": "exact native name/DER/copy transcript",
                                             "log": str(log)})
        if hashes(map(Path, snapshot["original_inputs_sha256"])) != snapshot["original_inputs_sha256"] \
            or hashes(check_inputs) != original_checks:
            raise RuntimeError("Original production/test input changed")
        report["status"] = "passed"
    except Exception as error:
        report["error"] = str(error)
    receipt = artifacts / "receipt.json"
    receipt.write_text(json.dumps(report, indent=2) + "\n")
    print("RECEIPT", receipt, report["status"], flush=True)
    return 0 if report["status"] == "passed" else 1


if __name__ == "__main__":
    raise SystemExit(main())
