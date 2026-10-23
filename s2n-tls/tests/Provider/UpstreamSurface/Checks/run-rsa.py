"""Focused canonical public-RSA bindings in an immutable test-provider snapshot."""
from datetime import datetime, timezone
import hashlib
import json
from pathlib import Path
import shutil
import subprocess
import sys
import uuid

HERE = Path(__file__).resolve().parent
SURFACE = HERE.parent
ROOT = SURFACE.parents[2]
sys.path.insert(0, str(SURFACE))
from provider_snapshot import snapshot_provider
sys.path.insert(0, str(ROOT / "tests/Managed"))
from execution_identity import capture, hashes


def main():
    stamp = datetime.now(timezone.utc).strftime("%Y%m%dT%H%M%SZ") + "-" + uuid.uuid4().hex[:8]
    work = ROOT / "build/provider/rsa-surface-check" / stamp
    artifacts = ROOT / "artifacts/provider/rsa-surface-check" / stamp
    artifacts.mkdir(parents=True)
    snapshot = snapshot_provider(SURFACE / "CryptoProvider.Upstream.csproj", [SURFACE / "Bindings/Rsa.cs"], work / "provider")
    checks = Path(snapshot["project"]).parent / "Checks"
    checks.mkdir()
    selected = [HERE / name for name in ("RsaSurfaceChecks.csproj", "RsaMain.cs.txt", "RsaChecks.cs.txt", "RsaOpaqueTypes.cs.txt", "rsa-vectors.c", "rsa-opaque-provenance.json")]
    for path in selected:
        shutil.copyfile(path,checks / path.name)
    shutil.copyfile(ROOT / "samples/ManagedConsumer/ExecutionIdentity.cs", checks / "ExecutionIdentity.cs")
    project = checks / "RsaSurfaceChecks.csproj"
    inputs = hashes([*map(Path, snapshot["snapshot_inputs_sha256"]), *checks.iterdir()])
    authored = hashes([*selected, HERE / "run-rsa.py"])
    authored_copy = artifacts / "authored"
    authored_copy.mkdir()
    for path in authored:
        shutil.copyfile(path, authored_copy / Path(path).name)
    native = work / "native-vectors"
    fixture = ROOT / "ref/s2n-tls/tests/pems/rsa_2048_sha256_client_key.pem"
    library = ROOT / "build/native/openssl-legacy/libcrypto.a"
    opaque = json.loads((checks / "rsa-opaque-provenance.json").read_text())
    native_inputs = hashes([fixture, library, checks / "rsa-vectors.c", Path(opaque["actual_translated_header_source"])])
    if native_inputs[opaque["actual_translated_header_source"]] != opaque["actual_translated_header_source_sha256"]:
        raise RuntimeError("Retained actual opaque declarations source changed")
    report = {"status": "failed", "scope": "Canonical typed public-RSA test bindings, 277 native rows JIT+AOT; no original-C qualification or policy selection",
              "provider_snapshot": snapshot, "authored_candidate_sha256": authored, "private_inputs_sha256": inputs,
              "native_inputs_sha256": native_inputs, "commands": [], "executions": []}

    def run(command, label):
        command = list(map(str, command))
        log = artifacts / (label + ".log")
        with log.open("w") as stream:
            result = subprocess.run(command, cwd=ROOT, stdout=stream, stderr=subprocess.STDOUT, timeout=600)
        report["commands"].append({"command": command, "exit_code": result.returncode, "log": str(log),
                                   "log_sha256": hashes([log])[str(log)]})
        if result.returncode:
            raise RuntimeError("Private candidate failed: " + str(log))
        if hashes(inputs) != inputs or hashes(snapshot["original_inputs_sha256"]) != snapshot["original_inputs_sha256"] or hashes(authored) != authored:
            raise RuntimeError("Frozen private/live candidate inputs changed")
        return log

    try:
        run(["cc", "-O2", "-std=c11", "-Wall", "-Wextra", "-I", ROOT / "build/native/openssl-legacy/include",
             checks / "rsa-vectors.c", library, "-ldl", "-pthread", "-o", native], "native-build")
        prefix = json.loads((ROOT / "config/provider.json").read_text())["native_version_prefix"]
        expected = run([native, fixture, prefix], "native-execute").read_text()
        for mode in ("jit", "aot"):
            target = work / mode
            command = ["dotnet", "publish" if mode == "aot" else "build", project, "-c", "Release", "--artifacts-path", target / "artifacts",
                       "-m:1", "-p:RestoreLockedMode=true", "-p:UseSharedCompilation=false"]
            if mode == "aot":
                command += ["-r", "linux-x64", "--self-contained", "true", "-p:PublishAot=true", "-p:DebugSymbols=false", "-p:DebugType=None", "-o", target / "publish"]
            run(command, mode + "-build")
            executable = target / "publish/RsaSurfaceChecks" if mode == "aot" else target / "artifacts/bin/RsaSurfaceChecks/release/RsaSurfaceChecks.dll"
            launch = [executable] if mode == "aot" else ["dotnet", executable]
            output = run([*launch, fixture], mode + "-execute").read_text()
            if output != expected:
                raise RuntimeError("Private RSA differs from actual native transcript: " + mode)
            runtime = json.loads(run([*launch, "--identity"], mode + "-identity").read_text())
            identity = capture(list(map(str, launch)), target / "artifacts", runtime, inputs,
                               {"private_provider_snapshot": snapshot, "candidate_sha256": authored})
            path = artifacts / (mode + "-execution-identity.json")
            path.write_text(json.dumps(identity, indent=2) + "\n")
            report["executions"].append({"mode": mode, "status": "passed", "vectors": len(output.splitlines()),
                                         "application_execution_receipt": str(path), "application_execution_receipt_sha256": hashes([path])[str(path)],
                                         "managed_context_cleanup": "actual LiveContexts baseline assertion passed"})
        if hashes(native_inputs) != native_inputs:
            raise RuntimeError("Pinned native proof inputs changed")
        report["status"] = "passed"
    except Exception as error:
        report["error"] = str(error)
    receipt = artifacts / "receipt.json"
    receipt.write_text(json.dumps(report, indent=2) + "\n")
    print("RSA SURFACE", receipt, report["status"], flush=True)
    return 0 if report["status"] == "passed" else 1


if __name__ == "__main__":
    raise SystemExit(main())
